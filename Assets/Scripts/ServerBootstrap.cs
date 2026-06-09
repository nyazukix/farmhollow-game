using Unity.Netcode;
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
            bool headless = Application.isBatchMode;
#if UNITY_SERVER
            headless = true;
#endif
            if (!headless) return;

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[Server] Kein NetworkManager gefunden.");
                return;
            }
            NetworkManager.Singleton.StartServer();
            Debug.Log("[Server] Dedicated Server gestartet (Port 7777).");
        }
    }
}
