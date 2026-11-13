# CoSoB2 — four-wing campus

Open **UEH → CoSoB2 → Open four-wing campus** or `Scenes/CoSoB2_Campus.unity`, then press Play.

The inspector panel provides overview, courtyard, glass bridge, floor isolation, wall cutaway and room selection. Hold right mouse + WASD to fly; Q/E changes height; Shift speeds up. This is a free camera, not a collision-based walking controller.

Model: `Prefabs/CoSoB2.prefab`. Rooms contain `B2CampusRoom` metadata; storeys contain `B2CampusFloor` components. Three new wings have ground + levels 1–6. The old yellow connector has three elevations, named ground / floor 2 / floor 3 as described by the user. Its top-floor use is intentionally unknown.

See [layout, assumptions and source notes](../../Docs/CoSoB2Campus/Layout.vi.md). Previous B1, GameWorld and one-wing B2 scenes are preserved. The new campus has its own folder, materials, mesh assets and preview scene.
