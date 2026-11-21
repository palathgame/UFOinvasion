# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

"Invasion" — a 2D side-scrolling mobile shoot-'em-up (Android + iOS) built with **Unity 6000.3.6f1** (see `ProjectSettings/ProjectVersion.txt`). The player's plane sits on the left and shoots enemies spawning from the right. Monetized with Google Mobile Ads (AdMob); online leaderboard via dreamlo.

## Building / running

- There is no command-line build script and no test assemblies (the Test Framework package is installed but unused). Compilation and play-testing happen in the Unity Editor; use the `unity-cli` / `unity-pipeline` skills to compile, enter Play Mode, or build from the shell.
- Build scenes, in order: `Assets/Scenes/StartPage.unity` (menu, index 0) → `Assets/Scenes/Game.unity`. Scenes are loaded by name via `SceneManager.LoadScene("StartPage" | "Game")`.
- `Assets/post-build.bash` is a Unity Cloud Build post-build hook that uploads the iOS `.ipa` to App Store Connect (`xcrun altool`). `iosbuil/` is a local Xcode export, not source.
- Generated `.csproj`/`.sln`, `Library/`, `Temp/`, `obj/` are Unity-generated — don't edit them.

## Code layout

All game code is in the default `Assembly-CSharp` (no custom asmdefs). Comments and some identifiers are in Czech/Slovak (e.g. `pozadi` = background, `snih` = snow, `Nenaslo Hudbu` = "music not found"); keep existing misspelled names (`DestoyShip`, `HeatlhBar`, `bosShip`, `chanceToBoos`) since prefabs/scenes reference them by serialized name.

- `Assets/Scripts/` — in-game (Game scene) logic.
- `Assets/ScriptIntroducionMenu/` — StartPage menu: difficulty, skins shop, achievements, settings, leaderboard display.
- `Assets/dreamlo/` — third-party dreamlo leaderboard sample code.
- `Assets/GoogleMobileAds/`, `Assets/ExternalDependencyManager/`, `Assets/Plugins/` — vendored SDKs; don't modify unless upgrading.

## Architecture

**Ships/combat (inheritance + component pairs):**
- `EnemyShip` is the base for anything with HP: `hp`, `HeatlhBar`, `TakeDamage`, `checkHP`, virtual `DestoyShip` (which also calls `InGameManager.plusKills()`). `PlaneShip` (player) and `EnemyShipBoss` derive from it. Note `PlaneShip.TakeDamage` uses `new` (hides, not overrides) to apply shield immunity — callers must hold a `PlaneShip` reference for the shield to apply.
- Firing is a separate `Attack` component (fire rate + timed speed/damage buffs, plays `attackName` sound). Bosses have their own `AttackBoss1..6` variants. Projectiles are `Shoot`, `ShootSinusoid`, `ShotWave`, `Laser`, etc.
- Movement scripts are separate components too (`EnemyShipMove`, `KamikazeShipMove`, `EnemyShipBossMoveStay`, `aimMob`).
- Player pickups are handled in `PlaneShip.OnTriggerEnter2D` by **tag** (`SpeedBuff`, `DmgBuff`, `HealBuff`, `HealBuff1`, `ShieldBuff`, `Coin`); each buff prefab carries `BuffScript` plus a stat component.

