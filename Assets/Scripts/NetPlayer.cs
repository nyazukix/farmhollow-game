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
        public float sprintSpeed = 9f;
        public float gravity = -20f;
        public float jumpForce = 7f;
        public float turnSpeed = 12f;
        public TextMesh nameLabel;   // 3D-Text ueber dem Kopf
        [System.NonSerialized] public bool controlsLocked = false;   // bei Tod gesperrt

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
        private Animator anim;
        private Vector3 lastPos;

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
                // Hunger/Durst nur fuer den lokalen Spieler
                gameObject.AddComponent<Survival>();
            }

            UpdateLabel();
            ApplyModel(charKey.Value.ToString());
            lastPos = transform.position;
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
            anim = currentModel.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("PlayerAnimator");
                anim.applyRootMotion = false;
            }
        }

        // Vom Survival-System bei Tod aufgerufen.
        public void PlayDie()
        {
            if (anim != null) anim.SetTrigger("Die");
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

            // Lauf-Animation aus tatsaechlicher Bewegung (fuer ALLE Clients, auch andere Spieler)
            if (anim != null)
            {
                Vector3 d = transform.position - lastPos; d.y = 0f;
                float velMag = d.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
                anim.SetFloat("Speed", Mathf.Clamp(velMag / speed, 0f, 2f), 0.12f, Time.deltaTime);
            }
            lastPos = transform.position;

            if (!IsOwner || cc == null) return;

            float h = controlsLocked ? 0f : Input.GetAxisRaw("Horizontal");
            float v = controlsLocked ? 0f : Input.GetAxisRaw("Vertical");
            // Bewegung relativ zur Kamera (WASD dreht mit der Kamera mit)
            Vector3 move;
            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 f = cam.transform.forward; f.y = 0f; f.Normalize();
                Vector3 r = cam.transform.right; r.y = 0f; r.Normalize();
                move = f * v + r * h;
            }
            else move = new Vector3(h, 0f, v);
            if (move.sqrMagnitude > 1f) move.Normalize();

            bool sprint = !controlsLocked && Input.GetKey(KeyCode.LeftShift);
            float curSpeed = sprint ? sprintSpeed : speed;

            if (cc.isGrounded)
            {
                if (vy < 0f) vy = -2f;
                if (!controlsLocked && Input.GetKeyDown(KeyCode.Space))
                {
                    vy = jumpForce;
                    if (anim != null) anim.SetTrigger("Jump");
                }
            }
            vy += gravity * Time.deltaTime;

            Vector3 vel = move * curSpeed;
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
