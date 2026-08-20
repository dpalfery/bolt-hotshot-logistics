# Failure mechanisms

A mechanism explains *how* a correctly-written instruction produced non-compliance. "The agent
ignored it" is not one — it restates the symptom. Naming the mechanism is what turns a complaint
into an edit, because each mechanism below has a characteristic fix and they are not
interchangeable.

Rung numbers refer to the fix ladder in [SKILL.md](../SKILL.md).

## Contents

- [Instruction axis](#instruction-axis) — wording, position, and gating
- [Tool axis](#tool-axis) — whether the named surface is reachable
- [Model axis](#model-axis) — whether the reader can hold it
- [Permission axis](#permission-axis) — whether the action is allowed
- [Delegation axis](#delegation-axis) — what the caller sent and accepted
- [When several agents miss the same thing](#when-several-agents-miss-the-same-thing)

---

## Instruction axis

### Unreachable surface
The instruction names something only a human sees: an IDE panel, a dashboard, a browser console,
"the output window". The agent has no such surface. What usually happens next is a good-faith
substitution — it reads the file, judges the code sound, and reports compliance sincerely.

*Signature:* the instruction names a UI location rather than a command or a tool.
*Fix:* rung 1. Name the command or tool call that produces the same evidence.

### No observable done-state
"Address any findings", "make sure it is clean", "handle errors properly". Nothing here can be
checked, so the instruction is satisfiable by believing it is satisfied — and the agent that
reports compliance is not lying, it has simply met the only bar available.

*Signature:* the verb has no object you could count, diff, or exit-code.
*Fix:* rung 2. Define done as a command plus its expected output.

### Not a gate
The instruction is present and checkable, but nothing in the completion path runs through it. It
sits in a workflow list as one step among many, and the agent reaches a defensible "done"
without it.

*Signature:* the constraint appears in a numbered list; the completion criteria do not mention it.
*Fix:* rung 3. Move it into the completion criteria — done is *defined* as having passed it.

### Positional decay
The constraint is the last item of a long list, or in a section read after the approach has
already been chosen. Instructions that arrive after a plan is formed get evaluated against that
plan rather than shaping it, and a trailing step competes with the sense of being finished.

*Signature:* the constraint is the final bullet, or appears after the body's main workflow.
*Fix:* rung 4. Move it to where the approach is decided. Do not add emphasis in place.

### Scope ambiguity
"In the code you changed" versus the workspace; "the files you touched" versus their dependents.
Under time pressure the narrow reading wins, and it is a legitimate reading.

*Signature:* the critique and the agent disagree about the denominator, not the behavior.
*Fix:* rung 2, defining the set explicitly — often the correct answer is that the agent complied
and the expectation needs restating.

### Outranked by a supersede clause
The file contains a "this supersedes all other instructions" block, and the missed instruction
sits outside it. These blocks work, which is precisely the problem: they demote everything they
do not name.

*Signature:* a categorical override earlier in the file that does not mention the constraint.
*Fix:* fold the constraint into the block, or narrow the block's claim.

### Contradicted by scope or description
The body asks for something the agent's `description` or scope section disclaims — a review-only
agent told to resolve findings, a backend agent told to check the client build. The agent obeys
its identity, correctly.

*Signature:* body and frontmatter make incompatible claims about what the agent does.
*Fix:* decide which is true and change the other. Do not leave both.

---

## Tool axis

### No tool reaches the evidence
The instruction is executable in principle but the agent has no granted tool that produces the
required signal — no language-server access, no build execution, no diagnostics API.

*Signature:* the four-axis check returns *impossible*; the frontmatter tool list has no candidate.
*Fix:* rung 5, or rung 1 if an already-granted tool can produce the same evidence another way.

### Tool granted but unnamed
The capability exists and the agent never thinks to use it, because the instruction describes an
outcome and leaves the route unspecified.

*Signature:* the tool is in the grant list; the transcript shows it was never called.
*Fix:* rung 1. Name the tool and the invocation in the instruction.

### Evidence available only upstream
Common for reviewers. The signal exists, but only in the implementer's environment — the reviewer
sees a diff and nothing else, and cannot regenerate the finding set.

*Signature:* a reviewer is asked to verify something only the implementer's run could produce.
*Fix:* delegation axis — require the implementer to attach the evidence and the caller to pass it
through. Grant the reviewer the capability only if it genuinely needs to reproduce it.

---

## Model axis

### Tail-position under a small profile
A long body read by a fast model, with the constraint at the end. Instruction adherence degrades
with body length and distance from the task, and cheaper profiles have less margin.

*Signature:* `model-profile` resolves to the fast tier; the body is long; the constraint is late.
*Fix:* try rungs 3 and 4 first — gating and repositioning are cheap and often sufficient. Escalate
to rung 6 only with the length and position measured, because a profile change costs on every run.

### Compliance load exceeds the profile
Many simultaneous constraints, each individually clear. Something drops, and which one drops
varies between runs — the tell that separates this from a wording problem.

*Signature:* different constraints missed across runs of the same agent on similar tasks.
*Fix:* reduce the constraint count (merge, delete, move to rung 7) before raising the profile.

---

## Permission axis

### Required action denied
The instruction requires an action the capability profile denies. Verifying a build requires
process execution; resolving a finding requires write access. Denied means the instruction was
never executable, no matter how it was worded.

*Signature:* resolve `capability-profile` through the profiles file; a required action is `deny`.
*Fix:* rung 5 if the agent should have it, or rewrite the instruction to what it *can* do — often
"require and check the evidence" rather than "produce the evidence".

### Instruction contradicts its own profile
Sharper version of the above and the most common structural defect: an agent told to *verify and
resolve* while holding a profile that denies both execution and write. Two impossibilities in one
sentence, and no amount of rewording touches either.

*Signature:* a single instruction requires two separately denied actions.
*Fix:* split the sentence by owner. Verification and resolution usually belong to different roles,
and that is what the profile is already saying.

### Path scoping that is instruction-only
Permission models often express *allow read* but cannot express *allow read of these folders*.
Path restrictions are therefore prompt-enforced and degrade like any other prose constraint.

*Signature:* the boundary crossed is a folder boundary, not a capability boundary.
*Fix:* rung 3 or 4 within the prompt — or accept that it is advisory and put the real boundary in
tooling.

---

## Delegation axis

These are the caller's, and they are frequently the whole answer.

### Constraint never entered the packet
It lived in the agent file and the caller assumed that was enough. Task text is fresher and more
specific than the system prompt, so a packet that omits the constraint effectively demotes it.

*Fix:* add it to the task-packet template for that class of work. This is a caller-side edit.

### Packet stated a goal, not an acceptance criterion
"Implement X" without "and X is done when `<command>` reports zero". The agent returns when the
goal looks met, which is exactly what was asked.

*Fix:* acceptance criteria in the packet template, phrased as an observable.

### Completion accepted without evidence
The agent reported clean; the caller did not ask what command produced that. This is the single
most repeatable delegation defect, and the most fixable.

*Fix:* require the evidence in the completion contract — the command and its output, not the claim.

### Reviewer received the diff but not the signal
Review quality is bounded by the inputs. A reviewer given only changed lines will review changed
lines, and will not surface a finding set nobody handed it.

*Fix:* pass the implementer's evidence into the review packet.

---

## When several agents miss the same thing

Resist the single root cause. Agents differ in profile, body length, tool grants, and the packet
they received, so the same symptom across three agents most often decomposes into three
mechanisms — typically some combination of *absent instruction*, *required action denied*, and
*positional decay*. A shared remedy applied to all three fixes at most one and leaves the others
to recur, while appearing to have addressed everything.

Diagnose each agent separately, then look for what the mechanisms have in common. That common
factor — a packet template, a profile assignment, a missing build gate — is usually the higher-
leverage edit, and you can only see it once the individual mechanisms are named.
