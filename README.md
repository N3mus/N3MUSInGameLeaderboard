
# N3MUS Unity Tournament SDK — Integration Guide

A step-by-step guide for integrating the N3MUS Tournament SDK into your Unity game.

---

## 1. Features

- Carousel of past, ongoing, and upcoming tournaments
- Leaderboards with score, bonus, and total score
- Self rank highlight
- Registration prompt for unregistered users
- WebView support for mobile (via UniWebView)
- Dynamic tournament loading via API
- Prefab-based UI (modular design)

---

## 2. Requirements

- Unity 2019.4 or later
- Unity UI (Canvas, ScrollRect, Button, Text, Image)
- Internet access
- [Optional] UniWebView for mobile WebView support

---

## 3. Setup

### File Structure
```
Assets/
└── N3MUS/
    └── TournamentSDK/
        └── UnityTournamentSDK.cs
Prefabs/
├── TournamentCardPrefab
├── ParticipantItemPrefab
└── RegisterPromptPrefab
```

### Create Prefabs

#### 1. `TournamentCardPrefab`
- Button root
- Children: `NameText`, `StatusText`, `BannerImage`

#### 2. `ParticipantItemPrefab`
- Root: GameObject (optional background `Image`)
- Children: `RankText`, `HandleText`, `ScoreText`, `BonusText`, `TotalScoreText`

#### 3. `RegisterPromptPrefab`
- Button root + child `Text` (e.g. "CLICK HERE TO REGISTER")

### Unity Scene UI

1. **Canvas**
2. **ScrollRect** (WeeklyCarousel)
   - Horizontal
   - Content: HorizontalLayoutGroup + ContentSizeFitter
3. **ParticipantListParent**
   - VerticalLayoutGroup + ContentSizeFitter

---

## 4. TournamentManager Inspector

| Field | Description |
|-------|-------------|
| AuthToken | Your bearer token (starts with `n3m_sk_...`) |
| PlayerWallet | Current user's wallet address |
| WeeklyCarousel | Reference to horizontal ScrollRect |
| ParticipantListParent | Vertical container for participants |
| TournamentCardPrefab | Prefab for each tournament in the carousel |
| ParticipantItemPrefab | Prefab for leaderboard entries |
| RegisterPromptPrefab | Prefab shown if user not registered |

---

## 5. Mobile WebView (Optional)

Add [UniWebView](https://uniwebview.com/) to support in-app WebView on iOS/Android.

```csharp
#if UNITY_ANDROID || UNITY_IOS
using UniWebView;
#endif
```

Register button opens:

```url
https://hub.n3mus.com/tournaments/{slug}
```

---

## 6. Notes

- You can style the "YOU" row with a background color or icon via the `Image` on `ParticipantItemPrefab`.
- Unregistered users will see a prompt that opens the N3MUS hub registration.

---

## 7. Support

Questions or issues? Email `NEAL@N3MUS.com` or visit https://hub.n3mus.com/
