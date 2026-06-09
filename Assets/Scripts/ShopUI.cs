using UnityEngine;
using UnityEngine.UI;

namespace Farmhollow
{
    // Interaktions-Hinweis + modernes Karten-Shop-Modal ("Erweiterungen").
    // Baut die Karten zur Laufzeit aus dem ShopManager-Katalog.
    public class ShopUI : MonoBehaviour
    {
        [Header("Hinweis")]
        public GameObject promptRoot;
        public Text promptText;

        [Header("Modal")]
        public GameObject modalRoot;
        public Transform cardGrid;     // Eltern mit GridLayoutGroup
        public Font font;

        private bool built;

        public bool IsOpen { get { return modalRoot != null && modalRoot.activeSelf; } }

        void Start()
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (modalRoot != null) modalRoot.SetActive(false);
            ShowPrompt(false);
        }

        public void ShowPrompt(bool show) { if (promptRoot != null) promptRoot.SetActive(show); }

        public void Open()
        {
            if (modalRoot == null) return;
            if (!built) { Build(); built = true; }
            modalRoot.SetActive(true);
            ShowPrompt(false);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }

        public void Close() { if (modalRoot != null) { modalRoot.SetActive(false); Time.timeScale = 1f; } }

        void Update() { if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close(); }

        void Build()
        {
            if (cardGrid == null || ShopManager.Instance == null) return;
            foreach (Transform c in cardGrid) Destroy(c.gameObject);
            foreach (var item in ShopManager.Instance.catalog) MakeCard(item);
        }

        void MakeCard(ShopItemDef item)
        {
            var card = new GameObject("Card_" + item.id);
            card.transform.SetParent(cardGrid, false);
            var bg = card.AddComponent<Image>(); bg.color = new Color(0.16f, 0.18f, 0.22f, 1f);

            // Icon
            var ico = new GameObject("Icon"); ico.transform.SetParent(card.transform, false);
            var icoImg = ico.AddComponent<Image>(); icoImg.preserveAspect = true; if (item.icon != null) icoImg.sprite = item.icon; else icoImg.color = new Color(0.3f,0.32f,0.36f,1f);
            var ir = icoImg.rectTransform; ir.anchorMin = new Vector2(0.08f, 0.42f); ir.anchorMax = new Vector2(0.92f, 0.95f); ir.offsetMin = Vector2.zero; ir.offsetMax = Vector2.zero;

            Txt("Name", card.transform, item.displayName, 22, TextAnchor.MiddleCenter, Color.white, 0.04f, 0.30f, 0.96f, 0.42f, FontStyle.Bold);
            Txt("Price", card.transform, "$ " + item.price, 24, TextAnchor.MiddleCenter, new Color(0.4f, 0.9f, 0.45f), 0.04f, 0.17f, 0.96f, 0.30f, FontStyle.Normal);

            var buy = new GameObject("Buy"); buy.transform.SetParent(card.transform, false);
            var buyImg = buy.AddComponent<Image>(); buyImg.color = new Color(0.2f, 0.55f, 0.85f, 1f);
            var btn = buy.AddComponent<Button>();
            var brt = buyImg.rectTransform; brt.anchorMin = new Vector2(0.1f, 0.03f); brt.anchorMax = new Vector2(0.9f, 0.15f); brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
            Txt("T", buy.transform, "Kaufen", 20, TextAnchor.MiddleCenter, Color.white, 0f, 0f, 1f, 1f, FontStyle.Bold);
            var captured = item;
            btn.onClick.AddListener(() => { ShopManager.Instance.Buy(captured); });
        }

        Text Txt(string nm, Transform par, string s, int size, TextAnchor a, Color col, float aMinX, float aMinY, float aMaxX, float aMaxY, FontStyle style)
        {
            var g = new GameObject(nm); g.transform.SetParent(par, false);
            var t = g.AddComponent<Text>(); t.font = font; t.fontSize = size; t.alignment = a; t.color = col; t.text = s; t.fontStyle = style; t.horizontalOverflow = HorizontalWrapMode.Wrap;
            var rt = t.rectTransform; rt.anchorMin = new Vector2(aMinX, aMinY); rt.anchorMax = new Vector2(aMaxX, aMaxY); rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return t;
        }
    }
}
