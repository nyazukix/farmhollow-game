using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

namespace Farmhollow
{
    // Vernetzter Spieler. Bewegung per CharacterController (mit Schwerkraft), dreht
    // sich in Laufrichtung. Name + gewaehlter Charakter (charKey) werden synchronisiert;
    // das Modell wird zur Laufzeit aus dem CharacterCatalog (Resources) geladen, damit
    // ALLE Clients die gewaehlte Figur jedes Spielers sehen.
    [RequireComponent(typeof(CharacterController))]
    public class NetPlayer : NetworkBehaviour
    {
        public float speed = 5f;
        public float gravity = -20f;
        public float turnSpeed = 12f;
        public TextMesh nameLabel;   // 3D-Text ueber dem Kopf

        public NetworkVariable<FixedString32Bytes> netName =
            new NetworkVariable<FixedString32Bytes>("",
                NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<FixedString32Bytes> charKey =
            new NetworkVariable<FixedString32Bytes>("",
                NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private CharacterController cc;
        private float vy;
        private CharacterCatalog catalog;
        private GameObject currentModel;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            catalog = Resources.Load<CharacterCatalog>("CharacterCatalog");
        }

        public override void OnNetworkSpawn()
        {
            netName.OnValueChanged += (oldV, newV) => UpdateLabel();
            charKey.OnValueChanged += (oldV, newV) => ApplyModel(newV.ToString());

            if (IsOwner)
            {
                netName.Value = new FixedString32Bytes(Auth.PlayerName);
                charKey.Value = new FixedString32Bytes(Auth.Character ?? "");

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
            ApplyModel(charKey.Value.ToString());
        }

        // Modell aus dem Katalog unter den Spieler haengen (altes ersetzen).
        void ApplyModel(string key)
        {
            if (catalog == null) catalog = Resources.Load<CharacterCatalog>("CharacterCatalog");
            if (catalog == null) return;
            var model = catalog.ModelFor(key);
            if (model == null) return;
            if (currentModel != null) Destroy(currentModel);
            currentModel = Instantiate(model, transform);
            currentModel.name = "Model";
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
        }

        void UpdateLabel()
        {
            if (nameLabel != null) nameLabel.text = netName.Value.ToString();
        }

        void Update()
        {
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

            if (cc.isGrounded && vy < 0f) vy = -2f;
            vy += gravity * Time.deltaTime;

            Vector3 vel = move * speed;
            vel.y = vy;
            cc.Move(vel * Time.deltaTime);

            if (move.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(new Vector3(move.x, 0f, move.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
        }
    }
}
