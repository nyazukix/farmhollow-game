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
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Set("Update-Check fehlgeschlagen: " + req.error);
                    yield break;
                }
                Manifest m = null;
                try { m = JsonUtility.FromJson<Manifest>(req.downloadHandler.text); } catch { }
                if (m == null || string.IsNullOrEmpty(m.version)) { Set("Ungueltige Versionsinfo."); yield break; }

                if (m.available && IsNewer(m.version, Application.version))
                {
                    Set("Update verfuegbar: " + m.version + " - lade herunter...");
                    yield return Download(m);
                }
                else Set("Du hast die neueste Version (" + Application.version + ").");
            }
        }

        IEnumerator Download(Manifest m)
        {
            if (string.IsNullOrEmpty(m.url)) { Set("Keine Download-URL."); yield break; }
            string url = AbsoluteUrl(m.url);
            string zip = Path.Combine(Application.temporaryCachePath, "farmhollow_update.zip");
            using (var req = UnityWebRequest.Get(url))
            {
                req.downloadHandler = new DownloadHandlerFile(zip);
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success) { Set("Download fehlgeschlagen: " + req.error); yield break; }
            }
            Set("Update geladen. Installiere & starte neu...");
            ApplyAndRestart(zip);
        }

        void ApplyAndRestart(string zipPath)
        {
            // Ordner, der die .exe enthält (Application.dataPath = .../<Spiel>_Data)
            string installDir = Path.GetDirectoryName(Application.dataPath);
            string exePath = Path.Combine(installDir, Application.productName + ".exe");
            string bat = Path.Combine(Application.temporaryCachePath, "farmhollow_apply_update.bat");

            string script =
                "@echo off\r\n" +
                "timeout /t 2 /nobreak >nul\r\n" +
                "powershell -NoProfile -Command \"Expand-Archive -Force '" + zipPath + "' '" + installDir + "'\"\r\n" +
                "start \"\" \"" + exePath + "\"\r\n" +
                "del \"%~f0\"\r\n";
            File.WriteAllText(bat, script);

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = bat,
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
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
