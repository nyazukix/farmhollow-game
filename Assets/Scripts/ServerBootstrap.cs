using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Farmhollow
{
    // Startet automatisch den Server, wenn der Build headless läuft
    // (Dedicated Server bzw. -batchmode -nographics auf dem VPS).
    // Im normalen Client-Build passiert nichts -> dort greift das Login/Connect-UI.
    public class ServerBootstrap : MonoBehaviour
    {
        void Start()
        {
            // Im Editor NIE automatisch den Server starten (sonst kann man im
            // Play-Modus nicht als Host testen). Nur echte headless-Builds starten.
            if (Application.isEditor) return;

            bool headless = Application.isBatchMode;
#if UNITY_SERVER
            headless = true;
#endif
            if (!headless) return;

            // CPU drosseln: headless-Server lief sonst ungedrosselt (200% CPU)
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[Server] Kein NetworkManager gefunden.");
                return;
            }
            // Auf allen Interfaces lauschen, damit externe Spieler verbinden koennen
            var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (utp != null) utp.SetConnectionData("0.0.0.0", 7777, "0.0.0.0");

            NetworkManager.Singleton.StartServer();
            Debug.Log("[Server] Dedicated Server gestartet (Port 7777).");
        }
    }
}
