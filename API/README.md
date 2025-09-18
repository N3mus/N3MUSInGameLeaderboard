# N3mus Studio API (Concise Guide)

Use this API to read games, tournaments, and perform user / participant lookups with scoped permissions.

Base Paths:

| Environment | Base URL                        |
| ----------- | ------------------------------- |
| Production  | `https://hub-bck.n3mus.com`     |
| Staging     | `https://dev-backend.n3mus.com` |

All endpoints are under `/studio-api`.

---

## Auth

Header:

```http
Authorization: Bearer <API_KEY>
```

Keys are issued & rotated by N3mus. Request separate keys per environment.

---

## Permissions (Additive)

| Domain             | Basic                           | Elevated                            | Effect                         |
| ------------------ | ------------------------------- | ----------------------------------- | ------------------------------ |
| Games              | `game:read`                     | `game:read:all`                     | Own vs all studios             |
| Tournaments        | `tournament:read`               | `tournament:read:all`               | Own vs all studios             |
| User Lookup        | `user:lookup:basic`             | `user:lookup:enriched`              | Existence vs enriched profile  |
| Participant Lookup | `tournament:participant:lookup` | `tournament:participant:lookup:all` | Existence vs enriched + global |

Notes:

- Basic user lookup = email only; wallet requires enriched.
- Enriched participant lookup returned if key has `tournament:participant:lookup:all` OR `user:lookup:enriched`.

### Data Shaping Summary

| Permission                          | Additional Fields                           |
| ----------------------------------- | ------------------------------------------- |
| `user:lookup:basic`                 | none (exists only)                          |
| `user:lookup:enriched`              | `user_id`, `handle`, `last_active`, wallets |
| `tournament:participant:lookup`     | none (found only)                           |
| `tournament:participant:lookup:all` | `user_id`, `handle`, `wallet_address`       |

---

## Pagination

Query: `page` (default 1), `limit` (default 10). Response: `data`, `page`, `limit`, `total`, `has_next`.

---

## Endpoints Summary

| Endpoint                                          | Method | Purpose                       | Required Permission(s)                        |
| ------------------------------------------------- | ------ | ----------------------------- | --------------------------------------------- |
| `/games`                                          | GET    | List games                    | `game:read` or `game:read:all`                |
| `/games/{gameId}`                                 | GET    | Get game                      | `game:read` or `game:read:all`                |
| `/tournaments`                                    | GET    | List tournaments              | `tournament:read` or `tournament:read:all`    |
| `/tournaments/{tournamentId}`                     | GET    | Get tournament                | `tournament:read` or `tournament:read:all`    |
| `/users/lookup`                                   | GET    | User existence / profile      | `user:lookup:basic` or `user:lookup:enriched` |
| `/tournaments/{tournamentId}/participants/lookup` | GET    | Participant existence/profile | participant or user lookup permissions        |

---

## Endpoint Details

### Games

#### List Games

```http
GET /studio-api/games?page=1&limit=10
```

**Permissions:** `game:read` OR `game:read:all`

- With `game:read`: Returns only games owned by the requesting studio.
- With `game:read:all`: Returns games across all studios.

Sample Response:
Example (curl):

```bash
curl -s -H "Authorization: Bearer $STUDIO_API_KEY" \
  "https://api.n3mus.com/studio-api/games?page=1&limit=10"
```

```json
{
  "data": [
    {
      "id": "game_123",
      "title": "Example Game",
      "description": "...",
      "status": "active",
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

#### Get Game

Example (curl):

```bash
curl -s -H "Authorization: Bearer $STUDIO_API_KEY" \
  "https://api.n3mus.com/studio-api/games/game_123"
```

```http
GET /studio-api/games/{gameId}
```

**Permissions:** `game:read` OR `game:read:all`

Errors:

- `404` if not found
- `403` if game not owned (when only `game:read`)

---

### Tournaments

#### List Tournaments

Example (curl):

```bash
curl -s -H "Authorization: Bearer $STUDIO_API_KEY" \
  "https://api.n3mus.com/studio-api/tournaments?page=1&limit=10"
```

```http
GET /studio-api/tournaments?page=1&limit=10&game_id={optional}&status={optional}
```

**Permissions:** `tournament:read` OR `tournament:read:all`

Filters:

- `game_id` (optional) – filter tournaments for a specific game
- `status` (optional) – implementation-specific status values

#### Get Tournament

Example (curl):

```bash
curl -s -H "Authorization: Bearer $STUDIO_API_KEY" \
  "https://api.n3mus.com/studio-api/tournaments/tour_123"
