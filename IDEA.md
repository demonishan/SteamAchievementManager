# SAM Reborn - Feature Ideas

This document tracks potential features and enhancements for future versions of SAM Reborn.

## 🚀 Core Enhancements
- [ ] **Steam Web API Integration**: Support for using a user-provided Web API Key.
    -   *Benefit*: Enables fetching achievement stats (e.g., "10/50 unlocked") for the entire game library in the Home view.
    -   *Benefit*: Faster loading of game metadata without relying on scraping or local cache.
- [ ] **Data Persistence**: Local database (SQLite/JSON) to cache game details and stats, reducing the need to re-fetch on every launch.

## 📊 Statistics & Visualization
- [ ] **Global Progress Tracking**: A dashboard showing total unlocked achievements across all games, average completion rate, and rarest achievements owned.
- [ ] **Rarity Charts**: Visual breakdown of achievements by rarity (Common, Rare, Ultra Rare).
- [ ] **Unlock Timelines**: Interactive graph showing when achievements were unlocked over time.

## 🛠️ Tools & Utilities
- [ ] **"Legit" Mode Generator**: Advanced timer that automatically schedules achievements to unlock at "realistic" intervals based on average playtime data (if available).
- [ ] **Backup & Restore**: One-click backup of locked/unlocked states for specific games to a local file.
- [ ] **Comparison Tool**: Compare achievements with a friend (via Steam ID) to see what they have that you don't.

## 🎨 UI/UX Polish
- [ ] **Theme Support**: Allow users to customize the primary accent color (currently fixed Blue).
- [ ] **Spoiler Protection**: Option to blur descriptions and icons of hidden achievements until hovered or clicked.
- [ ] **Compact View**: A denser list view for the game library for users with massive collections.
- [ ] **Filters**: Filter main game list by "100% Completed", "In Progress", or "Untouched".

## 🔌 Integrations
- [ ] **Discord Rich Presence**: Show which game you are currently managing/idling in SAM on Discord.
- [ ] **SteamGridDB Integration**: Fetch high-quality custom grids/hero images instead of standard capsule images.
