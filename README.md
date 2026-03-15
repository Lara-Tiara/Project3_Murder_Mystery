# Arcadia Mystery — Multiplayer Narrative Murder Mystery (Unity / Photon PUN2)

A 3-player online **murder mystery / Jubensha-inspired** narrative game prototype built with **Unity (C#)** and **Photon PUN 2**.
Players read private scripts, investigate and manage shared clues, discuss in timed chat rounds, and vote in **two rounds** to reach branching endings.

> Focus: data-driven narrative + multiplayer UI synchronization + reconnection recovery.

---

## Screenshots / Diagrams

### Overall Game Flow
![Game Flowchart](<images/Game_Flowchart.png>)

### Match-making (Storyboard + Activity Diagram)
![Match-making Storyboard](<images/Storyboard_matchmaking.png>)
![Match-making Activity Diagram](<images/Match Activity diagram.png>)

### Story / Clues Architecture
![StoryNode ScriptableObject](<images/StoryNode.png>)
![UML Class Diagram](<images/UML class-story.png>)

### Clues Investigation (UI + Activity Diagram)
![Clues UI](<images/Clues.png>)
![Clues Storyboard](<images/ClueStoryboard.png>)
![Clues Activity Diagram](<images/Clues Activity diagram.png>)

### Chat / Reading / Voting
![Chat UI](<images/chat.png>)
![Reading Scene](<images/ReadStoryPage.png>)
![Voting Storyboard](<images/StoryboardVote.png>)

### Networking / Debug
![Reconnect Log](<images/Reconnecting_log.png>)

---

## Key Features (Implementation)

### 1) Lobby + Room Match-making
- Connect to Photon Cloud and join lobby for room listing.
- Create/join rooms by room name.
- Player list + ready status; **auto-start** when all players are ready.
- Input validation with regex for room name / nickname.

### 2) Data-driven Narrative (ScriptableObjects)
- `StoryNode : ScriptableObject`
  - Loads story content via `TextAsset` (plain `.txt`).
  - Contains `NodeId`, `characterName`, and `isActive` to control which parts of story are enabled.
- Designed to support reusing the “engine-like” structure for future stories by swapping ScriptableObjects.

### 3) Reading Progress Tracking (Anti-starvation Pressure)
- Each player reads paged text (split by **double newlines**).
- A progress slider represents reading progress; updated by page navigation.
- Uses Photon **RPC** (e.g., `UpdateSliderValue`) to update all clients’ UI in real time.

### 4) Shared Clues: Real-time Sync + Share/Destroy
- `CluesManager` is a cross-scene singleton (`DontDestroyOnLoad`) that tracks clue state.
- Shared clues are synchronized using **Photon Custom Room Properties**:
  - Serialize clue list to **JSON string**
  - Store it into room properties via `SetCustomProperties`
  - Other clients deserialize + filter destroyed clues
- Each clue can be checked by only one player; players decide **Share** vs **Destroy** based on rules per round.

### 5) Timed Chat Rounds + Chatlog Export
- Chat messages synced using Photon **RPC**.
- UI formatting: self messages on the right, others on the left.
- Auto-focus on input for usability.
- At end of each chat session, export chatlog to local file so each session forms a unique “novel-like” record.

### 6) Notes System (Per-player, Session-lifecycle Safe)
- Notes persist across chat phases using `PlayerPrefs`.
- A unique key is derived from Photon `UserId` so different players don’t collide.
- Cleanup on `OnApplicationQuit()` / `OnGameEnd()` to avoid data leaking into future sessions.

### 7) Two-round Voting + Branching Endings
- `Vote` manages UI + vote collection across **two rounds**.
- Votes synced via Photon RPC; **only MasterClient writes** final tallies into room properties to avoid duplication.
- Each player loads the correct ending `StoryNode` based on combined round results.

### 8) Reconnection Handling (Resync Critical State)
- Handles unexpected disconnections:
  - **ClueListRecovery**: reload clue list from JSON/hashtable and rebuild local dictionary to prevent duplicate selections.
  - **VoteRecovery**: if disconnected during vote, treat as abstain and assign vote; if reconnect after vote ends, load final result from room properties.
  - Chat RPC buffering helps late clients receive messages sent during disconnection (when enabled in Photon settings).

---

## Tech Stack
- **Unity**: 2022.3.11f1 (recommended)
- **Language**: C#
- **Networking**: Photon PUN 2 (RPC + Custom Room Properties + Callbacks)
- **Data**: ScriptableObjects + TextAsset (`.txt`) + JSON serialization
- **Platform**: Windows (primary); macOS status depends on build settings & plugins

