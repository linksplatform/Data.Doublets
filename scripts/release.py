"""Validate release credentials, package C++ headers, and publish tested artifacts.

No command prints credential values. CI_VERBOSE adds safe operation summaries.
"""

import argparse
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request
import xml.etree.ElementTree as ET
import zipfile

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "csharp/Platform.Data.Doublets/Platform.Data.Doublets.csproj"
NUSPEC = ROOT / "cpp/Platform.Data.Doublets/Platform.Data.Doublets.TemplateLibrary.nuspec"
NAMESPACE = {"n": "http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd"}
HINT = (
    "Configure NuGet trusted publishing: set the non-secret Actions variable NUGET_USER "
    "to the policy creator's actual NuGet profile name (not an email or an inferred GitHub login). "
    "Verify an active policy for linksplatform/Data.Doublets, workflow filename ci.yml, "
    "with no environment, owned by the NuGet account/organization owning both "
    "Platform.Data.Doublets and Platform.Data.Doublets.TemplateLibrary, and package push/version scope "
    "covering those IDs. Grant id-token: write to the login job. "
    "See docs/trusted-publishing.md. Only after trusted publishing fails, the provided "
    "NUGET_TOKEN secret may be used as fallback; never rotate or delete it automatically."
)


def check_auth_config():
    user = os.environ.get("NUGET_USER", "")
    if not re.fullmatch(r"[A-Za-z0-9_.-]+", user):
        raise RuntimeError("NUGET_USER must be the actual NuGet policy creator's profile name. " + HINT)


def info(language):
    if language == "csharp":
        tree = ET.parse(PROJECT)
        return {"id": tree.findtext(".//PackageId"), "version": tree.findtext(".//VersionPrefix"), "notes": tree.findtext(".//PackageReleaseNotes")}
    tree = ET.parse(NUSPEC)
    return {key: tree.findtext(f".//n:{tag}", namespaces=NAMESPACE) for key, tag in [("id", "id"), ("version", "version"), ("notes", "releaseNotes")]}


def request(url, *, method="GET", headers=None):
    # Redirects may forward custom NuGet API key headers; never follow them.
    class NoRedirect(urllib.request.HTTPRedirectHandler):
        def redirect_request(self, req, fp, code, msg, hdrs, newurl):
            return None

    req = urllib.request.Request(url, headers=headers or {}, method=method, data=b"" if method == "POST" else None)
    try:
        with urllib.request.build_opener(NoRedirect).open(req, timeout=30) as response:
            return response.status, response.read()
    except urllib.error.HTTPError as error:
        # Preserve a bounded server explanation, never headers or exception text.
        return error.code, error.read(8192)
    except (OSError, urllib.error.URLError):
        return 0, b""


def diagnostic(body, *credentials):
    """Bound and flatten untrusted text before emitting any workflow annotation."""
    value = body.decode('utf-8', errors='replace') if isinstance(body, bytes) else str(body)
    secrets = [*credentials, *(os.environ.get(name, '') for name in
               ('NUGET_TOKEN', 'NUGET_FALLBACK_TOKEN', 'GH_TOKEN', 'ACTIONS_ID_TOKEN_REQUEST_TOKEN'))]
    for secret in sorted(set(secrets), key=len, reverse=True):
        if secret:
            for form in (secret, urllib.parse.quote(secret, safe='')):
                value = value.replace(form, '[REDACTED]')
    return ' '.join(value.split())[:1024]


def authenticate_fallback(token, package):
    # This endpoint authenticates push credentials, including first publication.
    # It REPLACES the package subject: the returned key is never a scope proof.
    package_id = urllib.parse.quote(package['id'], safe='')
    version = urllib.parse.quote(package['version'], safe='')
    status, body = request(f'https://www.nuget.org/api/v2/package/create-verification-key/{package_id}/{version}',
                           method='POST', headers={'X-NuGet-ApiKey': token})
    if status != 200:
        if status in (401, 403):
            category = 'authentication/authorization rejected'
        elif status in (0, 408, 429) or status >= 500:
            category = 'authentication probe transport/transient failure'
        else:
            category = 'authentication probe failed/unsupported (not package-scope evidence)'
        raise RuntimeError(f'{category}: HTTP {status or "unreachable"}; {diagnostic(body, token)}')
    try:
        key = json.loads(body)['Key']
        if not isinstance(key, str) or not key:
            raise ValueError('Missing authentication result')
    except (ValueError, KeyError, TypeError):
        raise RuntimeError('authentication probe returned a malformed response') from None
    print('Provided key authenticated for push actions; package scope remains unverified. '
          'The derived verification key is discarded and cannot prove the original package subject.')


