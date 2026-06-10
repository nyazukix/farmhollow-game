using UnityEngine;

namespace Farmhollow
{
    // ============================================================
    //  CameraFollow
    //  Folgt dem Spieler aus einem cozy, leicht erhoehten Winkel.
    //  Rechte Maustaste halten + Maus bewegen = um den Spieler drehen
    //  (Yaw + Pitch). Mausrad = Zoom. Belongs on the Main Camera.
    // ============================================================
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;                 // wer verfolgt wird (der Spieler)

        [Header("Abstand / Hoehe")]
        public float distance = 9f;              // Abstand hinter dem Spieler
        public float minDistance = 4f;
        public float maxDistance = 16f;
        public float lookAtHeight = 1.2f;        // schaut leicht ueber die Fuesse

        [Header("Drehen (rechte Maustaste)")]
        public float yaw = 0f;                   // horizontaler Winkel
        public float pitch = 35f;                // vertikaler Winkel (von oben)
        public float minPitch = 8f;
        public float maxPitch = 75f;
        public float mouseSensitivity = 3.2f;    // Maus-Empfindlichkeit (hoeher = schneller)
        public float zoomSpeed = 4f;
        public float smoothTime = 0.12f;

        private Vector3 velocity;
        private bool initialized;

        void LateUpdate()
        {
            if (target == null) return;

            // Cursor sperren fuer freie Maus-Kamera (Escape loest ihn, z.B. fuer Menues)
            if (!initialized) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; initialized = true; }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool locked = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = locked;
            }

            // Kamera folgt einfach der Maus (wenn Cursor gesperrt)
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * 0.7f;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }
            // Zoom per Mausrad
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
                distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desired = target.position + Vector3.up * lookAtHeight + rot * (Vector3.back * distance);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            transform.LookAt(target.position + Vector3.up * lookAtHeight);
        }
    }
}
