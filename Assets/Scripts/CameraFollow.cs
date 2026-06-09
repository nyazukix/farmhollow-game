using UnityEngine;

namespace Farmhollow
{
    // ============================================================
    //  CameraFollow
    //  Smoothly follows a target (the player) from a fixed offset,
    //  keeping the cozy angled top-down view.
    //  Belongs on the Main Camera.
    // ============================================================
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;                              // who to follow (the player)

        [Header("Position")]
        public Vector3 offset = new Vector3(0f, 10f, -10f);   // camera height / distance behind
        public float smoothTime = 0.2f;                       // higher = softer, lazier follow

        [Header("Look")]
        public float lookAtHeight = 1f;                       // look slightly above the feet

        private Vector3 velocity;                             // internal, used by SmoothDamp

        // LateUpdate runs after the player has moved this frame,
        // so the camera never lags a frame behind.
        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

            transform.LookAt(target.position + Vector3.up * lookAtHeight);
        }
    }
}
