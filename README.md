# N3MUS Integration Guide

This repository contains two separate integration paths for studios:

1. **API** – Fetch leaderboard data and display it in your game or application.
2. **POINTS** – Push player scores and match results into the leaderboard system.

Both are independent modules but work best when used together.

---

## 📂 Repository Structure

```
.
├── API/       # Read-only access to leaderboard data (for showing in-game leaderboards)
└── POINTS/    # Write access for studios to push match results/points
```

---

## 🔎 API (Leaderboard Data Access)

**Purpose:** Allow studios to **retrieve leaderboard data** and display it in-game.

Typical use cases:
- Show global or tournament leaderboards inside your game UI.
- Fetch a player’s current rank and score.
- Refresh leaderboards periodically or on demand.

📖 See [`API/README.md`](./API/README.md) for full usage and endpoints.

---

## 🎯 POINTS (Leaderboard Write Access)

**Purpose:** Allow studios to **submit match results / points** to N3MUS leaderboards.

Typical use cases:
- Award points after a match.
- Record achievements or quest completions.
- Push metadata (like match numbers or map names).

📖 See [`POINTS/README.md`](./POINTS/README.md) for authentication details, request format, and examples (cURL, Python, Unity, Unreal, etc.).

---

## 🔐 Authentication

- Both modules use **Bearer tokens**.
- Each studio receives a token from N3MUS.
- Include it in the `Authorization` header:

```
Authorization: Bearer <your-api-token>
```

---

## 🚀 Typical Flow

1. **Game ends →** studio backend calls the **POINTS** API to submit results.
2. **N3MUS processes →** results are validated, stored, and reflected on-chain.
3. **Game frontend →** calls the **API** module to fetch leaderboard standings and show updated ranks.

---

## 📬 Support

Need help integrating?

📧 **updates@n3mus.com**

---

© N3MUS

