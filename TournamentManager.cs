// UnityTournamentSDK.cs
// N3MUS Tournament Screen SDK for Unity
// ------------------------------
// Usage:
// 1. Import this script into your Unity project.
// 2. Create a Canvas in your scene with:
//    • Scroll Rect for horizontal swiping of tournament cards (WeeklyCarousel).
//    • Panel for displaying leaderboard details (LeaderboardPanel).
//    • Prefabs: TournamentCard (banner, name, status), ParticipantItem (rank, handle, score, bonus).
// 3. Add the TournamentManager component to an empty GameObject.
//    • Assign GameID and APIKey in the inspector.
//    • Link WeeklyCarousel.content to contentParent.
//    • Link ParticipantListParent in LeaderboardPanel.
//    • Drag in the TournamentCard and ParticipantItem prefabs.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace N3MUS.TournamentSDK {
    [Serializable]
    public class Participant {
        public string user_id;
        public string handle;
        public string wallet_address;
        public int score;
        public int bonus_score;
        public string email;
        public int total_score => score + bonus_score;
    }

    [Serializable]
    public class Tournament {
        public string tournament_id;
        public string status;
        public string slug;
        public string name;
        public string banner;
        public int prize_pool;
        public string tournament_type;
        public int max_winners;
        public string start_date;
        public string end_date;
        public string last_updated;
        public List<Participant> participants;
    }

    [Serializable]
    public class TournamentResponse {
        public List<Tournament> tournaments;
    }

    public class TournamentManager : MonoBehaviour {
        [Tooltip("Your Game ID from N3MUS")] public string GameID;
        [Tooltip("Your API key from N3MUS")] public string APIKey;
        [Header("API Settings")] public string apiUrl = "https://hub-bck.n3mus.com/gametournament/";

        [Header("UI References")] 
        public ScrollRect WeeklyCarousel;
        public Transform ParticipantListParent;
        public GameObject TournamentCardPrefab;
        public GameObject ParticipantItemPrefab;

        private List<Tournament> _tournaments;

        void Start() {
            if (string.IsNullOrEmpty(GameID) || string.IsNullOrEmpty(APIKey)) {
                Debug.LogError("[TournamentSDK] GameID and APIKey must be provided.");
                return;
            }
            StartCoroutine(FetchTournaments());
        }

        IEnumerator FetchTournaments() {
            var url = $"{apiUrl}{GameID}";
            using (var www = UnityWebRequest.Get(url)) {
                www.SetRequestHeader("x-api-key", APIKey);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success) {
                    Debug.LogError($"[TournamentSDK] Fetch Error: {www.error}");
                } else {
                    var json = www.downloadHandler.text;
                    var resp = JsonUtility.FromJson<TournamentResponse>(json);
                    _tournaments = resp.tournaments;

                    PopulateCarousel();
                    ShowOngoingLeaderboard();
                }
            }
        }

        void PopulateCarousel() {
            foreach (Transform child in WeeklyCarousel.content)
                Destroy(child.gameObject);

            foreach (var t in _tournaments) {
                var card = Instantiate(TournamentCardPrefab, WeeklyCarousel.content);
                // TODO: assign UI fields (banner, name, status, dates)
                var button = card.GetComponent<Button>();
                button.onClick.AddListener(() => ShowLeaderboardFor(t));
            }
        }

        void ShowOngoingLeaderboard() {
            var ongoing = _tournaments.Find(t => t.status == "ONGOING");
            if (ongoing != null) ShowLeaderboardFor(ongoing);
        }

        public void ShowLeaderboardFor(Tournament t) {
            // Clear existing items
            foreach (Transform child in ParticipantListParent)
                Destroy(child.gameObject);

            // Sort by total score descending
            t.participants.Sort((a, b) => b.total_score.CompareTo(a.total_score));

            // Instantiate and populate participant items
            for (int i = 0; i < t.participants.Count; i++) {
                var p = t.participants[i];
                var item = Instantiate(ParticipantItemPrefab, ParticipantListParent);
                // TODO: assign UI texts:
                //  - Rank: i+1
                //  - Handle: p.handle
                //  - Score: p.score
                //  - Bonus: p.bonus_score
                //  - Total: p.total_score
            }
        }
    }
}
