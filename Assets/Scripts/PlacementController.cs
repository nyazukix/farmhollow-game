using UnityEngine;

namespace Farmhollow
{
    // Platzier-System: ein Item aus dem Inventar wählen -> ein "Geist" folgt der Maus
    // auf dem Boden (Raster) -> Linksklick platziert, Rechtsklick/Esc bricht ab.
    public class PlacementController : MonoBehaviour
    {
        public static PlacementController Instance;
        public float maxRayDistance = 300f;

        private ShopItemDef current;
        private GameObject ghost;

        void Awake() { Instance = this; }

        public bool IsPlacing { get { return current != null; } }

        public void Begin(string id)
        {
            var d = ShopManager.Instance.Get(id);
            if (d == null || d.prefab == null) return;
            if (ShopManager.Instance.OwnedCount(id) <= 0) return;
            Cancel();
            current = d;
            ghost = Instantiate(d.prefab);
            ghost.name = "Ghost_" + d.id;
            ghost.transform.rotation = Quaternion.Euler(0f, d.placeYaw, 0f);
            // Geist soll nicht kollidieren
            foreach (var c in ghost.GetComponentsInChildren<Collider>()) c.enabled = false;
            MakeTransparent(ghost, 0.5f);
        }

        public void Cancel()
        {
            if (ghost != null) Destroy(ghost);
            ghost = null; current = null;
        }

        void Update()
        {
            if (current == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxRayDistance))
            {
                float s = Mathf.Max(0.01f, current.gridSnap);
                Vector3 p = new Vector3(Mathf.Round(hit.point.x / s) * s, hit.point.y, Mathf.Round(hit.point.z / s) * s);
                if (ghost != null) ghost.transform.position = p;

                if (Input.GetMouseButtonDown(0)) Place(p);
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)) Cancel();
        }

        void Place(Vector3 pos)
        {
            if (current == null) return;
            if (!ShopManager.Instance.ConsumeOwned(current.id)) { Cancel(); return; }
            var go = Instantiate(current.prefab, pos, Quaternion.Euler(0f, current.placeYaw, 0f));
            go.transform.SetParent(transform, true);
            // weiter platzieren, solange man noch welche hat
            if (ShopManager.Instance.OwnedCount(current.id) <= 0) Cancel();
        }

        void MakeTransparent(GameObject g, float alpha)
        {
            foreach (var rend in g.GetComponentsInChildren<Renderer>())
            {
                foreach (var m in rend.materials)
                {
                    if (m.HasProperty("_BaseColor"))
                    {
                        var c = m.GetColor("_BaseColor"); c.a = alpha; m.SetColor("_BaseColor", c);
                    }
                }
            }
        }
    }
}
