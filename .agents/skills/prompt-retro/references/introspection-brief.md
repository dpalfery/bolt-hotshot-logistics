# Introspection brief

The template for directing an implicated agent to analyze its own instructions. Spawn it with
its own `subagent_type` so its real system prompt is loaded and it reads the instruction the way
it read it during the task.

## What the agent can and cannot tell you

It has no memory of the original run. It is not recalling why it decided something — it is
analyzing how its standing instructions parse against a task it is being shown. That is
genuinely useful, and it is the only direct read available on how the wording lands, but it is
analysis rather than testimony.

Everything in the brief follows from that. It gets its own file, the verbatim packet, its own
output, and the measured ground truth, because an agent asked "why did you miss this?" with
nothing attached will produce a confident and entirely invented account. Give it artifacts and
it has something real to reason against.

## Template

Fill every placeholder. An omitted section is a section the agent will invent.

```
This is an instruction-design review, not a work task. Nothing you say here will be used to
judge your performance — the output is an edit to your own instruction file, and its quality
depends on your analysis being blunt.

## What happened
<the user's critique, verbatim>

## Ground truth
<the measured figure, the command that produced it, and how much of it falls in files the
task touched — from Phase A step 1>

## Your instructions
Read <path to your agent file> in full before answering. That is the file under review.
Quote the lines you refer to with their line numbers.

## The task you were given
<the task packet, verbatim from the transcript — do not summarize it>

## What was returned
<the agent's completion report, verbatim>

## Structural findings already established
<the four-axis verdict: which model profile you resolve to, which capabilities your profile
allows and denies, which tools you were granted, and whether the instruction was reachable
at all — from Phase B step 4>

## What to return

1. **Mechanism.** How does this instruction fail to produce the behavior it asks for? Locate it
   at a file and line. If the instruction is sound and the failure is elsewhere — the task
   packet, your permissions, another instruction that outranks it — say that instead, and name
   where.
2. **Reading.** How does the instruction parse against this specific task? If it is ambiguous,
   give both readings and say which one this task selects. If it names something you have no
   tool or permission to reach, say so directly.
3. **Competition.** What else in your instructions competes with it for attention — a superseding
   block, a scope boundary, a longer list it is buried in, a directive that arrives later?
4. **Counterfactual.** What specific change would have produced the right behavior? Wording,
   position, a gate in the completion criteria, a named command, a tool grant, a permission.
   Write the replacement text.
5. **Cost.** What would your proposed change break or slow down if it applied to every task you
   receive, not just this one?

## Constraints

- Do not fix the underlying defect. Do not edit code, and do not edit your instruction file —
  propose the text and stop.
- Do not apologize, and do not commit to doing better. Neither survives this session; only a file
  edit reaches the next run, which is the entire point of the exercise.
- If you cannot reconstruct something from the artifacts above, say "cannot reconstruct" and
  name what would settle it. A stated gap is worth more than a plausible account.
- Answer in under 400 words. Length here is a symptom of guessing.
```

## Running the interviews

Run them in parallel and independently. One agent's account should not colour another's, and
independent agreement on a mechanism is evidence — agreement produced by sharing context is not.

Interview only agents the feasibility check marked *possible but unsupported* or *fully
supported*. Where the verdict was *impossible*, the finding is already established and the
interview will only produce rationalization around a settled fact.

## Grading what comes back

Take the strong returns into findings, and treat weak ones as signal about the retro rather than
about the agent:

**Strong** — quotes its own file with line numbers, names one mechanism, proposes replacement
text, and states a cost. Best case, it contradicts your hypothesis and points at the packet or
the permissions; that is the interview earning its cost.

**Weak** — restates the instruction and agrees it should have followed it. Almost always means
the brief was thin: a missing packet, missing ground truth, or a missing structural verdict.
Re-send with the gap filled rather than accepting it.

**Discard** — apology, resolutions, or a mechanism with no file and line behind it. Contributes
nothing to a diff.

**Watch for** — several agents returning the same mechanism in similar words. That is either a
real shared cause or contamination from over-specified briefs. Check whether the mechanism is
actually true of each agent's file independently; often it is true of one and borrowed by the
rest.