def fallback_auth(reason, languages, package=None):
    token = os.environ.get('NUGET_FALLBACK_TOKEN', '')
    if not token:
        print(f'auth=none; {reason}; token-fallback=missing')
        raise RuntimeError(f'{reason}; provided token fallback is missing. {HINT}')
    try:
        authenticate_fallback(token, package or info(languages[0]))
    except RuntimeError as error:
        print(f'auth=none; {reason}; token-fallback=failed')
        raise RuntimeError(f'{reason}; provided token fallback failed: {diagnostic(error, token)}') from None
    print(f'auth=token-fallback; {reason}')
    return 'token-fallback', token


def select_nuget_auth(languages):
    token = os.environ.get('NUGET_TOKEN', '')
    try:
        check_auth_config()
    except RuntimeError:
        return fallback_auth('trusted publishing failed/unavailable; failed-step=configuration (NUGET_USER profile)', languages)
    outcome = os.environ.get('NUGET_LOGIN_OUTCOME', '')
    if token and outcome == 'success':
        print('auth=trusted-publishing; NuGet/login exchange succeeded; stored fallback was not tested')
        return 'trusted-publishing', token
    return fallback_auth('trusted publishing failed/unavailable; failed-step=NuGet/login (exchange failed or key output missing)', languages)


def verify_nuget(key, package):
    # VerifyPackageKey evaluates PackageVerify user policies before owner/scope.
    # Push-only OIDC keys can get HTTP 400 even after successful authentication.
    # No supported non-upload proof of the original package subject is available.
    status, body = request(f"https://api.nuget.org/v3-flatcontainer/{package['id'].lower()}/index.json")
    if status == 404:
        print(f"::notice::NuGet package scope for {package['id']} is unverified: no public version exists; the first push must enforce ownership and scope.")
        return None
    if status != 200:
        return f"NuGet public package index returned HTTP {status or 'unreachable'}; scope is unverified. {diagnostic(body, key)}"
    try:
        versions = json.loads(body)["versions"]
        if not isinstance(versions, list) or not versions or not isinstance(versions[-1], str) or not re.fullmatch(r"[0-9A-Za-z.+-]+", versions[-1]):
            raise ValueError("No versions")
    except (ValueError, KeyError, TypeError):
        return "NuGet package index is malformed; refusing to assume publish permissions."
    print(f"::notice::NuGet package scope for {package['id']} is unverified/unsupported by non-upload probes for push-only keys; actual publication must enforce ownership and package authorization.")
    return None


def verify_github(mode):
    github_token = os.environ.get("GH_TOKEN", "")
    if not github_token:
        return "GH_TOKEN is missing; release jobs need contents: write and packages: write."
    if mode != "release":
        return None
    repository = os.environ.get("GITHUB_REPOSITORY", "linksplatform/Data.Doublets")
    status, body = request(f"https://api.github.com/repos/{repository}", headers={"Authorization": f"Bearer {github_token}", "Accept": "application/vnd.github+json"})
    try:
        can_push = status == 200 and json.loads(body).get("permissions", {}).get("push")
    except (ValueError, TypeError, AttributeError):
        can_push = False
    if not can_push:
        return "Workflow token cannot write releases; grant contents: write and packages: write on the release job."
    return None


def verify_nuget_auth(mode, languages):
    if mode != 'release':
        return ['NuGet trusted publishing and package scope are unverified in report mode. ' + HINT]
    try:
        _, token = select_nuget_auth(languages)
    except RuntimeError as error:
        return [str(error)]
    errors = []
    for language in languages:
        error = verify_nuget(token, info(language))
        if error:
            errors.append(error)
    return errors


def preflight(mode, languages, credentials_only=False):
    errors = verify_nuget_auth(mode, languages)
    if not credentials_only:
        error = verify_github(mode)
        if error:
            errors.append(error)
    for error in errors:
        print(f"::{'error' if mode == 'release' else 'notice'}::{error}")
    if errors and mode == "release":
        raise RuntimeError("Release preflight failed; no packages or tags were published.")
    if mode != "release":
        print("PR validation continues without publishing credentials; OIDC exchange and package scope are unverified in report mode")
    else:
        print("Release preflight passed; publication and public downloads remain unverified until publish succeeds")


