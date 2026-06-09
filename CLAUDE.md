# Farmhollow — Project context for Claude

## What we're building
A **cozy 3D farming / life-simulation game** in the style of *Animal Crossing*.
Long-term goal: **release on Steam**.

## Language convention
- **All project content is English**: code, comments, file names, asset/GameObject names, and the in-game default language.
- **In-game text is localized, never hardcoded** (see "Localization" below). Default language: English.
- **The chat language with the user is German** — Claude replies to the user in German. (German = how I talk to the user; English = what goes into the project. Don't confuse the two.)

## About the user
- **Programming beginner** — explain everything step by step.
- Wants Claude to **do as much as possible** (that's why the Unity MCP bridge is set up).
- Division of labor: **Claude writes code & builds the scene (via MCP)**, the user only does what's strictly necessary (manual clicks, asset downloads).

## Tech setup (already done)
- **Unity 6.4** (6000.x), template **Universal 3D (URP)**, project at `C:\Users\tobia\farmhollow`.
- **Active Input Handling** is set to **"Both"** (we use the classic `Input.GetAxis`).
- Installed: **Python 3.12.13**, **uv 0.11.19**, **Git 2.54.0** (all on PATH). Note: in the Bash tool `python`/`py` are not found — use the PowerShell tool for Python if needed.
- **Unity MCP (CoplayDev/unity-mcp)** package installed (`com.coplaydev.unity-mcp`), configured for **Claude Code**. Server entry in `~/.claude.json` under the project: `UnityMCP` → `type: http`, `url: http://127.0.0.1:8080/mcp`.
  ⚠️ The MCP server must be running in Unity (Window → MCP for Unity → Start Server), and Claude Code must be started **from the project folder**. If tools don't appear after starting the server, run `/mcp` to reconnect.

## Art style
- **Low-poly** (like *A Short Hike* / *Alba*) — forgiving for free assets, runs smoothly, looks coherent.
- Camera: **angled top-down** (cozy), character walks around freely (no first-person).
- Free asset sources: **Mixamo** (character + animations), **Quaternius** & **Kenney** (nature/items, CC0), **Poly.pizza**, **Sketchfab** (CC0).

## Code conventions
- All gameplay scripts live in the **`Farmhollow` namespace** (avoids type-name clashes with Unity package examples — we already hit one with `PlayerController`).
- Scripts are in `Assets/Scripts/`.

## Already written
- `Assets/Scripts/PlayerController.cs` — walk controller (WASD, turns toward movement direction, gravity). Needs a `CharacterController` (added automatically via `[RequireComponent]`). Movement in world coordinates.
- `Assets/Scripts/LocalizationManager.cs` — see Localization below.

## Localization
- Locale files: `Assets/Resources/Localization/<code>.json` — currently `en-US.json` (default) and `de-DE.json`.
- JSON shape: `{ "language": "...", "entries": [ { "key": "ui.quit", "value": "Quit" }, ... ] }` (array-of-pairs so Unity's built-in `JsonUtility` can parse it without extra packages).
- Usage: `LocalizationManager.Get("ui.quit")` returns text for the active language; `LocalizationManager.SetLanguage("de-DE")` switches at runtime. Keep both JSON files in sync (same keys). Key prefixes so far: `ui.*`, `error.*`.

## Current scene (SampleScene)
- `Ground` (Plane, scale 5/1/5), `Player` (Capsule at 0/1/0 with `PlayerController` + `CharacterController`), `Main Camera` at (0,10,-10) rotated 45/0/0, plus the default `Directional Light` and `Global Volume`.

## Roadmap — Stage 1 (first playable mini-game)
1. ✅ PlayerController script written
2. ✅ Scene: Ground, Player, angled camera
3. ✅ Test: character walks with WASD → next: camera follow
4. ⬜ Real character (Mixamo) instead of the placeholder capsule; build/import assets (character, animals, map)
5. ⬜ Plantable ground (grid), seed → grows → harvestable
6. ⬜ Inventory + picking things up
7. ⬜ Day/night cycle
8. ⬜ Selling & money
9. ⬜ Build as .exe

## Next step
Camera follow + start building/importing assets (low-poly placeholders procedurally via MCP, and/or guided downloads of free CC0 packs and a Mixamo character).
