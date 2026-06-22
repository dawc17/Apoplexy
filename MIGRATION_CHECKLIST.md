# Unity Migration Checklist

This tracks feature parity with the archived raylib prototype at the
`raylib-prorotype-final` tag. An item is complete only when it works in a Unity
play-mode build, not merely when its files or code have been copied.

## Project foundation

- [x] Create the Unity project on the `unity-migration` branch
- [x] Configure Git ignores, Git LFS, visible `.meta` files, and text serialization
- [x] Import the legacy models, textures, audio, fonts, shaders, and level JSON
- [ ] Organize runtime code under `Assets/Apoplexy`
- [ ] Create gameplay, UI, audio, and editor assembly definitions
- [ ] Define project layers, tags, collision matrix, and input actions
- [ ] Create a reusable bootstrap/gameplay scene
- [ ] Verify a clean clone opens and imports without missing references

## Player controller

- [ ] First-person mouse look with pitch limits
- [ ] Grounded movement and collision
- [ ] Gravity and vertical movement
- [ ] Smooth acceleration and deceleration
- [ ] Sprinting, including sprint FOV
- [ ] Crouching, including camera-height transition
- [ ] Walking, sprinting, and crouching head bob
- [ ] Player health, damage, death, and reset
- [ ] Damage vignette and camera shake

## Levels and environment

- [ ] Decide whether to import legacy level JSON or rebuild levels as Unity scenes
- [ ] Recreate the Killhouse test arena
- [ ] Recreate the Stealth Compound
- [ ] Replace the custom level editor with Unity editor tooling
- [ ] Configure world collision and spawn points
- [ ] Configure ambient, directional, and point lighting
- [ ] Port skybox support
- [ ] Port wall decals and animated decal support where still useful

## Weapons and combat

- [ ] Define weapon data as ScriptableObjects
- [ ] Implement weapon inventory and slot switching
- [ ] Import and configure the silenced pistol
- [ ] Import and configure the shotgun
- [ ] Hitscan shooting with level and enemy collision
- [ ] Weapon spread, movement penalties, bloom, and recovery
- [ ] Magazine and reserve ammunition
- [ ] Full-magazine reloads
- [ ] Shell-by-shell shotgun reloads
- [ ] Reload interruption and weapon-switch cancellation rules
- [ ] Weapon melee attack and hit window
- [ ] Shotgun reload-spin melee behavior
- [ ] Impact, damage, knockback, and death handling

## Viewmodel and procedural motion

- [ ] First-person weapon rendering
- [ ] Idle and walking bob
- [ ] Mouse sway
- [ ] Sprint pose
- [ ] Recoil and recovery curves
- [ ] Reload motion
- [ ] Weapon-switch motion
- [ ] Melee motion
- [ ] Muzzle flash and dynamic muzzle light
- [ ] Replace the old viewmodel debug panel with Unity editor controls

## Enemy presentation

- [ ] Configure the soldier model, materials, scale, and orientation
- [ ] Configure the soldier rig and humanoid avatar
- [ ] Add an idle animation
- [ ] Add locomotion animation and velocity-driven blending
- [ ] Add alert, attack, hit-reaction, and death presentation
- [ ] Keep visual bounds and gameplay colliders aligned

## Enemy AI

- [ ] Bake and validate NavMeshes for gameplay levels
- [ ] Implement enemy health and damage reactions
- [ ] Implement field-of-view, range, and occlusion checks
- [ ] Implement gradual suspicion and confirmed detection
- [ ] Implement noise events with radius and wall obstruction
- [ ] Implement idle, suspicious, alert, chase, search, and attack states
- [ ] Track last-seen and last-heard positions separately
- [ ] Investigate noise without receiving the player's exact position
- [ ] Search multiple nearby locations after losing the player
- [ ] Return to a post or patrol after an unsuccessful search
- [ ] Implement attack windup, recovery, range, and cooldown
- [ ] Prevent enemy crowding and overlapping
- [ ] Add AI debug visualization for vision, hearing, paths, and state

## Audio

- [ ] Music playback and lifecycle
- [ ] Spatial enemy and weapon audio
- [ ] Randomized player footsteps
- [ ] Movement-dependent footstep volume, pitch, and noise radius
- [ ] Pistol, shotgun, reload, switch, hit, and hurt sounds
- [ ] Audio listener follows the player camera

## Effects and rendering

- [ ] Recreate the low-resolution PSX render path in URP
- [ ] Port color quantization, dithering, scanlines, vignette, and noise
- [ ] Port chromatic offset and horizontal jitter
- [ ] Recreate lit world materials without breaking imported textures
- [ ] Enemy hit particles
- [ ] Enemy death particles
- [ ] Crouch and damage vignettes
- [ ] Validate performance in a player build

## UI and game flow

- [ ] Recreate the terminal HUD style
- [ ] Health display
- [ ] Ammunition, reload, and weapon-slot display
- [ ] Dynamic crosshair and spread feedback
- [ ] Enemy-awareness indicator
- [ ] Main menu and mission selection
- [ ] Playing, win-sequence, win, and dead states
- [ ] Restart flow
- [ ] Win condition when all required targets are eliminated
- [ ] Debug overlay

## Parity milestone

- [ ] Complete one playable Killhouse run from menu to win/death
- [ ] Complete one playable Stealth Compound run from menu to win/death
- [ ] Compare movement, weapon timing, enemy perception, and audio with the archived build
- [ ] Produce Windows and Linux development builds
- [ ] Merge `unity-migration` into `main`

## Post-migration gameplay roadmap

These are planned improvements, not requirements for raylib parity.

- [ ] Leaning around cover
- [ ] Silent rear takedowns
- [ ] Light- and exposure-aware enemy vision
- [ ] Better investigation and coordinated enemy behavior
- [ ] Alternative infiltration and extraction objectives
- [ ] Hit reactions
- [ ] Second enemy type
- [ ] Health and ammunition pickups
