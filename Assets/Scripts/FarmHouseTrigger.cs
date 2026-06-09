using UnityEngine;

namespace Farmhollow
{
    // Eine Trigger-Zone am Farmhaus. Ist der Spieler drin und drückt E,
    // öffnet sich das "Erweiterungen"-Modal.
    [RequireComponent(typeof(Collider))]
    public class FarmHouseTrigger : MonoBehaviour
    {
        public ShopUI shopUI;
        public string promptMessage = "[E] Interagieren";

        private bool inRange;

        void Reset()
        {
            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            inRange = true;
            if (shopUI != null)
            {
                shopUI.ShowPrompt(true);
                if (shopUI.promptText != null) shopUI.promptText.text = promptMessage;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            inRange = false;
            if (shopUI != null) shopUI.ShowPrompt(false);
        }

        void Update()
        {
            if (inRange && shopUI != null && !shopUI.IsOpen && Input.GetKeyDown(KeyCode.E))
                shopUI.Open();
        }
    }
}
