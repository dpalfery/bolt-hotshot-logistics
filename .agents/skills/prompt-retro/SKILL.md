---
name: prompt-retro
description: Diagnoses why an agent or subagent missed, skipped, or half-did an instruction, and converts the diagnosis into concrete edits to the agent files. Separates instruction wording and position, tool grants, model profile, and capability permissions as distinct causes, and directs each implicated subagent to analyze its own instructions against the task packet it actually received. Use when the user reports that an agent ignored something it was told to do, that a reviewer approved work with obvious defects, or that a standing instruction is not taking; when the user questions prompt effectiveness or agent alignment, or asks for a retro or post-mortem on a miss. Run it in the session where the miss occurred, while the task and its transcript are still in context. Do not use to fix the underlying defect itself — that is ordinary work.
license: MIT
metadata:
  author: David R Palfery
  version: 1.0.0
---

# Prompt Retro

An instruction that agents do not follow is a defect in the instruction system, and it lives
in a file. This skill finds which file, which line, and which of four things went wrong —
what the instruction said, whether a tool could reach what it named, which model read it,
whether the permissions allowed it — and ends by editing that file.

You are running this inside the session where the miss happened. That is deliberate: the task
packets, the agent outputs, and the user's correction are all still in context, and they are
the evidence. Once this session ends they are gone, and the retro becomes speculation.

## The only durable output is a diff

Do not apologize. Do not commit to doing better. Both are stored in a context window that is
about to be discarded, so neither one reaches the next run — they read as accountability while
changing nothing. The next run inherits exactly one thing from this conversation: whatever you
wrote to a file.

So every finding this retro produces has to terminate in a proposed edit to an instruction
artifact — an agent file, a skill, a standards document, a task-packet template, or a build
gate. A finding you cannot land in a file is not finished, and should be reported as such
rather than dressed up as a resolution.

The same rule governs the subagents you interview. Their brief forbids apology for the same
reason, and asks for a mechanism instead.

## Shared improvement means the caller is in scope

The user's critique names the agent that missed. Take it seriously and measure it, but do not
assume the agent is where the defect lives. The retro is allowed — expected — to conclude any
of these:

- The agent never had the instruction; it only exists in someone's memory of having written it.
- The instruction named something the agent had no tool or permission to reach.
- The instruction was in the agent's file but never made it into the task packet you sent.
- You accepted a completion claim without asking for the evidence that would have exposed it.
- The agent complied correctly and the expectation was wrong.

That last one is a real verdict, not an escape hatch. If the agent was told to fix findings
"in the code you changed" and 70 of the 77 findings are in files it never touched, the agent
did what it was told and the defect is in the scope wording — or in the expectation. Say so
plainly with the counts that show it.

---

# Phase A — Establish the record

Do this before forming any hypothesis. Retros fail most often by diagnosing a miss that did
not happen the way it was described.

## 1. Measure the miss

Turn the critique into a number and the command that produces it. "77 new findings" is a
claim until you have run the thing that prints 77.

- What command, tool, or panel produces the figure the user is citing?
- How many of those land in files the agents actually touched? Get the changed-file list from
  `git diff --name-only` against the base and intersect it.
- Are they new, or pre-existing and newly surfaced? A build-configuration change can light up
  hundreds of findings that no agent introduced.

Record the real number and the real overlap. Every later finding is scored against it, and the
fix for "the agent skipped its own 3 warnings" is nothing like the fix for "a config change
surfaced 74 in untouched files".

## 2. Locate the instruction, verbatim

For each agent named in the critique, find the instruction and quote it with a path and line
number. This is the step people skip, and it is the step that most often ends the retro early.

Three outcomes, and they lead to completely different fixes:

- **Present as remembered** — proceed to Phase B.
- **Present but weaker than remembered** — the gap between what was written and what the author
  believed was written is itself a finding. It usually means the instruction reads as a
  suggestion, or sits somewhere it is skimmed past.
- **Absent** — there was no miss. There was a gap. The fix is to write the instruction, and
  there is nothing for that agent to introspect about.

