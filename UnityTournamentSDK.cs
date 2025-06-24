// UnityTournamentSDK.cs
// Updated with:
// - RegisterPromptPrefab usage
// - Self row highlight
// - UniWebView integration for mobile

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

#if UNITY_ANDROID || UNITY_IOS
using UniWebView;
#endif

namespace N3MUS.TournamentSDK {
    [Serializable]
    public class Participant {
        public int rank;
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
        public string prize_pool;
        public string tournament_type;
        public int max_winners;
        public string start_date;
        public string end_date;
        public string last_updated;
        public List<Participant> participants;
    }

    [Serializable]
    public class TournamentResponse {
        public List<Tournament> data;
        public int total;
        public int page;
        public int limit;
        public int totalPages;
    }

    public class TournamentManager : MonoBehaviour {
        [Tooltip("Your N3MUS Bearer Token (starts with 'n3m_sk_')")] public string AuthToken;
        [Tooltip("Wallet address of the current player")] public string PlayerWallet;
        [Tooltip("Prefab shown when user is not registered")] public GameObject RegisterPromptPrefab;

        [Header("API Settings")]
        public string apiUrl = "https://hub-bck.n3mus.com/studio-api/tournaments/";

        [Header("UI References")] 
        public ScrollRect WeeklyCarousel;
        public Transform ParticipantListParent;
        public GameObject TournamentCardPrefab;
        public GameObject ParticipantItemPrefab;

        private List<Tournament> _tournaments = new();
        private List<Tournament> _orderedTournaments = new();

        void Start() {
            if (string.IsNullOrEmpty(AuthToken)) {
                Debug.LogError("[TournamentSDK] AuthToken must be provided.");
                return;
            }
            StartCoroutine(FetchTournaments());
        }

        IEnumerator FetchTournaments() {
            using (var www = UnityWebRequest.Get(apiUrl)) {
                www.SetRequestHeader("Authorization", $"Bearer {AuthToken}");
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success) {
                    Debug.LogError($"[TournamentSDK] Fetch Error: {www.error}");
                } else {
                    var json = www.downloadHandler.text;
                    var resp = JsonUtility.FromJson<TournamentResponse>(json);
                    _tournaments = resp.data;

                    CategorizeAndOrderTournaments();
                    PopulateCarousel();
                    SnapToOngoing();
                }
            }
        }

        void CategorizeAndOrderTournaments() {
            List<Tournament> past = _tournaments.FindAll(t => t.status == "COMPLETED");
            List<Tournament> ongoing = _tournaments.FindAll(t => t.status == "ONGOING");
            List<Tournament> upcoming = _tournaments.FindAll(t => t.status == "UPCOMING");

            _orderedTournaments.Clear();
            _orderedTournaments.AddRange(past);
            _orderedTournaments.AddRange(ongoing);
            _orderedTournaments.AddRange(upcoming);
        }

        void PopulateCarousel() {
            foreach (Transform child in WeeklyCarousel.content)
                Destroy(child.gameObject);

            foreach (var t in _orderedTournaments) {
                var card = Instantiate(TournamentCardPrefab, WeeklyCarousel.content);

                var nameText = card.transform.Find("NameText")?.GetComponent<Text>();
                var statusText = card.transform.Find("StatusText")?.GetComponent<Text>();
                var bannerImage = card.transform.Find("BannerImage")?.GetComponent<Image>();

                if (nameText != null) nameText.text = t.name;
                if (statusText != null) statusText.text = t.status;

                if (bannerImage != null) {
                    StartCoroutine(LoadImageFromURL(t.banner, bannerImage));
                }
            }

            WeeklyCarousel.onValueChanged.AddListener(OnCarouselScroll);
        }

        void SnapToOngoing() {
            int index = _orderedTournaments.FindIndex(t => t.status == "ONGOING");
            if (index != -1) {
                StartCoroutine(SnapToIndex(index));
                ShowLeaderboardFor(_orderedTournaments[index]);
            } else if (_orderedTournaments.Count > 0) {
                ShowLeaderboardFor(_orderedTournaments[0]);
            }
        }

        IEnumerator SnapToIndex(int index) {
            yield return new WaitForEndOfFrame();
            float normalized = _orderedTournaments.Count <= 1 ? 0f : index / (float)(_orderedTournaments.Count - 1);
            WeeklyCarousel.horizontalNormalizedPosition = normalized;
        }

