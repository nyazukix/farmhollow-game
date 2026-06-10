using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace Farmhollow
{
    // Charakter-Auswahl beim ersten Login (users.character leer). Baut UI + 3D-Vorschau
    // selbst auf (kein manuelles Verdrahten). Bestaetigen -> POST /api/character speichern
    // -> Auth.Character setzen -> onConfirmed() (= verbinden).
    public class CharacterSelect : MonoBehaviour
    {
        public const string SetUrl = "https://app.farmhollow.de/api/character";

        CharacterCatalog catalog;
        int index;
        System.Action onConfirmed;

        GameObject canvasGo;
        Text nameText, statusText;
        RawImage previewImage;
        Button confirmBtn;

        Camera previewCam;
        RenderTexture previewRT;
        Transform modelAnchor;
        GameObject previewModel;

        public void Show(System.Action confirmed)
        {
            onConfirmed = confirmed;
            catalog = Resources.Load<CharacterCatalog>("CharacterCatalog");
            if (catalog == null || catalog.entries.Count == 0)
            {
                if (onConfirmed != null) onConfirmed();   // nichts zu waehlen -> direkt weiter
                return;
            }
            BuildStage();
            BuildUI();
            index = 0;
            SelectIndex(0);
            canvasGo.SetActive(true);
        }

        void BuildStage()
        {
            if (previewCam != null) { return; }
            var stage = new GameObject("CharPreviewStage");
            stage.transform.position = new Vector3(2000f, 2000f, 2000f);

            var lightGo = new GameObject("PreviewLight");
            lightGo.transform.SetParent(stage.transform, false);
            var l = lightGo.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(35f, 200f, 0f);

            modelAnchor = new GameObject("Anchor").transform;
            modelAnchor.SetParent(stage.transform, false);
            modelAnchor.localPosition = Vector3.zero;

            var camGo = new GameObject("PreviewCam");
            camGo.transform.SetParent(stage.transform, false);
            previewCam = camGo.AddComponent<Camera>();
            previewCam.clearFlags = CameraClearFlags.SolidColor;
            previewCam.backgroundColor = new Color(0.16f, 0.20f, 0.15f);
            previewCam.cullingMask = ~0;
            previewCam.transform.localPosition = new Vector3(0f, 1.0f, 2.7f);
            previewCam.transform.LookAt(stage.transform.position + Vector3.up * 0.95f);
            previewCam.fieldOfView = 32f;
            previewRT = new RenderTexture(512, 640, 16);
            previewCam.targetTexture = previewRT;
        }

        void BuildUI()
        {
            if (canvasGo != null) { return; }
            canvasGo = new GameObject("CharSelectCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            canvasGo.AddComponent<GraphicRaycaster>();

            var bg = MakeImage("Backdrop", canvasGo.transform, new Color(0.08f, 0.10f, 0.09f, 1f));
            Stretch(bg.rectTransform);

            var title = MakeText("Title", canvasGo.transform, "Waehle deinen Charakter", 40, TextAnchor.MiddleCenter);
            Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -70), new Vector2(900, 60));

            var piGo = new GameObject("Preview"); piGo.transform.SetParent(canvasGo.transform, false);
            previewImage = piGo.AddComponent<RawImage>(); previewImage.texture = previewRT;
            Anchor(previewImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -5), new Vector2(330, 410));

            nameText = MakeText("Name", canvasGo.transform, "", 30, TextAnchor.MiddleCenter);
            Anchor(nameText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -240), new Vector2(500, 44));

            var prev = MakeButton("Prev", canvasGo.transform, "<", () => Step(-1));
            Anchor((RectTransform)prev.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-245, -5), new Vector2(72, 72));
            var next = MakeButton("Next", canvasGo.transform, ">", () => Step(1));
            Anchor((RectTransform)next.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(245, -5), new Vector2(72, 72));

            confirmBtn = MakeButton("Confirm", canvasGo.transform, "Bestaetigen", OnConfirm);
            Anchor((RectTransform)confirmBtn.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 70), new Vector2(320, 64));

            statusText = MakeText("Status", canvasGo.transform, "", 22, TextAnchor.MiddleCenter);
            Anchor(statusText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 28), new Vector2(800, 30));
        }

        void Update()
        {
            if (modelAnchor != null) modelAnchor.Rotate(0f, 28f * Time.deltaTime, 0f);
        }

        void Step(int d)
        {
            int n = catalog.entries.Count;
            index = ((index + d) % n + n) % n;
            SelectIndex(index);
        }

        void SelectIndex(int i)
        {
            index = i;
            var e = catalog.entries[i];
            nameText.text = e.displayName;
            if (previewModel != null) Destroy(previewModel);
            if (e.model != null)
            {
                previewModel = Instantiate(e.model, modelAnchor);
                previewModel.transform.localPosition = Vector3.zero;
                previewModel.transform.localRotation = Quaternion.identity;
            }
        }

        void OnConfirm() { StartCoroutine(SaveAndProceed()); }

        IEnumerator SaveAndProceed()
        {
            var e = catalog.entries[index];
            if (confirmBtn != null) confirmBtn.interactable = false;
            if (statusText != null) statusText.text = "Speichere...";
            string json = "{\"token\":\"" + Auth.Token + "\",\"character\":\"" + e.key + "\"}";
            using (var req = new UnityWebRequest(SetUrl, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 15;
                yield return req.SendWebRequest();
                bool ok = req.responseCode == 200 && (req.downloadHandler.text ?? "").Contains("\"ok\":true");
                if (!ok) Debug.LogWarning("[CharSelect] save failed code=" + req.responseCode + " body=" + (req.downloadHandler != null ? req.downloadHandler.text : ""));
            }
            Auth.Character = e.key;   // lokal merken (auch falls Server gerade zickt)
            Hide();
            if (onConfirmed != null) onConfirmed();
        }

        void Hide()
        {
            if (canvasGo != null) canvasGo.SetActive(false);
            if (previewModel != null) Destroy(previewModel);
        }

        // ---- UI-Helfer ----
        Image MakeImage(string name, Transform parent, Color c)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>(); img.color = c; return img;
        }
        Text MakeText(string name, Transform parent, string s, int size, TextAnchor a)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>(); t.text = s;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size; t.alignment = a; t.color = Color.white; return t;
        }
        Button MakeButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>(); img.color = new Color(0.42f, 0.62f, 0.30f);
            var b = go.AddComponent<Button>(); b.onClick.AddListener(onClick);
            var t = MakeText("T", go.transform, label, 26, TextAnchor.MiddleCenter); Stretch(t.rectTransform);
            return b;
        }
        void Stretch(RectTransform rt) { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; }
        void Anchor(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }
    }
}
