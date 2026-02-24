---
name: code-flow-visual-report
description: Generates an interactive web-style report that analyzes source code, diagrams program flow, simulates step-by-step execution, and highlights likely logic-bug locations with explanations. Supports localized output (including a Korean-language version) when a user requests a localized interactive report. Triggered when a user asks for a visual, navigable report of a codebase or code snippet for learning, documentation, or debugging.
---
# Skill purpose

This Skill examines provided source code (single file or small multi-file module), and produces a concise human-readable summary plus an interactive web-style report that includes: a high-level summary, program flow diagrams, expandable/interactive step-by-step execution simulation, candidate logic-bug locations with root-cause hypotheses, and recommendations to fix or harden the code. It supports two user personas: Basic (learning and overview) and Advanced (detailed debugging and root-cause analysis). The Skill can produce localized reports (default English). If the user requests a Korean-language report, the Skill will produce translated UI text, section headings, and localized explanations while preserving code excerpts and technical terms as appropriate.

# Step-by-step instructions

1. Intake and scope
   - If user input is incomplete, ask for language, runtime, entry point(s), sample inputs, desired localization (default: English; explicit option: Korean), and whether to render Basic or Advanced mode (default: combined Basic+Advanced behavior).
   - Accept code as inline text, files, or a pointer to a repository subset. For large repos, request specific files or modules.
   - If the user requests a Korean report, confirm whether they want only headings/UI translated or all narrative explanations and recommendations translated into Korean.

2. Static analysis and parsing
   - Parse the code (use language-appropriate heuristics if full parser is unavailable). Extract functions, classes, modules, globals, imports, and public APIs.
   - Identify control-flow constructs (conditionals, loops, branches, exceptions, async/event handlers) and main data-flow (key variables, state mutations, I/O points).
   - Annotate call graph edges and potential side effects (I/O, network, filesystem, database, external APIs).

3. Behavioral modeling and flow diagrams
   - Build a high-level flow diagram: entry point → major components → data/control transitions. Represent branching and loops succinctly.
   - For Advanced mode, generate more granular subgraphs for complex functions or modules.
   - Produce textual labels for each node explaining purpose, inputs, outputs, and estimated complexity. If localization is requested, translate node labels and textual labels into the requested language, keeping code identifiers untranslated.

4. Interactive step-by-step execution simulation
   - Create a sequence of execution steps for a representative input (use sample input if provided; otherwise synthesize a simple canonical input). For each step include: executed statement/function, changed state (variables and values), and explanation of why the step matters.
   - Mark branching decisions and show alternative paths where applicable. For asynchronous or event-driven code show timeline or event sequence.
   - Localize step descriptions and explanations when the user requests a translated report; retain code, variable names, and literal values as-is.

5. Logic-bug detection and hypothesis generation (Advanced)
   - Use heuristics to find suspicious patterns: unhandled exceptions, off-by-one loops, unchecked null/None values, race conditions, inconsistent state updates, unreachable code, duplicated logic, and ambiguous type conversions.
   - For each candidate bug location produce: code excerpt, why it is suspicious, concrete example input that would trigger the issue (if possible), and ranked confidence level (high/medium/low). Localize the narrative explanation if requested.

6. Remediation guidance and tests
   - For each bug candidate propose a specific fix or mitigation, including minimal code change examples or assertions to add.
   - Suggest targeted unit tests or property-based tests with example inputs to reproduce and verify fixes.
   - Translate recommendations and test descriptions when localization is requested; include code diffs and test code unchanged.

7. Report generation and UX considerations
   - Structure the report as an interactive web-style document: Summary landing page, Flow diagrams (clickable nodes), Step-by-step simulator (expandable steps), Bug candidates panel (click to reveal evidence and proposed fixes), and Appendix with full annotated code.
   - Output a JSON or markdown representation that maps to UI components: sections, nodes (with ids), step sequences, highlighted ranges (file, line start, line end), and suggested code diffs.
   - Provide an exportable lightweight HTML+JS scaffold (or instructions) that can render the interactive report using common libraries (D3/Graphviz for diagrams, collapsible panels, syntax-highlighted code blocks). When localization is requested, include localized UI strings and labels in the scaffold (resource dictionary or i18n map), e.g., an object mapping keys to Korean text.

8. Localization specifics
   - Confirm desired localization target (English by default; supported: Korean).
   - Create a resource map for UI labels and section headings. For Korean requests, produce translated strings for headings, button labels, panel titles, and narrative paragraphs. Keep code snippets, file paths, and identifiers untranslated.
   - When translating technical recommendations, preserve technical terms (e.g., function names, class names, error codes) in their original form and translate explanatory context.
   - If the user requests bilingual output, include both English and Korean versions of each narrative section and a toggle in the scaffold resource map.

9. Deliverables
   - Primary deliverable: a structured report payload (markdown + JSON metadata) that can be rendered into a web page, with localization applied as requested.
   - Optionally, include an HTML scaffold sample and minimal JS to render diagrams and expand/collapse steps when requested. The scaffold should reference a small i18n dictionary for localized UI strings when localization is used.

# Usage examples

- Example 1 (Basic learning, English): "Analyze this Python file and produce a Basic web report that explains the flow and shows an interactive step-by-step simulation for input x=5."
- Example 2 (Advanced debugging, English): "Inspect these three JS files, find likely logic-bug locations, give confidence levels and minimal code fixes, and output a JSON report for the web UI."
- Example 3 (Documentation, Korean): "Please produce a Korean interactive web report for this module so new engineers who read Korean can understand entry points and state transitions." (Skill should confirm whether full narrative translation or just headings/UI is desired.)

# Best practices

- Ask clarifying questions when files, entry points, or sample inputs are missing.
- Prefer concrete examples: provide sample inputs to create realistic execution simulations.
- For large codebases, scope the analysis to relevant modules or request a focused diff.
- Make the report modular: separate summary, diagrams, simulator, bug candidates, and annotated source so UI can render partial views.
- When producing fixes, include minimal reproducible test cases and clearly label assumptions.
- For localization, explicitly confirm whether translation should be literal or adapted for clarity in the target language; prefer clear, concise translations for educational text.

# Output formats and links

- Primary formats: structured JSON metadata (sections, nodes, steps, highlights), Markdown report, and optional HTML+JS scaffold for interactive rendering.
- When requested, include an example scaffold: ./scaffold/report.html and ./scaffold/report.js. If localization is requested, include a small i18n resource file such as ./scaffold/i18n.json mapping keys to localized Korean strings.

# When to ask follow-ups

- If language/runtime is unknown.
- If no entry point or sample input is provided and simulation is requested.
- If repository size prevents whole-project analysis.

# Example minimal JSON shape (for the UI)

A sample output shape the Skill should produce (brief):

{
  "summary": "...",
  "files": [{"path":"src/main.py","content":"...","highlights":[{"start":10,"end":20,"id":"h1"}]}],
  "flow_graph": {"nodes":[{"id":"n1","label":"main()"}],"edges":[["n1","n2"]]},
  "steps": [{"id":"s1","desc":"call init","highlights":["h1"]}],
  "bug_candidates": [{"file":"src/main.py","range":{"start":50,"end":58},"issue":"possible None deref","confidence":"medium","suggestion":"add null check"}]
}

supporting_files: []
