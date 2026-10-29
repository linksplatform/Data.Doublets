"""Validate release credentials, package C++ headers, and publish tested artifacts.

No command prints credential values. CI_VERBOSE adds safe operation summaries.
"""

import argparse
import json
import os
from pathlib import Path
import subprocess
import sys
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
HINT = "Rotate the NUGET_TOKEN repository secret with a valid NuGet push key scoped to both Platform.Data.Doublets packages, or configure NuGet trusted publishing."


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
        return error.code, b""
    except (OSError, urllib.error.URLError):
        return 0, b""


def verify_nuget(key, package):
    package_id = urllib.parse.quote(package["id"], safe="")
    status, body = request(f"https://www.nuget.org/api/v2/package/create-verification-key/{package_id}", method="POST", headers={"X-NuGet-ApiKey": key})
    if status != 200:
        return f"NuGet API key verification for {package['id']} returned HTTP {status or 'unreachable'}. {HINT}"
    try:
        verification_key = json.loads(body)["Key"]
    except (ValueError, KeyError, TypeError):
        return "NuGet did not return a verification key. " + HINT
    if not isinstance(verification_key, str) or not verification_key:
        return "NuGet returned an empty verification key. " + HINT
    # Verify the actual owner/glob against an existing version, not just key validity.
    status, body = request(f"https://api.nuget.org/v3-flatcontainer/{package['id'].lower()}/index.json")
    if status == 404:
        return None  # First publication; push enforces scope.
    if status != 200:
        return f"Cannot verify NuGet package scope: package index returned HTTP {status or 'unreachable'}."
    try:
        versions = json.loads(body)["versions"]
        if not isinstance(versions, list) or not versions:
            raise ValueError("No versions")
    except (ValueError, KeyError, TypeError):
        return "NuGet package index is malformed; refusing to assume publish permissions."
    status, _ = request(f"https://www.nuget.org/api/v2/verifykey/{package_id}/{versions[-1]}", headers={"X-NuGet-ApiKey": verification_key})
    return None if status == 200 else f"NuGet owner/glob verification returned HTTP {status or 'unreachable'}. {HINT}"


def preflight(mode, languages, credentials_only=False):
    errors = []
    token = os.environ.get("NUGET_TOKEN", "")
    if not token:
        errors.append("NUGET_TOKEN is missing. " + HINT)
    elif mode == "release":
        for language in languages:
            error = verify_nuget(token, info(language))
            if error:
                errors.append(error)
    github_token = os.environ.get("GH_TOKEN", "")
    if credentials_only:
        github_token = "preflight-read-only"
    if not github_token:
        errors.append("GH_TOKEN is missing; release jobs need contents: write and packages: write.")
    elif mode == "release" and not credentials_only:
        repository = os.environ.get("GITHUB_REPOSITORY", "linksplatform/Data.Doublets")
        status, body = request(f"https://api.github.com/repos/{repository}", headers={"Authorization": f"Bearer {github_token}", "Accept": "application/vnd.github+json"})
        try:
            can_push = status == 200 and json.loads(body).get("permissions", {}).get("push")
        except (ValueError, TypeError, AttributeError):
            can_push = False
        if not can_push:
            errors.append("Workflow token cannot write releases; grant contents: write and packages: write on the release job.")
    for error in errors:
        print(f"::{'error' if mode == 'release' else 'notice'}::{error}")
    if errors and mode == "release":
        raise RuntimeError("Release preflight failed; no packages or tags were published.")
    print("Release preflight passed" if not errors else "PR validation continues without publishing credentials")


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
    # Each destination can be repaired independently on a rerun. A failed mirror
    # must never delete, hide, or prevent repairing a successful public release.
    for source, token in [("https://api.nuget.org/v3/index.json", os.environ["NUGET_TOKEN"]), ("https://nuget.pkg.github.com/linksplatform/index.json", os.environ["GH_TOKEN"])]:
        command = ["dotnet", "nuget", "push", str(output / f"{package['id']}.{package['version']}.nupkg"), "--source", source, "--api-key", token, "--skip-duplicate"]
        if source.startswith("https://nuget.pkg.github.com/"):
            command.append("--no-symbols")  # GitHub Packages has no symbols server.
        result = subprocess.run(command, check=False)
        if result.returncode:
            failures.append(f"Package push to {source} failed (exit {result.returncode})")
        elif source.startswith("https://api.nuget.org/"):
            try:
                wait_for_package(package)
            except RuntimeError as error:
                failures.append(str(error))
    notes = output / "release-notes.md"
    notes.write_text(f"https://www.nuget.org/packages/{package['id']}/{package['version']}\n\n{package['notes']}\n", encoding="utf-8")
    if status == 404:
        target = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip()
        subprocess.run(["gh", "release", "create", tag, "--repo", repository, "--target", target, "--title", f"[{'C#' if language == 'csharp' else 'C++'}] {package['version']}", "--notes-file", str(notes), "--draft"], check=True)
    attachments = artifacts + sorted(output.glob("*.zip"))
    subprocess.run(["gh", "release", "upload", tag, "--repo", repository, "--clobber", *map(str, attachments)], check=True)
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
    parser.add_argument("command", choices=["preflight", "pack-cpp", "publish"])
    parser.add_argument("--credentials-only", action="store_true")
    parser.add_argument("--mode", choices=["report", "release"], default="report")
    parser.add_argument("--language", choices=["csharp", "cpp"], default="csharp")
    parser.add_argument("--output", type=Path, default=ROOT / "artifacts/packages")
    args = parser.parse_args()
    try:
        if args.command == "preflight":
            preflight(args.mode, ["csharp", "cpp"] if args.credentials_only else [args.language], args.credentials_only)
        elif args.command == "pack-cpp":
            pack_cpp(args.output)
        else:
            publish(args.language, args.output)
    except (RuntimeError, subprocess.CalledProcessError) as error:
        # CalledProcessError includes secret argv; only show the exit code.
        print(f"::error::{error if isinstance(error, RuntimeError) else 'Release command failed (exit ' + str(error.returncode) + ')'}", file=sys.stderr)
        sys.exit(1)
