using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace Farmhollow
{
    // Steuert das Login-Fenster und danach Host / Beitreten.
    public class NetworkLauncher : MonoBehaviour
    {
        [Header("Login")]
        public GameObject loginPanel;
        public InputField emailInput;
        public InputField passwordInput;
        public Text loginStatus;

        [Header("Connect")]
        public GameObject connectPanel;
        public InputField ipInput;          // Server-Adresse (Hostname oder IP)
        public string defaultServer = "app.farmhollow.de";
        public ushort port = 7777;

        void Start()
        {
            if (loginPanel != null) loginPanel.SetActive(true);
            if (connectPanel != null) connectPanel.SetActive(false);
            if (emailInput != null) emailInput.text = Auth.FixedEmail;
            if (ipInput != null) ipInput.text = defaultServer;
        }

        public void OnLoginClicked()
        {
            string email = emailInput != null ? emailInput.text : "";
            string pass = passwordInput != null ? passwordInput.text : "";
            if (Auth.TryLogin(email, pass))
            {
                if (loginStatus != null) loginStatus.text = "";
                if (loginPanel != null) loginPanel.SetActive(false);
                if (connectPanel != null) connectPanel.SetActive(true);
            }
            else
            {
                if (loginStatus != null) loginStatus.text = "Login fehlgeschlagen.";
            }
        }

        public void OnHostClicked()
        {
            ApplyAddress();
            NetworkManager.Singleton.StartHost();
            HideAll();
        }

        public void OnJoinClicked()
        {
            ApplyAddress();
            NetworkManager.Singleton.StartClient();
            HideAll();
        }

        void ApplyAddress()
        {
            var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (utp == null) return;
            string addr = (ipInput != null && !string.IsNullOrEmpty(ipInput.text)) ? ipInput.text : defaultServer;
            utp.SetConnectionData(ResolveToIPv4(addr), port);
        }

        // UnityTransport braucht eine IP — Hostnamen (z. B. app.farmhollow.de) hier auflösen.
        string ResolveToIPv4(string host)
        {
            System.Net.IPAddress parsed;
            if (System.Net.IPAddress.TryParse(host, out parsed)) return host;  // ist schon eine IP
            try
            {
                var addrs = System.Net.Dns.GetHostAddresses(host);
                foreach (var a in addrs)
                    if (a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) return a.ToString();
                if (addrs.Length > 0) return addrs[0].ToString();
            }
            catch { }
            return host;
        }

        void HideAll()
        {
            if (loginPanel != null) loginPanel.SetActive(false);
            if (connectPanel != null) connectPanel.SetActive(false);
        }
    }
}
