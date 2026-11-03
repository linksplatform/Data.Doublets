<!-- markdownlint-configure-file
{
  "MD043": {
    "headings": [
      "# DocFX source-map false positives",
      "*"
    ]
  }
}
-->

# DocFX source-map false positives

Main run 40883578849 published the tested documentation to gh-pages commit
`a558df87521024c50433f8300ec63659ed6f0880`. The next full-history audit in
[run 40886193744](https://github.com/linksplatform/Data.Doublets/actions/runs/40886193744)
reported five generic-api-key matches in line 6 of the generated DocFX asset
`csharp/public/chunk-HKAKSKF2.min.js.map`. Its log lines 408–475 preserve the
redacted findings and exact fingerprints. The dependency audit passed.

All five matches are JavaScript identifiers in the source map's `names` array:

- `isKeyable_default2`
- `keys_default2`
- `getAllKeys_default2`
- `isKey_default2`
- `toKey_default2`

They are public source identifiers, with no credential data. The deployed map
matches the tested main documentation artifact. All 1,050 deployed documentation
files match that artifact's Git blob hashes; the served API indexes and PDF also
match byte for byte. No credential was exposed or rotated.

Reproduction, using the same pinned Gitleaks 8.30.1 as CI:

```sh
git fetch origin gh-pages
docs_commit=a558df87521024c50433f8300ec63659ed6f0880
gitleaks git --redact --no-banner \
  --log-opts="$docs_commit^..$docs_commit"
```

Before the correction, this exits 1 with five findings sharing one fingerprint.
After the correction, this exits 0, and the full-history scan must also pass.
Local verification scanned 3,303 commits (377.71 MB) with no leaks found.
The ignore entry is restricted to this exact commit, source-map path, rule and
line. No path, source-map type, rule or branch is excluded from scanning. Future
changes and credentials remain subject to the same scanner.
