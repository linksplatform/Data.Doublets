"""One-time restructuring of the issue 524 workflow into reader/writer jobs."""
from pathlib import Path
p=Path('.github/workflows/ci.yml')
s=p.read_text()
start=s.index('  publish-csharp:')
end=s.index('  required:',start)
s=s[:start]+'''  publish:
    if: >-
      !cancelled() && github.ref == 'refs/heads/main' &&
      needs.quality.result == 'success' &&
      !contains(needs.*.result, 'failure') && !contains(needs.*.result, 'cancelled') &&
      (needs.csharp.result == 'success' || needs.cpp.result == 'success' || needs.docs.result == 'success')
    needs: [credentials, quality, csharp, cpp, docs]
    runs-on: ubuntu-24.04
    timeout-minutes: 40
    permissions:
      contents: write
      packages: write
    concurrency:
      group: main-writer-${{ github.repository }}-main
      cancel-in-progress: false
      queue: max
    steps:
      - uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7
        with:
          fetch-depth: 0
          persist-credentials: false
      - uses: actions/setup-dotnet@26b0ec14cb23fa6904739307f278c14f94c95bf1 # v5
        with:
          global-json-file: global.json
      - name: Prepare and commit only validated release metadata
        if: needs.csharp.result == 'success' || needs.cpp.result == 'success'
        env:
          GH_TOKEN: ${{ github.token }}
        run: |
          gh auth setup-git
          python3 scripts/versions.py prepare
          python3 scripts/versions.py commit
      - uses: actions/download-artifact@9000827ccba6bdab643e8b6fd33ac0654aef8333 # v8
        if: needs.csharp.result == 'success'
        with:
          name: csharp-ubuntu-24.04
          path: artifacts
      - uses: actions/download-artifact@9000827ccba6bdab643e8b6fd33ac0654aef8333 # v8
        if: needs.cpp.result == 'success'
        with:
          name: cpp-gcc
          path: .
      - name: Publish validated C# artifacts
        if: needs.csharp.result == 'success'
        env:
          NUGET_TOKEN: ${{ secrets.NUGET_TOKEN }}
          GH_TOKEN: ${{ github.token }}
        run: |
          python3 scripts/release.py preflight --mode release --language csharp
          python3 scripts/release.py publish --language csharp
      - name: Publish validated C++ artifacts
        if: ${{ !cancelled() && needs.cpp.result == 'success' }}
        env:
          NUGET_TOKEN: ${{ secrets.NUGET_TOKEN }}
          GH_TOKEN: ${{ github.token }}
        run: |
          python3 scripts/release.py preflight --mode release --language cpp
          python3 scripts/release.py publish --language cpp
      - uses: actions/download-artifact@9000827ccba6bdab643e8b6fd33ac0654aef8333 # v8
        if: ${{ !cancelled() && needs.docs.result == 'success' }}
        with:
          name: documentation
          path: _site
      - name: Publish documentation and source PDF
        if: ${{ !cancelled() && needs.docs.result == 'success' }}
        env:
          GH_TOKEN: ${{ github.token }}
        run: bash scripts/publish-docs.sh
'''+s[end:]
s=s.replace("    if: github.ref == 'refs/heads/main' && github.event_name != 'pull_request'", "    if: >-\n      github.ref == 'refs/heads/main' && github.event_name != 'pull_request' &&\n      (needs.changes.outputs.csharp == 'true' || needs.changes.outputs.cpp == 'true' || needs.changes.outputs.pipeline == 'true')")
# A skipped credential probe on PRs/docs-only changes is expected; failures block builds.
for job in ('csharp','cpp','docs'):
 a=s.index('  '+job+':\n');b=s.find('\n  ',a+3)
 # Find the next top-level job, rather than a nested key.
 import re
 match=re.search(r'\n  [a-z][a-z-]*:\n',s[a+3:]);b=a+3+match.start() if match else len(s)
 part=s[a:b]
 part=part.replace('needs: [changes, quality]','needs: [changes, quality, credentials]')
 part=part.replace('    if: ', "    if: >-\n      !cancelled() && needs.quality.result == 'success' &&\n      (needs.credentials.result == 'success' || needs.credentials.result == 'skipped') &&\n      (")
 # Close the changed expression before its next key.
 part=part.replace(" == 'true'\n", " == 'true')\n",1)
 part=part.replace('          persist-credentials: false','          fetch-depth: 0\n          persist-credentials: false',1)
 marker='      - uses: actions/setup-'
 position=part.index(marker)
 part=part[:position]+'      - name: Prepare current merge and automatic release versions\n        run: bash scripts/prepare-checkout.sh\n'+part[position:]
 if job=='cpp':
  pos=part.index('      - uses: actions/setup-python')
  part=part[:pos]+'''      - uses: actions/setup-dotnet@26b0ec14cb23fa6904739307f278c14f94c95bf1 # v5
        with:
          global-json-file: global.json
'''+part[pos:]
  pos=part.index('      - name: Package C++')
  part=part[:pos]+'''      - name: Static analysis with the native compilation database
        if: matrix.compiler == 'clang'
        run: |
          sudo apt-get update
          sudo apt-get install -y clang-tidy-18 cppcheck
          python3 scripts/check-cpp.py --build-dir cpp/build
      - name: Compile C and C++ FFI declarations
        run: |
          printf '#include "c/ffi.h"\\nint main(void) { return 0; }\\n' | cc -x c -std=c11 -Werror -Wall -Wextra -I . -fsyntax-only -
          printf '#include "c/ffi.h"\\nint main() { return 0; }\\n' | c++ -x c++ -std=c++20 -Werror -Wall -Wextra -I . -fsyntax-only -
'''+part[pos:]
 s=s[:a]+part+s[b:]
s=s.replace('    needs: [quality, csharp, cpp, docs]', '    needs: [changes, credentials, quality, csharp, cpp, docs, publish]')
s=s.replace('    if: always()\n    needs:', '    if: ${{ !cancelled() }}\n    needs:')
s=s.replace("          assert jobs['quality']['result'] == 'success', 'Quality validation is required'", "          assert jobs['quality']['result'] == 'success', 'Quality validation is required'\n          for name, job in jobs.items():\n              if job['result'] == 'skipped':\n                  print(f'::notice::{name} skipped: event/change prerequisites did not require this job')")
s=s.replace('      - name: Validate repository configuration', '''      - name: Check native formatting
        run: python3 scripts/check-cpp.py
      - name: Validate changesets and prohibit manual version edits
        if: github.event_name == 'pull_request'
        run: python3 scripts/versions.py validate
      - name: Validate repository configuration''')
p.write_text(s)