def pack_cpp(output):
    package = info("cpp")
    output.mkdir(parents=True, exist_ok=True)
    headers = sorted((ROOT / "cpp/Platform.Data.Doublets").rglob("*.h"))
    if not headers:
        raise RuntimeError("No C++ headers to package")
    ET.register_namespace("", NAMESPACE["n"])
    metadata = ET.tostring(ET.parse(NUSPEC).getroot(), encoding="utf-8", xml_declaration=True)
    target = output / f"{package['id']}.{package['version']}.nupkg"
    with zipfile.ZipFile(target, "w", compression=zipfile.ZIP_DEFLATED) as archive:
        archive.writestr(f"{package['id']}.nuspec", metadata)
        archive.writestr("[Content_Types].xml", '<?xml version="1.0"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="nuspec" ContentType="application/octet"/><Default Extension="h" ContentType="text/plain"/><Default Extension="png" ContentType="image/png"/><Default Extension="targets" ContentType="application/xml"/></Types>')
        archive.writestr("_rels/.rels", f'<?xml version="1.0"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Type="http://schemas.microsoft.com/packaging/2010/07/manifest" Target="/{package["id"]}.nuspec" Id="manifest"/></Relationships>')
        for header in headers:
            archive.write(header, "lib/native/include/" + header.relative_to(ROOT / "cpp/Platform.Data.Doublets").as_posix())
        archive.write(ROOT / "assets/icon.png", "images/icon.png")
        archive.write(ROOT / "LICENSE", "LICENSE")
        archive.writestr(f"build/native/{package['id']}.targets", '<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003"><ItemDefinitionGroup><ClCompile><AdditionalIncludeDirectories>$(MSBuildThisFileDirectory)..\\..\\lib\\native\\include;%(AdditionalIncludeDirectories)</AdditionalIncludeDirectories></ClCompile></ItemDefinitionGroup></Project>')
    with zipfile.ZipFile(output / f"platform.data.doublets_{package['version']}.zip", "w", compression=zipfile.ZIP_DEFLATED) as archive:
        for header in headers:
            archive.write(header, "Platform.Data.Doublets/" + header.relative_to(ROOT / "cpp/Platform.Data.Doublets").as_posix())
        archive.write(ROOT / "LICENSE", "Platform.Data.Doublets/LICENSE")
    print(f"Packed {len(headers)} headers into {target}")


def wait_for_package(package, attempts=46, interval=20):
    """NuGet indexing is asynchronous. Verify anonymously, for at most 15 minutes."""
    url = f"https://api.nuget.org/v3-flatcontainer/{package['id'].lower()}/{package['version']}/{package['id'].lower()}.{package['version']}.nupkg"
    for attempt in range(attempts):
        status, body = request(url)
        if status == 200 and body.startswith(b"PK"):
            print(f"Anonymous NuGet download verified for {package['id']} {package['version']}")
            return
        if status not in (0, 404, 429, 500, 502, 503, 504):
            raise RuntimeError(f"Anonymous NuGet download returned HTTP {status}; verification failed")
        if attempt + 1 < attempts:
            print(f"Waiting for public NuGet indexing ({attempt + 1}/{attempts}, HTTP {status})", flush=True)
            time.sleep(interval)
    raise RuntimeError("NuGet package was not publicly downloadable within 15 minutes; rerun publication to repair without changing the version")


def run_nuget_push(artifact, source, token):
    """Capture client diagnostics without printing argv or unbounded secret output."""
    command = ['dotnet', 'nuget', 'push', str(artifact), '--source', source,
               '--api-key', token, '--skip-duplicate']
    if source.startswith('https://nuget.pkg.github.com/'):
        command.append('--no-symbols')
    # File-backed output also bounds memory for a verbose registry/client failure.
    with tempfile.TemporaryFile() as log:
        result = subprocess.run(command, stdout=log, stderr=log, check=False)  # nosec B603 - Fixed dotnet command, argument list, no shell.
        log.seek(0, 2)
        log.seek(max(0, log.tell() - 16384))
        output = log.read().decode('utf-8', errors='replace')
    return result.returncode, output


def push_nuget(artifact, package, method, token):
    source = 'https://api.nuget.org/v3/index.json'
    code, output = run_nuget_push(artifact, source, token)
    if not code:
        print(f'NuGet publication auth={method}; {diagnostic(output, token)}')
        return
    # Only an actual HTTP authentication/authorization rejection permits retry.
    rejection = re.search(r'(?:HTTP\s+|status code(?: does not indicate success)?:\s*)(401|403)\b', output, re.I)
    category = 'authentication/authorization rejection' if rejection else 'publication/client/transport failure (no credential retry)'
    outcome = f'{method} push failed (exit {code}; {category}): {diagnostic(output, token)}'
    if method != 'trusted-publishing' or not rejection:
        raise RuntimeError(outcome)
    print(f'::warning::{outcome}; checking the exact version before token fallback')
    # A combined package/symbol push may have partially succeeded. Check the exact
    # public version before a duplicate-safe retry, without rebuilding or bumping.
    url = f"https://api.nuget.org/v3-flatcontainer/{package['id'].lower()}/{package['version']}/{package['id'].lower()}.{package['version']}.nupkg"
    status, body = request(url)
    if status == 200 and body.startswith(b'PK'):
        print('Exact NuGet version is already public; retry only to repair with --skip-duplicate')
    elif status == 404:
        print('Exact NuGet version is not public yet; retry uses the same validated artifact/version')
    else:
        raise RuntimeError(f'{outcome}; exact-version lookup indeterminate (HTTP {status}); no authentication retry')
    reason = f'trusted publishing failed; failed-step=publication; HTTP {rejection[1]}'
    try:
        _, fallback = fallback_auth(reason, [], package)
    except RuntimeError as error:
        raise RuntimeError(f'{outcome}; {error}') from None
    code, output = run_nuget_push(artifact, source, fallback)
    if code:
        raise RuntimeError(f'{outcome}; token-fallback push failed (exit {code}): {diagnostic(output, token, fallback)}')
    print(f'NuGet publication auth=token-fallback; {reason}; {diagnostic(output, token, fallback)}')


