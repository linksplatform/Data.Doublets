# Issue 524 / PR 526 investigation

Repository: https://github.com/linksplatform/Data.Doublets
Issue: https://github.com/linksplatform/Data.Doublets/issues/524
Pull request: https://github.com/linksplatform/Data.Doublets/pull/526

This report distinguishes observed failures from inferred causes. Raw GitHub
metadata, downloaded logs, upstream source snapshots, local reproductions and
verification output are retained beside it. No secret value is included.
The implementation is verified by fully green [run 40842727244](https://github.com/linksplatform/Data.Doublets/actions/runs/40842727244)
at `5cf40c5a338a38978221aab65a8e017005c95303` (08:31:20–08:39:37 UTC).
Both C# platforms pass **82 tests**, each of three native configurations passes
**46 tests**, and all **44 pipeline regressions** execute without skips.
All three complete CodeQL reports contain **zero findings**, all analysis
invocations succeed, and the complete compiler/runtime/action-warning audit is
empty. Documentation contains **538 HTML pages and 277 PDF pages**.
`github/seventh-verified.json` is the machine-checked evidence summary;
`ci-artifacts/seventh.tar.gz` and its SHA256/size manifest retain all **1,097**
artifact files. The final section supersedes pending statements in earlier,
chronological investigation entries. Live registry publication remains an
owner-controlled operation; the original unusable NuGet credential is explicitly
diagnosed before main builds, while PR validation requires no publishing secret.

## Requirements and implementation inventory

The complete issue body is in `github/issue.json`. Initial issue comments,
required sub-issues, PR conversation comments, inline review comments and reviews
were empty (`github/*comments.json`, `sub-issues.json`, `pr-reviews.json`).
Related issue 522 and merged PR 523 were also read; main subsequently merged
PR 525, which is included without discarding its query-handling changes.

| ID | Requirement, including the execution instructions | Implementation / verification evidence |
| --- | --- | --- |
| R1 | Investigate every false positive, false negative, warning and error in the cited main run | Complete 3,272-line baseline log, extracted diagnostic families, reproducing tests; root causes below |
| R2 | Compare the full C# template file tree, including every workflow and helper | All 81 files at `4cc372ebf2075a0c948b46881143f7d66f0f0e7e` archived and individually mapped in `research/TEMPLATE-COMPARISON.md` |
| R3 | Compare the full C/C++ template file tree | All 216 files at `7ac75134c697e9468f1242c20f1d229594decbc3` archived and individually mapped; same report, SHA256/size manifest |
| R4 | Apply all relevant best practices from the linked guide | Seventeen-principle mapping below; executable checks and regressions, not merely recommendations |
| R5 | Report shared defects to the templates and related projects | Thirteen reproducible upstream reports, complete bodies/URLs in `upstream/`; Conan issue creation is disabled and the workaround is implemented locally |
| R6 | Download logs/data into the requested folder; reconstruct the timeline and diagnose actual causes | This folder contains raw API responses, complete workflow logs, source archives, online primary sources and bounded local experiment logs |
| R7 | Propose solutions and plans per requirement; research existing components | Root-cause table, completed implementation sequence, component evaluation below; keep existing .NET/GTest/Conan/DocFX frameworks |
| R8 | Reproduce bugs before changing production code | Baseline failures in `local/*before.log`, helper tests under `experiments/`, language regression tests and C++ compilation reproductions |
| R9 | Apply matching fixes across the entire maintained codebase | All C# projects share policy; native source/test/benchmark/profile paths are formatted and checked; both READMEs, all publishing/docs entry points and both memory storage families reviewed |
| R10 | Preserve existing features and publishable packages; prepare next releases | Existing NuGet IDs and `csharp_`/`cpp_` release tags retained, both language APIs and source PDF retained, native ZIP layout preserved, independent automatic changeset for both packages |
| R11 | Add verbose diagnostics when more evidence is needed, default off | `CI_VERBOSE=1` enables shared native/release operation tracing; compiler/static-analysis/test outputs and job artifacts retain failures; no token tracing |
| R12 | Complete everything in this single PR; preserve history and the prepared branch | Incremental forward commits only on `issue-524-b1512fd1c7a8`; merge latest main, no direct merge, all required work tracked in `PLAN.md` |
| R13 | Run local checks and fresh CI, inspect actual errors, fix feedback, leave a clean tree | Repeated runs are tied to exact SHAs/timestamps in `github/runs-*.json`; green run 40842727244 plus raw-report verification in github/seventh-verified.json |
| R14 | Update existing PR title/body, retain positive closing references, mark ready at completion | PR 526 is the only implementation PR; body retains `Fixes #524`; no required child issues existed at inventory time |

## Timeline (UTC)

1. **2026-10-28 20:54:33** — main run `40767419162` starts for
   `acf599361d99e7d462524db30cc5982a0eb16d37` (merge of PR 523).
2. **20:54:44–20:55:29** — test job reports success despite a large compiler/XML/
   nullable warning stream. The legacy pipeline does not make warnings fatal.
3. **20:55:32** — GitHub package and release jobs start independently of public
   NuGet publication. They subsequently report success.
4. **20:55:58** — public NuGet rejects the push with HTTP 403: "API key is
   invalid, has expired, or does not have permission" (baseline log line 2211).
   The response cannot distinguish expiry, revocation, ownership or key scope.
5. **20:55:57** — documentation setup fails with `Package 'nuget' has no
   installation candidate` on Ubuntu 24.04 (line 2995).
6. **20:56:22** — PDF setup fails with `Package 'python-pygments' has no
   installation candidate` (line 3249). This is Python 2-era tooling.
7. **2026-10-29 04:28:57** — issue 524 opened; **04:29:57**, prepared draft PR
   526 opened. Exact source snapshots and original run metadata collected.
8. Local reproductions establish masked native build failures, disabled tests,
   unsafe temporary names, C# update/contract failures, C/C++ FFI declaration
   errors, outdated dependency metadata and template retry defects.
9. **05:28:35** — main advances through PR 525. The first implementation push
   cannot run PR CI because the merge result conflicts. Main is merged, the
   release-note conflict resolved, and all added query behavior retained.
10. **05:43:25** — first complete new CI runs at `1d1c66df...`: C# Linux, GCC,
    documentation, quality, all CodeQL languages and security audit pass.
    Clang exposes additional unused/invalid helper code and self-typed constexpr
    problems; Windows exposes CRLF versus formatter LF; live links expose stale
    badges and old API paths. The required gate correctly fails.
11. Second local iteration reproduces native helper instantiation failures,
    missing first-deployment branch and a security/link gate that could skip.
    Fixes add real regressions, portable constexpr storage, LF checkout policy,
    documentation artifact URL mapping, and security/link release dependencies.
12. The expanded native build exceeds a **1.6 GB virtual-memory probe cap**.
    An old 37-test binary output after that failed build is explicitly **not**
    evidence for the new tests (`native-build-second-ci.log`). The fail-fast
    rerun uses `-O0 -g0`, one worker and a 1.9 GB virtual-memory cap: it builds
    successfully at **1,565,780 KiB peak child RSS** and runs all **40** tests.

13. **06:09** — second CI at `b7ab81c1...` passes both C# platforms, GCC,
    Clang analysis, docs, links and security execution, but ASan correctly fails
    on upstream malloc/delete mismatch and owned helper leaks. Full CodeQL SARIF
    is also downloaded: **67 C# + 20 C++ findings**, despite all analysis jobs
    being green. Only 32 appeared in the PR alert API; diff-only alerts are not a
    complete inventory (`github/codeql-*-before.sarif`).
14. The third iteration reproduces C# allocation/free-list errors and constructor
    virtual dispatch with focused failing tests. It adds a fatal full-SARIF gate,
    resolves every recorded diagnostic, and uses the real native build for CodeQL
    so benchmark registration macros are modeled accurately.
15. ASan's 4096-byte upstream heap reproduction fails before the destructor
    patch and passes after (`local/heap-sanitizer-{before,after}.log`). Ownership
    tests cover helper replacement on growth and cleanup on destruction in both
    stores. Small reservations also expose upstream shrinking underflow; both
    split stores now preserve an already sufficient reservation.
16. The expanded GCC build reaches **1,809,328 KiB RSS** and fails its 1.9 GB
    virtual-memory cap without producing a new binary. Rather than retry it,
    the same translation units are built with Clang, `-O0 -g0`, one worker:
    **950,672 KiB peak RSS**, successful build (`native-clang-build-iteration3.log`).
17. A real, exclusively locked transaction log reproduces silent success from
    explicit disposal. The regression fails before the catch filter change and
    requires an IOException afterward (`transaction-disposal-before.log`).

18. A standalone umbrella-header consumer exposes an additional template
    lookup failure masked by the aggregate test includes. After correcting that,
    a finite five-point ASan reproduction reports `count=3, exists(4)=1`:
    deletion left an indexed ghost. Resetting each temporary point before delete
    makes both gaps absent. The installed native NuGet package is now actually
    compiled and executed using `examples/CppMemoryLinks.cpp`, for both stores.
19. The united ownership test also exposes C++ reference assignment after
    reallocation: `Value [1600] ... out of range [0..1536]`. A reference cannot be
    rebound with assignment in C++; use a pointer and reacquire after relocation.
    The before trace is `local/native-tests-iteration3.log`.

## Observed problems, root causes and solutions

| Problem / baseline evidence | Actual cause or uncertainty | Solution and verification |
| --- | --- | --- |
| NuGet HTTP 403, baseline line 2211 | Credential invalid/expired or insufficient package scope; exact account state is not observable from the response | Before building main, create a NuGet verification key and probe package ownership/glob. Missing/unknown/invalid credentials fail main with rotation instructions. PRs never require secrets. Mocked invalid/unknown API regressions pass. An owner must supply the valid external secret; no credential repair or public publication is claimed from PR tests |
| Release/package jobs green while public publication fails | Independent legacy jobs report partial success; no aggregate gate | One writer depends on all applicable builds/docs/security/links. Required gate fails cancelled, failed or unexpectedly skipped jobs. Registries and GitHub release assets are independently repairable; anonymous verification confirms public visibility |
| Deprecated apt NuGet/Mono documentation, baseline 2995 | Downloaded mutable shared script assumes unavailable Ubuntu packages | Pinned SDK-local DocFX; build C# and Doxygen native API into one tested artifact; deploy generated artifact, not a rebuild from a moving branch |
| PDF dependency unavailable, baseline 3249 | Python 2 package name and legacy HTML rendering stack | Pinned fpdf2 and DejaVu fonts, Unicode PDF generation. Validate `%PDF-` and generated destination. Full source PDF builds locally and in CI |
| `CS1584`, `CS1658` (also syntax detail `CS0081`), `CS1574`, `CS1573`, `CS1572`, `CS1591`, `CS1711` | Invalid generic cref syntax, stale type/parameter names and missing public docs across source tree | Repair XML and public contracts throughout the library; DocFX and compiler warnings fail validation. `ci-logs/warning-families.json` records first original line per family |
| `CS8765`, `CS8622`, `CS8618`, `CS8603/4/2/0/1`, `CS8625`, `CS8631`, `CS8620` | Reference-capable generic arithmetic, optional handlers modeled as non-null, nullable equality contracts, fields initialized outside visible constructor flow | Value-type numeric constraints where storage requires addresses; optional callback contracts, valid initialized state and actual null handling. Strict build applies to library, tests, benchmarks and example; no global warning blanket |
| NuGet `NU5048`; SourceLink cannot find Git worktree metadata | Deprecated icon URL and old explicit SourceLink tasks | Embedded package icon; patched SDK-bundled SourceLink with Git repository checkout. New SourceLink package audit finding is removed rather than suppressed |
| Skipped combined-decorator test; UniLinks Update creates another link | Wrong test assertion and hidden logic bug previously unexercised | Correct assertion, execute the test, update requested link in place; callback regressions cover creation/deletion branches. All tests execute without skips |
| Shell native runner can exit green after a failed build | No fail-fast mode; stale binaries and inconsistent cwd/flags | Shared `set -euo pipefail` runner, script-relative paths, mandatory CTest discovery (`--no-tests=error`) and tests+benchmarks enabled. Mock build exit 42 reproduces old false green |
| Missing native CI tests | Reused shared workflow compiled but never executed its test binary; former native workflows inactive | GCC, Clang and ASan/UBSan matrix executes discovered tests, bounded benchmark smoke, C11/C++20 FFI syntax, packaging and consumer checks |
| Conan setup/build compatibility failures | Pinned old recipes omit Conan 2 header-library metadata; Platform.Data forms constexpr through an unknown reference | Bounded recipe patches at a pinned Conan Center snapshot; standard type-based constants. Report to Data; Conan upstream issues disabled, retain proposed report and implemented local remedy |
| C++ predicate precedence | `!source == any` tests negation instead of inequality in count selection | `source != any`, query-count regression fails before fix; fix analogous legacy predicate too |
| C++ `Link::Null` rejected by Clang | In-class object has incomplete enclosing type; out-of-class const declaration alone is not usable in Clang constant expressions | Delayed namespace variable-template value and constexpr reference; GCC and Clang strict compilation plus constant-expression regression. Value/copy semantics retained; `decltype(Link<T>::Null)` is now a const reference |
| Clang unused warnings reveal creation/deletion helper defects | EnsureCreated iterates an empty set, calls nonexistent members, uses invalid vector API and never advances allocation; DeleteByQuery discards matches | Iterate requested internal addresses, allocate until all missing requests exist, retain intermediates until allocation ends, remove only unrequested new IDs; snapshot query matches before deletion. Three real-storage regression tests fail to compile before and pass after |
| Extra comparison parentheses and empty test loops | Ported source hides Clang diagnostics and leaves random-test observations unused | Fix all matching native comparisons; assert every enumerated address exists and created/deleted totals agree; keep ignored callback parameters unnamed |
| `tmpnam` warning/race across native tests, benchmarks and profile | Check-then-open temporary names, no ownership boundary | Atomic private temporary directory with RAII cleanup; replace all uses. Distinct-ownership/cleanup regression and benchmark smoke |
| Invalid public C/C++ FFI declarations | Enum/forward declaration types and absent C++ linkage boundary | Opaque record plus language guards; independent C11 and C++20 compiler reproductions and CI checks |
| Cppcheck CRTP customization warnings | Base no-op hooks intentionally selected/overridden by static dispatch; analyzer misreads this established API | Exact inline suppressions only at documented customization points, with no global diagnostic exemption; use googletest library model for test macros. Full findings retained |
| actionlint rejects `queue: max` | Installed linter schema lags a documented GitHub Actions feature | Ignore only exact unknown-key diagnostic and independently validate allowed queue values and cancellation incompatibility; all other workflow/shell errors remain fatal |
| Windows emits thousands of ENDOFLINE diagnostics | Git checkout autocrlf converts LF sources to CRLF, while `.editorconfig` requires LF | `.gitattributes` makes maintained text LF on every platform; raw evidence explicitly `-text` to preserve original bytes |
| Broken Russian badges and documentation URLs | Old workflow/API layout and external API reference no longer exist | Correct both READMEs and link directly to maintained ILinks source. Validate own deployment URLs against generated artifacts to permit pages before first deployment and detect removed pages even if still live |
| Template singleton-link retry crashes | Retry delay variable assigned only for another path/cardinality | Bound and initialize delay for every request; regression tests for timeout, HTTP failure and policy refusal; upstream report with reproducible cases |
| Missing gh-pages branch; publication race | Clone assumes deployment already exists; queued writers may still have stale checkouts | Initialize absent branch, preserve other languages, reapply only generated outputs after concurrent updates, stop on access/rule refusal, three-attempt limit. Real bare-Git tests cover first publish, idempotence, preserved Rust docs and a competing publisher |
| Duplicate queued version writer | Same validated metadata landed remotely but second writer treats non-fast-forward as unrelated change | Fetch and compare trees, adopt identical confirmed remote release without a second bump; real two-writer bare-Git regression |
| Security/link checks independent of release | Successful build could publish before these separate workflows finish | Reusable workflows remain independently scheduled/manual, but CI invokes them as explicit required dependencies; skipped-check regression and full release `needs` graph |
| CodeQL jobs green with 87 findings | Analysis/upload success is not a findings gate; PR-visible alerts omit findings outside changed lines | Parse every complete SARIF report and reject every result, including notes, plus failed analysis notifications. Five regression tests verify missing/incomplete reports and every severity. Keep complete reports as CI artifacts |
| Native benchmark unused-static false positives | The real build confirms the queries also ignore standard `maybe_unused` and scalar registration initializer side effects; no-build extraction alone was not the complete explanation | Use supported registration API with retained handles and explicit loop-token consumption; all six names/arguments preserved. Report query/model defects upstream. Included definitions use .h rather than .cpp |
| C# EnsureCreated loops or misses lower free IDs | Creator result never advances; stopping at the maximum requested ID ignores nonmonotonic free-list allocation | Track every missing internal request, allocate unique IDs until all are satisfied, then reset/delete only unrequested intermediates. Both stores cover free-list order, self-point gaps, external/sentinel IDs and a non-progressing creator |
| C# virtual calls during storage construction | Base Constants and derived Init/SetPointers dispatch before the most-derived fields are initialized | Assign backing constants directly and use concrete nonvirtual construction routines; preserve virtual APIs for later calls. Two derived-probe tests fail before and pass after |
| Native ASan allocation/deallocation mismatch | Upstream Platform.Memory allocates with malloc/realloc but destroys with delete | Exact pinned-source recipe patch uses std::free; standalone ASan reproduction before/after, upstream Memory #92 |
| Native growth uses a stale header reference | C++ assignment after realloc copies through a potentially freed reference instead of rebinding it | Reacquire the header through a pointer after relocation; the 128-allocation small-step united ownership test reproduces the capacity error before and verifies every growth afterward |
| Native helper leaks and duplicate allocation | Raw new pointers are overwritten on growth and never destroyed; split target helper allocated twice | Unique ownership for every helper, replacement automatically releases old instance; remove duplicate allocation and initialize linked-list helper. Two finite 128-allocation ownership regressions plus CI ASan |
| Split memory initialization fails at small reservation steps | Equality check shrinks a larger caller reservation; upstream C#/C++ zero-fill computes an invalid negative/unsigned length | Grow only when existing capacity is insufficient in both implementations; small-step tests and upstream Memory #93 with source-level fix suggestion |
| Exceptions hidden without diagnostics | Intentional NoExceptionsDecorator has no observer/filter; explicit transaction disposal also swallows failed commit persistence | Optional exception filter and callback default off; default conversion behavior retained. Explicit disposal propagates persistence failure while finalization remains best effort; real I/O-failure regression |
| Native package consumer and EnsurePointsCreated | Header-existence-only smoke never instantiates shipped templates; umbrella-only compilation reveals two-phase lookup hidden by test helpers, and self-point cleanup leaves indexed ghosts | Compile and run a standalone consumer from installed NuGet headers with real Conan dependency flags on every compiler. Use the visible base Update overload; reset intermediate values before deletion. Finite ASan reproduction changes exists(4) from true to false and the public example covers both stores |
| Split storage runs out of index space while data space remains | Both languages calculate link capacity from data bytes alone; initialization also multiplies index count by the data record size | Reserve full layouts and use the minimum of both usable record counts. Three C# equal/unequal-buffer cases and a native 128-allocation regression fail before and pass after; full native suite now has 43 tests |
| Remaining CodeQL quality findings | Malformed summury tags/missing summaries, dead assignments, mutable private fields and needlessly complex conditionals | Repair all recorded locations throughout C# and native sources; preserve active behavior and registration. Full new SARIF must contain zero results |
| Historical credential detection | Removed old key remains in immutable Git history | Verify revoked via harmless authentication probe (HTTP 401), suppress only exact historical commit/rule/line fingerprint; keep full-history scan and fail new credentials. No history rewrite or broad exclusion |

## Best-practice mapping (all 17 principles)

| Principle from archived guide | Applied implementation / scope decision |
| --- | --- |
| P1 Relevant changes | Event's actual base, deleted paths and configuration routing in `scripts/changes.py`; evidence/experiments excluded from product release routing |
| P2 File limits | AST/config parse plus 1,500-line source limit in `check_repository.py`; evidence exempt; source stays below limit |
| P3 Formatting | .NET formatter on all solution projects, clang-format 18 on maintained C/C++ files, LF policy |
| P4 Static analysis | .NET warnings as errors, compilation-database clang-tidy, Cppcheck, CodeQL actions/C#/C++ |
| P5 Fast before slow | Quality and workflow checks precede all builds; security audit and tests can then run in parallel |
| P6 Changesets | Independent language-aware fragments, highest semantic bump, generated changelog, reject manual version edits; docs-only needs no release |
| P7 Actual merge | Fetch and merge current main for PR validation; apply identical version preparation before tests/package/docs |
| P8 Pre-commit | Local formatting, workflow/config checks, pipeline tests and full-history secret hook |
| P9 Release automation | Automatic main and manual dispatch, tested artifacts, guarded metadata-only bot commit, bounded registry indexing, complete anonymous public verification and rerun repair |
| P10 Concurrency | Job-level superseded PR cancellation, one noncancellable repository writer with `queue: max`, classified failures and safe bounded race recovery. Main readers finish because each validated writer must retain its source/artifacts |
| P11 Secrets | Pinned/checksummed full-history Gitleaks, least permissions, no persisted checkout credentials, trusted Dependabot metadata only |
| P12 Documentation | Both APIs, Unicode PDF, local artifact links and live maintained Markdown links; archive suggestions never turn a broken link green |
| P13 Container architecture | No container image is distributed by this library, so multiarchitecture image publishing is not applicable. No QEMU or new image-release channel introduced |
| P14 Workflow lint | Pinned official actionlint container supplies binary; ShellCheck confirmed available; zizmor confidence floor only and narrowly explained suppression |
| P15 Dependency audit | Locked transitive NuGet audit on PR/main and weekly schedule; Dependabot for NuGet/actions/Python/submodule with cooldown |
| P16 Preflight | Main verifies NuGet key/scope before expensive work; PRs report secret requirements without requiring secrets; unknown API results never pass |
| P17 Changed world / evidence | Known policy rejections are not retried; missing branch is initialized; job skips explained; required empty artifacts fail; publication and security failures stay visible |

## Existing components and online research

The current templates were read in full, including fixtures, case studies,
consumer tests and workflow helpers. `research/template-file-manifest.tsv` records
all 297 file hashes, sizes, revisions, adaptation targets and reasons. Template
Calculator/bootstrap/demo content is replaced by real Doublets code/tests;
template history is evidence, not production code. New Conan/vcpkg distribution
registries and container images are not existing project products; the existing
NuGet and native header ZIP channels are maintained and tested instead.

Selected components already solve most infrastructure needs:

- .NET SDK analyzers, `dotnet format`, xUnit/coverlet and locked NuGet restore:
  one supported toolchain for build, test, coverage, package and audit.
- CMake/CTest/GTest, Google Benchmark and Conan 2: real dependency-aware builds,
  discovered tests, sanitizer/compiler matrix and bounded performance smoke.
- clang-format, clang-tidy, Cppcheck, actionlint/ShellCheck and zizmor: use actual
  compiler databases and workflow semantics instead of homegrown parsers for
  language/shell analysis. See [official clang-tidy documentation](https://clang.llvm.org/extra/clang-tidy/index.html).
- gcov/gcovr: standard native coverage via compiler instrumentation and execution,
  not inferred test success. See [GCC's coverage invocation](https://gcc.gnu.org/onlinedocs/gcc/Invoking-Gcov.html).
- DocFX, Doxygen and fpdf2: SDK-native managed docs, native API docs and Unicode
  source PDF; remove obsolete Mono/NuGet/Python 2 document dependencies.
- Lychee plus bounded retry/archive helpers: preserve actual failures, provide
  diagnostics and possible replacements. Do not accept archive existence as a
  passing live URL.
- GitHub job concurrency supports `queue: max`; stale writers still need explicit
  state reconciliation. See [official concurrency documentation](https://docs.github.com/en/actions/how-tos/write-workflows/choose-when-workflows-run/control-workflow-concurrency).
- [NuGet trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
  removes long-lived key rotation, but requires an account-owner trust policy.
  It cannot be silently activated from a code-only PR. The implemented key/scope
  preflight remains compatible with the existing repository publication model.

Primary pages are downloaded under `research/primary/`, in addition to exact
upstream source archives and related issue searches. Tool versions, action SHAs,
release metadata and recipe snapshots are retained in `research/`.

## Upstream reports and local remedies

Each report contains a reproduction, workaround and code-level suggestion.

- https://github.com/link-foundation/csharp-ai-driven-development-pipeline-template/issues/79
- https://github.com/link-foundation/cpp-ai-driven-development-pipeline-template/issues/13
- https://github.com/link-foundation/cpp-ai-driven-development-pipeline-template/issues/14
- https://github.com/linksplatform/Data/issues/176
- https://github.com/linksplatform/Data/issues/177
- https://github.com/linksplatform/Scripts/issues/38
- https://github.com/linksplatform/Workflows/issues/9
- https://github.com/linksplatform/Memory/issues/92
- https://github.com/linksplatform/Memory/issues/93
- https://github.com/pytorch/cpuinfo/issues/450
- https://github.com/github/codeql/issues/22928
- https://github.com/github/codeql/issues/22929
- https://github.com/github/codeql/issues/22931

Conan Center has issues disabled (`upstream/conan-issues-disabled.json`); proposed
report is retained as `upstream/conan-recipes.md`. This does not block the fix:
the necessary bounded recipe changes are applied before building here.

## Verification and external-state limits

Local verification includes **82 C# tests, 46 native CTest cases and 44 pipeline
regressions**, real installed-package consumers, both native translation units
under strict analysis, both C FFI language modes, native ZIP/NuGet byte-content
validation, bounded benchmark smoke, both API documentation trees and PDF, and
workflow/repository/security analysis. Exact commands and output are in `local/`;
CI results tied to the final source SHA are recorded below.
The first complete CI run and subsequent fixes are preserved separately, so
passing results cannot be confused with earlier failures or another SHA.

An invalid public NuGet secret cannot be rotated from source code. Main now
fails explicitly before building if it is invalid; PR checks exercise this
behavior without requiring a secret. Public release publication is intentionally
not attempted from this PR. The external Rust submodule retains its original
nightly selection for its optional standalone build; this repository's active
C/C++ FFI contract is verified in both language modes.

Final CI, feedback refresh, default-branch containment and clean-tree evidence
will be appended when the latest implementation run finishes.

## Third-iteration local verification

- Strict .NET build: 0 warnings, 0 errors; **77 tests, 0 skipped**, example and
  isolated packaged consumer pass (`local/csharp-full-iteration3-final2.log`).
- Native Clang build, one worker: **950,852 KiB** peak RSS; **42/42 CTest cases**
  pass (`native-clang-build-iteration3-final.log`, `native-tests-iteration3-final.log`).
- Installed native NuGet headers compile and run independently for both stores
  (`native-consumer-iteration3.log`); ZIP and NuGet header bytes match sources.
- Finite ASan/UBSan heap and self-point probes pass after fixes; their original
  failures are retained separately. The public C++ example passes sanitizers.
- **42 pipeline regressions** pass, including routing of the native package
  example; native formatting, repository checks, actionlint and pedantic zizmor
  pass. Full-SARIF gating rejects the complete 87-finding original reports.
- A default `dotnet build-server shutdown` hung after its servers exited; only
  that shutdown command was terminated. Subsequent tests completed normally.

Fresh CI, full sanitizer matrix and final SARIF reports are still pending.

The final dependency reconfiguration initially reused CMake’s cached older
toolchain and a local harness attempted unavailable `/usr/bin/time`. Those
attempts are retained as `native-final-configure.log` / `native-final-build.log`;
they are not successful validation. The cache was preserved under a new name,
and the clean configuration uses the new explicit Clang profile and the existing
Python RSS measurement helper (`native-final-*.log`, suffix `2`).

## Complete warning-stream audit

The second CI log was rechecked for every warning, including third-party output.
`local/native-dependency-diagnostics.log` proves six benchmark diagnostics were
hidden by the previous clang-tidy header filter. The filter now includes all
maintained test/benchmark/example headers; benchmark iteration tokens explicitly
use `[[maybe_unused]]`. `native-static-expanded.log` passes with no diagnostics.

Conan's profile auto-detection printed guess/instability warnings. Explicit
profiles now derive the selected compiler and version from its predefined macros;
unsupported platforms require a supplied profile. Legacy dotted **internal Conan
aliases** are normalized to hyphens throughout the dependency graph. Public
namespaces, CMake targets, header paths and NuGet IDs remain intact. The metadata
cleanup also handles final lines without newlines and removes deprecated
`cpp_info.names`/`filenames`. Exports and installs treat every Conan warning as an
error. `local/conan-strict-warning-install3.log` verifies a clean installation.

The same pinned recipe snapshot repairs obsolete dependency CMake minimums,
including version ranges. Google Benchmark uses the actual `Threads::Threads`
target instead of guessing `-pthread` from an empty legacy variable (which is
valid with modern merged glibc). GTest moves from the obsolete unmaintained
snapshot to the recipe-supported 1.13.0 release; its two floating conversion
warnings use the explicit casts already present in upstream's current source
(`research/gtest-printers-current.h`). Production library versions are preserved.

Five fortified GCC logging warnings in cpuinfo expose ignored short writes and
EINTR. The finite regression first fails on preserved upstream source, then
passes with the shared POSIX write helper and `-Werror=unused-result`. CMake and
metadata shape changes fail closed instead of applying unreviewed replacements.
Report and workaround: https://github.com/pytorch/cpuinfo/issues/450 .

CodeQL's 300-file GitHub API diff limit also emitted warnings. Full-repository
analysis is now explicitly selected using the pinned action's supported
`CODEQL_ACTION_DIFF_INFORMED_QUERIES=false` feature flag. The supporting official
source is preserved in `research/codeql-feature-flags.ts` (lines 243–246) and
`research/codeql-diff-informed-analysis.ts` (lines 34–38, 158–164). This also makes
full-SARIF gating independent of how much of the PR's diff GitHub can expose.

Final clean native rebuild: 955,456 KiB RSS, all 42 tests pass, bounded benchmark
passes (`local/native-final-build2.log`, `native-final-tests2.log`,
`native-final-benchmark2.log`). The 42-test pipeline suite passes as well.

The installed native NuGet consumer passes for both stores after the final
dependency rebuild (`local/native-consumer-final.log`). Complete native static
analysis also passes with the broadened header filter (`native-static-final.log`).
The early feedback refresh preceded the later automated reviews; the final
review inventory below supersedes it.

### Fourth validation iteration (2026-10-29 07:12–07:22 UTC)

Run [40833592739](https://github.com/linksplatform/Data.Doublets/actions/runs/40833592739),
head `65cadabdd11c4bf0f2ac9735d6a6385fe34845d4`, passed C# on both operating
systems, docs, quality, links, audit and Actions CodeQL. The full log is
`ci-logs/third-40833592739.log` (11,302 lines). Four native jobs failed at strict
recipe export: cpuinfo's `cci.20220228` version had no explicit binary identity
policy (`third-codeql-cpp.plain.log:1390`). Prior cached installs did not exercise
this export warning. Conan's [official guidance](https://docs.conan.io/2/knowledge/guidelines.html#guidelines-bad-alphanumeric-majors)
requires full-version identity for these non-semantic versions. The patch now
sets both non-embedded and unknown relationship modes to `full_mode`.
`experiments/test_conan_compatibility.py` reproduces the real strict export
failure in an empty cache, then verifies the patched recipe succeeds. Before
and after outputs and all 18 strict exports are preserved under `local/`.

The enforced C# SARIF gate correctly failed three new findings: one implicit
LINQ filter and two Path.Combine quality findings in the new disposal test
(`third-40833592739.log:7084` onward; full SARIF under `github/`). The loop now
filters explicitly and temporary paths use Path.Join. All 77 C# tests, strict
build, formatting and installed-package consumer pass again (`local/csharp-fourth.log`).
All 43 pipeline regressions pass (`local/pipeline-fourth.log`).

The native CodeQL job also warned about attempting an overlay with a manual
build (`third-40833592739.log:3872`). Full-database analysis is now explicitly
selected by disabling overlay analysis alongside diff-informed queries. The
pinned action feature definitions are preserved in `research/codeql-feature-flags.ts`.
These changes retain full-repository analysis. Fresh CI remains required.


### Fifth validation iteration: split-buffer capacity (2026-10-29)

Fourth run [40834656679](https://github.com/linksplatform/Data.Doublets/actions/runs/40834656679)
started at 07:19:06 UTC for `feb4dd0dfaba8c6efe4ce49d9c56eab2cff58577`.
All strict recipe exports now succeed. Both C# platforms, docs, links, audit,
quality, Clang and sanitizer jobs pass. The complete Actions and C# SARIF reports
contain zero findings; no warning lines remain in the complete run log.
GCC and the real GCC CodeQL build both expose the same actual storage defect:
`Value [4160] ... out of range [0..4096]` in the split ownership test
(`ci-logs/fourth-gcc.plain.log:1779`,
`ci-logs/fourth-codeql-cpp-progress.log:2502`). Native CodeQL never reaches
analysis because its prerequisite test fails; missing SARIF correctly fails too.
All 1,058 available artifact files are archived and byte-verified in
`ci-artifacts/fourth.tar.gz` with a SHA256 manifest. Full logs and each failed
job log are retained, rather than treating another compiler's success as proof.

The split store computed available links from its data buffer alone. For 64-bit
addresses each data record occupies 16 bytes but each index record occupies 64.
Two 4,096-byte buffers therefore fill at different link counts. The ignored
index limit makes allocation 64 request 4,160 bytes. Initialization also used the
data record size for index reservation and omitted header/zero-link space.
This is present in **both C++ and C#**, not an upstream Conan or compiler issue.
A finite standalone native probe reproduces allocation 64 failing
(`local/split-growth-native-before3.log`). Three new C# theory cases with equal
and unequal reservations all fail before the change
(`local/split-growth-csharp-before.log`). The native unit regression covers the
same three configurations, 128 allocations each, with explicit buffer bounds.

Both implementations now reserve complete data/index layouts and define
`ReservedLinks` as the minimum usable capacity after subtracting the zero data
record and index header. Growth checks and recalculation use that same bound.
All focused C# cases and the standalone ASan/UBSan probe pass afterward.
The full C# check passes **80 tests, zero skipped, zero warnings/errors** and
validates its installed NuGet consumer (`local/csharp-fifth.log`). All **43**
pipeline regressions pass with Conan installed (`local/pipeline-fifth.log`).
Conan is now included in quality requirements so the real empty-cache export
regression cannot silently skip in CI. Full native validation and fresh CI are
required before marking ready.

One initial native probe accidentally selected Platform.Data's similarly named
options type and failed to compile; the corrected fully-qualified Doublets type
is retained. A subsequent ASan launch under a virtual-address cap could not map
its shadow memory. Neither is evidence of the storage fix. The corrected finite
probe's before/after results are separate and successful compilation precedes
the actual failing allocation. No full GCC build is repeated in this container.

### Final review inventory

All issue comments and required sub-issues remain empty. Refreshed API responses
for all three PR feedback types are under `github/*final-review2.json`.
Eight inline comments are automated CodeQL findings, all addressed:
commented-out native benchmark/test placeholders removed; included definition
files changed from `.cpp` to `.h`; the implicit C# filter made explicit; temporary
paths use `Path.Join`. The four automated reviews are verified against the
complete SARIF inventory, not only changed-line alerts. There is one general
informational CodeQL setup comment and no additional human requirements.

Main `799502ce9275edc80d9fbc1953ff8892fe0eca19` is an ancestor of the PR head.
GitHub rejects `gh pr diff` with HTTP 406 for this >300-file change
(`local/pr-final-api-error.log`). The equivalent local main-to-head diff is saved,
plus a 3,964-line code-focused comparison that normalizes formatting/comments
while preserving string literals (`local/final-code-review.diff`). Every code
section was reviewed in bounded chunks alongside the original source, and the
latest focused changes were reviewed separately. Active API, package channels,
release tags, tests, examples and benchmark registrations are retained; removed
native files contained only commented-out placeholder code. The comparison
helper is retained under `experiments/review_semantic_changes.py`.

The complete local native rebuild passes at **955,052 KiB peak child RSS**;
all **43 native tests** pass (`local/native-fifth-build2.log`,
`local/native-fifth-tests.log`). The first new native test compilation rejected
signed `int` capacities in a brace initializer; the fixture now uses explicit
unsigned capacity types (`native-fifth-build.log` preserves that diagnostic).
This was a test-fixture compile error, not a passing test run.


Latest-main evidence was refreshed while final CI ran. Main remains
`799502ce9275edc80d9fbc1953ff8892fe0eca19` and is already merged here. Its run
[40823615243](https://github.com/linksplatform/Data.Doublets/actions/runs/40823615243)
at 05:28:39 UTC (after PR 525) has the same three unresolved baseline failures:
public NuGet 403 (`main-after-525-40823615243.log:1337`), apt NuGet unavailable
(line 2981), and Python 2 pygments unavailable (line 3235). All 3,258 raw log
lines and job metadata are now collected. Its 19 compiler/package diagnostic
families are unchanged; exact first occurrences/counts are in
`local/main-warning-families-final.json`. Release and GitHub package jobs still
reported success independently. This confirms the current main failures are
addressed by this PR's explicit gates and tool replacements, rather than relying
only on the older issue-linked run. The NuGet account action remains external.

The final review refresh (`github/*final3.json`) still has no issue comments or
required children and the same eight automated inline findings. Local full-history
Gitleaks passes across 3,261 commits / 310.80 MB
(`local/gitleaks-fifth-history.log`). CI quality on the new head actually executes
all 43 regressions, including the real strict empty-cache Conan export
(`ci-logs/fifth-quality.log:564,612`), with no test skipped for missing tooling.


### Fifth CI: GCC package-consumer output isolation

[Run 40836388311](https://github.com/linksplatform/Data.Doublets/actions/runs/40836388311)
at implementation `b80b5b325` passes all 43 native tests with GCC, Clang and
ASan/UBSan, plus 80 C# cases on Linux and Windows. GCC then fails in the installed
package consumer, not the library tests (`ci-logs/fifth-gcc.log:2096–2110`).
Its generated CMake link command contains
`-Wl,--dependency-file=CMakeFiles/Platform.Data.Doublets.Tests.dir/link.d`.
The consumer retained every `-Wl,` argument while changing working directories,
so GNU ld attempts to write to a nonexistent build-relative path.

This is a local smoke-test adapter defect, not a CMake or GNU linker defect.
The [CMake variable documentation](https://cmake.org/cmake/help/latest/variable/CMAKE_LINK_DEPENDS_USE_LINKER.html)
explains linker-generated dependencies, and [GNU ld documents the output option](https://sourceware.org/binutils/docs/ld/Options.html).
Raw primary-source pages are saved under `research/cmake-link-depends.html` and
`research/gnu-ld-options.html`. Suppressing CMake dependency tracking globally or
creating unrelated build directories inside the consumer would hide the actual
coupling. The consumer now discards only this build-specific dependency output;
library paths, archive order and other linker flags remain intact.

`experiments/test_package_consumer.py` compiles and runs a tiny real installed
header consumer using the exact generated linker flag. Before the fix it fails
with the identical GNU ld message (`local/gcc-consumer-before.log`); afterward it
passes as part of all **44 pipeline regressions** (`local/pipeline-sixth.log`).
The full native NuGet consumer also passes (`local/native-sixth-consumer.log`).
Repository validation and actionlint pass; a first actionlint invocation used the
system Python without PyYAML, then the documented tool environment passed.

### Fifth CI: complete native SARIF and sixth local iteration

At **07:47:19 UTC**, the fifth run finished with the expected failing required
gate: GCC consumer isolation and **39 native CodeQL results**. The real native
CodeQL build and tests succeeded; the SARIF findings gate correctly failed.
`github/codeql-native-fifth-findings.json` inventories every location/message;
`ci-logs/fifth-native-codeql.log:3844–3897` contains the fatal listing. C# and
Actions SARIF contain zero results. The complete fifth artifacts contain 1,097
byte-verified files, archived with SHA256/size manifest in `ci-artifacts/fifth*`.

| Diagnostic | Count | Cause and remedy |
| --- | ---: | --- |
| comparison-with-wider-type | 1 | A record-field loop uses an address-width counter against size_t; use size_t |
| suspicious-pointer-scaling | 6 | Intentional record indexing casts std::byte storage before arithmetic; query exempts char/void but omits std::byte. Use equivalent array subscripting throughout |
| unused-local-variable | 12 | Six annotated benchmark loop tokens ignored by the query; explicitly consume them. Six test helpers construct an unused duplicate store; remove only the duplicate, retaining UsingStorage's actual store |
| unused-static-variable | 7 | Six benchmark registration bindings and one abbreviated parameter pack are falsely flagged. Retain registration handles through the supported API and separate empty/nonempty overloads. Inspection additionally finds the real discarded All restriction, which is fixed and reproduced independently |
| empty-if | 1 | United native allocation silently ignores exhaustion; throw the existing limit exception |
| commented-out-code | 8 | Remove only inactive commented declarations/algorithms from five headers |
| integer-used-for-enum | 1 | The public size_t field-index API is indexed, rather than an enum domain; explicit index comparisons preserve reference semantics and range errors |
| poorly-documented-function | 3 | Explain query-shape, wildcard/index selection, live/free records and callback termination within the long Count/Each implementations |

The exhaustion condition is also off by one in **all four** C#/C++ united/split
stores: a fresh allocation must reject `AllocatedLinks >= Maximum` before
incrementing. Reuse from the free list must still work at the limit. Two C# cases
and two native cases fail before the fix; the native All restriction case also
fails before its fix (`local/address-limit-csharp-before2.log`,
`local/native-sixth-before-tests.log`). The initial native fixture used a
factory-function name as an exception type; that compile error is retained
separately and is not claimed as evidence for the production bug.

After fixing all four allocation checks and the variadic overload, all **82 C#
cases**, all **46 native tests**, and all **44 pipeline regressions** pass locally
with no skips. The full C# locked build/format/example/package/installed consumer
check passes (`local/csharp-sixth.log`). Native builds use one Clang worker and
-O0 -g0; the full changed-source build peaks at **956,120 KiB RSS**. All six
benchmark registrations have identical names/arguments before and after.
An initial smoke command used the newer benchmark seconds suffix; pinned 1.6.0
requires a bare numeric duration, matching the existing CI command. The bounded
smoke and rejection of unknown arguments pass after correcting that invocation.

Calling RegisterBenchmark from main exposed a clang analyzer ownership-model
false positive inside Google Benchmark's header: registration transfers ownership
to the external registry, which the analyzer cannot see. Registration handles are
now returned and retained in a static tuple, preserving startup registration and
BENCHMARK_MAIN without suppressing either analyzer. The focused benchmark
clang-tidy check passes (`local/benchmark-sixth-static3.log`).

Exact upstream query sources and the pinned Google Benchmark guide are archived
under research/. GCC and Clang compile/execute both reduced CodeQL experiments
with strict warnings. The reduced examples have not been independently run
through a local CodeQL database: the observed findings are from the complete
real-build CI SARIF. Existing-issue searches found no matching report. Reports
[github/codeql#22928](https://github.com/github/codeql/issues/22928) and
[github/codeql#22929](https://github.com/github/codeql/issues/22929) contain the
observed diagnostics, reduced examples, source-level suggestions and workarounds.
The complete native formatting, clang-tidy and exhaustive Cppcheck checks pass
(`local/native-sixth-final-static.log`), as does the rebuilt native package
consumer (`local/native-sixth-final-consumer.log`). Full fresh CodeQL,
matrix/sanitizers and installed GCC consumer remain required
before readiness; local success is not substituted for those checks.


A whole-working-directory credential scan additionally sees five generic-key
matches in ignored generated DocFX JavaScript source maps. These files are not
repository source or staged changes; the evidence-directory scan passes without
an exemption. The authoritative full-history scan remains required after the
new commits. No generated source map, raw unredacted scanner report, or new
allowlist is added to the repository.

### Sixth CI and the remaining parameter-pack extraction finding

[Run 40840157071](https://github.com/linksplatform/Data.Doublets/actions/runs/40840157071)
at **3e61f330d**, created **08:09:41 UTC**, finishes with every runtime/build,
compiler/static-analysis, sanitizer, consumer, documentation, link and audit job
passing. Both C# TRX reports contain **82 executed / 82 passed / zero skipped**.
All three native configurations pass **46 tests** and the real installed NuGet
consumer (GCC consumer line 2104, ASan consumer line 2107 of their downloaded job
logs). Quality executes all **44** pipeline regressions. Documentation validates
**538 HTML pages and a 277-page source PDF**. The complete compiler/runtime/
action-warning audit contains **zero** diagnostics, not just zero annotations.

Native CodeQL clears **38 of 39** findings; its sole remaining result is
`cpp/unused-static-variable` on the **restrictionPack function parameter**,
`ILinksExtensions.h:252`, despite its braced-initializer expansion being used
(`ci-logs/sixth-native-codeql.log:3847`). This is separate from the actual ignored
restriction bug, whose reproducing regression now passes. Complete SARIF and
all 1,097 downloaded artifact files are byte-verified in `ci-artifacts/sixth*`;
`github/codeql-native-sixth-findings.json` records the exact remaining result.
The findings gate and required job correctly remain red.

The function is instantiated with both an empty and a nonempty pack. An empty
instantiation has no expanded argument accesses; the query/extractor appears to
retain an unread static placeholder. That explanation is an inference from
source and SARIF, pending extractor-fact inspection upstream. The repair adds
an explicit zero-argument overload and an explicitly declared, nonempty,
convertible variadic overload, preserving existing zero-, scalar- and
collection-argument calls. The full 46-test native suite passes after this
change at **955,860 KiB peak build RSS** (`local/native-seventh-build.log`,
`local/native-seventh-tests.log`). Full fresh CodeQL verification is still
required; the result is not waived or suppressed.

[github/codeql#22931](https://github.com/github/codeql/issues/22931) records the
observed real-build result, a reduced example, strict GCC/Clang executions,
workaround and suggested extractor/query regression. The reduced standalone
example has not been analyzed locally with CodeQL. Benchmark documentation is
also corrected to the actual pinned **1.6.0** dependency (`cpp/conanfile.txt`);
its exact guide is archived alongside the previously consulted 1.8.0 guide.
`experiments/archive_ci_artifacts.py` retains the reusable archive/hash verifier;
its real artifact smoke verifies every downloaded file after writing the archive.
The complete branch workflow inventory and every available completed branch log
are collected, including the earlier auto-merge and standalone security/link runs.


### Seventh CI: every implementation criterion verified

The native parameter-pack workaround clears the final result. Run
**40842727244**, created **08:31:20 UTC**, completes **08:39:37 UTC** at exact
HEAD **5cf40c5a338a38978221aab65a8e017005c95303** with all 14 applicable jobs
successful. Only main-only credentials/publication jobs are deliberately skipped
on this PR. The required aggregate job succeeds; no diagnostic is suppressed to
obtain this result. Complete workflow and every executed job log are preserved
as `ci-logs/seventh-full.log` and `ci-logs/seventh-job-*.log`.

The reusable `experiments/verify_ci_evidence.py` independently checks the exact
SHA and job outcomes, every complete SARIF result/invocation, both raw TRX
counters, all three CTest artifact pass counts, all three installed native
consumers and both installed C# consumers, quality test execution without skips,
HTML count and PDF page objects. Its output is `github/seventh-verified.json`.
The existing production SARIF validator separately accepts all three reports
(`local/seventh-sarif.log`). The whole executed-job warning scan contains zero
compiler, CMake, runtime, sanitizer or action warnings. All 1,097 artifact files
are re-read from their saved archive and checked byte-for-byte against their
SHA256/size manifest (`local/seventh-artifact-archive.log`).

The final feedback refresh has no issue comments, no required sub-issues, one
informational CodeQL conversation comment, **nine** automated inline comments
and **five** automated reviews (`github/*-final.json`). The ninth inline comment
is exactly the sixth-run parameter-pack result, now cleared by complete SARIF;
there are no new human requests. Latest main is still
`799502ce9275edc80d9fbc1953ff8892fe0eca19`, confirmed as an ancestor of this
branch. All branch run metadata and every available completed run log, including
earlier separate link/security and auto-merge runs, are retained. The refreshed
normalized code diff contains 4,392 lines; the final overload change was also
reviewed directly against the previous green runtime implementation. Existing
zero/scalar/collection calls and every active feature remain covered.

The three CodeQL upstream reports now include this exact successful verification
and distinguish the production workaround from the reduced compiler-only probes.
All thirteen upstream report bodies and URLs remain in `upstream/`. No required
implementation or verification criterion is deferred. The repository owner still
must rotate/configure publication credentials before a public release; that
account operation is not represented as completed. The final evidence-only
commit receives the same full CI before handoff, with its final metadata/logs
collected locally in this folder; GitHub records the final readiness state.
