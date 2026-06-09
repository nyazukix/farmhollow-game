using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Farmhollow
{
    // Server-Login gegen die users-Tabelle auf dem VPS (bcrypt, /api/login).
    // Ersetzt das frühere lokale Hash-Login. Username, Rolle und Geld kommen
    // jetzt vom Server.
    public static class Auth
    {
        // Login-Endpunkt der Website (prüft gegen app_farmhollow.users).
        public const string LoginUrl = "https://app.farmhollow.de/api/login";

        // Nur als Vorbelegung des E-Mail-Feldes im Login-Fenster.
        public const string FixedEmail = "tobiasprang@icloud.com";

        public static bool LoggedIn { get; private set; }
        public static string PlayerName = "Spieler";   // wird nach Login vom Server gesetzt
        public static string Role = "player";
        public static long Balance;                     // Bank-Guthaben
        public static long Cash;                         // Bargeld
        public static string Token = "";                 // Session-Token (für spätere Server-Calls)

        // --- JSON-Modelle (JsonUtility braucht [Serializable] + öffentliche Felder) ---
        [Serializable] private class LoginBody { public string email; public string password; }

        [Serializable]
        private class UserDto
        {
            public long id;
            public string email;
            public string username;
            public string role;
            public long balance;
            public long cash;
        }

        [Serializable]
        private class LoginResponse
        {
            public bool ok;
            public string error;
            public string token;
            public UserDto user;
        }

        // Coroutine: ruft das Server-Login auf und meldet das Ergebnis über onDone(success, message).
        // Muss von einem MonoBehaviour per StartCoroutine ausgeführt werden.
        public static IEnumerator Login(string email, string password, Action<bool, string> onDone)
        {
            var body = new LoginBody { email = (email ?? "").Trim(), password = password ?? "" };
            string json = JsonUtility.ToJson(body);

            using (var req = new UnityWebRequest(LoginUrl, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 15;

                yield return req.SendWebRequest();

                // responseCode 0 = der Server war gar nicht erreichbar (echter Netzwerkfehler).
                // Bei 401/500 liefert der Server trotzdem JSON, das wir auswerten.
                if (req.responseCode == 0)
                {
                    onDone?.Invoke(false, "Server nicht erreichbar.");
                    yield break;
                }

                LoginResponse resp = null;
                try { resp = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text); }
                catch { resp = null; }

                if (resp == null)
                {
                    onDone?.Invoke(false, "Ungueltige Server-Antwort.");
                    yield break;
                }

                if (!resp.ok)
                {
                    string msg;
                    switch (resp.error)
                    {
                        case "invalid_credentials": msg = "E-Mail oder Passwort falsch."; break;
                        case "banned": msg = "Account gesperrt."; break;
                        default: msg = "Login fehlgeschlagen."; break;
                    }
                    onDone?.Invoke(false, msg);
                    yield break;
                }

                // Erfolg: Spielerdaten vom Server übernehmen.
                LoggedIn = true;
                Token = resp.token ?? "";
                if (resp.user != null)
                {
                    PlayerName = string.IsNullOrEmpty(resp.user.username) ? "Spieler" : resp.user.username;
                    Role = string.IsNullOrEmpty(resp.user.role) ? "player" : resp.user.role;
                    Balance = resp.user.balance;
                    Cash = resp.user.cash;
                }
                onDone?.Invoke(true, "");
            }
        }
    }
}