Check the agent's `description` and scope sections too, not just the body. An agent whose
description says it is review-only will defer a fix-it instruction, correctly.

## 3. Recover the task packet

The agent file is only half of what the agent read. The other half is the prompt you sent, and
it is more salient — it is fresh, specific, and last. Pull it out of the transcript verbatim
for each implicated agent, along with what the agent returned.

Note specifically whether the packet carried the constraint, restated it, or silently relied
on the agent file to supply it.

---

# Phase B — Diagnose

## 4. Run the four-axis feasibility check first

Before asking any agent to introspect, determine whether the instruction was executable at all.
This ordering matters: an agent asked why it failed at something structurally impossible will
produce a fluent, plausible, entirely invented account of its reasoning. Rule out impossibility
with file reads, which are cheap and deterministic, and interview only where an actual choice
existed.

For each implicated agent resolve all four:

| Axis | What to resolve | Where |
|---|---|---|
| **Instruction** | Exact text, its position in the file, whether anything gates completion on it | the agent `.md` body |
| **Tools** | Is there a granted tool that reaches the surface the instruction names? | frontmatter tool list, MCP grants, harness defaults |
| **Model** | Which concrete model does `model-profile` resolve to, and how long is the body it must hold? | `model-profile` → `profiles/models.yml` |
| **Permissions** | Does any action the instruction requires resolve to `deny`? | `capability-profile` → `profiles/capabilities.yml` |

Then classify:

- **Impossible** — a required action is denied, or no tool reaches the named surface. The
  finding is structural. Skip the interview; there is nothing to learn from asking.
- **Possible but unsupported** — the agent could have complied, but nothing named the command,
  defined done, or blocked completion. Interview: the wording is what you are testing.
- **Fully supported** — it was reachable, checkable, and gated, and it still did not happen.
  Interview, and look hard at salience, position, and competing directives.

[references/mechanisms.md](references/mechanisms.md) catalogs the recurring mechanisms on each
axis with the fix each one takes. Read it before writing findings — naming the mechanism is
what makes a finding actionable, and "the agent did not follow instructions" is not a mechanism.

**A worked shape.** Three agents "missing the same instruction" routinely turn out to have
three different causes needing three different fixes: one was never given the instruction at
all; one was told to verify something its capability profile denies it the means to run, and
told to fix what its profile denies it write access to; one had the instruction, and the tools
and permissions for it, but it sat last in a seven-step workflow list read by a small model.
Add the instruction, resolve the contradiction, and re-position with a gate — respectively.
One shared remedy would have missed two of the three.

## 5. Interview the implicated agents

Spawn each implicated agent with its own `subagent_type`, so its real system prompt is in force
and it reads the instruction the way it read it during the task.

**Be clear with yourself about what this produces.** The instance you spawn is not the instance
that did the work and has no memory of it. It cannot recall a decision; it can only analyze how
its instructions parse against the task it was given. That is still worth a great deal — it is
the closest available read on how the wording actually lands — but treat the output as analysis,
not testimony. This is exactly why the brief hands it the artifacts: its own file, the verbatim
packet, its own output, and the ground truth. Without those it will confabulate.

Use the brief in [references/introspection-brief.md](references/introspection-brief.md). It
constrains the return to a mechanism at a file and line plus a counterfactual, and it forbids
apology, self-criticism, and fixing the code.

Run them in parallel — they are independent, and one agent's account should not colour another's.

## 6. Review your own delegation

You are the only participant with actual memory of the session, so this part is testimony and
carries more weight than anything the subagents return. Apply the same four axes to yourself:

- **Instruction** — did the packet carry the constraint, or did you assume the agent file would?
  Did the packet state an acceptance criterion, or only a goal?
- **Tools** — did you hand the reviewer the ground-truth signal, or only the diff? A reviewer
  told to check findings it cannot generate will review what it can see.
- **Model** — did you route a long, compliance-heavy instruction to a fast profile?
- **Permissions** — did you assign work to an agent whose profile denies what the work needs?

And the one that is usually the real answer: **did you accept "done" without the evidence?** If
the completion report said the work was clean and you did not ask what command produced that,
the verification gap is yours, and it is fixable in your own orchestration instructions.

