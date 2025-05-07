# N3MUS Unity Tournament SDK — Integration Guide

A comprehensive step-by-step guide for game developers to integrate the N3MUS Tournament Unity SDK into your game projects.

---

## 1. Overview

The N3MUS Unity Tournament SDK enables you to:

- Display a **swipeable carousel** of weekly tournaments
- Show real-time **leaderboards** sorted by `score + bonus_score`
- Present separate **score**, **bonus score**, and **total score** fields
- Offer a **Join Tournament** button for non-participants
- **Map** on-chain wallet addresses to your in-game user profiles

All you need to supply are: **GameID**, **APIKey**, and the current player’s **wallet address**.

---

## 2. Prerequisites

- **Unity** 2019.4 or later
- **Unity UI** package (Canvas, ScrollRect, Button, Text, Image)
- **Internet access** at runtime
- Your backend or user service must be able to lookup users by **wallet address**

---

## 3. Installation

1. **Import Script**
   - Create folder `Assets/N3MUS/TournamentSDK/`
   - Copy `TournamentManager.cs` (provided script) into this folder.

2. **Create Prefabs** under `Assets/Prefabs/N3MUS/`:
   - **TournamentCard.prefab**
     - Root: `GameObject` with `Button` component
     - Children:
       - `Image` (for banner)
       - `Text` (for name)
       - `Text` (for status)
       - `Text` (for dates)
   - **ParticipantItem.prefab**
     - Root: `GameObject`
     - Children:
       - `Text` (Rank)
       - `Text` (DisplayName)
       - `Text` (Score)
       - `Text` (BonusScore)
       - `Text` (TotalScore)
       - *(Optional)* `Image` (Avatar)
   - **JoinButton.prefab**
     - `Button` with child `Text` reading “Join Tournament”

---

## 4. UI Hierarchy Setup

1. **Canvas** (UI Scale Mode: _Scale With Screen Size_)
2. **WeeklyCarouselPanel** (child of Canvas)
   - Add **ScrollRect** component
   - Configure as **Horizontal** scroll
   - Under `Viewport`:
     - Add `Mask` + (optional) semi-transparent `Image`
     - `Content`: `Horizontal Layout Group` + `Content Size Fitter (Horizontal preferred)`
3. **LeaderboardPanel** (below carousel)
   - Add **ScrollRect** as **Vertical** scroll
   - Under `Viewport`:
     - `Mask` + optional `Image`
     - `Content`: `Vertical Layout Group` + `Content Size Fitter (Vertical preferred)`
   - Under LeaderboardPanel create two empty `GameObject`s:
     - `ParticipantListParent`
     - `JoinButtonParent`

---

## 5. Configuring TournamentManager

1. **Add Component**
   - Create an empty GameObject named **`TournamentManager`**.
   - Attach the `TournamentManager.cs` script.

2. **Inspector Fields**
   - **GameID**: Your N3MUS Game ID
   - **APIKey**: Your N3MUS API Key
   - **CurrentUserWallet**: Player’s wallet (string)
   - **API URL**: (default `https://hub-bck.n3mus.com/gametournament/`)
   - **WeeklyCarousel**: Drag the ScrollRect from **WeeklyCarouselPanel**
   - **ParticipantListParent**: Drag the `ParticipantListParent` GameObject
   - **JoinButtonParent**: Drag the `JoinButtonParent` GameObject
   - **TournamentCardPrefab**, **ParticipantItemPrefab**, **JoinButtonPrefab**: assign your created prefabs

---

## 6. API Data Flow

1. **Fetch**
   - Endpoint: `GET https://hub-bck.n3mus.com/gametournament/{GameID}`
   - Header: `x-api-key: {APIKey}`

2. **Response Model**
   ```json
   {
     "tournaments": [
       {
         "tournament_id":"...",
         "status":"ONGOING",
         "slug":"...",
         "name":"...",
         "banner":"https://...png",
         "prize_pool":500,
         "tournament_type":"SUM_SCORE",
         "max_winners":20,
         "start_date":"2024-12-05T17:00:00.000Z",
         "end_date":"2024-12-12T17:00:00.000Z",
         "last_updated":"...",
         "participants": [
           {
             "user_id":"...",
             "handle":"...",
             "wallet_address":"0x...",
             "score":5231,
             "bonus_score":213,
             "email":"..."
           }
         ]
       }
     ]
   }
   ```

---

## 7. Mapping to In-Game Profiles

Use the SDK’s **`OnLeaderboardLoaded`** callback to resolve each participant’s `wallet_address` to your internal user engine.

```csharp
void Start() {
  var mgr = FindObjectOfType<TournamentManager>();
  mgr.OnLeaderboardLoaded += participants => {
    foreach (var p in participants) {
      var profile = YourUserService.GetByWallet(p.wallet_address);
      if (profile != null) {
        userMap[p.wallet_address] = profile;
        // Pre-load avatar, name, etc.
      }
    }
  };
}
```

Then in `ShowLeaderboardFor()`, within the TODO block:
```csharp
// Bind: DisplayNameText.text = userMap[p.wallet_address].displayName;
// Bind: AvatarImage.sprite  = userMap[p.wallet_address].avatar;
```

---

## 8. Customization & Extensions

- **Error Handling UI**: Add pop-ups or placeholders if the fetch fails.
- **Image Caching**: Implement `LoadSpriteFromUrl()` helper to download and cache banners/avatars.
- **Animations**: Use Unity’s `CanvasGroup` or `Animator` to animate card transitions.
- **Filters & Pagination**: Extend the API wrapper to support query params (e.g. status filters).

---

## 9. Troubleshooting

| Issue                                       | Solution                                                         |
|---------------------------------------------|------------------------------------------------------------------|
| Blank carousel or errors in console         | Verify `GameID` & `APIKey`, check network connectivity.          |
| Participants not appearing                  | Ensure `CurrentUserWallet` is set and callback mapping is used.  |
| Join button not showing                     | Confirm `JoinButtonPrefab` assigned and `JoinButtonParent` exists.|
| UI elements overlap                         | Adjust `LayoutGroup` padding and spacing on parent containers.   |

---

## 10. Support

For questions, feedback, or feature requests, reach out to `NEAL@N3MUS.com`

Happy integrating! 🎮✨
