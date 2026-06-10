using System.Collections.Generic;
using UnityEngine;

namespace Farmhollow
{
    // Datenbasis der auswählbaren Spieler-Charaktere (Key -> Modell + Anzeigename).
    // Wird vom Auswahl-Modal (Vorschau) UND vom NetPlayer (Spawn-Modell) genutzt.
    // Neue Charaktere = einfach hier einen Eintrag (Key, Name, importiertes GLB-Modell) ergänzen.
    [CreateAssetMenu(fileName = "CharacterCatalog", menuName = "Farmhollow/Character Catalog")]
    public class CharacterCatalog : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public string key;            // item_key aus der DB, z.B. "char_player_w-1"
            public string displayName;    // Anzeigename, z.B. "Mila"
            public GameObject model;      // importiertes Modell-Prefab (GLB)
        }

        public List<Entry> entries = new List<Entry>();

        public Entry Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            foreach (var e in entries) if (e.key == key) return e;
            return null;
        }

        public Entry First() { return entries.Count > 0 ? entries[0] : null; }

        // Modell für einen Key; faellt auf den ersten Eintrag zurueck, falls unbekannt.
        public GameObject ModelFor(string key)
        {
            var e = Get(key);
            if (e != null && e.model != null) return e.model;
            var f = First();
            return f != null ? f.model : null;
        }

        public int IndexOf(string key)
        {
            for (int i = 0; i < entries.Count; i++) if (entries[i].key == key) return i;
            return -1;
        }
    }
}
