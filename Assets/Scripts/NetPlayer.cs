using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

namespace Farmhollow
{
    // Vernetzter Spieler. Nur der Besitzer steuert (WASD) per CharacterController
    // (mit Schwerkraft, damit man auf dem Insel-Terrain laeuft). Dreht sich in die
    // Laufrichtung. Der Name wird synchronisiert und ueber dem Kopf angezeigt.
    // Die Kamera (CameraFollow auf Main Camera) wird beim Spawn auf den lokalen
    // Spieler gehaengt.
    [RequireComponent(typeof(CharacterController))]
    public class NetPlayer : NetworkBehaviour
    {
        public float speed = 5f;
        public float gravity = -20f;
        public float turnSpeed = 12f;
        public TextMesh nameLabel;   // 3D-Text ueber dem Kopf

        public NetworkVariable<FixedString32Bytes> netName =
            new NetworkVariable<FixedString32Bytes>(
                "",
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner);

        private CharacterController cc;
        private float vy;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
        }

        public override void OnNetworkSpawn()
        {
            netName.OnValueChanged += (oldV, newV) => UpdateLabel();
            if (IsOwner)
            {
                netName.Value = new FixedString32Bytes(Auth.PlayerName);
                // Ueber der Insel starten, dann faellt man per Schwerkraft auf den Boden
                var spawn = GameObject.Find("SpawnPoint");
                Vector3 startPos = spawn != null ? spawn.transform.position : new Vector3(0f, 12f, 0f);
                if (cc != null) cc.enabled = false;
                transform.position = startPos;
                if (cc != null) cc.enabled = true;
                // Kamera an den lokalen Spieler haengen
                var cam = Camera.main;
                if (cam != null)
                {
                    var follow = cam.GetComponent<CameraFollow>();
                    if (follow != null) follow.target = transform;
                }
            }
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

            if (!IsOwner || cc == null) return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 move = new Vector3(h, 0f, v);
            if (move.sqrMagnitude > 1f) move.Normalize();

            // Schwerkraft
            if (cc.isGrounded && vy < 0f) vy = -2f;
            vy += gravity * Time.deltaTime;

            Vector3 vel = move * speed;
            vel.y = vy;
            cc.Move(vel * Time.deltaTime);

            // In Laufrichtung drehen
            if (move.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(new Vector3(move.x, 0f, move.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
        }
    }
}
