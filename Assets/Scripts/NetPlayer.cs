using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

namespace Farmhollow
{
    // Vernetzter Spieler: nur der Besitzer steuert (WASD). Der Name wird
    // synchronisiert und über dem Kopf angezeigt.
    public class NetPlayer : NetworkBehaviour
    {
        public float speed = 5f;
        public TextMesh nameLabel;   // 3D-Text über dem Kopf

        public NetworkVariable<FixedString32Bytes> netName =
            new NetworkVariable<FixedString32Bytes>(
                "",
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            netName.OnValueChanged += (oldV, newV) => UpdateLabel();
            if (IsOwner)
                netName.Value = new FixedString32Bytes(Auth.PlayerName);
            UpdateLabel();
        }

        void UpdateLabel()
        {
            if (nameLabel != null) nameLabel.text = netName.Value.ToString();
        }

        void Update()
        {
            // Namens-Schild immer zur Kamera drehen (Billboard)
            if (nameLabel != null && Camera.main != null)
            {
                Vector3 dir = nameLabel.transform.position - Camera.main.transform.position;
                if (dir.sqrMagnitude > 0.001f) nameLabel.transform.rotation = Quaternion.LookRotation(dir);
            }

            if (!IsOwner) return;
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 move = new Vector3(h, 0f, v);
            if (move.sqrMagnitude > 1f) move.Normalize();
            transform.position += move * speed * Time.deltaTime;
        }
    }
}
