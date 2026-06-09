using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Farmhollow
{
    // Ladebildschirm: wird beim Verbinden gezeigt, blendet rotierende Tipps ein,
    // und verschwindet, sobald der Client verbunden ist.
    public class LoadingScreen : MonoBehaviour
    {
        public GameObject root;        // das Lade-Panel (Vollbild)
        public Text tipText;
        public Text statusText;

        public string[] tips = new string[]
        {
            "Willkommen in Farm Hollow!",
            "Mit WASD bewegst du dich.",
            "Triff andere Spieler in der Stadt.",
            "Im Farmhaus kannst du Erweiterungen kaufen.",
            "Dein Fortschritt wird auf dem Server gespeichert.",
            "Sei nett im Chat. :)"
        };

        private float tipTimer;
        private int tipIndex;

        void Start()
        {
            if (root != null) root.SetActive(false);
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnected;
            }
        }

        void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnected;
            }
        }

        public void Show(string status)
        {
            if (root != null) root.SetActive(true);
            if (statusText != null) statusText.text = status;
            tipTimer = 0f; NextTip();
        }

        void Hide() { if (root != null) root.SetActive(false); }

        void OnConnected(ulong clientId)
        {
            if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId) Hide();
        }

        void OnDisconnected(ulong clientId)
        {
            if (statusText != null) statusText.text = "Verbindung getrennt.";
            // kurz sichtbar lassen, dann ausblenden
            Invoke(nameof(Hide), 2.5f);
        }

        void Update()
        {
            if (root != null && root.activeSelf)
            {
                tipTimer += Time.unscaledDeltaTime;
                if (tipTimer > 4f) { tipTimer = 0f; NextTip(); }
            }
        }

        void NextTip()
        {
            if (tips != null && tips.Length > 0 && tipText != null)
            {
                tipText.text = "Tipp: " + tips[tipIndex % tips.Length];
                tipIndex++;
            }
        }
    }
}