        void OnCarouselScroll(Vector2 pos) {
            int index = Mathf.RoundToInt(WeeklyCarousel.horizontalNormalizedPosition * (_orderedTournaments.Count - 1));
            index = Mathf.Clamp(index, 0, _orderedTournaments.Count - 1);
            ShowLeaderboardFor(_orderedTournaments[index]);
        }

        public void ShowLeaderboardFor(Tournament t) {
            foreach (Transform child in ParticipantListParent)
                Destroy(child.gameObject);

            t.participants.Sort((a, b) => b.total_score.CompareTo(a.total_score));

            Participant self = null;
            if (!string.IsNullOrEmpty(PlayerWallet)) {
                self = t.participants.Find(p =>
                    string.Equals(p.wallet_address, PlayerWallet, StringComparison.OrdinalIgnoreCase));
            }

            if (self != null) {
                var topItem = Instantiate(ParticipantItemPrefab, ParticipantListParent);

                var rankText = topItem.transform.Find("RankText")?.GetComponent<Text>();
                var handleText = topItem.transform.Find("HandleText")?.GetComponent<Text>();
                var scoreText = topItem.transform.Find("ScoreText")?.GetComponent<Text>();
                var bonusText = topItem.transform.Find("BonusText")?.GetComponent<Text>();
                var totalText = topItem.transform.Find("TotalScoreText")?.GetComponent<Text>();
                var bgImage = topItem.GetComponent<Image>();

                if (rankText != null) rankText.text = self.rank.ToString();
                if (handleText != null) handleText.text = self.handle + " (YOU)";
                if (scoreText != null) scoreText.text = self.score.ToString();
                if (bonusText != null) bonusText.text = self.bonus_score.ToString();
                if (totalText != null) totalText.text = self.total_score.ToString();

                if (bgImage != null)
                    bgImage.color = new Color(0.2f, 0.6f, 0.3f, 0.7f);
            } else {
                if (RegisterPromptPrefab != null) {
                    var prompt = Instantiate(RegisterPromptPrefab, ParticipantListParent);
                    var btn = prompt.GetComponent<Button>();
                    if (btn != null) {
                        btn.onClick.AddListener(() => ShowWebView($"https://hub.n3mus.com/tournaments/{t.slug}"));
                    }
                } else {
                    Debug.LogWarning("[TournamentSDK] RegisterPromptPrefab not assigned.");
                }
            }

            for (int i = 0; i < t.participants.Count; i++) {
                var p = t.participants[i];
                if (self != null && p.wallet_address == self.wallet_address)
                    continue;

                var item = Instantiate(ParticipantItemPrefab, ParticipantListParent);

                var rankText = item.transform.Find("RankText")?.GetComponent<Text>();
                var handleText = item.transform.Find("HandleText")?.GetComponent<Text>();
                var scoreText = item.transform.Find("ScoreText")?.GetComponent<Text>();
                var bonusText = item.transform.Find("BonusText")?.GetComponent<Text>();
                var totalText = item.transform.Find("TotalScoreText")?.GetComponent<Text>();

                if (rankText != null) rankText.text = (i + 1).ToString();
                if (handleText != null) handleText.text = p.handle;
                if (scoreText != null) scoreText.text = p.score.ToString();
                if (bonusText != null) bonusText.text = p.bonus_score.ToString();
                if (totalText != null) totalText.text = p.total_score.ToString();
            }
        }

        IEnumerator LoadImageFromURL(string url, Image targetImage) {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url)) {
                yield return uwr.SendWebRequest();
                if (uwr.result == UnityWebRequest.Result.Success) {
                    var texture = DownloadHandlerTexture.GetContent(uwr);
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                    targetImage.sprite = sprite;
                } else {
                    Debug.LogWarning($"[TournamentSDK] Failed to load image: {url} — {uwr.error}");
                }
            }
        }

        void ShowWebView(string url) {
#if UNITY_ANDROID || UNITY_IOS
            var webView = new UniWebView();
            webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            webView.Load(url);
            webView.Show();
#else
            Application.OpenURL(url);
#endif
        }
    }
}
