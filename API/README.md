<!--
  Unified documentation: combines earlier concise guide and tournament-focused integration doc.
  Audience: Game studio engineers integrating N3MUS tournaments & game data.
-->

# N3MUS Studio API Documentation

The N3MUS Studio API lets approved game studios:

- List & inspect their games
- List, filter, and fetch tournaments (past, ongoing, upcoming)
- Retrieve tournament leaderboards (participants with scores)
- Determine if (and how) a user exists in the N3MUS ecosystem
- Determine if a user is participating in a specific tournament

This merged guide expands prior docs and adds new lookup endpoints plus practical client integration patterns (Unity / Unreal) and a changelog.

---

## Base URLs

| Environment | Base Host                     | Full Example (tournaments list)                                |
| ----------- | ----------------------------- | --------------------------------------------------------------- |
| Production  | `https://hub-bck.n3mus.com`   | `https://hub-bck.n3mus.com/studio-api/tournaments`             |
| Staging     | `https://dev-backend.n3mus.com` | `https://dev-backend.n3mus.com/studio-api/tournaments`       |

All endpoints are prefixed with `/studio-api`.

---

## Authentication

Provide a bearer token issued to your studio.

```http
Authorization: Bearer n3m_sk_<your_token>
```

Notes:

- Tokens are environment‑scoped; use separate tokens per environment.
- Rotate keys via N3MUS support; decommission old keys promptly.

---

## Permissions Model (Additive)

Permissions gate data scope & enrichment. Elevated implies basic behavior plus broader scope / extra fields.

| Domain             | Basic                           | Elevated                            | Scope / Enrichment Effect                     |
| ------------------ | -------------------------------- | ----------------------------------- | ---------------------------------------------- |
| Games              | `game:read`                      | `game:read:all`                     | Own studio games vs all studios (admin ops)    |
| Tournaments        | `tournament:read`                | `tournament:read:all`               | Own studio tournaments vs global               |
| User Lookup        | `user:lookup:basic`              | `user:lookup:enriched`              | Existence (email only) vs enriched profile     |
| Participant Lookup | `tournament:participant:lookup`  | `tournament:participant:lookup:all` | Existence in owned tournaments vs enriched all |

Data field additions when elevated:

| Permission                          | Adds Fields                                      |
| ----------------------------------- | ------------------------------------------------ |
| `user:lookup:enriched`              | `user_id`, `handle`, `last_active`, wallets       |
| `tournament:participant:lookup:all` | `user_id`, `handle`, `wallet_address`             |

Interplay:

- Wallet lookups require enriched user permission.
- Enriched participant response granted if token has either `tournament:participant:lookup:all` OR `user:lookup:enriched`.

---

## Pagination Standard

Query parameters: `page` (default 1), `limit` (default 10, max 100 unless specified). Responses include:

```json
{
  "data": [],
  "page": 1,
  "limit": 10,
  "total": 57,
  "has_next": true
}
```

Some legacy responses may include `totalPages`; new format uses `has_next`.

---

## Endpoint Overview

| Endpoint                                          | Method | Purpose                               | Required Permissions                                      |
| ------------------------------------------------- | ------ | ------------------------------------- | ---------------------------------------------------------- |
| `/games`                                          | GET    | Paginated list of games               | `game:read` or `game:read:all`                             |
| `/games/{gameId}`                                 | GET    | Fetch single game                     | `game:read` or `game:read:all`                             |
| `/tournaments`                                    | GET    | Paginated tournaments (filterable)    | `tournament:read` or `tournament:read:all`                 |
| `/tournaments/{tournamentId}`                     | GET    | Tournament details + participants     | `tournament:read` or `tournament:read:all`                 |
| `/users/lookup`                                   | GET    | User existence / profile lookup       | `user:lookup:basic` or `user:lookup:enriched`              |
| `/tournaments/{tournamentId}/participants/lookup` | GET    | Participant existence / enrichment    | participant or user lookup permissions (see matrix)        |

---

## Games

### List Games

```http
GET /studio-api/games?page=1&limit=10&status={optional}
```

Behavior:

- `game:read` => only your studio's games
- `game:read:all` => global (internal / admin usage)

Sample:

