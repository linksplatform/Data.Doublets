# Issue 541 / PR 542 work plan

Objective: implement and verify all requirements from issue 541 and required sub-issues in PR 542, retaining evidence and documenting any external blockers without claiming completion.

- [x] Collect the complete issue, comments, required sub-issues, PR conversation, inline comments, and reviews; inventory every requirement. Refreshed after the first CI run, including the CodeQL bot's review/import finding, addressed in the follow-up.
- [x] Read repository guidance, current implementation, related merged PRs, workflows, and release rules.
- [x] Collect relevant CI runs with timestamps and SHAs, download all available logs, and diagnose actual errors using numbered evidence. Cancelled-run logs recovered through the REST archive endpoint.
- [x] Research primary online sources and existing components; reconstruct the event timeline and root causes; record alternatives and implementation plans. All 620 template files inventoried and archived.
- [x] Create a minimal failing regression test or bounded experiment before fixing each reproducible bug; retain scripts in experiments/ or examples/.
- [x] Implement fixes across all affected locations and retain default-off verbose diagnostics. Tooling-only changes require no product-version fragment. External release and native advisory-service criteria remain unfinished below.
- [x] Run focused checks, then applicable repository checks within the container memory budget; retain logs. Full native local compile stopped safely; all hosted native jobs passed.
- [x] Review diff, compatibility, scope, and requirement-to-evidence mapping; commit atomic changes after local checks. AST comparison confirms formatting-only changes outside the listed semantic repairs; no library API source changes.
- [ ] Integrate current default branch, push only issue-541-ef4751af423d, update PR 542 title/body and positive closing references, and mark ready when implementation is finished.
- [ ] Verify checks on the latest commit (timestamps and SHA), download final CI logs, and confirm a clean tree and consistency of code/tests/description.
- [ ] **External blocker:** authorize the intended main release actor under repository protection, rerun a trusted main release and verify publication. No policy weakening or main writes are authorized by this task.
- [ ] **External coverage limit:** provision Conan advisory provider credentials, map local aliases to upstream coordinates and obtain a real native dependency advisory verdict. The missing-token probe is retained; no native all-clear is claimed.

Constraints: no default-branch pushes or merges; preserve history; no secrets in logs; no host-exhausting probes; run memory-heavy builds sequentially with one worker; do not repeat builds killed by the memory limit. No requested work is complete without verification.
