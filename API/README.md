# N3MUS Tournament API Documentation for Studios

The N3MUS Studio API allows game studios to fetch tournament data and determine if a player is registered in an ongoing tournament leaderboard. It also enables studios to show past and upcoming tournaments in-game for user engagement.

---

## Base URL
```
https://hub-bck.n3mus.com/studio-api/tournaments/
```

---

## Authentication
To access the API, studios must provide an **Authorization** header:

### Header
```
Authorization: Bearer n3m_sk_<your_token>
```

> ⚠️ Each studio will receive a unique `n3m_sk_` token upon onboarding.

Example:
```
Authorization: Bearer n3m_sk_code
```

---

## Endpoint: `GET /studio-api/tournaments/`

Fetches all tournaments for the authenticated studio.

### Sample Response
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
				{
					"rank": 1,
					"user_id": "6655ecc78e64b86bae1d7f2b",
					"handle": "Nealawdawd",
					"wallet_address": "0xf0e144a19d3e62E64d3D6159d0B4e6862c18e270",
					"score": 25,
					"bonus_score": 0,
					"email": "neal@n3mus.com"
				},
				{
					"rank": 2,
					"user_id": "656dcbebced29ecdc6beb1be",
					"handle": "lode_marketer#9169",
					"wallet_address": "0xf10bab2117700e7911e42c83b952b53b080d0383",
					"score": 10,
					"bonus_score": 2,
					"email": "nealtjeee@gmail.com"
				}
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
				{
					"rank": 1,
					"user_id": "6655ecc78e64b86bae1d7f2b",
					"handle": "Nealawdawd",
					"wallet_address": "0xf0e144a19d3e62E64d3D6159d0B4e6862c18e270",
					"score": 25,
					"bonus_score": 0,
					"email": "neal@n3mus.com"
				},
				{
					"rank": 2,
					"user_id": "656dcbebced29ecdc6beb1be",
					"handle": "lode_marketer#9169",
					"wallet_address": "0xf10bab2117700e7911e42c83b952b53b080d0383",
					"score": 10,
					"bonus_score": 2,
					"email": "nealtjeee@gmail.com"
				}
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
				{
					"rank": 1,
					"user_id": "6655ecc78e64b86bae1d7f2b",
					"handle": "Nealawdawd",
					"wallet_address": "0xf0e144a19d3e62E64d3D6159d0B4e6862c18e270",
					"score": 25,
					"bonus_score": 0,
					"email": "neal@n3mus.com"
				},
				{
					"rank": 2,
					"user_id": "656dcbebced29ecdc6beb1be",
					"handle": "lode_marketer#9169",
					"wallet_address": "0xf10bab2117700e7911e42c83b952b53b080d0383",
					"score": 10,
					"bonus_score": 2,
					"email": "nealtjeee@gmail.com"
				}
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

## How to Check if a Player Is Registered in an Ongoing Tournament

You can determine if a player is participating in the current tournament with the following steps:

### Step-by-Step:
1. **Fetch all tournaments:**
```http
GET https://dev-backend.n3mus.com/studio-api/tournaments/
Headers:
  Authorization: Bearer n3m_sk_<your_key>
```

2. **Filter by `status == "ONGOING"`** from the response.

3. **Loop through the `participants[]` array** of the ongoing tournament(s).

4. **Match the `wallet_address`** against the current player’s wallet (case-insensitive match).

### Sample Code (Pseudocode)
```csharp
var ongoing = tournaments.Find(t => t.status == "ONGOING");
if (ongoing != null) {
    var player = ongoing.participants.Find(p => 
        p.wallet_address.ToLower() == currentWallet.ToLower());
    if (player != null) {
        Debug.Log("User is registered: " + player.handle);
    } else {
        Debug.Log("User is NOT registered.");
    }
}
```

---

## Displaying Completed & Upcoming Tournaments

The API response includes all tournaments with one of the following statuses:
- `ONGOING`: Active tournaments that users can participate in
- `UPCOMING`: Future tournaments (not yet started)
- `COMPLETED`: Finished tournaments with final leaderboards

### Carousel Integration
Studios are encouraged to create an **in-game carousel UI** that:
- Displays the current tournament
- Allows users to swipe left to view past (COMPLETED) tournaments
- Allows users to swipe right to preview upcoming (UPCOMING) tournaments

Each tournament object includes metadata such as:
- `start_date`, `end_date`
- `banner` URL for visuals
- `slug` for direct registration or leaderboard links (e.g. `https://hub.n3mus.com/tournaments/{slug}`)
- `prize_pool` for the Prizepool size
- `max_winners` for the number of winners
- `tournament_type` Can be SUM_SCORE for 'accumulated points' or HIGH_SCORE for HIGH Score Only

This improves user engagement by showcasing tournament history and teasing upcoming competitions.

---
To determine a player’s **total score** and their **ranking** in the leaderboard:

> ⚠️ The API does not return `total_score` directly — studios must calculate this on the client side.

### Formula
```
Total Score = score + bonus_score
```

The `participants` array is not sorted but we includue the rank — you need to include and calculate `total_score` in descending order.

### Example
```csharp
public class Participant {
    public string handle;
    public int score;
    public int bonus_score;
    public int TotalScore => score + bonus_score;
}

List<Participant> sorted = participants.OrderByDescending(p => p.TotalScore).ToList();

// Example display
foreach (var p in sorted) {
    Debug.Log($"{p.handle} - Total Score: {p.TotalScore}");
}
Total Score = score + bonus_score
```

Ensure you reflect this logic in your UI if you’re rendering or re-sorting leaderboard data on your end.

---

## Notes
- If `participants` is empty, no players are registered yet.
- Use the `slug` field to build URLs like `https://hub.n3mus.com/tournaments/{slug}`
- Prizes, banners, and types (`SUM_SCORE` or `HIGH_SCORE`) are included per tournament for customization.

---

For support or questions, contact us at **https://t.me/n3muschat**