**Game loop (Game scene):**
- `InGameManager.startGame()` instantiates the player from `Resources/Plane/Plane{AktualSkin}.prefab` — skins are indexed prefabs loaded by path, so a new skin needs a matching `PlaneN` prefab and an entry in `Skins.cs`.
- `EnemyManager` (FixedUpdate) drives spawning: parallel arrays `enemyShip[]`, `spawnTime[]`, `addDMG[]` (Inspector) and the hardcoded `innerSpawnTime[]` must all stay the same length (currently 9). Enemy types unlock by `Score.scoreValue` thresholds (800 / 1550 / 2700). Each spawn raises a boss chance; when a boss spawns, normal spawning pauses until the boss calls `setFalseActiveBoss()`. `bosShip[]` is picked with `Random.Range(0, 8)`, so it must have 8 entries. Difficulty (`PlayerPrefs "setDific"`: "0"/"1"/"2") changes spawn rate, boss chance and boss bonus score.
- Score from kills goes through `EnemyShip.ComboScore()` → `InGameManager.RegisterComboKill()` (kills within 2 s chain a combo: 5+ = x2, 10+ = x3). `InGameManager` also tracks per-game coins/bosses for the end-of-game summary in `ResultScore`.
- `GameHud` (created at runtime by `InGameManager.Start`, nothing in the scene) draws the boss warning, boss HP bar, combo and active-buff timers; `FloatingLabel` puts world-space text over power-ups. Juice helpers: `HitFlash` (swaps sprites to `Resources/flash.mat`, skips `Shield`), `CameraShake`, `Haptics` — all invoked statically.
- Enemy drops go through `BuffDrops.Roll()` (coin odds 1:45 / 1:40 / 1:30 by combo multiplier; also spawns the `MagnetBuff`, which is built at runtime from the shield prefab). End of game: 1 coin per 1000 points (`InGameManager.AwardScoreCoins`), optional rewarded-ad x2 coins button created by `ResultScore`.
- Skin bonuses (HP / fire rate / damage per skin index) live in `SkinBonus`, applied in `PlaneShip.Start`; heal cap is `MaxHp`, not 100.
- `GameHud.StartCountdown()` freezes the game (`Time.timeScale = 0`) and runs 3-2-1 — used after unpause, after rewarded continue and after the first-run `TutorialOverlay`. `DailyReward.CheckAndShow()` runs from `StartPageManager.Start`.
- Ads must wait for consent: never call `MobileAds.Initialize` directly — use `AdsConsent.WhenAdsReady(...)` (Google UMP form where required, then a single AdMob init). Privacy options button lives in the menu's "?" panel.
- `Tools > Store Media > Capture Screenshots + Video` (editor-only; `Assets/StoreMedia/Editor` + `StoreMediaAutopilot`, video via built-in `UnityEditor.Media.MediaEncoder`, no Recorder package) plays the game on autopilot and writes screenshots + `trailer.mp4` to `StoreMedia/` (git-ignored); player PlayerPrefs are backed up and restored.
- Physics objects push themselves with `this.Push(force)` / `this.OwnBody()` (`Rigidbody2DExtensions`) — never `FindFirstObjectByType<Rigidbody2D>()`, which grabs an arbitrary body in the scene.
- Keep one `MonoBehaviour` per file named after the class (player builds can fail otherwise).
- `Score.scoreValue` is a global static; `EnemyPoints` supplies spawn transforms; `Paralaxer` runs the scrolling backgrounds and day/night/snow cycle (textures in `Resources/`).
- On player death `PlaneShip.DestoyShip` shows an interstitial via `AdIns.ShowInterstitialCapped()` (every 3rd death, counter in `PlayerPrefs` int `AdDeathCounter`) (`AdIns` found by GameObject name `"AdIns"`) and instantiates the back-menu/results UI (`BackMenu`, `ResultScore`, which writes stats/achievements and uploads the score via `HighscoresUpload`).

**Cross-cutting:**
- `AudioManager` is a `DontDestroyOnLoad` singleton; sounds are looked up by string name (`Play("BosSound")`, `"BackGround"`, `"PlayerExplosion"`, and each `Attack.attackName`) and configured in the Inspector `sounds[]`. Most code reaches it via `FindAnyObjectByType<AudioManager>()`.
- Objects are frequently located by hierarchy path / name strings (e.g. `GameObject.Find("Main Camera/HpCanvas/HpBar")`) — renaming scene objects breaks these.
- **All persistence is `PlayerPrefs`, and numbers are stored as strings** (`GetString` + `long.Parse`, empty string = 0). Keys: `Coins`, `Skins`, `AktualSkin`, `HighScore`, `Games`, `KillAliens`, `KillAliensOneGame`, `BossKilled`, `BossKilled2`, `BossKilledSpecies`, `survivedDays`, `Nick`, `setDific`, `Direction`, `SoundActive`, `MusicActive` (`"1"` means *muted*), `VibrationOff` (`"1"` = off), `TutorialDone`, `DailyRewardDate`, `DailyRewardStreak`. Keep the string format when adding/reading values.
- Ads: `AdIns` (interstitial, in-game), `Rewarded` and `ScriptIntroducionMenu/AdManager` (menu). Ad unit IDs are selected with `#if UNITY_ANDROID / UNITY_IPHONE`.
