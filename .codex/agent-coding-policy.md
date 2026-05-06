# Agent Coding Policy

This note is for Codex working style inside this workspace. It is not project documentation.

## Priority

- Minimize the number of classes and files when possible.
- Optimize for low reading fatigue over textbook decomposition.
- Prefer keeping related logic in the caller when the extracted method would be used only once.
- Extract a private method only when the role is clearly different, the boundary is meaningful, or reuse is likely.
- Latest direct user instruction overrides this note.

## Refactoring Bias

- Do not split code into many tiny controller, presenter, executor, or helper classes just to satisfy abstract cleanliness.
- Prefer one coherent class with a readable flow over several thin classes that require file-hopping.
- Prefer a slightly longer method over multiple one-off private methods when the logic belongs to one continuous action.
- Split files only when the separation is durable and easy to justify.
- When constants or enums are truly shared building blocks, prefer one reusable common definition instead of redefining similar values in multiple classes.
- For shared enums and constants, optimize for low management cost: prefer a dedicated common location and reuse it from callers.

## Good Reasons To Split

- The new type is reused by multiple features or scenes.
- The new type becomes a clear source of truth for persistent data or master data.
- Unity serialization, inspector wiring, or lifecycle handling clearly benefits from a dedicated component.
- Pure logic becomes meaningfully testable and independent after extraction.
- One class currently mixes obviously different concerns and the separation reduces confusion more than it increases navigation cost.

## BattleScene-Specific Guidance

- Keep `BattleScene` as the main orchestration class unless a separation is clearly worth the extra file.
- Prefer consolidating scene-only flow inside `BattleScene` rather than creating one-use helper classes.
- Keep runtime source-of-truth data in `UserData`, `StageData`, or existing services when appropriate.
- Avoid introducing new files for presentation details that are only touched from `BattleScene`, unless the boundary is already established and repeatedly useful.
- Before adding a new class or extracting a one-use method, ask whether it truly reduces reading effort for a future pass through the code.

## Practical Check Before Refactoring

Ask these in order:

1. Can this stay in the current class without making the flow confusing?
2. If extracted, will it reduce reading effort more than it increases navigation?
3. Is the extracted unit reused, or does it represent a genuinely distinct responsibility?
4. If not, keep it local and avoid adding a file.
