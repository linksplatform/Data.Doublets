# Design: storage mechanisms and optional policies

These guidelines apply to the C# and Rust implementations of Doublets. They
record the design decision in [PR #527's review](https://github.com/linksplatform/Data.Doublets/pull/527#issuecomment-6492081425).
See [C# deletion](deletion-csharp.md), [Rust deletion](deletion-rust.md), and the
[implementation audit](deletion-audit.md) for concrete APIs and current gaps.

## Core vision

A user chooses the contract and cost of each layer. The storage core contains
the minimum machinery necessary to represent, allocate, index and release
links correctly under its documented preconditions. It does not decide what an
application should do when a caller violates those preconditions. A caller who
already guarantees a correct sequence of operations must be able to omit
redundant contract verification.

Existence validation, rejecting or ignoring missing deletions, uniqueness,
usage validation or cascading, logging, and recovery are separate choices.
Helpers can make those choices per call; decorators can apply them to a store.
Convenient compositions must remain optional. A reset resolver supplies endpoint
resetting; it must not silently become an existence validator too.

The option to bypass validation is intentional. It does not promise correct
results for invalid inputs. In particular, deleting a freed raw C# address can
corrupt the free list, and unchecked addresses can access invalid memory. A user
may take responsibility for operation ordering, or choose a checked boundary.
A dangling reference or raw numeric endpoint is not automatically invalid for
all applications; the configured reference and indexing model determines that.

## Layer responsibilities

| Layer | Responsibility | Caller choice |
| --- | --- | --- |
| Storage mechanism | Allocate/release slots and maintain the selected representation under its preconditions | Select backend and supported indexing strategy |
| Contract validator | Reject invalid addresses, usages, or duplicate values before writes | Include, omit, or replace |
| Resolver | Apply a named transformation, such as reset, cascade, or ignore missing deletion | Include, omit, or replace |
| Helper | Perform an explicitly requested sequence or check for one operation | Choose per call |
| Composition | Offer a convenient ordered set of policies | Inspect, customize, or build another stack |

Do not remove structural bookkeeping that makes a selected representation work.
For example, detaching a freed slot from an index and classifying references for
split internal/external trees are mechanisms. Rejecting a missing slot or deciding
to cascade to its users are policies. Existing mandatory tree indexes and free-slot
reuse have separate proposals for making their selection configurable; this PR
does not claim those proposals are already implemented.

## Guidelines for changes in either language

1. Write down the operation's preconditions, result, callbacks, and mutation
   boundary. Identify which layer owns each check or transformation.
2. Search for existing helpers and decorators before adding checks to a root
   implementation. Preserve a route with no redundant validation.
3. Keep independent policies independent. Existence, contents reset, incoming
   usages, uniqueness, error suppression and synchronization are not substitutes
   for one another.
4. Put guards before layers that read or write the guarded link. Document order
   and recursive dispatch: C# propagates `Facade` through a stack; Rust's static
   decorators recurse through the layers they own.
5. Test checked failures and no-ops for unchanged contents, allocation, count and
   callbacks. Test valid unchecked calls without deliberately corrupting memory.
   Verify that omitting a policy really omits its work.
6. Distinguish memory safety from application validation. Rust's safe APIs must
   uphold Rust's safety rules. An unchecked primitive that needs caller guarantees
   must use an appropriate `unsafe` API with explicit safety conditions; do not
   remove checks from a safe method and create undefined behavior.
7. In Rust, prefer generic wrappers, concrete builder return types, inlinable
   forwarding, and compile-time policies. In C#, keep forwarding simple and
   avoid making every call pay for an optional policy. Measure performance claims.
8. Document current behavior separately from the intended architecture. When
   another operation violates this design, report it separately with evidence
   instead of expanding a focused fix into unrelated changes.

These rules govern update and creation as well as deletion. Passing tests for a
checked backend does not prove that validation is optional. Conversely, a crash
from violating a documented unchecked precondition is not by itself a reason to
make every raw operation checked.
