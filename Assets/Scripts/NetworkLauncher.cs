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
        public InputField ipInput;          // Server-IP (Standard 127.0.0.1)
        public ushort port = 7777;

        void Start()
        {
            if (loginPanel != null) loginPanel.SetActive(true);
            if (connectPanel != null) connectPanel.SetActive(false);
            if (emailInput != null) emailInput.text = Auth.FixedEmail;
            if (ipInput != null) ipInput.text = "127.0.0.1";
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
            if (utp != null)
            {
                string ip = (ipInput != null && !string.IsNullOrEmpty(ipInput.text)) ? ipInput.text : "127.0.0.1";
                utp.SetConnectionData(ip, port);
            }
        }

        void HideAll()
        {
            if (loginPanel != null) loginPanel.SetActive(false);
            if (connectPanel != null) connectPanel.SetActive(false);
        }
    }
}
