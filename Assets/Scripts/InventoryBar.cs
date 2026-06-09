using UnityEngine;
using UnityEngine.UI;

namespace Farmhollow
{
    // Untere Leiste: zeigt Geld ($) und die besessenen Items als Slots.
    // Klick auf einen Slot startet das Platzieren.
    public class InventoryBar : MonoBehaviour
    {
        public Text moneyText;
        public Transform slotParent;   // Eltern mit HorizontalLayoutGroup
        public Font font;

        void Start()
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (EconomyManager.Instance != null) EconomyManager.Instance.OnChanged += RefreshMoney;
            if (ShopManager.Instance != null) ShopManager.Instance.OnInventoryChanged += RefreshSlots;
            RefreshMoney();
            RefreshSlots();
        }

        void RefreshMoney()
        {
            if (moneyText != null && EconomyManager.Instance != null)
                moneyText.text = "$ " + EconomyManager.Instance.money;
        }

        void RefreshSlots()
        {
            if (slotParent == null || ShopManager.Instance == null) return;
            foreach (Transform c in slotParent) Destroy(c.gameObject);
            foreach (var kv in ShopManager.Instance.owned)
            {
                if (kv.Value <= 0) continue;
                var def = ShopManager.Instance.Get(kv.Key);
                if (def == null) continue;
                MakeSlot(def, kv.Value);
            }
        }

        void MakeSlot(ShopItemDef def, int count)
        {
            var slot = new GameObject("Slot_" + def.id); slot.transform.SetParent(slotParent, false);
            var bg = slot.AddComponent<Image>(); bg.color = new Color(0f, 0f, 0f, 0.55f);
            var le = slot.AddComponent<LayoutElement>(); le.preferredWidth = 84; le.preferredHeight = 84;
            var btn = slot.AddComponent<Button>();

            var ico = new GameObject("Icon"); ico.transform.SetParent(slot.transform, false);
            var icoImg = ico.AddComponent<Image>(); icoImg.preserveAspect = true; if (def.icon != null) icoImg.sprite = def.icon; else icoImg.color = new Color(0.4f,0.42f,0.46f,1f);
            var ir = icoImg.rectTransform; ir.anchorMin = new Vector2(0.1f, 0.1f); ir.anchorMax = new Vector2(0.9f, 0.9f); ir.offsetMin = Vector2.zero; ir.offsetMax = Vector2.zero;

            var cnt = new GameObject("Count"); cnt.transform.SetParent(slot.transform, false);
            var cntT = cnt.AddComponent<Text>(); cntT.font = font; cntT.fontSize = 22; cntT.fontStyle = FontStyle.Bold; cntT.alignment = TextAnchor.LowerRight; cntT.color = Color.white; cntT.text = "x" + count;
            var cr = cntT.rectTransform; cr.anchorMin = Vector2.zero; cr.anchorMax = Vector2.one; cr.offsetMin = new Vector2(2, 2); cr.offsetMax = new Vector2(-4, -2);

            var capturedId = def.id;
            btn.onClick.AddListener(() => { if (PlacementController.Instance != null) PlacementController.Instance.Begin(capturedId); });
        }
    }
}