```bash
curl -s -H "Authorization: Bearer $STUDIO_API_KEY" \
  "$BASE_HOST/studio-api/games?page=1&limit=10"
```

Response (representative):

```json
{
  "data": [
    {
      "id": "game_123",
      "title": "Example Game",
      "description": "...",
      "status": "ACTIVE",
      "socials": { "site": null, "twitter": null, "discord": null },
      "images": { "thumbnail": null, "banner": null },
      "created_at": "2025-09-01T12:00:00.000Z"
    }
  ],
  "page": 1,
  "limit": 10,
  "total": 1,
  "has_next": false
}
```

### Get Game

```http
GET /studio-api/games/{gameId}
```

Errors:

- `404` not found
- `403` forbidden (ownership) when only `game:read`

---

## Tournaments

### List Tournaments

```http
GET /studio-api/tournaments?page=1&limit=10&game_id={optional}&status={UPCOMING|ONGOING|COMPLETED}
```

Filters:

- `game_id` filter by a specific game
- `status` one of `UPCOMING`, `ONGOING`, `COMPLETED`

Sample:

```bash
curl -s -H "Authorization: Bearer $STUDIO_API_KEY" \
  "$BASE_HOST/studio-api/tournaments?page=1&limit=10"
```

### Get Tournament

```http
GET /studio-api/tournaments/{tournamentId}
```

Representative fields:

