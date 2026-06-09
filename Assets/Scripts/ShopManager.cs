using System.Collections.Generic;
using UnityEngine;

namespace Farmhollow
{
    // Ein kaufbarer Artikel: Name, Preis, Prefab (zum Platzieren), Icon.
    [System.Serializable]
    public class ShopItemDef
    {
        public string id;
        public string displayName;
        public int price;
        public GameObject prefab;
        public Sprite icon;
        public float placeYaw = 0f;       // Rotation beim Platzieren
        public float gridSnap = 2.5f;     // Raster-Schritt beim Platzieren
    }

    // Hält den Katalog + das Inventar (was der Spieler besitzt).
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance;
        public List<ShopItemDef> catalog = new List<ShopItemDef>();

        [System.NonSerialized] public Dictionary<string, int> owned = new Dictionary<string, int>();
        public System.Action OnInventoryChanged;

        void Awake() { Instance = this; }

        public ShopItemDef Get(string id)
        {
            foreach (var c in catalog) if (c.id == id) return c;
            return null;
        }

        public bool Buy(ShopItemDef d)
        {
            if (d == null || EconomyManager.Instance == null) return false;
            if (!EconomyManager.Instance.TrySpend(d.price)) return false;
            AddOwned(d.id, 1);
            return true;
        }

        public void AddOwned(string id, int n)
        {
            if (!owned.ContainsKey(id)) owned[id] = 0;
            owned[id] += n;
            if (OnInventoryChanged != null) OnInventoryChanged();
        }

        public bool ConsumeOwned(string id)
        {
            if (owned.ContainsKey(id) && owned[id] > 0)
            {
                owned[id]--;
                if (OnInventoryChanged != null) OnInventoryChanged();
                return true;
            }
            return false;
        }

        public int OwnedCount(string id) { return owned.ContainsKey(id) ? owned[id] : 0; }
    }
}
