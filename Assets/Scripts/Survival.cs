using UnityEngine;
using UnityEngine.UI;

namespace Farmhollow
{
    // Hunger + Durst fuer den lokalen Spieler. Beide sinken mit der Zeit; faellt einer
    // auf 0, stirbt man -> kurze Pause (spaeter Dying-Animation) -> Respawn am Spawnpunkt,
    // Werte zurueck auf respawnValue. ITEMS BLEIBEN ERHALTEN (Tod beruehrt das Inventar nicht).
    // Wird vom lokalen NetPlayer beim Spawn hinzugefuegt (nur Owner). Spaeter: Essen/Trinken
    // fuellt die Werte (Eat/Drink), und Persistenz auf dem Server.
    public class Survival : MonoBehaviour
    {
        public float max = 100f;
        public float hunger = 100f;
        public float thirst = 100f;
        public float hungerPerSec = 0.12f;   // ~14 min von voll auf leer
        public float thirstPerSec = 0.16f;   // ~10 min
        public float respawnValue = 60f;
        public float deathSeconds = 4f;

        NetPlayer player;
        bool dead;
        float deadTimer;

        GameObject canvasGo;
        Image hungerFill, thirstFill;
        Text deathText;

        void Start()
        {
            player = GetComponent<NetPlayer>();
            BuildHud();
        }

        void Update()
        {
            if (dead)
            {
                deadTimer -= Time.deltaTime;
                if (deathText != null) deathText.text = "Du bist gestorben\nRespawn in " + Mathf.CeilToInt(Mathf.Max(0f, deadTimer)) + "s";
                if (deadTimer <= 0f) Respawn();
                return;
            }

            hunger = Mathf.Max(0f, hunger - hungerPerSec * Time.deltaTime);
            thirst = Mathf.Max(0f, thirst - thirstPerSec * Time.deltaTime);
            if (hungerFill != null) hungerFill.fillAmount = hunger / max;
            if (thirstFill != null) thirstFill.fillAmount = thirst / max;

            if (hunger <= 0f || thirst <= 0f) Die();
        }

        void Die()
        {
            if (dead) return;
            dead = true;
            deadTimer = deathSeconds;
            if (player != null) { player.controlsLocked = true; player.PlayDie(); }
            if (deathText != null) deathText.gameObject.SetActive(true);
        }

        void Respawn()
        {
            dead = false;
            hunger = respawnValue; thirst = respawnValue;
            if (deathText != null) deathText.gameObject.SetActive(false);
            var spawn = GameObject.Find("SpawnPoint");
            Vector3 pos = spawn != null ? spawn.transform.position : new Vector3(0f, 12f, 0f);
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            transform.position = pos;
            if (cc != null) cc.enabled = true;
            if (player != null) player.controlsLocked = false;
            // ITEMS bleiben unberuehrt.
        }

        // Fuer spaeter (Essen/Trinken-Items):
        public void Eat(float amount) { hunger = Mathf.Min(max, hunger + amount); }
        public void Drink(float amount) { thirst = Mathf.Min(max, thirst + amount); }

        // ---- HUD (selbst aufgebaut) ----
        void BuildHud()
        {
            canvasGo = new GameObject("SurvivalHUD");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            hungerFill = MakeBar("Hunger", new Vector2(0, -24), new Color(0.85f, 0.55f, 0.25f));
            thirstFill = MakeBar("Durst", new Vector2(0, -58), new Color(0.30f, 0.62f, 0.90f));

            deathText = MakeText("Death", "", 44, TextAnchor.MiddleCenter, Color.white);
            Anchor(deathText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(700, 160));
            var dbg = deathText.gameObject.AddComponent<Outline>(); dbg.effectColor = new Color(0, 0, 0, 0.8f); dbg.effectDistance = new Vector2(2, -2);
            deathText.gameObject.SetActive(false);
        }

        Image MakeBar(string label, Vector2 pos, Color fillColor)
        {
            // Hintergrund
            var bgGo = new GameObject("Bar_" + label); bgGo.transform.SetParent(canvasGo.transform, false);
            var bg = bgGo.AddComponent<Image>(); bg.color = new Color(0f, 0f, 0f, 0.45f);
            Anchor(bg.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(150, pos.y), new Vector2(220, 22));
            // Fuellung
            var fillGo = new GameObject("Fill"); fillGo.transform.SetParent(bgGo.transform, false);
            var fill = fillGo.AddComponent<Image>(); fill.color = fillColor;
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0; fill.fillAmount = 1f;
            var frt = fill.rectTransform; frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one; frt.offsetMin = new Vector2(2, 2); frt.offsetMax = new Vector2(-2, -2);
            // Label
            var t = MakeText("L", label, 16, TextAnchor.MiddleLeft, Color.white);
            Anchor(t.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34, pos.y), new Vector2(110, 22));
            return fill;
        }

        Text MakeText(string name, string s, int size, TextAnchor a, Color c)
        {
            var go = new GameObject(name); go.transform.SetParent(canvasGo.transform, false);
            var t = go.AddComponent<Text>(); t.text = s;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size; t.alignment = a; t.color = c; t.fontStyle = FontStyle.Bold;
            return t;
        }

        void Anchor(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }
    }
}