- Identity: `tournament_id`, `slug`, `name`
- Meta: `status`, `banner`, `prize_pool`, `tournament_type`, `max_winners`
- Timing: `start_date`, `end_date`, `last_updated`
- Leaderboard `participants[]` objects: `rank`, `user_id`, `handle",`wallet_address`,`score`,`bonus_score`,`email`

Scoring:

```text
total_score = score + bonus_score
```

`participants` are returned with a `rank`; if you locally re‑sort (e.g. to recompute `total_score`) preserve the original ranking for UI clarity unless you intend to recompute entirely.

### Full Sample (Paginated Tournament List)

Below is a representative multi-status response (UPCOMING / ONGOING / COMPLETED) as used in integration examples:

```json
{
  "data": [
    {
      "tournament_id": "fgoji7ktymnq4ovsq9fv1kb7",
      "status": "UPCOMING",
      "slug": "kugle-upcoming",
      "name": "kugle-upcoming",
      "banner": "http://res.cloudinary.com/dtnbuzwfd/image/upload/v1750837587/tournaments/1750837586175-banner.png",
      "prize_pool": "500",
      "tournament_type": "SUM_SCORE",
      "max_winners": 100,
      "start_date": "2025-07-09T07:41:00.000Z",
      "end_date": "2025-07-31T07:41:00.000Z",
      "last_updated": "2025-06-25T07:46:41.399Z",
      "participants": [
        { "rank": 1, "user_id": "6655ecc78e64b86bae1d7f2b", "handle": "Nealawdawd", "wallet_address": "0xf0e144a19d3e62E64d3D6159d0B4e6862c18e270", "score": 25, "bonus_score": 0, "email": "neal@n3mus.com" },
        { "rank": 2, "user_id": "656dcbebced29ecdc6beb1be", "handle": "lode_marketer#9169", "wallet_address": "0xf10bab2117700e7911e42c83b952b53b080d0383", "score": 10, "bonus_score": 2, "email": "nealtjeee@gmail.com" }
      ]
    },
    {
      "tournament_id": "zz8lbjldh2s46hju083pxc5o",
      "status": "ONGOING",
      "slug": "kugle-test-ongoing",
      "name": "Kugle-test-ongoing",
      "banner": "http://res.cloudinary.com/dtnbuzwfd/image/upload/v1750837551/tournaments/1750837551226-banner.png",
      "prize_pool": "500",
      "tournament_type": "SUM_SCORE",
      "max_winners": 100,
      "start_date": "2025-06-25T07:47:00.000Z",
      "end_date": "2025-06-30T07:41:00.000Z",
      "last_updated": "2025-06-26T09:13:20.047Z",
      "participants": [
        { "rank": 1, "user_id": "6655ecc78e64b86bae1d7f2b", "handle": "Nealawdawd", "wallet_address": "0xf0e144a19d3e62E64d3D6159d0B4e6862c18e270", "score": 25, "bonus_score": 0, "email": "neal@n3mus.com" },
        { "rank": 2, "user_id": "656dcbebced29ecdc6beb1be", "handle": "lode_marketer#9169", "wallet_address": "0xf10bab2117700e7911e42c83b952b53b080d0383", "score": 10, "bonus_score": 2, "email": "nealtjeee@gmail.com" }
      ]
    },
    {
      "tournament_id": "jjusadkp43yv54doefe3qgfl",
      "status": "COMPLETED",
      "slug": "kugle-test",
      "name": "Kugle-test",
      "banner": "http://res.cloudinary.com/dtnbuzwfd/image/upload/v1750837318/tournaments/1750837317348-banner.png",
      "prize_pool": "500",
      "tournament_type": "SUM_SCORE",
      "max_winners": 100,
      "start_date": "2025-06-25T07:43:00.000Z",
      "end_date": "2025-06-25T07:46:00.000Z",
      "last_updated": "2025-06-25T07:46:20.542Z",
      "participants": [
        { "rank": 1, "user_id": "6655ecc78e64b86bae1d7f2b", "handle": "Nealawdawd", "wallet_address": "0xf0e144a19d3e62E64d3D6159d0B4e6862c18e270", "score": 25, "bonus_score": 0, "email": "neal@n3mus.com" },
        { "rank": 2, "user_id": "656dcbebced29ecdc6beb1be", "handle": "lode_marketer#9169", "wallet_address": "0xf10bab2117700e7911e42c83b952b53b080d0383", "score": 10, "bonus_score": 2, "email": "nealtjeee@gmail.com" }
      ]
    }
  ],
  "total": 3,
  "page": 1,
  "limit": 10,
  "totalPages": 1
}
```

---

## User Lookup

Check if a user exists or fetch an enriched profile.

```http
GET /studio-api/users/lookup?email={email}
GET /studio-api/users/lookup?wallet={wallet}   # wallet requires enriched permission
```

Permissions:

- Basic => existence only, email queries only
- Enriched => email OR wallet, returns profile & wallets

Basic response:

```json
{ "exists": true }
```

Enriched response (example):

```json
{
  "exists": true,
  "user_id": "usr_123",
  "handle": "playerOne",
  "last_active": "2025-09-10T08:30:22.000Z",
  "n3mus_wallet": "0xABC...",
  "external_wallets": [
    { "id": "w1", "address": "0xDEF...", "origin": "evm", "provider": "metamask", "verified": true }
  ]
}
```

Errors:

- `400` if neither (or both) email & wallet omitted / conflict
- `403` if wallet used without enriched permission

---

## Tournament Participant Lookup

Determine if (and optionally who) a user is within a specific tournament leaderboard.

```http
GET /studio-api/tournaments/{tournamentId}/participants/lookup?user_id={id}
GET /studio-api/tournaments/{tournamentId}/participants/lookup?email={email}
GET /studio-api/tournaments/{tournamentId}/participants/lookup?wallet={wallet}
```

Rules:

- Supply exactly one identifier.
- Wallet requires enriched context (participant enriched OR user enriched).

Permissions mapping:

- Basic existence: `tournament:participant:lookup` (scoped to owned tournaments) OR `user:lookup:basic`
- Enriched: `tournament:participant:lookup:all` OR `user:lookup:enriched`

Responses:

```json
{ "found": false }
```

```json
{ "found": true }
```

Enriched:

```json
{ "found": true, "user_id": "usr_123", "handle": "playerOne", "wallet_address": "0xAAA..." }
```

Errors:

- `400` zero or multiple identifiers

---

## Integration Patterns

### Checking If a Player Is Registered (Tournament Ongoing)

1. Fetch tournaments: `GET /studio-api/tournaments?status=ONGOING` (or fetch all and filter locally).
2. Select the active tournament (if multiple, choose by game or most recent start date).
3. Search `participants[]` for a case‑insensitive wallet address (or user id/email if you perform earlier lookups).
4. Render UI accordingly (e.g., PLAY vs JOIN / SIGN UP button).

Pseudocode (C#):

```csharp
var ongoing = tournaments.FirstOrDefault(t => t.status == "ONGOING");
if (ongoing != null) {
    var player = ongoing.participants.FirstOrDefault(p => 
        string.Equals(p.wallet_address, currentWallet, StringComparison.OrdinalIgnoreCase));
    if (player != null) {
        // Show PLAY / progress UI
    } else {
        // Show JOIN TOURNAMENT button
    }
}
```

### Building a Tournament Carousel

Recommended UX:

- Center: Ongoing tournament
- Left: Completed (historical) tournaments
- Right: Upcoming (preview)

Use fields:

- Visual: `banner`
- Timing: `start_date`, `end_date`
- Links: `slug` → `https://hub.n3mus.com/tournaments/{slug}`
- Incentives: `prize_pool`, `max_winners`, `tournament_type` (`SUM_SCORE` or `HIGH_SCORE`)

