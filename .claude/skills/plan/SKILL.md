---
name: plan
description: Project-level planning — multi-phase roadmap, dependency analysis, sprint integration
---

# Plan Skill

**All reports, questions, and approval requests must be in Korean.**

When the user inputs `/plan`, collect the goal and create a project-level execution plan.

> This skill is for **multi-phase planning** (orchestrating multiple `/dev` tasks into a cohesive roadmap).
> For single-task planning, `/dev` Step 2 is sufficient.

---

## Step 0: Context Scan (automatic, no approval needed)

Quickly assess the current situation. **Target: complete within 30 seconds.**

### 0-1. Current Sprint Check
- Read the Current Sprint section from `CLAUDE.md`
- Identify in-progress, completed, and pending Phases

### 0-2. Project Structure Scan
- Read target repository README (project overview, tech stack)
- Quick scan of relevant `.claude/specs/` (select based on goal)
- Check solution structure (`dotnet sln list` or `.sln` file)

### 0-3. Git Status
- Current branch, uncommitted changes
- Last 5 commits (understand recent work flow)

### 0-4. Status Report

```
[Sprint] {current sprint goal} / Phase {N}/{Total} in progress
[Project] {target project} / {key tech stack}
[Git] {branch} / uncommitted: {yes/no}
```

---

## Step 1: Goal Collection

Ask the user: **"What goal should we plan for?"**

Analyze the user's response to:
1. **Goal summary** — 1-2 lines
2. **Scope assessment** — single repo vs multi-repo, affected layers (SDK/Infra/App/UI)
3. **Relationship to current sprint** — new sprint or add to current sprint

> If the user already provided the goal with the command (`/plan MotionCalculator migration`), proceed without additional questions.

---

## Step 2: Deep Analysis (automatic, no approval needed)

Deeply analyze code and specs related to the goal.

### 2-1. Spec Reading
Read spec documents relevant to the goal:

| Related Area | Specs to Read |
|-------------|---------------|
| Architecture | `ARCHITECTURE.md` |
| Plugin | `PLUGIN-SYSTEM.md`, `SDK-INTERFACES.md` |
| SDK | `SDK-INTERFACES.md` |
| UI | `DESIGN-SYSTEM.md`, `I18N.md` |

### 2-2. Codebase Analysis
- Search for files/classes that are change targets (Grep, Glob)
- Map dependency graph (who references whom)
- Check for existing patterns (similar implementations)

### 2-3. Risk Identification
- Backward compatibility impact
- Build dependency chain
- Technical debt: sync-over-async, memory leaks, etc.

---

## Step 3: Plan Proposal → **Wait for approval**

Propose an execution plan based on the analysis results.

### Plan Structure

```markdown
# Plan: {goal title}

## Goal
{1-2 line goal description}

## Phases

| Phase | Task | Skill | Dependencies | Est. Files |
|-------|------|-------|--------------|------------|
| 1 | ... | `/dev` | - | N |
| 2 | ... | `/service-dev` | Phase 1 | N |
| 3 | ... | `/dev` | Phase 1, 2 | N |

## Phase Details

### Phase 1: {title}
- **What**: {change summary}
- **Where**: {target files/projects}
- **Why**: {reason this Phase must come first}
- **Risk**: {risk factors}

### Phase 2: ...

## Dependency Graph
{ASCII visualization of Phase dependencies}

## Out of Scope
{intentionally excluded items and reasons}

## Verification
{verification method after full completion}
```

### Planning Principles
- **Minimum Phase count** — no excessive decomposition. Merge Phases that cannot be executed independently
- **One skill per Phase** — each Phase must be executable with a single `/dev`, `/service-dev`, or `/ui-dev` invocation
- **Explicit dependencies** — clearly state why Phase ordering is required
- **Mark parallelizable Phases** — indicate which Phases can run independently
- **Over-engineering warning** — if Phases exceed 6, reconsider scope

---

## Step 4: Sprint Integration → **Wait for approval**

Reflect the approved plan into `CLAUDE.md`.

### 4-1. Sprint Update
Ask the user:

| Option | Action |
|--------|--------|
| **Add to current sprint** | Append new Phases to existing Phase table |
| **Replace with new sprint** | Replace entire Current Sprint section |
| **Save only (no sprint update)** | Save plan file to `.claude/plans/` only |

### 4-2. Update CLAUDE.md
Update Current Sprint section based on approved approach:

```markdown
# Current Sprint (yyyy-MM-dd ~ MM-dd)

**Goal: {goal}**

| Phase | Task | Status |
|-------|------|--------|
| 1 | {task} | Pending |
| 2 | {task} | Pending |
| ...

**Principles**:
- {goal-appropriate principles}
```

---

## Important Notes

- **No code modifications** — this skill only creates plans. Execution uses `/dev` or other skills
- **No over-engineering** — do not include "might be needed" Phases. Only include what is certain
- **Based on actual codebase** — plan from real code analysis, not assumptions or guesses
- **Reflect sprint principles** — carry forward principles from CLAUDE.md's current sprint
- **Fast analysis** — Steps 0-2 proceed automatically, minimize user wait time
