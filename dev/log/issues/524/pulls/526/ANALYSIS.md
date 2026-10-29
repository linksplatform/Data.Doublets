# Issue 524 / PR 526 investigation

Repository: https://github.com/linksplatform/Data.Doublets
Issue: https://github.com/linksplatform/Data.Doublets/issues/524
Pull request: https://github.com/linksplatform/Data.Doublets/pull/526

This report distinguishes observed failures from inferred causes. Raw GitHub
metadata, downloaded logs, upstream source snapshots, local reproductions and
verification output are retained beside it. No secret value is included.
Final CI results and feedback are recorded below after the implementation run.

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
| R5 | Report shared defects to the templates and related projects | Ten reproducible upstream reports, complete bodies/URLs in `upstream/`; Conan issue creation is disabled and the workaround is implemented locally |
| R6 | Download logs/data into the requested folder; reconstruct the timeline and diagnose actual causes | This folder contains raw API responses, complete workflow logs, source archives, online primary sources and bounded local experiment logs |
| R7 | Propose solutions and plans per requirement; research existing components | Root-cause table, completed implementation sequence, component evaluation below; keep existing .NET/GTest/Conan/DocFX frameworks |
| R8 | Reproduce bugs before changing production code | Baseline failures in `local/*before.log`, helper tests under `experiments/`, language regression tests and C++ compilation reproductions |
| R9 | Apply matching fixes across the entire maintained codebase | All C# projects share policy; native source/test/benchmark/profile paths are formatted and checked; both READMEs, all publishing/docs entry points and both memory storage families reviewed |
| R10 | Preserve existing features and publishable packages; prepare next releases | Existing NuGet IDs and `csharp_`/`cpp_` release tags retained, both language APIs and source PDF retained, native ZIP layout preserved, independent automatic changeset for both packages |
| R11 | Add verbose diagnostics when more evidence is needed, default off | `CI_VERBOSE=1` enables shared native/release operation tracing; compiler/static-analysis/test outputs and job artifacts retain failures; no token tracing |
| R12 | Complete everything in this single PR; preserve history and the prepared branch | Incremental forward commits only on `issue-524-b1512fd1c7a8`; merge latest main, no direct merge, all required work tracked in `PLAN.md` |
| R13 | Run local checks and fresh CI, inspect actual errors, fix feedback, leave a clean tree | Repeated runs are tied to exact SHAs/timestamps in `github/runs-*.json`; final verification to be appended after the last run |
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
| Native benchmark unused-static false positives | No-build C++ extraction cannot resolve the Conan-supplied Google Benchmark registration macros | Trace the real dependency-aware build in CodeQL; retain all benchmark functions and registrations. Rename included test/benchmark definition files from .cpp to .h rather than suppress include diagnostics |
| C# EnsureCreated loops or misses lower free IDs | Creator result never advances; stopping at the maximum requested ID ignores nonmonotonic free-list allocation | Track every missing internal request, allocate unique IDs until all are satisfied, then reset/delete only unrequested intermediates. Both stores cover free-list order, self-point gaps, external/sentinel IDs and a non-progressing creator |
| C# virtual calls during storage construction | Base Constants and derived Init/SetPointers dispatch before the most-derived fields are initialized | Assign backing constants directly and use concrete nonvirtual construction routines; preserve virtual APIs for later calls. Two derived-probe tests fail before and pass after |
| Native ASan allocation/deallocation mismatch | Upstream Platform.Memory allocates with malloc/realloc but destroys with delete | Exact pinned-source recipe patch uses std::free; standalone ASan reproduction before/after, upstream Memory #92 |
| Native growth uses a stale header reference | C++ assignment after realloc copies through a potentially freed reference instead of rebinding it | Reacquire the header through a pointer after relocation; the 128-allocation small-step united ownership test reproduces the capacity error before and verifies every growth afterward |
| Native helper leaks and duplicate allocation | Raw new pointers are overwritten on growth and never destroyed; split target helper allocated twice | Unique ownership for every helper, replacement automatically releases old instance; remove duplicate allocation and initialize linked-list helper. Two finite 128-allocation ownership regressions plus CI ASan |
| Split memory initialization fails at small reservation steps | Equality check shrinks a larger caller reservation; upstream C#/C++ zero-fill computes an invalid negative/unsigned length | Grow only when existing capacity is insufficient in both implementations; small-step tests and upstream Memory #93 with source-level fix suggestion |
| Exceptions hidden without diagnostics | Intentional NoExceptionsDecorator has no observer/filter; explicit transaction disposal also swallows failed commit persistence | Optional exception filter and callback default off; default conversion behavior retained. Explicit disposal propagates persistence failure while finalization remains best effort; real I/O-failure regression |
| Native package consumer and EnsurePointsCreated | Header-existence-only smoke never instantiates shipped templates; umbrella-only compilation reveals two-phase lookup hidden by test helpers, and self-point cleanup leaves indexed ghosts | Compile and run a standalone consumer from installed NuGet headers with real Conan dependency flags on every compiler. Use the visible base Update overload; reset intermediate values before deletion. Finite ASan reproduction changes exists(4) from true to false and the public example covers both stores |
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

Conan Center has issues disabled (`upstream/conan-issues-disabled.json`); proposed
report is retained as `upstream/conan-recipes.md`. This does not block the fix:
the necessary bounded recipe changes are applied before building here.

## Verification and external-state limits

Local verification includes all C# tests and package consumers, all 42 native
CTest cases, both native translation units under strict GCC and Clang syntax,
both C FFI language modes, native ZIP/NuGet byte-content validation, benchmark
smoke, 42 pipeline regression tests, both API documentation trees and PDF,
workflow/repository/security analysis. The expanded suite now has 77 C# tests and 42 pipeline tests; see the final run evidence below. Exact commands and output are in `local/`.
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
Final feedback refresh found only GitHub’s informational code-scanning setup
comment; no issue comments, required children, inline feedback or reviews.