### Calculating Total Score

The API does not emit a `total_score` field. Compute client‑side:

```csharp
int TotalScore(int score, int bonus) => score + bonus;
```

If you re‑rank locally based on `score + bonus_score`, ensure clarity if server `rank` diverges (e.g., show both or denote "Client View").

---
## UI Examples

### Unregistered Player State
<img width="482" height="561" alt="unregistered user" src="https://github.com/user-attachments/assets/b8fbfef0-a9f4-404a-8f26-abb8c7c0a6ae" />

### Registered Player (Play Enabled)
<img width="457" height="531" alt="registered user play state" src="https://github.com/user-attachments/assets/ca9634b7-dc45-44be-839c-051c0767d16a" />

### Carousel Leaderboard UI
<img width="1280" height="752" alt="carousel leaderboard" src="https://github.com/user-attachments/assets/8d63d824-633c-4f85-b0a7-276f54fb9261" />

These visuals illustrate recommended affordances for JOIN / PLAY transitions and historical vs upcoming tournament discovery.

---

## JOIN / Registration Button Logic

Show a `JOIN TOURNAMENT` button only when the user is not present in an ongoing tournament's participant list.

Join URL pattern:

```text
https://hub.n3mus.com/tournaments/{slug}
```

### Unity Example

```csharp
public void ShowJoinTournamentButton(string slug) {
    string joinUrl = $"https://hub.n3mus.com/tournaments/{slug}";
    Button btn = Instantiate(joinButtonPrefab, parentTransform);
    var txt = btn.GetComponentInChildren<UnityEngine.UI.Text>();
    if (txt) txt.text = "JOIN TOURNAMENT";
    btn.onClick.AddListener(() => Application.OpenURL(joinUrl));
}
```

#### Unreal (C++)

```cpp
FString JoinUrl = FString::Printf(TEXT("https://hub.n3mus.com/tournaments/%s"), *TournamentSlug);
FPlatformProcess::LaunchURL(*JoinUrl, nullptr, nullptr);
```

---

## Error Handling

| Code | Meaning      | Typical Causes                                        |
| ---- | ------------ | ----------------------------------------------------- |
| 400  | Bad Request  | Missing / invalid query, multiple identifiers         |
| 401  | Unauthorized | Missing or invalid bearer token                       |
| 403  | Forbidden    | Insufficient permission or studio ownership violation |
| 404  | Not Found    | Resource absent or outside scope (intentionally vague)|
| 429  | Rate Limited | Throttling (if enabled)                               |

Example:

```json
{ "statusCode": 403, "error": "Forbidden", "message": "Insufficient permissions" }
```

Retry Guidance:

- Avoid tight retry loops on 401/403 (requires operator / key action).
- For 429 use exponential backoff (e.g., 1s, 2s, 4s ... max 30s).

---

## Security & Operational Guidance

- Never embed tokens in a distributed game binary without a secure proxy layer.
- Never expose the api secret in a client environment, proxy requests through a backend.
- Separate tokens per environment & per internal service for blast‑radius reduction.
- Rotate keys periodically or upon suspicion of compromise.

---

## Changelog

| Date (YYYY-MM-DD) | Change | Notes |
| ----------------- | ------ | ----- |
| 2025-09-18 | Added `GET /users/lookup` | Basic & enriched user existence/profile lookup |
| 2025-09-18 | Added `GET /tournaments/{id}/participants/lookup` | Participant existence/enrichment endpoint |

---

## Support

For key management or questions: contact N3MUS (community: <https://t.me/n3muschat>).

---