```

```http
GET /studio-api/tournaments/{tournamentId}
```

**Permissions:** `tournament:read` OR `tournament:read:all`

Return Fields (representative):

- `tournament_id`, `name`, `slug`, `status`, `banner`, `prize_pool`, `tournament_type`
- `start_date`, `end_date`, `last_updated`
- `participants[]`: ranked list (score ordering)
  - `rank`, `user_id`, `handle`, `wallet_address`, `score`, `bonus_score`, `email`

---

### User Lookup

```http
GET /studio-api/users/lookup?email={email}
GET /studio-api/users/lookup?wallet={wallet}    (requires enriched)
```

**Permissions:** `user:lookup:basic` OR `user:lookup:enriched`

Response (Basic permission):

```json
{ "exists": true }
```

Response (Enriched permission):
Example (curl - basic email lookup):

```bash
curl -s -H "Authorization: Bearer $BASIC_KEY" \
  "https://api.n3mus.com/studio-api/users/lookup?email=user%40example.com"
```

Example (curl - enriched wallet lookup):

```bash
curl -s -H "Authorization: Bearer $ENRICHED_KEY" \
  "https://api.n3mus.com/studio-api/users/lookup?wallet=0xABCDEF..."
```

```json
{
  "exists": true,
  "user_id": "usr_123",
  "handle": "playerOne",
  "last_active": "2025-09-10T08:30:22.000Z",
  "n3mus_wallet": "0xABC...",
  "external_wallets": [
    {
      "id": "w1",
      "address": "0xDEF...",
      "origin": "evm",
      "provider": "metamask",
      "verified": true
    }
  ]
}
```

Errors:

- `400` if neither `email` nor `wallet` supplied
- `403` if `wallet` used with only basic permission

---

### Tournament Participant Lookup

```http
GET /studio-api/tournaments/{tournamentId}/participants/lookup?user_id={id}
GET /studio-api/tournaments/{tournamentId}/participants/lookup?email={email}
GET /studio-api/tournaments/{tournamentId}/participants/lookup?wallet={wallet}
```

Supply exactly one of `user_id`, `email`, or `wallet`.

**Permissions:** One of:

- Basic: `tournament:participant:lookup` (scoped to studio-owned tournaments) OR `user:lookup:basic`
- Enriched: `tournament:participant:lookup:all` OR `user:lookup:enriched`

Response (Not found):

```json
{ "found": false }
```

Response (Basic found):

```json
{ "found": true }
```

Response (Enriched found):
Example (curl - basic participant user id lookup):

```bash
curl -s -H "Authorization: Bearer $PARTICIPANT_BASIC_KEY" \
  "https://api.n3mus.com/studio-api/tournaments/tour_123/participants/lookup?user_id=usr_123"
```

Example (curl - enriched participant wallet lookup):

```bash
curl -s -H "Authorization: Bearer $PARTICIPANT_ENRICHED_KEY" \
  "https://api.n3mus.com/studio-api/tournaments/tour_123/participants/lookup?wallet=0xAAA..."
```

```json
{
  "found": true,
  "user_id": "usr_123",
  "handle": "playerOne",
  "wallet_address": "0xAAA..."
}
```

Errors:

- `400` if zero or multiple identifiers provided

---

## Errors

| Code | Meaning      | Common Causes                                                      |
| ---- | ------------ | ------------------------------------------------------------------ |
| 400  | Bad Request  | Missing/invalid query params, multiple identifiers supplied        |
| 401  | Unauthorized | Invalid / missing API key header                                   |
| 403  | Forbidden    | Key lacks required permission or studio scoping violation          |
| 404  | Not Found    | Resource ID does not exist or outside scope (treated as not found) |
| 429  | Rate Limited | (If enabled) Too many requests in window                           |

### Example Error Payload

```json
{
  "statusCode": 403,
  "error": "Forbidden",
  "message": "Insufficient permissions"
}
```

---

## Security & Ops

- API keys must be stored securely; never embed in publicly distributed clients.
- Rotate keys by requesting a new key from N3mus, then decommission the old one.
- Do not embed API keys in public front-end code or distribute them in client binaries. Use a secure backend proxy pattern if building user-facing features.
- Consider a distinct key per environment (e.g. production, staging) for operational isolation.

---

For questions or key management requests, contact N3mus.
