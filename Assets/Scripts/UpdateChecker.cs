using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Farmhollow
{
    // In-Game-Updater: prüft eine Versions-Datei online, lädt bei Bedarf die neue
    // Build-ZIP, und startet einen kleinen Helfer (.bat), der die Dateien austauscht
    // und das Spiel neu startet (eine laufende .exe kann sich nicht selbst ersetzen).
    public class UpdateChecker : MonoBehaviour
    {
        // Website-Endpoint: { "available": true, "version": "1.0.1", "url": "/download", ... }
        public string manifestUrl = "https://farmhollow.de/api/update";
        public Text statusText;

        [System.Serializable]
        private class Manifest { public bool available; public string version; public string url; }

        public void CheckForUpdates() { StartCoroutine(Check()); }

        IEnumerator Check()
        {
            Set("Suche nach Updates... (aktuell " + Application.version + ")");
            using (var req = UnityWebRequest.Get(manifestUrl))
            {
                // Kein manuelles Accept-Encoding setzen: das löst einen UnityWebRequest-Bug
                // aus (leerer Body). Der Server schickt "no-transform", daher komprimiert
                // Cloudflare ohnehin nicht — Unity bekommt sauberes JSON.
                req.timeout = 15;
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Set("Update-Check fehlgeschlagen: " + req.error);
                    yield break;
                }
                string text = req.downloadHandler != null ? req.downloadHandler.text : null;
                Manifest m = null;
                try { m = JsonUtility.FromJson<Manifest>(text); } catch { }
                if (m == null || string.IsNullOrEmpty(m.version))
                {
                    Debug.LogWarning("[Update] Roh-Antwort (Code " + req.responseCode + "): " + (text ?? "(null)"));
                    Set("Ungültige Versionsinfo (Code " + req.responseCode + ").");
                    yield break;
                }

                if (m.available && IsNewer(m.version, Application.version))
                {
                    Set("Update verfügbar: " + m.version + " – lade herunter...");
                    yield return Download(m);
                }
                else Set("Du hast die neueste Version (" + Application.version + ").");
            }
        }

        IEnumerator Download(Manifest m)
        {
            if (string.IsNullOrEmpty(m.url)) { Set("Keine Download-URL."); yield break; }
            string url = AbsoluteUrl(m.url);
            string installer = Path.Combine(Application.temporaryCachePath, "FarmhollowSetup.exe");
            using (var req = UnityWebRequest.Get(url))
            {
                req.downloadHandler = new DownloadHandlerFile(installer);
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success) { Set("Download fehlgeschlagen: " + req.error); yield break; }
            }
            Set("Update geladen. Installer wird gestartet...");
            ApplyAndRestart(installer);
        }

        void ApplyAndRestart(string installerPath)
        {
            // Installer ausfuehren (aktualisiert die Installation am gewaehlten Pfad,
            // startet das Spiel danach neu), dann das laufende Spiel beenden.
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
            Application.Quit();
        }

        // Relative URLs (z. B. "/download") gegen die manifestUrl absolut machen.
        string AbsoluteUrl(string u)
        {
            if (string.IsNullOrEmpty(u) || u.StartsWith("http")) return u;
            try { return new System.Uri(new System.Uri(manifestUrl), u).ToString(); }
            catch { return u; }
        }

        bool IsNewer(string remote, string local)
        {
            remote = (remote ?? "").TrimStart('v', 'V').Trim();
            local = (local ?? "").TrimStart('v', 'V').Trim();
            System.Version vr, vl;
            if (System.Version.TryParse(remote, out vr) && System.Version.TryParse(local, out vl))
                return vr > vl;
            return remote != local; // Fallback
        }

        void Set(string s) { Debug.Log("[Update] " + s); if (statusText != null) statusText.text = s; }
    }
}
