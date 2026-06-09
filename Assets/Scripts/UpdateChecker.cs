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
        // version.json:  { "version": "1.0.1", "url": "https://.../Farmhollow-1.0.1.zip" }
        public string manifestUrl = "https://farmhollow.de/version.json";
        public Text statusText;

        [System.Serializable]
        private class Manifest { public string version; public string url; }

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

                if (IsNewer(m.version, Application.version))
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
            string zip = Path.Combine(Application.temporaryCachePath, "farmhollow_update.zip");
            using (var req = UnityWebRequest.Get(m.url))
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

        bool IsNewer(string remote, string local)
        {
            System.Version vr, vl;
            if (System.Version.TryParse(remote, out vr) && System.Version.TryParse(local, out vl))
                return vr > vl;
            return remote != local; // Fallback
        }

        void Set(string s) { Debug.Log("[Update] " + s); if (statusText != null) statusText.text = s; }
    }
}
