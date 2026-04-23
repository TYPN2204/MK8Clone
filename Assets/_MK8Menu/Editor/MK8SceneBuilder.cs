#if UNITY_EDITOR
using System.IO;
using MK8.Menu.Effects;
using MK8.Menu.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace MK8.Menu.Editor
{
    /// <summary>
    /// Editor-only tool that creates the Boot, Frontend (placeholder), and Loading
    /// (placeholder) scenes from code, then updates Build Settings order.
    ///
    /// Usage: menu bar → MK8 → Scenes → Build All Scenes + Update Build Settings
    ///
    /// Re-running is safe — it overwrites existing placeholder scenes.
    /// </summary>
    public static class MK8SceneBuilder
    {
        // ── Paths ────────────────────────────────────────────────────────────────
        private const string ScenesFolder   = "Assets/_MK8Menu/Scenes";
        private const string BootScene      = "MK8Menu_Boot";
        private const string FrontendScene  = "MK8Menu_Frontend";
        private const string LoadingScene   = "MK8Menu_Loading";

        // ── Reference resolution (1920×1080) ────────────────────────────────────
        private static readonly Vector2 RefRes = new Vector2(1920f, 1080f);

        // ════════════════════════════════════════════════════════════════════════
        //  MENU ITEMS
        // ════════════════════════════════════════════════════════════════════════

        [MenuItem("MK8/Scenes/▶ Build ALL Scenes + Update Build Settings", priority = 0)]
        public static void BuildAll()
        {
            // Save current open scene first so the user doesn't lose work
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            BuildBootSceneInternal();
            BuildFrontendSceneInternal();
            BuildLoadingSceneInternal();
            UpdateBuildSettingsInternal();

            Debug.Log(
                "[MK8SceneBuilder] ✅ Done.\n" +
                "  Build Settings → 0: MK8Menu_Boot | 1: MK8Menu_Frontend | 2: MK8Menu_Loading\n" +
                "  Open MK8Menu_Boot and press Play to test the boot sequence.");
        }

        [MenuItem("MK8/Scenes/Build Boot Scene", priority = 11)]
        public static void BuildBootScene() => BuildBootSceneInternal();

        [MenuItem("MK8/Scenes/Build Frontend Scene (Placeholder)", priority = 12)]
        public static void BuildFrontendScene() => BuildFrontendSceneInternal();

        [MenuItem("MK8/Scenes/Build Loading Scene (Placeholder)", priority = 13)]
        public static void BuildLoadingScene() => BuildLoadingSceneInternal();

        [MenuItem("MK8/Scenes/Update Build Settings Order", priority = 24)]
        public static void UpdateBuildSettings() => UpdateBuildSettingsInternal();

        // ════════════════════════════════════════════════════════════════════════
        //  SCENE BUILDERS
        // ════════════════════════════════════════════════════════════════════════

        private static void BuildBootSceneInternal()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Main Camera (black background) ───────────────────────────────────
            var cameraGO = CreateCamera("Main Camera", Color.black);

            // ── EventSystem ──────────────────────────────────────────────────────
            CreateEventSystem();

            // ── Canvas (Screen Space Overlay, 1920×1080 scale) ───────────────────
            var canvasGO = CreateCanvas("Canvas_Boot");

            // ── LogoContainer: dark panel in center, CanvasGroup alpha=0 ─────────
            var logoContainer = CreateStretchPanel(canvasGO.transform, "LogoContainer",
                                                   new Color(0f, 0f, 0f, 0f)); // invisible bg
            var logoGroup = logoContainer.AddComponent<CanvasGroup>();
            logoGroup.alpha          = 0f;
            logoGroup.interactable   = false;
            logoGroup.blocksRaycasts = false;

            // Logo text placeholder (centered)
            var logoText = CreateTMPLabel(logoContainer.transform, "LogoText",
                                          "MARIOKART 8",
                                          fontSize: 96f,
                                          color: Color.white,
                                          size: new Vector2(800f, 180f),
                                          anchoredPos: Vector2.zero);

            // Sub-label
            CreateTMPLabel(logoContainer.transform, "LogoSubText",
                           "[placeholder — replace with logo sprite]",
                           fontSize: 24f,
                           color: new Color(1f, 1f, 1f, 0.6f),
                           size: new Vector2(600f, 40f),
                           anchoredPos: new Vector2(0f, -110f));

            // ── White Overlay (full-screen, alpha=0 initially) ────────────────────
            var whiteOverlayGO = CreateStretchPanel(canvasGO.transform, "WhiteOverlay", Color.white);
            var whiteGroup = whiteOverlayGO.AddComponent<CanvasGroup>();
            whiteGroup.alpha          = 0f;
            whiteGroup.interactable   = false;
            whiteGroup.blocksRaycasts = false;
            // Put white overlay last in hierarchy so it renders on top
            whiteOverlayGO.transform.SetAsLastSibling();

            // ── BootSequencer — wire SerializeField references ────────────────────
            var bootGO  = new GameObject("BootSequencer");
            var bootSeq = bootGO.AddComponent<MK8BootSequence>();

            var so = new SerializedObject(bootSeq);
            so.FindProperty("_logoGroup").objectReferenceValue    = logoGroup;
            so.FindProperty("_whiteOverlay").objectReferenceValue = whiteGroup;
            so.ApplyModifiedPropertiesWithoutUndo();

            // ── Save ─────────────────────────────────────────────────────────────
            var path = $"{ScenesFolder}/{BootScene}.unity";
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[MK8SceneBuilder] Saved: {path}");
        }

        private static void BuildFrontendSceneInternal()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Camera (white bg) ─────────────────────────────────────────────────
            CreateCamera("Main Camera", Color.white);
            CreateEventSystem();

            // ── Input Reader ──────────────────────────────────────────────────────
            var inputReaderGO = new GameObject("InputReader");
            var inputReader   = inputReaderGO.AddComponent<MK8InputReader>();
            var actionsAsset  = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/_MK8Menu/MK8MenuControls.inputactions");
            if (actionsAsset != null)
            {
                var soReader = new SerializedObject(inputReader);
                soReader.FindProperty("_actions").objectReferenceValue = actionsAsset;
                soReader.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning("[MK8SceneBuilder] MK8MenuControls.inputactions not found — " +
                                 "drag it onto InputReader._actions manually.");
            }

            // ── Canvas ────────────────────────────────────────────────────────────
            var canvasGO = CreateCanvas("Canvas_Frontend");

            // ── TitleScreen hierarchy ─────────────────────────────────────────────
            var titleScreen = BuildTitleScreenHierarchy(canvasGO.transform, inputReader);
            titleScreen.gameObject.SetActive(false);

            // ── MainMenuScreen hierarchy ──────────────────────────────────────────
            var mainMenuScreen = BuildMainMenuHierarchy(canvasGO.transform, inputReader);
            mainMenuScreen.gameObject.SetActive(false);

            // ── ModeSelectScreen hierarchy ────────────────────────────────────────
            var modeSelectScreen = BuildModeSelectHierarchy(canvasGO.transform, inputReader);
            modeSelectScreen.gameObject.SetActive(false);

            // ── White Overlay (alpha=1 on load; FrontendFlow + screens share it) ──
            var whiteGO    = CreateStretchPanel(canvasGO.transform, "WhiteOverlay", Color.white);
            var whiteGroup = whiteGO.AddComponent<CanvasGroup>();
            whiteGroup.alpha          = 1f;
            whiteGroup.interactable   = false;
            whiteGroup.blocksRaycasts = false;
            whiteGO.transform.SetAsLastSibling();

            // ── Wire cross-screen references ──────────────────────────────────────
            // TitleScreen → MainMenu
            {
                var so = new SerializedObject(titleScreen);
                so.FindProperty("_nextScreen").objectReferenceValue = mainMenuScreen;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            // MainMenu → ModeSelect + overlay
            {
                var so = new SerializedObject(mainMenuScreen);
                so.FindProperty("_nextScreen").objectReferenceValue  = modeSelectScreen;
                so.FindProperty("_sceneOverlay").objectReferenceValue = whiteGroup;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            // ModeSelect → overlay (nextScreen wired in Bước 6)
            {
                var so = new SerializedObject(modeSelectScreen);
                so.FindProperty("_sceneOverlay").objectReferenceValue = whiteGroup;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // ── FrontendFlow ──────────────────────────────────────────────────────
            var flowGO = new GameObject("FrontendFlow");
            var flow   = flowGO.AddComponent<MK8FrontendFlow>();

            var soFlow = new SerializedObject(flow);
            soFlow.FindProperty("_title").objectReferenceValue        = titleScreen;
            soFlow.FindProperty("_mainMenu").objectReferenceValue     = mainMenuScreen;
            soFlow.FindProperty("_modeSelect").objectReferenceValue   = modeSelectScreen;
            soFlow.FindProperty("_whiteOverlay").objectReferenceValue = whiteGroup;
            soFlow.ApplyModifiedPropertiesWithoutUndo();

            // ── Save ──────────────────────────────────────────────────────────────
            var path = $"{ScenesFolder}/{FrontendScene}.unity";
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[MK8SceneBuilder] Saved: {path}");
        }

        /// <summary>
        /// Creates the full TitleScreen GameObject hierarchy on the given canvas parent.
        /// Returns the MK8UI_TitleScreen component (root GO = "TitleScreen").
        /// </summary>
        private static MK8UI_TitleScreen BuildTitleScreenHierarchy(
            Transform canvasParent, MK8InputReader inputReader)
        {
            // Root ── stretch full canvas, own CanvasGroup (required by MK8UIScreen base)
            var rootGO   = new GameObject("TitleScreen");
            rootGO.transform.SetParent(canvasParent, false);
            var rootRect = rootGO.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var rootCG           = rootGO.AddComponent<CanvasGroup>();
            rootCG.alpha         = 1f;
            rootCG.interactable  = true;
            rootCG.blocksRaycasts = true;

            var titleScreen = rootGO.AddComponent<MK8UI_TitleScreen>();

            // ── Background (white) ────────────────────────────────────────────────
            CreateStretchPanel(rootGO.transform, "Background", Color.white);

            // ── Mario Kart placeholder (right-center) ─────────────────────────────
            var marioGO   = new GameObject("MarioKart_Placeholder");
            marioGO.transform.SetParent(rootGO.transform, false);
            var marioRect = marioGO.AddComponent<RectTransform>();
            marioRect.anchorMin        = new Vector2(0.5f, 0.5f);
            marioRect.anchorMax        = new Vector2(0.5f, 0.5f);
            marioRect.pivot            = new Vector2(0.5f, 0.5f);
            marioRect.anchoredPosition = new Vector2(480f, 20f);
            marioRect.sizeDelta        = new Vector2(380f, 460f);
            var marioImg   = marioGO.AddComponent<Image>();
            marioImg.color = new Color(0.75f, 0.75f, 0.75f, 1f); // gray placeholder
            marioImg.raycastTarget = false;
            CreateTMPLabel(marioGO.transform, "Label",
                           "[Mario on Kart\nplaceholder]",
                           18f, new Color(0.3f, 0.3f, 0.3f),
                           new Vector2(360f, 80f), Vector2.zero);

            // ── Bottom-left group: logo + "Ver. 4.1" ─────────────────────────────
            var logoContainerGO = new GameObject("LogoGroup");
            logoContainerGO.transform.SetParent(rootGO.transform, false);
            var logoContainerRect               = logoContainerGO.AddComponent<RectTransform>();
            logoContainerRect.anchorMin         = new Vector2(0f, 0f);
            logoContainerRect.anchorMax         = new Vector2(0f, 0f);
            logoContainerRect.pivot             = new Vector2(0f, 0f);
            logoContainerRect.anchoredPosition  = new Vector2(80f, 60f);
            logoContainerRect.sizeDelta         = new Vector2(440f, 200f);
            var logoGroup = logoContainerGO.AddComponent<CanvasGroup>();

            CreateTMPLabel(logoContainerGO.transform, "LogoText",
                           "MARIOKART 8",
                           64f, Color.black, new Vector2(440f, 100f), new Vector2(220f, 130f));
            CreateTMPLabel(logoContainerGO.transform, "VersionText",
                           "Ver. 4.1",
                           22f, new Color(0.3f, 0.3f, 0.3f), new Vector2(200f, 36f),
                           new Vector2(100f, 50f));

            // ── "Press A to start" group (alpha=0 initially) ──────────────────────
            var pressAGO   = new GameObject("PressAGroup");
            pressAGO.transform.SetParent(rootGO.transform, false);
            var pressARect = pressAGO.AddComponent<RectTransform>();
            pressARect.anchorMin        = new Vector2(0f, 0f);
            pressARect.anchorMax        = new Vector2(0f, 0f);
            pressARect.pivot            = new Vector2(0f, 0f);
            pressARect.anchoredPosition = new Vector2(80f, 280f);
            pressARect.sizeDelta        = new Vector2(440f, 60f);
            var pressAGroup             = pressAGO.AddComponent<CanvasGroup>();
            pressAGroup.alpha           = 0f;
            pressAGroup.interactable    = false;
            pressAGroup.blocksRaycasts  = false;

            // PressA text + MK8BlinkText component
            var pressATextGO = new GameObject("PressAText");
            pressATextGO.transform.SetParent(pressAGO.transform, false);
            var pressATextRect               = pressATextGO.AddComponent<RectTransform>();
            pressATextRect.anchorMin         = Vector2.zero;
            pressATextRect.anchorMax         = Vector2.one;
            pressATextRect.offsetMin         = Vector2.zero;
            pressATextRect.offsetMax         = Vector2.zero;
            var pressATMP                    = pressATextGO.AddComponent<TextMeshProUGUI>();
            pressATMP.text                   = "Press A  to start";   // Ⓐ replaced with plain A
            pressATMP.fontSize               = 30f;
            pressATMP.color                  = Color.black;
            pressATMP.alignment              = TextAlignmentOptions.Left;
            pressATMP.raycastTarget          = false;
            var blinkText                    = pressATextGO.AddComponent<MK8BlinkText>();

            // Wire MK8BlinkText._text via SerializedObject
            var soBlink = new SerializedObject(blinkText);
            soBlink.FindProperty("_text").objectReferenceValue = pressATMP;
            soBlink.ApplyModifiedPropertiesWithoutUndo();

            // ── Diagonal Blue Panel (off-screen) ──────────────────────────────────
            var panelGO   = new GameObject("DiagonalPanel");
            panelGO.transform.SetParent(rootGO.transform, false);
            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin        = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax        = new Vector2(0.5f, 0.5f);
            panelRect.pivot            = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = new Vector2(-2200f, 600f); // hidden pos (matches _panelHiddenPos)
            panelRect.sizeDelta        = new Vector2(1000f, 1600f);
            panelGO.transform.localEulerAngles = new Vector3(0f, 0f, -8f); // slight diagonal slant
            var panelImg   = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0f, 0.4f, 0.8f, 1f); // #0066CC blue

            // ── Ripple Effect (inactive, centered) ────────────────────────────────
            var rippleGO   = new GameObject("RippleEffect");
            rippleGO.transform.SetParent(rootGO.transform, false);
            var rippleRect = rippleGO.AddComponent<RectTransform>();
            rippleRect.anchorMin        = new Vector2(0.5f, 0.5f);
            rippleRect.anchorMax        = new Vector2(0.5f, 0.5f);
            rippleRect.pivot            = new Vector2(0.5f, 0.5f);
            rippleRect.anchoredPosition = Vector2.zero;
            rippleRect.sizeDelta        = new Vector2(200f, 200f);
            var rippleGroup             = rippleGO.AddComponent<CanvasGroup>();
            rippleGroup.alpha           = 0f;
            var rippleImg               = rippleGO.AddComponent<Image>();
            rippleImg.color             = new Color(0.4f, 0.65f, 1f, 0.85f);
            rippleImg.raycastTarget     = false;
            var rippleEffect            = rippleGO.AddComponent<MK8WaterRippleEffect>();
            rippleGO.SetActive(false);

            var soRipple = new SerializedObject(rippleEffect);
            soRipple.FindProperty("_rippleRect").objectReferenceValue  = rippleRect;
            soRipple.FindProperty("_rippleGroup").objectReferenceValue = rippleGroup;
            soRipple.ApplyModifiedPropertiesWithoutUndo();

            // ── Wire all MK8UI_TitleScreen SerializeFields ────────────────────────
            var soTitle = new SerializedObject(titleScreen);
            // base class field (private but [SerializeField] — accessible via SO)
            soTitle.FindProperty("_canvasGroup").objectReferenceValue    = rootCG;
            soTitle.FindProperty("_marioKartRect").objectReferenceValue  = marioRect;
            soTitle.FindProperty("_logoGroup").objectReferenceValue      = logoGroup;
            soTitle.FindProperty("_pressAGroup").objectReferenceValue    = pressAGroup;
            soTitle.FindProperty("_blinkText").objectReferenceValue      = blinkText;
            soTitle.FindProperty("_diagonalPanel").objectReferenceValue  = panelRect;
            soTitle.FindProperty("_rippleEffect").objectReferenceValue   = rippleEffect;
            soTitle.FindProperty("_inputReader").objectReferenceValue    = inputReader;
            // _nextScreen: wired after MainMenuScreen is created (see BuildFrontendSceneInternal)
            soTitle.ApplyModifiedPropertiesWithoutUndo();

            return titleScreen;
        }

        // ── MODE NAMES / CONFIG ──────────────────────────────────────────────────
        private static readonly string[] ModeNames     = { "Single Player", "Multiplayer",
            "Online - One Player", "Online - Two Players", "Mario Kart TV", "Shop" };
        private static readonly bool[]   ModeDisabled  = { false, false, true, true, true, true };
        // Placeholder colours for the right-side character scenes
        private static readonly Color[]  CharColors    =
        {
            new Color(0.85f, 0.92f, 1f),   // SP  — light blue
            new Color(1f, 0.85f, 0.92f),   // MP  — light pink
            new Color(0.85f, 1f, 0.88f),   // OL1 — light green
            new Color(0.92f, 0.85f, 1f),   // OL2 — light purple
            new Color(1f, 0.95f, 0.75f),   // TV  — light yellow
            new Color(0.9f, 0.9f, 0.9f),   // Shop — light gray
        };

        /// <summary>Builds the MainMenu screen hierarchy. Returns MK8UI_MainMenuScreen.</summary>
        private static MK8UI_MainMenuScreen BuildMainMenuHierarchy(
            Transform canvasParent, MK8InputReader inputReader)
        {
            // ── Root (full-canvas stretch + CanvasGroup) ──────────────────────────
            var rootGO   = new GameObject("MainMenuScreen");
            rootGO.transform.SetParent(canvasParent, false);
            var rootRect = rootGO.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
            var rootCG   = rootGO.AddComponent<CanvasGroup>();
            var mainMenu = rootGO.AddComponent<MK8UI_MainMenuScreen>();

            // ── Left Blue Panel ───────────────────────────────────────────────────
            var leftPanelGO   = new GameObject("LeftPanel");
            leftPanelGO.transform.SetParent(rootGO.transform, false);
            var leftRect      = leftPanelGO.AddComponent<RectTransform>();
            leftRect.anchorMin        = new Vector2(0f, 0f);
            leftRect.anchorMax        = new Vector2(0f, 1f);
            leftRect.pivot            = new Vector2(0f, 0.5f);
            leftRect.anchoredPosition = Vector2.zero;
            leftRect.sizeDelta        = new Vector2(540f, 0f);   // 540px wide, full height
            var leftImg  = leftPanelGO.AddComponent<Image>();
            leftImg.color = new Color(0f, 0.4f, 0.8f, 1f);      // #0066CC blue
            leftImg.raycastTarget = false;

            // ── Mode Items Container (manual layout — no LayoutGroup to avoid reorder bugs)
            var itemsGO   = new GameObject("ModeItemsContainer");
            itemsGO.transform.SetParent(leftPanelGO.transform, false);
            var itemsRect = itemsGO.AddComponent<RectTransform>();
            itemsRect.anchorMin = Vector2.zero;
            itemsRect.anchorMax = Vector2.one;
            itemsRect.offsetMin = new Vector2(40f, 80f);
            itemsRect.offsetMax = new Vector2(-10f, -60f);

            var modeItems = new MK8UI_ModeItem[6];
            float itemH   = 72f;
            float itemGap = 8f;
            float totalH  = 6 * itemH + 5 * itemGap;
            float startY  = totalH * 0.5f - itemH * 0.5f;  // top item Y offset from center

            for (int i = 0; i < 6; i++)
            {
                float yPos    = startY - i * (itemH + itemGap);
                modeItems[i]  = CreateModeItem(itemsGO.transform, ModeNames[i],
                                               ModeDisabled[i], yPos, itemH);
            }

            // ── Arrow Indicators (inside left panel, on edges of items container) ─
            var arrowLeft  = CreateArrowIndicator(leftPanelGO.transform, "ArrowLeft",
                                                   new Vector2(10f, 0f), isLeft: true);
            var arrowRight = CreateArrowIndicator(leftPanelGO.transform, "ArrowRight",
                                                   new Vector2(490f, 0f), isLeft: false);
            // Snap arrows to first item Y initially
            if (modeItems[0] != null)
            {
                float firstY = startY;
                if (arrowLeft.GetComponent<RectTransform>()  != null)
                    arrowLeft.GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(10f, firstY);
                if (arrowRight.GetComponent<RectTransform>() != null)
                    arrowRight.GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(490f, firstY);
            }

            // ── Right Character Area ───────────────────────────────────────────────
            var rightGO   = new GameObject("RightCharacterArea");
            rightGO.transform.SetParent(rootGO.transform, false);
            var rightRect = rightGO.AddComponent<RectTransform>();
            rightRect.anchorMin        = new Vector2(0f, 0f);
            rightRect.anchorMax        = new Vector2(1f, 1f);
            rightRect.offsetMin        = new Vector2(560f, 0f);   // starts after left panel
            rightRect.offsetMax        = Vector2.zero;

            var charScenes = new CanvasGroup[6];
            for (int i = 0; i < 6; i++)
            {
                var sceneGO   = new GameObject($"CharScene_{ModeNames[i].Replace(" ", "")}");
                sceneGO.transform.SetParent(rightGO.transform, false);
                var sceneRect = sceneGO.AddComponent<RectTransform>();
                sceneRect.anchorMin = Vector2.zero;
                sceneRect.anchorMax = Vector2.one;
                sceneRect.offsetMin = sceneRect.offsetMax = Vector2.zero;
                charScenes[i] = sceneGO.AddComponent<CanvasGroup>();
                charScenes[i].alpha = i == 0 ? 1f : 0f;
                sceneGO.SetActive(i == 0);

                // Background colour
                var bgImg  = sceneGO.AddComponent<Image>();
                bgImg.color = CharColors[i];
                bgImg.raycastTarget = false;

                // Label
                CreateTMPLabel(sceneGO.transform, "Label",
                    $"[{ModeNames[i]}]\nCharacter Scene Placeholder\n(replace with sprite art)",
                    28f, new Color(0.25f, 0.25f, 0.25f),
                    new Vector2(500f, 120f), Vector2.zero);
            }

            // ── Bottom Bar ────────────────────────────────────────────────────────
            var bottomGO   = new GameObject("BottomBar");
            bottomGO.transform.SetParent(rootGO.transform, false);
            var bottomRect = bottomGO.AddComponent<RectTransform>();
            bottomRect.anchorMin        = new Vector2(0f, 0f);
            bottomRect.anchorMax        = new Vector2(1f, 0f);
            bottomRect.pivot            = new Vector2(0.5f, 0f);
            bottomRect.anchoredPosition = Vector2.zero;
            bottomRect.sizeDelta        = new Vector2(0f, 60f);
            var bottomImg  = bottomGO.AddComponent<Image>();
            bottomImg.color = new Color(0f, 0f, 0f, 0.35f);
            bottomImg.raycastTarget = false;

            // Coin counter "38/100" (Section 5.3)
            CreateTMPLabel(bottomGO.transform, "CoinCounter",
                "38 / 100", 26f, Color.white,
                new Vector2(160f, 48f), new Vector2(-800f, 0f));

            // Bottom icons placeholder
            CreateTMPLabel(bottomGO.transform, "BottomIcons",
                "[icons placeholder]", 20f, new Color(1f, 1f, 1f, 0.7f),
                new Vector2(300f, 48f), new Vector2(-530f, 0f));

            // ── Wire MK8UI_MainMenuScreen ─────────────────────────────────────────
            var soMain = new SerializedObject(mainMenu);
            soMain.FindProperty("_canvasGroup").objectReferenceValue = rootCG;
            soMain.FindProperty("_inputReader").objectReferenceValue = inputReader;

            // Wire mode items array
            var itemsProp = soMain.FindProperty("_modeItems");
            itemsProp.arraySize = 6;
            for (int i = 0; i < 6; i++)
                itemsProp.GetArrayElementAtIndex(i).objectReferenceValue = modeItems[i];

            // Wire char scenes array
            var charProp = soMain.FindProperty("_charScenes");
            charProp.arraySize = 6;
            for (int i = 0; i < 6; i++)
                charProp.GetArrayElementAtIndex(i).objectReferenceValue = charScenes[i];

            soMain.FindProperty("_arrowLeft").objectReferenceValue  =
                arrowLeft.GetComponent<MK8UI_ArrowIndicator>();
            soMain.FindProperty("_arrowRight").objectReferenceValue =
                arrowRight.GetComponent<MK8UI_ArrowIndicator>();
            // _sceneOverlay wired after WhiteOverlay is created (in BuildFrontendSceneInternal)
            // _nextScreen: wired in Bước 5
            soMain.ApplyModifiedPropertiesWithoutUndo();

            return mainMenu;
        }

        private static MK8UI_ModeItem CreateModeItem(
            Transform parent, string name, bool disabled, float yPos, float height)
        {
            var go   = new GameObject($"ModeItem_{name.Replace(" ", "")}");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin        = new Vector2(0f, 0.5f);
            rect.anchorMax        = new Vector2(1f, 0.5f);
            rect.pivot            = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, yPos);
            rect.sizeDelta        = new Vector2(0f, height);

            var item = go.AddComponent<MK8UI_ModeItem>();

            // BgNormal (white semi-transparent)
            var bgNormalGO = new GameObject("BgNormal");
            bgNormalGO.transform.SetParent(go.transform, false);
            StretchRect(bgNormalGO);
            var bgNormal = bgNormalGO.AddComponent<Image>();
            bgNormal.color = new Color(1f, 1f, 1f, disabled ? 0.12f : 0.18f);
            bgNormal.raycastTarget = false;

            // BgSelected (yellow)
            var bgSelGO = new GameObject("BgSelected");
            bgSelGO.transform.SetParent(go.transform, false);
            StretchRect(bgSelGO);
            var bgSel   = bgSelGO.AddComponent<Image>();
            bgSel.color = new Color(1f, 0.85f, 0f, 1f); // gold yellow
            bgSel.raycastTarget = false;
            bgSelGO.SetActive(false); // hidden by default

            // Label
            var labelGO   = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin        = new Vector2(0f, 0f);
            labelRect.anchorMax        = new Vector2(1f, 1f);
            labelRect.offsetMin        = new Vector2(20f, 0f);
            labelRect.offsetMax        = Vector2.zero;
            var label                  = labelGO.AddComponent<TextMeshProUGUI>();
            label.text                 = name;
            label.fontSize             = 28f;
            label.color                = disabled ? new Color(0.55f, 0.55f, 0.55f) : Color.white;
            label.alignment            = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget        = false;

            // Disabled overlay
            var disGO   = new GameObject("DisabledOverlay");
            disGO.transform.SetParent(go.transform, false);
            StretchRect(disGO);
            var disImg  = disGO.AddComponent<Image>();
            disImg.color = new Color(0f, 0f, 0f, disabled ? 0.3f : 0f);
            disImg.raycastTarget = false;
            var disCG   = disGO.AddComponent<CanvasGroup>();

            // Wire item's serialized fields
            var so = new SerializedObject(item);
            so.FindProperty("_bgNormal").objectReferenceValue       = bgNormal;
            so.FindProperty("_bgSelected").objectReferenceValue     = bgSel;
            so.FindProperty("_label").objectReferenceValue          = label;
            so.FindProperty("_disabledOverlay").objectReferenceValue = disCG;
            so.FindProperty("_displayName").stringValue             = name;
            so.FindProperty("_isDisabled").boolValue                = disabled;
            so.ApplyModifiedPropertiesWithoutUndo();

            return item;
        }

        private static GameObject CreateArrowIndicator(
            Transform parent, string name, Vector2 anchoredPos, bool isLeft)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin        = new Vector2(0f, 0.5f);
            rect.anchorMax        = new Vector2(0f, 0.5f);
            rect.pivot            = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta        = new Vector2(28f, 48f);
            var cg = go.AddComponent<CanvasGroup>();

            // Base arrow image (solid white triangle placeholder)
            var baseGO  = new GameObject("ArrowBase");
            baseGO.transform.SetParent(go.transform, false);
            StretchRect(baseGO);
            var baseImg = baseGO.AddComponent<Image>();
            baseImg.color = Color.white;
            baseImg.raycastTarget = false;

            // Glow image (slightly larger, yellow, starts at 0.3 alpha)
            var glowGO  = new GameObject("ArrowGlow");
            glowGO.transform.SetParent(go.transform, false);
            var glowRect = glowGO.AddComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 0.5f);
            glowRect.anchorMax = new Vector2(0.5f, 0.5f);
            glowRect.sizeDelta = new Vector2(36f, 56f);
            var glowImg  = glowGO.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.95f, 0.3f, 0.3f);
            glowImg.raycastTarget = false;

            var indicator = go.AddComponent<MK8UI_ArrowIndicator>();
            var so        = new SerializedObject(indicator);
            so.FindProperty("_arrowRect").objectReferenceValue    = rect;
            so.FindProperty("_glowImage").objectReferenceValue    = glowImg;
            so.FindProperty("_canvasGroup").objectReferenceValue  = cg;
            so.ApplyModifiedPropertiesWithoutUndo();

            return go;
        }

        // ── MODE SELECT DATA ─────────────────────────────────────────────────────
        private static readonly string[] ModeSelectNames    =
            { "Grand Prix", "Time Trials", "VS Race", "Battle" };
        private static readonly Color[]  ModeSelectBadge    =
        {
            new Color(0.2f, 0.5f, 1f),   // Grand Prix — blue
            new Color(0.2f, 0.75f, 0.3f),// Time Trials — green
            new Color(1f, 0.5f, 0.1f),   // VS Race — orange
            new Color(0.9f, 0.2f, 0.2f), // Battle — red
        };
        private static readonly Color[] ModeSelectPreview =
        {
            new Color(0.8f, 0.88f, 1f),
            new Color(0.8f, 1f, 0.84f),
            new Color(1f, 0.9f, 0.75f),
            new Color(1f, 0.78f, 0.78f),
        };

        private static MK8UI_ModeSelectScreen BuildModeSelectHierarchy(
            Transform canvasParent, MK8InputReader inputReader)
        {
            // Root
            var rootGO   = new GameObject("ModeSelectScreen");
            rootGO.transform.SetParent(canvasParent, false);
            var rootRect = rootGO.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
            var rootCG   = rootGO.AddComponent<CanvasGroup>();
            var screen   = rootGO.AddComponent<MK8UI_ModeSelectScreen>();

            // Background
            CreateStretchPanel(rootGO.transform, "Background", new Color(0.96f, 0.96f, 0.96f));

            // ── Left Panel (blue, ~45%) ────────────────────────────────────────────
            var leftGO   = new GameObject("LeftPanel");
            leftGO.transform.SetParent(rootGO.transform, false);
            var leftRect = leftGO.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0f, 0f);
            leftRect.anchorMax = new Vector2(0f, 1f);
            leftRect.pivot     = new Vector2(0f, 0.5f);
            leftRect.anchoredPosition = Vector2.zero;
            leftRect.sizeDelta = new Vector2(540f, 0f);
            leftGO.AddComponent<Image>().color = new Color(0f, 0.4f, 0.8f, 1f);

            // Screen title
            CreateTMPLabel(leftGO.transform, "ScreenTitle",
                "SELECT GAME MODE", 22f, new Color(1f, 1f, 1f, 0.7f),
                new Vector2(460f, 36f), new Vector2(230f, -30f));

            // Mode items (4 items)
            var itemsContainer = new GameObject("ModeItemsContainer");
            itemsContainer.transform.SetParent(leftGO.transform, false);
            var iRect = itemsContainer.AddComponent<RectTransform>();
            iRect.anchorMin = Vector2.zero;
            iRect.anchorMax = Vector2.one;
            iRect.offsetMin = new Vector2(44f, 100f);
            iRect.offsetMax = new Vector2(-10f, -80f);

            var modeItems = new MK8UI_ModeItem[4];
            float ih = 88f, igap = 12f;
            float totalH = 4 * ih + 3 * igap;
            float startY = totalH * 0.5f - ih * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                float yPos   = startY - i * (ih + igap);
                var item     = CreateModeItem(itemsContainer.transform,
                                              ModeSelectNames[i], false, yPos, ih);
                // Badge colour strip on left of item (visual only)
                var badgeGO  = new GameObject("Badge");
                badgeGO.transform.SetParent(item.gameObject.transform, false);
                var badgeRect = badgeGO.AddComponent<RectTransform>();
                badgeRect.anchorMin = new Vector2(0f, 0f);
                badgeRect.anchorMax = new Vector2(0f, 1f);
                badgeRect.pivot     = new Vector2(0f, 0.5f);
                badgeRect.anchoredPosition = Vector2.zero;
                badgeRect.sizeDelta = new Vector2(12f, 0f);
                badgeGO.AddComponent<Image>().color = ModeSelectBadge[i];
                modeItems[i] = item;
            }

            // Arrow indicator (left edge of items)
            var arrowGO = CreateArrowIndicator(leftGO.transform, "Arrow",
                                               new Vector2(14f, startY), isLeft: true);
            var arrowIndicator = arrowGO.GetComponent<MK8UI_ArrowIndicator>();

            // ── Right Preview Area ─────────────────────────────────────────────────
            var rightGO   = new GameObject("PreviewArea");
            rightGO.transform.SetParent(rootGO.transform, false);
            var rightRect = rightGO.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0f, 0f);
            rightRect.anchorMax = new Vector2(1f, 1f);
            rightRect.offsetMin = new Vector2(560f, 60f);
            rightRect.offsetMax = new Vector2(-20f, -60f);

            var previewPanels = new CanvasGroup[4];
            TextMeshProUGUI tooltipText = null;

            for (int i = 0; i < 4; i++)
            {
                var pGO   = new GameObject($"Preview_{ModeSelectNames[i].Replace(" ", "")}");
                pGO.transform.SetParent(rightGO.transform, false);
                var pRect = pGO.AddComponent<RectTransform>();
                pRect.anchorMin = Vector2.zero;
                pRect.anchorMax = Vector2.one;
                pRect.offsetMin = pRect.offsetMax = Vector2.zero;
                previewPanels[i] = pGO.AddComponent<CanvasGroup>();
                previewPanels[i].alpha = i == 0 ? 1f : 0f;
                pGO.SetActive(i == 0);

                var bgImg = pGO.AddComponent<Image>();
                bgImg.color = ModeSelectPreview[i];
                bgImg.raycastTarget = false;

                // Preview label
                CreateTMPLabel(pGO.transform, "PreviewLabel",
                    $"[{ModeSelectNames[i]} Preview]\n(replace with sprite sequence)",
                    24f, new Color(0.2f, 0.2f, 0.2f),
                    new Vector2(500f, 80f), new Vector2(0f, 40f));

                // Tooltip overlay (top-right, dark bg + text)
                var tipGO   = new GameObject("TooltipOverlay");
                tipGO.transform.SetParent(pGO.transform, false);
                var tipRect = tipGO.AddComponent<RectTransform>();
                tipRect.anchorMin        = new Vector2(1f, 1f);
                tipRect.anchorMax        = new Vector2(1f, 1f);
                tipRect.pivot            = new Vector2(1f, 1f);
                tipRect.anchoredPosition = new Vector2(-10f, -10f);
                tipRect.sizeDelta        = new Vector2(380f, 52f);
                var tipBg  = tipGO.AddComponent<Image>();
                tipBg.color = new Color(0f, 0f, 0f, 0.6f);
                tipBg.raycastTarget = false;

                if (i == 0)
                {
                    var tipLabel = CreateTMPLabel(tipGO.transform, "TooltipText",
                        "Go for gold in a 4-race cup!", 20f, Color.white,
                        new Vector2(360f, 48f), Vector2.zero);
                    tooltipText = tipLabel.GetComponent<TextMeshProUGUI>();
                }
            }

            // ── Bottom Bar ────────────────────────────────────────────────────────
            var botGO   = new GameObject("BottomBar");
            botGO.transform.SetParent(rootGO.transform, false);
            var botRect = botGO.AddComponent<RectTransform>();
            botRect.anchorMin = new Vector2(0f, 0f);
            botRect.anchorMax = new Vector2(1f, 0f);
            botRect.pivot     = new Vector2(0.5f, 0f);
            botRect.anchoredPosition = Vector2.zero;
            botRect.sizeDelta = new Vector2(0f, 56f);
            botGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.3f);

            CreateTMPLabel(botGO.transform, "BackPrompt",
                "◀  B", 24f, Color.white, new Vector2(100f, 48f), new Vector2(-880f, 0f));
            CreateTMPLabel(botGO.transform, "ConfirmPrompt",
                "Ⓐ  OK", 24f, Color.white, new Vector2(100f, 48f), new Vector2(880f, 0f));

            // ── Wire MK8UI_ModeSelectScreen ───────────────────────────────────────
            var so = new SerializedObject(screen);
            so.FindProperty("_canvasGroup").objectReferenceValue  = rootCG;
            so.FindProperty("_inputReader").objectReferenceValue  = inputReader;
            so.FindProperty("_arrow").objectReferenceValue        = arrowIndicator;
            if (tooltipText != null)
                so.FindProperty("_tooltipText").objectReferenceValue = tooltipText;

            var itemsProp = so.FindProperty("_modeItems");
            itemsProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                itemsProp.GetArrayElementAtIndex(i).objectReferenceValue = modeItems[i];

            var previewProp = so.FindProperty("_previewPanels");
            previewProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                previewProp.GetArrayElementAtIndex(i).objectReferenceValue = previewPanels[i];

            // _sceneOverlay + _nextScreen wired after creation in BuildFrontendSceneInternal
            so.ApplyModifiedPropertiesWithoutUndo();

            return screen;
        }

        private static void StretchRect(GameObject go)
        {
            var r    = go.AddComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        private static void BuildLoadingSceneInternal()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera("Main Camera", Color.white);

            var canvasGO = CreateCanvas("Canvas_Loading");

            CreateStretchPanel(canvasGO.transform, "Background", Color.white);

            CreateTMPLabel(canvasGO.transform, "PlaceholderLabel",
                           "MK8Menu_Loading\n[Placeholder — Bước 11 sẽ thêm kart parade]",
                           fontSize: 32f,
                           color: new Color(0.2f, 0.2f, 0.2f, 1f),
                           size: new Vector2(800f, 120f),
                           anchoredPos: Vector2.zero);

            var path = $"{ScenesFolder}/{LoadingScene}.unity";
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[MK8SceneBuilder] Saved: {path}");
        }

        // ════════════════════════════════════════════════════════════════════════
        //  BUILD SETTINGS
        // ════════════════════════════════════════════════════════════════════════

        private static void UpdateBuildSettingsInternal()
        {
            var bootPath     = $"{ScenesFolder}/{BootScene}.unity";
            var frontPath    = $"{ScenesFolder}/{FrontendScene}.unity";
            var loadPath     = $"{ScenesFolder}/{LoadingScene}.unity";

            bool bootExists  = File.Exists(bootPath);
            bool frontExists = File.Exists(frontPath);
            bool loadExists  = File.Exists(loadPath);

            if (!bootExists || !frontExists || !loadExists)
            {
                Debug.LogWarning(
                    "[MK8SceneBuilder] Some scene files not found. " +
                    "Run 'Build ALL Scenes' first, then update Build Settings.\n" +
                    $"  Boot    exists: {bootExists}\n" +
                    $"  Frontend exists: {frontExists}\n" +
                    $"  Loading exists: {loadExists}");
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(bootPath,  bootExists),
                new EditorBuildSettingsScene(frontPath, frontExists),
                new EditorBuildSettingsScene(loadPath,  loadExists),
            };

            Debug.Log("[MK8SceneBuilder] Build Settings updated:\n" +
                      "  [0] MK8Menu_Boot\n  [1] MK8Menu_Frontend\n  [2] MK8Menu_Loading");
        }

        // ════════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ════════════════════════════════════════════════════════════════════════

        private static GameObject CreateCamera(string name, Color bgColor)
        {
            var go  = new GameObject(name);
            go.tag  = "MainCamera";
            var cam = go.AddComponent<Camera>();
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = bgColor;
            cam.depth           = -1;
            go.AddComponent<AudioListener>();

            // URP additional data (required for URP camera settings)
            var urpData = go.AddComponent<UniversalAdditionalCameraData>();
            urpData.renderPostProcessing = false;
            return go;
        }

        private static GameObject CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            return go;
        }

        private static GameObject CreateCanvas(string name)
        {
            var go     = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = RefRes;
            scaler.matchWidthOrHeight  = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        /// <summary>Stretch-fill panel (anchor min=0,0 max=1,1).</summary>
        private static GameObject CreateStretchPanel(Transform parent, string name, Color color)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var img   = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return go;
        }

        /// <summary>Centered TextMeshPro label.</summary>
        private static GameObject CreateTMPLabel(Transform parent, string name,
                                                  string text, float fontSize,
                                                  Color color, Vector2 size,
                                                  Vector2 anchoredPos)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin        = new Vector2(0.5f, 0.5f);
            rect.anchorMax        = new Vector2(0.5f, 0.5f);
            rect.pivot            = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta        = size;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text                        = text;
            tmp.fontSize                    = fontSize;
            tmp.color                       = color;
            tmp.alignment                   = TextAlignmentOptions.Center;
            tmp.enableWordWrapping          = true;
            tmp.overflowMode                = TextOverflowModes.Overflow;
            tmp.raycastTarget               = false;
            return go;
        }
    }
}
#endif