---

# Phase C — Repair

## 7. Choose the cheapest fix that actually closes the mechanism

Climb this ladder and stop at the first rung that holds. Reaching for a higher rung than the
mechanism requires adds cost and side effects; stopping below it leaves the miss repeatable.

1. **Make it reachable** — replace a human surface ("the Problems tab") with the command or
   tool call that yields the same evidence to an agent.
2. **Make it checkable** — define done as an observable output, not a state of mind. "Zero
   warnings from `<command>`" instead of "address any findings".
3. **Make it a gate** — put it in the completion criteria so the path to "done" runs through it,
   rather than in a workflow list where it is one step among many.
4. **Move it** — position it where it is read while the plan is being formed, not after. A
   constraint at the end of a long file is read after the approach is already chosen.
5. **Grant the capability** — change the `capability-profile` or tool grants so the instruction
   becomes executable. Required whenever axis analysis returned *impossible*.
6. **Change the model profile** — a real fix when a long body must be held under compliance
   pressure, and an expensive one. Justify it with the body length and the position of the
   constraint, not with a hunch.
7. **Take it out of the prompt entirely** — if the check is mechanical and compliance genuinely
   matters, a build gate, analyzer setting, or CI step enforces it every time at zero context
   cost. `TreatWarningsAsErrors` never forgets and never runs out of context window. An
   instruction repeated to three agents because it keeps being missed is usually telling you it
   wants to be rung 7.

## 8. Write the diffs

Show each edit as before/after with its file and line. Hold every one of them to these:

- **One mechanism, one edit.** Do not answer a positional problem with stronger wording. If the
  constraint was buried, move it; adding emphasis to a buried line leaves it buried.
- **Prefer moving, merging, and deleting over appending.** Instruction files that only grow lose
  salience — every line added dilutes every line already there, and the retro that appends is
  the reason the file is long enough to hide things. Name what to cut alongside what to add.
- **No caps escalation.** `IMPORTANT: ALWAYS` works once. Applied twice, everything is important
  and the marker stops carrying information. If emphasis is the whole fix, the fix is a gate.
- **Verify the edit is executable.** Re-check the permission and tool axes against your new
  wording. Writing a second instruction the agent cannot follow is the failure mode of this
  entire exercise.
- **Sweep for contradiction.** Does the edit conflict with the agent's `description`, its scope
  section, or a "this supersedes all other instructions" block that outranks it?
- **Generalization test.** Would it have caught this miss, and does it still hold for a different
  task next week? An edit that only works for this incident is overfit — it will be dead weight
  in the file by the next one.

## 9. Report, then apply on approval

Lead with the findings table. Keep it short enough to read in one pass:

| # | Agent | Axis | Mechanism | Evidence | Counterfactual |
|---|-------|------|-----------|----------|----------------|

Every row needs `file:line` in Evidence, and a Counterfactual that names what specifically would
have produced a different outcome. A row that cannot fill both columns is a hypothesis — mark it
as one and say what would confirm it.

Follow with the proposed diffs grouped by file, then ask for approval before writing. These files
are contracts that shape every future run, which is why the edits are worth a moment of review
even when they look obvious.

After applying, run whatever gates cover the files you touched — the repository's `AGENTS.md` lists them.
If an agent file has a vendored copy under `apm_modules/`, that is a dependency mirror; edit the
source and let the dependency update carry it.

## Anti-patterns

These are the shapes a retro collapses into when it stops being useful:

- **Collecting confessions.** Agent accounts that agree with each other and locate nothing at a
  file and line are narrative, not diagnosis. You get these by interviewing before the
  feasibility check.
- **The single root cause.** Several agents missing the same thing usually means several
  mechanisms, because they have different profiles, bodies, and packets.
- **Blaming the model.** Sometimes true, rung 6, needs evidence. Reached for first, it ends
  every retro with nothing edited.
- **Fixing the code.** The 77 findings still need fixing. That is a separate task and does not
  belong in this retro — running it here buries the instruction work under the cleanup.