def publish(language, output):
    package = info(language)
    artifacts = sorted(output.glob(f"{package['id']}.{package['version']}.*nupkg"))
    if not any(path.suffix == ".nupkg" for path in artifacts):
        raise RuntimeError("The tested NuGet package is missing")
    tag = f"{language}_{package['version']}"
    repository = os.environ.get("GITHUB_REPOSITORY", "linksplatform/Data.Doublets")
    release_url = f"https://api.github.com/repos/{repository}/releases/tags/{tag}"
    status, _ = request(release_url, headers={"Authorization": "Bearer " + os.environ["GH_TOKEN"]})
    if status not in (200, 404):
        raise RuntimeError(f"Release lookup returned HTTP {status}; refusing to treat an unknown result as absent")
    failures = []
    method, token = select_nuget_auth([language])
    # Each destination can be repaired independently on a rerun. A failed mirror
    # must never delete, hide, or prevent repairing a successful public release.
    artifact = output / f"{package['id']}.{package['version']}.nupkg"
    nuget_verified = False
    try:
        push_nuget(artifact, {**package, 'language': language}, method, token)
        wait_for_package(package)
        nuget_verified = True
    except RuntimeError as error:
        failures.append(str(error))
    source = 'https://nuget.pkg.github.com/linksplatform/index.json'
    code, body = run_nuget_push(artifact, source, os.environ['GH_TOKEN'])
    if code:
        failures.append(f'Package push to {source} failed (exit {code}): {diagnostic(body)}')
    else:
        print('GitHub Packages mirror push succeeded')
    notes = output / "release-notes.md"
    notes.write_text(f"https://www.nuget.org/packages/{package['id']}/{package['version']}\n\n{package['notes']}\n", encoding="utf-8")
    if status == 404:
        target = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip()
        subprocess.run(["gh", "release", "create", tag, "--repo", repository, "--target", target, "--title", f"[{'C#' if language == 'csharp' else 'C++'}] {package['version']}", "--notes-file", str(notes), "--draft"], check=True)
    attachments = artifacts + sorted(output.glob("*.zip"))
    subprocess.run(["gh", "release", "upload", tag, "--repo", repository, "--clobber", *map(str, attachments)], check=True)
    if nuget_verified:
        subprocess.run(["gh", "release", "edit", tag, "--repo", repository, "--draft=false"], check=True)
    # Authentication is deliberately omitted to check the reader's actual view.
    status, body = request(release_url)
    try:
        public = json.loads(body)
        actual = {asset['name']: asset['size'] for asset in public['assets']}
        verified = status == 200 and not public['draft'] and all(actual.get(path.name) == path.stat().st_size for path in attachments)
    except (ValueError, KeyError, TypeError):
        verified = False
    if not verified:
        failures.append("Public GitHub release is missing or has incomplete assets; rerun publication to repair it")
    if failures:
        raise RuntimeError("; ".join(failures))
    print(f"Published and verified {tag} and all {len(attachments)} release assets")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("command", choices=["check-auth-config", "auth-error", "preflight", "pack-cpp", "publish"])
    parser.add_argument("--credentials-only", action="store_true")
    parser.add_argument("--mode", choices=["report", "release"], default="report")
    parser.add_argument("--language", choices=["csharp", "cpp"])
    parser.add_argument("--output", type=Path, default=ROOT / "artifacts/packages")
    args = parser.parse_args()
    try:
        if args.command == "check-auth-config":
            check_auth_config()
        elif args.command == "auth-error":
            raise RuntimeError("NuGet/login OIDC exchange failed: check username, workflow/environment match, inactive policy and package ownership/scope. " + HINT)
        elif args.command == "preflight":
            preflight(args.mode, [args.language] if args.language else ["csharp", "cpp"], args.credentials_only)
        elif args.command == "pack-cpp":
            pack_cpp(args.output)
        else:
            publish(args.language or "csharp", args.output)
    except (RuntimeError, subprocess.CalledProcessError) as error:
        # CalledProcessError includes secret argv; only show the exit code.
        print(f"::error::{error if isinstance(error, RuntimeError) else 'Release command failed (exit ' + str(error.returncode) + ')'}", file=sys.stderr)
        sys.exit(1)
