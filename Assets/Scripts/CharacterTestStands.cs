using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Farmhollow
{
    // TEST-Hilfe: stellt alle Katalog-Figuren in einer Reihe auf (idle, animiert).
    // Geht der lokale Spieler in die Naehe einer Figur + drueckt E, wechselt er zu
    // dieser Figur (zum Ausprobieren von Aussehen / Bein-Spreizung). Self-building.
    // Spaeter wieder entfernen (oder hinter einen Debug-Schalter haengen).
    public class CharacterTestStands : MonoBehaviour
    {
        public float interactRange = 2.6f;
        public Vector3 rowCenter = new Vector3(0f, 0f, 7f);
        public float spacing = 1.9f;

        CharacterCatalog catalog;
        RuntimeAnimatorController ctrl;

        class Stand { public string key; public string display; public Vector3 pos; public Transform model; public Transform lLeg, rLeg, lLow, rLow; public float spread; }
        readonly List<Stand> stands = new List<Stand>();
        Text hintText;

        void Start()
        {
            catalog = Resources.Load<CharacterCatalog>("CharacterCatalog");
            ctrl = Resources.Load<RuntimeAnimatorController>("PlayerAnimator");
            if (catalog == null) return;
            BuildStands();
            BuildHint();
        }

        void BuildStands()
        {
            int n = catalog.entries.Count;
            float x0 = rowCenter.x - (n - 1) * spacing / 2f;
            for (int i = 0; i < n; i++)
            {
                var e = catalog.entries[i];
                Vector3 pos = new Vector3(x0 + i * spacing, rowCenter.y, rowCenter.z);
                Transform modelT = null, lLeg = null, rLeg = null, lLow = null, rLow = null;
                if (e.model != null)
                {
                    var go = Instantiate(e.model, pos, Quaternion.Euler(0f, 180f, 0f)); // schaut zum Spawn
                    go.name = "Stand_" + e.key;
                    modelT = go.transform;
                    var a = go.GetComponentInChildren<Animator>();
                    if (a != null) { a.runtimeAnimatorController = ctrl; a.applyRootMotion = false; a.cullingMode = AnimatorCullingMode.AlwaysAnimate; }
                    if (a != null && e.legSpread != 0f) { lLeg = a.GetBoneTransform(HumanBodyBones.LeftUpperLeg); rLeg = a.GetBoneTransform(HumanBodyBones.RightUpperLeg); lLow = a.GetBoneTransform(HumanBodyBones.LeftLowerLeg); rLow = a.GetBoneTransform(HumanBodyBones.RightLowerLeg); }
                    var lblGo = new GameObject("Label");
                    lblGo.transform.SetParent(go.transform, false);
                    lblGo.transform.localPosition = new Vector3(0f, 2.15f, 0f);
                    lblGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // 180 der Figur ausgleichen -> lesbar
                    var tm = lblGo.AddComponent<TextMesh>();
                    tm.text = e.displayName; tm.fontSize = 56; tm.characterSize = 0.06f;
                    tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center; tm.color = Color.white;
                }
                stands.Add(new Stand { key = e.key, display = e.displayName, pos = pos, model = modelT, lLeg = lLeg, rLeg = rLeg, lLow = lLow, rLow = rLow, spread = e.legSpread });
            }
        }

        // Bein-Spreizung auch auf den Podest-Figuren (damit sie aussehen wie der Spieler).
        // Oberschenkel nach aussen, Unterschenkel gegen -> Fuesse bleiben parallel + breit.
        void LateUpdate()
        {
            foreach (var s in stands)
            {
                if (s.spread == 0f || s.lLeg == null || s.rLeg == null || s.model == null) continue;
                Vector3 fwd = s.model.forward;
                s.lLeg.Rotate(fwd, s.spread, Space.World);
                s.rLeg.Rotate(fwd, -s.spread, Space.World);
                if (s.lLow != null) s.lLow.Rotate(fwd, -s.spread, Space.World);
                if (s.rLow != null) s.rLow.Rotate(fwd, s.spread, Space.World);
            }
        }

        void BuildHint()
        {
            var canvasGo = new GameObject("SwitchHintCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 60;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720);
            var tGo = new GameObject("Hint"); tGo.transform.SetParent(canvasGo.transform, false);
            hintText = tGo.AddComponent<Text>();
            hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hintText.fontSize = 28; hintText.alignment = TextAnchor.MiddleCenter; hintText.color = Color.white; hintText.fontStyle = FontStyle.Bold;
            var rt = hintText.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f); rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 120f); rt.sizeDelta = new Vector2(900f, 50f);
            var outline = tGo.AddComponent<Outline>(); outline.effectColor = new Color(0f, 0f, 0f, 0.85f); outline.effectDistance = new Vector2(2f, -2f);
            hintText.text = "";
        }

        void Update()
        {
            NetPlayer me = null;
            foreach (var p in FindObjectsByType<NetPlayer>(FindObjectsSortMode.None))
                if (p.IsOwner) { me = p; break; }
            if (me == null) { if (hintText != null) hintText.text = ""; return; }

            Stand near = null; float best = interactRange;
            foreach (var s in stands)
            {
                float d = Vector3.Distance(me.transform.position, s.pos);
                if (d < best) { best = d; near = s; }
            }
            if (near != null)
            {
                hintText.text = "[E] zu " + near.display + " wechseln";
                if (Input.GetKeyDown(KeyCode.E)) me.SetCharacter(near.key);
            }
            else hintText.text = "";
        }
    }
}
