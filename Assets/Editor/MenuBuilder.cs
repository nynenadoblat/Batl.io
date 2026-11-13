using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BatlIO.UI;

namespace BatlIO.Editor
{
    public static class MenuBuilder
    {
        private const string MenuScenePath = "Assets/Scenes/Menu.unity";

        private const string FontYandexPath = "Assets/Plugin/yandex-sans-text-2 SDF.asset";
        private const string FontSegoePath = "Assets/Plugin/segoe-ui-7 SDF.asset";
        private const string FontSegoeDynamicPath = "Assets/Plugin/SegoeUI_Dynamic.asset";
        private const string FontLiberationPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        private const string SpriteBgSmall = "Assets/Sprites/UI/ProgressBar/UI Background Small.png";
        private const string SpriteOutlineSmall = "Assets/Sprites/UI/ProgressBar/Outline Small.png";
        private const string SpriteGlowBar = "Assets/Sprites/UI/ProgressBar/Glow Bar.png";
        private const string SpriteFgGradient = "Assets/Sprites/UI/ProgressBar/Foreground With Gradient 1.png";
        private const string SpriteRing3 = "Assets/Sprites/Ring3.png";
        private const string SpriteRing4 = "Assets/Sprites/Ring4.png";
        private const string SpriteSpark2 = "Assets/Sprites/Spark2.png";
        private const string SpriteIconDot = "Assets/Sprites/[Dot.io] Icon x512.png";

        private const string IconDamage = "Assets/Sprites/UI/Icon Damage.psd";
        private const string IconArmor = "Assets/Sprites/UI/Icon Armor.psd";
        private const string IconFireRate = "Assets/Sprites/UI/Icon FireRate.psd";
        private const string IconHealth = "Assets/Sprites/UI/Icon Health.psd";
        private const string IconCrit = "Assets/Sprites/UI/Icon Crit Power.psd";

        [MenuItem("BatlIO/Inspect Menu State")]
        public static string InspectState()
        {
            var go = GameObject.Find("TitleText");
            var tmp = go != null ? go.GetComponent<TextMeshProUGUI>() : null;
            var c = GameObject.Find("Canvas");
            var fontName = tmp != null && tmp.font != null ? tmp.font.name : "null";
            return "TitleText=" + (go != null ? "Found" : "Missing") + ", Font=" + fontName + ", CanvasChildren=" + (c != null ? c.transform.childCount : 0);
        }

        private static TMP_FontAsset GetFullCyrillicFont()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontSegoeDynamicPath);
            if (font == null)
            {
                var sourceFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Plugin/segoe-ui-7.ttf");
                if (sourceFont != null)
                {
                    font = TMP_FontAsset.CreateFontAsset(sourceFont, 64, 6, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
                    font.name = "SegoeUI_Dynamic";
                    AssetDatabase.CreateAsset(font, FontSegoeDynamicPath);
                    AssetDatabase.AddObjectToAsset(font.material, font);
                    AssetDatabase.AddObjectToAsset(font.atlasTexture, font);
                    AssetDatabase.SaveAssets();
                }
            }

            if (font != null)
            {
                // Ensure all uppercase/lowercase Cyrillic, Latin, and symbols are pre-cached
                font.TryAddCharacters("АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюяABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,:;!?-+()[]{}/*<>=_●•–— ");
                EditorUtility.SetDirty(font);
                return font;
            }

            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontSegoePath);
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontYandexPath);
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontLiberationPath);
            return font;
        }

        [MenuItem("BatlIO/Build Beautiful Menu")]
        public static void BuildMenu()
        {
            Debug.Log("[MenuBuilder] Building Beautiful Canvas Menu...");

            var scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);

            // 1. Camera setup
            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.045f, 0.065f, 0.11f, 1f); // Deep cyber midnight
                EditorUtility.SetDirty(cam);
            }

            // 2. EventSystem setup
            var es = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 3. Find or Create Canvas
            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null)
            {
                canvasGo = new GameObject("Canvas");
            }

            // Clear old children under Canvas to rebuild clean hierarchy
            for (int i = canvasGo.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(canvasGo.transform.GetChild(i).gameObject);
            }

            var canvas = canvasGo.GetComponent<Canvas>();
            if (canvas == null) canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var raycaster = canvasGo.GetComponent<GraphicRaycaster>();
            if (raycaster == null) canvasGo.AddComponent<GraphicRaycaster>();

            var mainMenuUI = canvasGo.GetComponent<MainMenuUI>();
            if (mainMenuUI == null) mainMenuUI = canvasGo.AddComponent<MainMenuUI>();

            // Load best Cyrillic font: Segoe UI with full dynamic uppercase/lowercase Cyrillic support
            var font = GetFullCyrillicFont();

            var sBgSmall = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteBgSmall);
            var sOutlineSmall = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteOutlineSmall);
            var sGlowBar = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteGlowBar);
            var sFgGradient = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFgGradient);
            var sRing3 = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteRing3);
            var sRing4 = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteRing4);
            var sSpark2 = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteSpark2);

            // ==========================================
            // LAYER 1: BACKGROUND & AMBIENT
            // ==========================================
            var bgContainer = CreateUIObject("BackgroundContainer", canvasGo.transform);
            SetStretchAll(bgContainer);

            // Soft radial ambient haze
            var bgGlowCenter = CreateUIObject("AmbientNebulaGlow", bgContainer.transform);
            SetAnchors(bgGlowCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(1400, 1000));
            var imgNebula = bgGlowCenter.AddComponent<Image>();
            imgNebula.sprite = sRing3;
            imgNebula.color = new Color(0.04f, 0.22f, 0.42f, 0.22f);
            imgNebula.raycastTarget = false;

            // Ambient particles animator
            var ambientParticles = bgContainer.AddComponent<UIAmbientParticles>();

            // Floating Cyber Dots
            CreateAmbientOrb(bgContainer.transform, ambientParticles, sRing3, new Vector2(-650, 320), new Vector2(90, 90), new Color(0f, 0.95f, 1f, 0.32f), 0.8f, 25f, 20f, 0.15f, 0.4f);
            CreateAmbientOrb(bgContainer.transform, ambientParticles, sRing3, new Vector2(680, 240), new Vector2(110, 110), new Color(1f, 0.45f, 0.15f, 0.28f), 0.6f, 30f, 25f, 0.1f, 0.35f);
            CreateAmbientOrb(bgContainer.transform, ambientParticles, sRing3, new Vector2(-520, -260), new Vector2(75, 75), new Color(0.15f, 0.75f, 1f, 0.25f), 1.1f, 20f, 30f, 0.15f, 0.35f);
            CreateAmbientOrb(bgContainer.transform, ambientParticles, sRing3, new Vector2(560, -310), new Vector2(85, 85), new Color(1f, 0.35f, 0.55f, 0.24f), 0.9f, 25f, 20f, 0.12f, 0.35f);
            CreateAmbientOrb(bgContainer.transform, ambientParticles, sRing4, new Vector2(0, 180), new Vector2(280, 280), new Color(0f, 0.85f, 1f, 0.08f), 0.4f, 15f, 15f, 0.04f, 0.12f);

            // ==========================================
            // LAYER 2: TOP BAR
            // ==========================================
            var topBar = CreateUIObject("TopBar", canvasGo.transform);
            SetAnchors(topBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(0, 140));

            // Profile Card (Top Left)
            var profileCard = CreateUIObject("PlayerProfileCard", topBar.transform);
            SetAnchors(profileCard, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(50, -4), new Vector2(740, 126));
            var imgProfileBg = profileCard.AddComponent<Image>();
            imgProfileBg.sprite = sBgSmall;
            imgProfileBg.type = Image.Type.Sliced;
            imgProfileBg.color = new Color(0.08f, 0.12f, 0.22f, 0.92f);
            imgProfileBg.raycastTarget = false;

            var profileBorder = CreateUIObject("Border", profileCard.transform);
            SetStretchAll(profileBorder);
            var imgProfileBorder = profileBorder.AddComponent<Image>();
            imgProfileBorder.sprite = sOutlineSmall;
            imgProfileBorder.type = Image.Type.Sliced;
            imgProfileBorder.color = new Color(0f, 0.85f, 1f, 0.45f);
            imgProfileBorder.raycastTarget = false;

            // Avatar Frame (Glowing Tank Dot Emblem)
            var avatarFrame = CreateUIObject("AvatarFrame", profileCard.transform);
            SetAnchors(avatarFrame, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(84, 84));
            var imgAvatarGlow = avatarFrame.AddComponent<Image>();
            imgAvatarGlow.sprite = sRing3;
            imgAvatarGlow.color = new Color(0f, 0.95f, 1f, 0.6f);
            imgAvatarGlow.raycastTarget = false;

            var avatarInner = CreateUIObject("InnerDot", avatarFrame.transform);
            SetAnchors(avatarInner, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(48, 48));
            var imgInnerDot = avatarInner.AddComponent<Image>();
            imgInnerDot.sprite = sRing3;
            imgInnerDot.color = new Color(1f, 1f, 1f, 1f);
            imgInnerDot.raycastTarget = false;

            // Nickname InputField
            var nickInputGo = CreateUIObject("NicknameInputField", profileCard.transform);
            SetAnchors(nickInputGo, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(126, 22), new Vector2(590, 52));
            var imgNickInput = nickInputGo.AddComponent<Image>();
            imgNickInput.sprite = sBgSmall;
            imgNickInput.type = Image.Type.Sliced;
            imgNickInput.color = new Color(0.05f, 0.08f, 0.16f, 0.75f);

            var nickTextArea = CreateUIObject("Text Area", nickInputGo.transform);
            SetStretchMargins(nickTextArea, 16, 14, 4, 4);
            nickTextArea.AddComponent<RectMask2D>();

            var nickPlaceholder = CreateUIObject("Placeholder", nickTextArea.transform);
            SetStretchAll(nickPlaceholder);
            var tmpNickPh = nickPlaceholder.AddComponent<TextMeshProUGUI>();
            tmpNickPh.font = font;
            tmpNickPh.text = "Введите ник...";
            tmpNickPh.fontSize = 32;
            tmpNickPh.fontStyle = FontStyles.Italic;
            tmpNickPh.color = new Color(0.5f, 0.65f, 0.85f, 0.55f);
            tmpNickPh.alignment = TextAlignmentOptions.MidlineLeft;
            tmpNickPh.raycastTarget = false;

            var nickText = CreateUIObject("Text", nickTextArea.transform);
            SetStretchAll(nickText);
            var tmpNickText = nickText.AddComponent<TextMeshProUGUI>();
            tmpNickText.font = font;
            tmpNickText.text = "";
            tmpNickText.fontSize = 32;
            tmpNickText.fontStyle = FontStyles.Bold;
            tmpNickText.color = new Color(0.92f, 0.97f, 1f, 1f);
            tmpNickText.alignment = TextAlignmentOptions.MidlineLeft;
            tmpNickText.raycastTarget = false;

            var inputNick = nickInputGo.AddComponent<TMP_InputField>();
            inputNick.textViewport = nickTextArea.GetComponent<RectTransform>();
            inputNick.textComponent = tmpNickText;
            inputNick.placeholder = tmpNickPh;
            inputNick.characterLimit = 16;

            // Status label below nickname
            var statusLabelGo = CreateUIObject("OnlineBadge", profileCard.transform);
            SetAnchors(statusLabelGo, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(128, -28), new Vector2(590, 32));
            var tmpOnline = statusLabelGo.AddComponent<TextMeshProUGUI>();
            tmpOnline.font = font;
            tmpOnline.text = "<color=#10B981>●</color> В СЕТИ  •  РАНГ: 1  •  РЕЙТИНГ: 1200";
            tmpOnline.fontSize = 22;
            tmpOnline.fontStyle = FontStyles.Bold;
            tmpOnline.textWrappingMode = TextWrappingModes.NoWrap;
            tmpOnline.color = new Color(0.6f, 0.75f, 0.9f, 0.85f);
            tmpOnline.alignment = TextAlignmentOptions.MidlineLeft;
            tmpOnline.raycastTarget = false;

            // Top-Right Action Buttons (Settings, Controls)
            var topActions = CreateUIObject("TopActionsGroup", topBar.transform);
            SetAnchors(topActions, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-50, -4), new Vector2(520, 70));
            var hlgTopActions = topActions.AddComponent<HorizontalLayoutGroup>();
            hlgTopActions.childControlWidth = false;
            hlgTopActions.childControlHeight = false;
            hlgTopActions.childForceExpandWidth = false;
            hlgTopActions.childForceExpandHeight = false;
            hlgTopActions.spacing = 20;
            hlgTopActions.childAlignment = TextAnchor.MiddleRight;

            var btnControls = CreateStyledNavButton("BtnControls", topActions.transform, "СПРАВКА", sBgSmall, sOutlineSmall, font, new Vector2(240, 66));
            var btnSettings = CreateStyledNavButton("BtnSettings", topActions.transform, "НАСТРОЙКИ", sBgSmall, sOutlineSmall, font, new Vector2(240, 66));

            // ==========================================
            // LAYER 3: HERO SECTION (TITLE & BRANDING)
            // ==========================================
            var heroSection = CreateUIObject("HeroSection", canvasGo.transform);
            SetAnchors(heroSection, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 275), new Vector2(1100, 250));

            // Soft Horizontal Flare Glow behind title (clean AAA glow, no heavy box!)
            var titleBackGlow = CreateUIObject("TitleBackGlow", heroSection.transform);
            SetAnchors(titleBackGlow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 48), new Vector2(900, 140));
            var imgTitleBackGlow = titleBackGlow.AddComponent<Image>();
            imgTitleBackGlow.sprite = sRing3;
            imgTitleBackGlow.color = new Color(0f, 0.85f, 1f, 0.14f);
            imgTitleBackGlow.raycastTarget = false;

            // Main Title "BATL.IO"
            var titleTextGo = CreateUIObject("TitleText", heroSection.transform);
            SetAnchors(titleTextGo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 52), new Vector2(1000, 130));
            var tmpTitle = titleTextGo.AddComponent<TextMeshProUGUI>();
            tmpTitle.font = font;
            tmpTitle.text = "BATL.IO";
            tmpTitle.fontSize = 130;
            tmpTitle.fontStyle = FontStyles.Bold;
            tmpTitle.characterSpacing = 8;
            tmpTitle.alignment = TextAlignmentOptions.Center;
            tmpTitle.color = new Color(1f, 1f, 1f, 1f);
            tmpTitle.raycastTarget = false;

            // Subtitle Badge
            var subBadge = CreateUIObject("SubtitleBadge", heroSection.transform);
            SetAnchors(subBadge, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -62), new Vector2(540, 54));
            var imgSubBadgeBg = subBadge.AddComponent<Image>();
            imgSubBadgeBg.sprite = sBgSmall;
            imgSubBadgeBg.type = Image.Type.Sliced;
            imgSubBadgeBg.color = new Color(0.06f, 0.12f, 0.22f, 0.85f);
            imgSubBadgeBg.raycastTarget = false;

            var subBadgeBorder = CreateUIObject("Border", subBadge.transform);
            SetStretchAll(subBadgeBorder);
            var imgSubBadgeBorder = subBadgeBorder.AddComponent<Image>();
            imgSubBadgeBorder.sprite = sOutlineSmall;
            imgSubBadgeBorder.type = Image.Type.Sliced;
            imgSubBadgeBorder.color = new Color(0f, 0.85f, 1f, 0.5f);
            imgSubBadgeBorder.raycastTarget = false;

            var subTextGo = CreateUIObject("Text", subBadge.transform);
            SetStretchAll(subTextGo);
            var tmpSubtitle = subTextGo.AddComponent<TextMeshProUGUI>();
            tmpSubtitle.font = font;
            tmpSubtitle.text = "ТАКТИЧЕСКАЯ АРЕНА 5v5";
            tmpSubtitle.fontSize = 28;
            tmpSubtitle.fontStyle = FontStyles.Bold;
            tmpSubtitle.characterSpacing = 4;
            tmpSubtitle.alignment = TextAlignmentOptions.Center;
            tmpSubtitle.color = new Color(0.35f, 0.88f, 1f, 1f);
            tmpSubtitle.raycastTarget = false;

            // ==========================================
            // LAYER 4: GAME MODE & PERKS CARD
            // ==========================================
            var modeCard = CreateUIObject("GameModeCard", canvasGo.transform);
            SetAnchors(modeCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -42), new Vector2(1180, 400));
            var imgModeBg = modeCard.AddComponent<Image>();
            imgModeBg.sprite = sBgSmall;
            imgModeBg.type = Image.Type.Sliced;
            imgModeBg.color = new Color(0.07f, 0.11f, 0.20f, 0.94f);
            imgModeBg.raycastTarget = false;

            var modeBorder = CreateUIObject("Border", modeCard.transform);
            SetStretchAll(modeBorder);
            var imgModeBorder = modeBorder.AddComponent<Image>();
            imgModeBorder.sprite = sOutlineSmall;
            imgModeBorder.type = Image.Type.Sliced;
            imgModeBorder.color = new Color(0.18f, 0.32f, 0.52f, 0.75f);
            imgModeBorder.raycastTarget = false;

            // Mode Header (Row)
            var modeHeader = CreateUIObject("Header", modeCard.transform);
            SetAnchors(modeHeader, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -22), new Vector2(-40, 56));
            var tmpModeHeader = modeHeader.AddComponent<TextMeshProUGUI>();
            tmpModeHeader.font = font;
            tmpModeHeader.text = "<color=#F8FAFC>РЕЖИМ: БИТВА ЦИТАДЕЛЕЙ</color>  <size=28><color=#10B981>[АКТИВЕН]</color></size>";
            tmpModeHeader.fontSize = 38;
            tmpModeHeader.fontStyle = FontStyles.Bold;
            tmpModeHeader.alignment = TextAlignmentOptions.Center;
            tmpModeHeader.raycastTarget = false;

            // Mode Description
            var modeDesc = CreateUIObject("Description", modeCard.transform);
            SetAnchors(modeDesc, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -82), new Vector2(-60, 95));
            var tmpModeDesc = modeDesc.AddComponent<TextMeshProUGUI>();
            tmpModeDesc.font = font;
            tmpModeDesc.text = "Сражайтесь в команде 5v5. Захватывайте башни, уничтожайте базу врага и развивайте уникальные перки в реальном времени!";
            tmpModeDesc.fontSize = 30;
            tmpModeDesc.fontStyle = FontStyles.Bold;
            tmpModeDesc.lineSpacing = 6;
            tmpModeDesc.color = new Color(0.7f, 0.8f, 0.92f, 0.95f);
            tmpModeDesc.alignment = TextAlignmentOptions.Center;
            tmpModeDesc.raycastTarget = false;

            // Perks Row Preview (Real project perk icons!)
            var perksRow = CreateUIObject("PerksRow", modeCard.transform);
            SetAnchors(perksRow, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 24), new Vector2(1060, 145));
            var hlgPerks = perksRow.AddComponent<HorizontalLayoutGroup>();
            hlgPerks.childControlWidth = false;
            hlgPerks.childControlHeight = false;
            hlgPerks.childForceExpandWidth = false;
            hlgPerks.childForceExpandHeight = false;
            hlgPerks.spacing = 30;
            hlgPerks.childAlignment = TextAnchor.MiddleCenter;

            CreatePerkBadge(perksRow.transform, IconDamage, "Урон", font, sBgSmall, sOutlineSmall);
            CreatePerkBadge(perksRow.transform, IconArmor, "Броня", font, sBgSmall, sOutlineSmall);
            CreatePerkBadge(perksRow.transform, IconFireRate, "Темп", font, sBgSmall, sOutlineSmall);
            CreatePerkBadge(perksRow.transform, IconHealth, "Жизни", font, sBgSmall, sOutlineSmall);
            CreatePerkBadge(perksRow.transform, IconCrit, "Крит", font, sBgSmall, sOutlineSmall);

            // ==========================================
            // LAYER 5: PLAY BUTTON & DIRECT CONNECT
            // ==========================================
            var playSection = CreateUIObject("PlaySection", canvasGo.transform);
            SetAnchors(playSection, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(720, 230));

            // Big CTA "В БОЙ" Button
            var btnPlayGo = CreateUIObject("BtnPlay", playSection.transform);
            SetAnchors(btnPlayGo, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(680, 134));

            // Outer Glow
            var btnGlow = CreateUIObject("Glow", btnPlayGo.transform);
            SetStretchAll(btnGlow, -14);
            var imgBtnGlow = btnGlow.AddComponent<Image>();
            imgBtnGlow.sprite = sGlowBar;
            imgBtnGlow.type = Image.Type.Sliced;
            imgBtnGlow.color = new Color(1f, 0.45f, 0.1f, 0.5f);
            imgBtnGlow.raycastTarget = false;

            // Button Body
            var imgBtnBg = btnPlayGo.AddComponent<Image>();
            imgBtnBg.sprite = sFgGradient;
            imgBtnBg.type = Image.Type.Sliced;
            imgBtnBg.color = new Color(1f, 0.42f, 0.12f, 1f); // Vibrant energetic battle orange

            // Button Outline
            var btnOutline = CreateUIObject("Outline", btnPlayGo.transform);
            SetStretchAll(btnOutline);
            var imgBtnOutline = btnOutline.AddComponent<Image>();
            imgBtnOutline.sprite = sOutlineSmall;
            imgBtnOutline.type = Image.Type.Sliced;
            imgBtnOutline.color = new Color(1f, 0.85f, 0.5f, 0.9f);
            imgBtnOutline.raycastTarget = false;

            // Main Play Title Text
            var btnPlayTitleGo = CreateUIObject("Title", btnPlayGo.transform);
            SetAnchors(btnPlayTitleGo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 16), new Vector2(600, 64));
            var tmpPlayTitle = btnPlayTitleGo.AddComponent<TextMeshProUGUI>();
            tmpPlayTitle.font = font;
            tmpPlayTitle.text = "В БОЙ";
            tmpPlayTitle.fontSize = 66;
            tmpPlayTitle.fontStyle = FontStyles.Bold;
            tmpPlayTitle.characterSpacing = 6;
            tmpPlayTitle.alignment = TextAlignmentOptions.Center;
            tmpPlayTitle.color = new Color(1f, 1f, 1f, 1f);
            tmpPlayTitle.raycastTarget = false;

            // Play Subtitle Text
            var btnPlaySubGo = CreateUIObject("Subtitle", btnPlayGo.transform);
            SetAnchors(btnPlaySubGo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -32), new Vector2(600, 36));
            var tmpPlaySubtitle = btnPlaySubGo.AddComponent<TextMeshProUGUI>();
            tmpPlaySubtitle.font = font;
            tmpPlaySubtitle.text = "БЫСТРЫЙ ПОИСК МАТЧА 5v5";
            tmpPlaySubtitle.fontSize = 26;
            tmpPlaySubtitle.fontStyle = FontStyles.Bold;
            tmpPlaySubtitle.characterSpacing = 3;
            tmpPlaySubtitle.alignment = TextAlignmentOptions.Center;
            tmpPlaySubtitle.color = new Color(1f, 0.92f, 0.8f, 0.95f);
            tmpPlaySubtitle.raycastTarget = false;

            var btnPlay = btnPlayGo.AddComponent<Button>();
            var colors = btnPlay.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btnPlay.colors = colors;
            btnPlayGo.AddComponent<UIButtonFeedback>();

            // Server Connection Bar (Below Play Button)
            var serverBarGo = CreateUIObject("ServerConnectionBar", playSection.transform);
            SetAnchors(serverBarGo, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(720, 62));

            var tmpServerLabel = CreateUIObject("Label", serverBarGo.transform).AddComponent<TextMeshProUGUI>();
            SetAnchors(tmpServerLabel.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(190, 52));
            tmpServerLabel.font = font;
            tmpServerLabel.text = "СЕРВЕР IP:";
            tmpServerLabel.fontSize = 26;
            tmpServerLabel.fontStyle = FontStyles.Bold;
            tmpServerLabel.color = new Color(0.70f, 0.82f, 0.95f, 1f);
            tmpServerLabel.alignment = TextAlignmentOptions.MidlineLeft;
            tmpServerLabel.raycastTarget = false;

            var ipInputGo = CreateUIObject("IpInput", serverBarGo.transform);
            SetAnchors(ipInputGo, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 0), new Vector2(505, 54));
            var imgIpInput = ipInputGo.AddComponent<Image>();
            imgIpInput.sprite = sBgSmall;
            imgIpInput.type = Image.Type.Sliced;
            imgIpInput.color = new Color(0.06f, 0.09f, 0.16f, 0.85f);

            var ipInputBorder = CreateUIObject("Border", ipInputGo.transform);
            SetStretchAll(ipInputBorder);
            var imgIpBorder = ipInputBorder.AddComponent<Image>();
            imgIpBorder.sprite = sOutlineSmall;
            imgIpBorder.type = Image.Type.Sliced;
            imgIpBorder.color = new Color(0.2f, 0.35f, 0.52f, 0.55f);
            imgIpBorder.raycastTarget = false;

            var ipTextArea = CreateUIObject("Text Area", ipInputGo.transform);
            SetStretchMargins(ipTextArea, 18, 16, 4, 4);
            ipTextArea.AddComponent<RectMask2D>();

            var ipPlaceholder = CreateUIObject("Placeholder", ipTextArea.transform);
            SetStretchAll(ipPlaceholder);
            var tmpIpPh = ipPlaceholder.AddComponent<TextMeshProUGUI>();
            tmpIpPh.font = font;
            tmpIpPh.text = "127.0.0.1 (локальный)";
            tmpIpPh.fontSize = 25;
            tmpIpPh.fontStyle = FontStyles.Italic;
            tmpIpPh.color = new Color(0.45f, 0.58f, 0.72f, 0.6f);
            tmpIpPh.alignment = TextAlignmentOptions.MidlineLeft;
            tmpIpPh.raycastTarget = false;

            var ipText = CreateUIObject("Text", ipTextArea.transform);
            SetStretchAll(ipText);
            var tmpIpText = ipText.AddComponent<TextMeshProUGUI>();
            tmpIpText.font = font;
            tmpIpText.text = "127.0.0.1";
            tmpIpText.fontSize = 25;
            tmpIpText.fontStyle = FontStyles.Bold;
            tmpIpText.color = new Color(0.85f, 0.94f, 1f, 1f);
            tmpIpText.alignment = TextAlignmentOptions.MidlineLeft;
            tmpIpText.raycastTarget = false;

            var inputIp = ipInputGo.AddComponent<TMP_InputField>();
            inputIp.textViewport = ipTextArea.GetComponent<RectTransform>();
            inputIp.textComponent = tmpIpText;
            inputIp.placeholder = tmpIpPh;

            // ==========================================
            // LAYER 6: STATUS / LOG NOTIFICATION TOAST
            // ==========================================
            var statusToast = CreateUIObject("StatusToast", canvasGo.transform);
            SetAnchors(statusToast, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 225), new Vector2(700, 52));
            var imgStatusToast = statusToast.AddComponent<Image>();
            imgStatusToast.sprite = sBgSmall;
            imgStatusToast.type = Image.Type.Sliced;
            imgStatusToast.color = new Color(0.06f, 0.12f, 0.22f, 0.95f);
            imgStatusToast.raycastTarget = false;

            var statusToastBorder = CreateUIObject("Border", statusToast.transform);
            SetStretchAll(statusToastBorder);
            var imgStatusToastBorder = statusToastBorder.AddComponent<Image>();
            imgStatusToastBorder.sprite = sOutlineSmall;
            imgStatusToastBorder.type = Image.Type.Sliced;
            imgStatusToastBorder.color = new Color(0f, 0.85f, 1f, 0.5f);
            imgStatusToastBorder.raycastTarget = false;

            var statusTextGo = CreateUIObject("StatusText", statusToast.transform);
            SetStretchAll(statusTextGo, 4);
            var tmpStatus = statusTextGo.AddComponent<TextMeshProUGUI>();
            tmpStatus.font = font;
            tmpStatus.text = "Готов к подключению";
            tmpStatus.fontSize = 22;
            tmpStatus.fontStyle = FontStyles.Bold;
            tmpStatus.color = new Color(0.85f, 0.95f, 1f, 0.95f);
            tmpStatus.alignment = TextAlignmentOptions.Center;
            tmpStatus.raycastTarget = false;
            statusToast.SetActive(false);

            // ==========================================
            // LAYER 7: SETTINGS MODAL
            // ==========================================
            var settingsModalGo = CreateUIObject("SettingsModal", canvasGo.transform);
            SetStretchAll(settingsModalGo);

            var settingsBackdrop = CreateUIObject("Backdrop", settingsModalGo.transform);
            SetStretchAll(settingsBackdrop);
            var imgSettingsDim = settingsBackdrop.AddComponent<Image>();
            imgSettingsDim.color = new Color(0.01f, 0.02f, 0.05f, 0.88f);

            var settingsWindow = CreateUIObject("Window", settingsModalGo.transform);
            SetAnchors(settingsWindow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(920, 680));

            var setWinBacking = CreateUIObject("Backing", settingsWindow.transform);
            SetStretchAll(setWinBacking, 14);
            var imgSetBacking = setWinBacking.AddComponent<Image>();
            imgSetBacking.color = new Color(0.09f, 0.13f, 0.22f, 1f);
            imgSetBacking.raycastTarget = false;

            var imgSetWin = settingsWindow.AddComponent<Image>();
            imgSetWin.sprite = sBgSmall;
            imgSetWin.type = Image.Type.Sliced;
            imgSetWin.color = new Color(0.09f, 0.13f, 0.22f, 1f);

            var setWinBorder = CreateUIObject("Border", settingsWindow.transform);
            SetStretchAll(setWinBorder);
            var imgSetBorder = setWinBorder.AddComponent<Image>();
            imgSetBorder.sprite = sOutlineSmall;
            imgSetBorder.type = Image.Type.Sliced;
            imgSetBorder.color = new Color(0f, 0.8f, 1f, 0.55f);
            imgSetBorder.raycastTarget = false;

            // Modal Title
            var setTitle = CreateUIObject("Title", settingsWindow.transform);
            SetAnchors(setTitle, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -42), new Vector2(700, 56));
            var tmpSetTitle = setTitle.AddComponent<TextMeshProUGUI>();
            tmpSetTitle.font = font;
            tmpSetTitle.text = "НАСТРОЙКИ";
            tmpSetTitle.fontSize = 44;
            tmpSetTitle.fontStyle = FontStyles.Bold;
            tmpSetTitle.characterSpacing = 4;
            tmpSetTitle.alignment = TextAlignmentOptions.Center;
            tmpSetTitle.color = new Color(0.95f, 0.98f, 1f, 1f);

            // Close button [X]
            var btnSetClose = CreateModalCloseButton("BtnClose", settingsWindow.transform, sBgSmall, sOutlineSmall, font);

            // Volume Row
            var volumeRow = CreateUIObject("VolumeRow", settingsWindow.transform);
            SetAnchors(volumeRow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 115), new Vector2(780, 72));
            var tmpVolLabel = CreateUIObject("Label", volumeRow.transform).AddComponent<TextMeshProUGUI>();
            SetAnchors(tmpVolLabel.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(350, 54));
            tmpVolLabel.font = font;
            tmpVolLabel.text = "ГРОМКОСТЬ ЗВУКА:";
            tmpVolLabel.fontSize = 30;
            tmpVolLabel.fontStyle = FontStyles.Bold;
            tmpVolLabel.color = new Color(0.85f, 0.9f, 0.98f, 1f);
            tmpVolLabel.alignment = TextAlignmentOptions.MidlineLeft;

            var tmpVolValue = CreateUIObject("Value", volumeRow.transform).AddComponent<TextMeshProUGUI>();
            SetAnchors(tmpVolValue.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 0), new Vector2(100, 54));
            tmpVolValue.font = font;
            tmpVolValue.text = "100%";
            tmpVolValue.fontSize = 30;
            tmpVolValue.fontStyle = FontStyles.Bold;
            tmpVolValue.color = new Color(0f, 0.85f, 1f, 1f);
            tmpVolValue.alignment = TextAlignmentOptions.MidlineRight;

            var sliderGo = CreateUIObject("VolumeSlider", volumeRow.transform);
            SetAnchors(sliderGo, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(360, 0), new Vector2(310, 40));
            var slider = CreateStyledSlider(sliderGo, sBgSmall, sFgGradient, sRing3);

            // Fullscreen Row
            var fsRow = CreateUIObject("FullscreenRow", settingsWindow.transform);
            SetAnchors(fsRow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 15), new Vector2(780, 72));
            var tmpFsLabel = CreateUIObject("Label", fsRow.transform).AddComponent<TextMeshProUGUI>();
            SetAnchors(tmpFsLabel.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(460, 54));
            tmpFsLabel.font = font;
            tmpFsLabel.text = "ПОЛНОЭКРАННЫЙ РЕЖИМ:";
            tmpFsLabel.fontSize = 30;
            tmpFsLabel.fontStyle = FontStyles.Bold;
            tmpFsLabel.color = new Color(0.85f, 0.9f, 0.98f, 1f);
            tmpFsLabel.alignment = TextAlignmentOptions.MidlineLeft;

            var toggleGo = CreateUIObject("FullscreenToggle", fsRow.transform);
            SetAnchors(toggleGo, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-10, 0), new Vector2(54, 54));
            var toggle = CreateStyledToggle(toggleGo, sBgSmall, sOutlineSmall, sRing3);

            // Reset Data Row / Warning
            var resetRow = CreateUIObject("ResetRow", settingsWindow.transform);
            SetAnchors(resetRow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -115), new Vector2(780, 72));
            var btnReset = CreateStyledNavButton("BtnResetPrefs", resetRow.transform, "СБРОСИТЬ НАСТРОЙКИ И ПРОФИЛЬ", sBgSmall, sOutlineSmall, font, new Vector2(780, 72));
            btnReset.onClick.AddListener(() =>
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Debug.Log("[Menu] PlayerPrefs reset.");
            });

            settingsModalGo.SetActive(false);

            // ==========================================
            // LAYER 8: CONTROLS & HOW-TO-PLAY MODAL
            // ==========================================
            var controlsModalGo = CreateUIObject("ControlsModal", canvasGo.transform);
            SetStretchAll(controlsModalGo);

            var controlsBackdrop = CreateUIObject("Backdrop", controlsModalGo.transform);
            SetStretchAll(controlsBackdrop);
            var imgControlsDim = controlsBackdrop.AddComponent<Image>();
            imgControlsDim.color = new Color(0.01f, 0.02f, 0.05f, 0.88f);

            var controlsWindow = CreateUIObject("Window", controlsModalGo.transform);
            SetAnchors(controlsWindow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(1180, 880));

            var ctrlWinBacking = CreateUIObject("Backing", controlsWindow.transform);
            SetStretchAll(ctrlWinBacking, 14);
            var imgCtrlBacking = ctrlWinBacking.AddComponent<Image>();
            imgCtrlBacking.color = new Color(0.09f, 0.13f, 0.22f, 1f);
            imgCtrlBacking.raycastTarget = false;

            var imgCtrlWin = controlsWindow.AddComponent<Image>();
            imgCtrlWin.sprite = sBgSmall;
            imgCtrlWin.type = Image.Type.Sliced;
            imgCtrlWin.color = new Color(0.09f, 0.13f, 0.22f, 1f);

            var ctrlWinBorder = CreateUIObject("Border", controlsWindow.transform);
            SetStretchAll(ctrlWinBorder);
            var imgCtrlBorder = ctrlWinBorder.AddComponent<Image>();
            imgCtrlBorder.sprite = sOutlineSmall;
            imgCtrlBorder.type = Image.Type.Sliced;
            imgCtrlBorder.color = new Color(0f, 0.8f, 1f, 0.55f);
            imgCtrlBorder.raycastTarget = false;

            // Modal Title
            var ctrlTitle = CreateUIObject("Title", controlsWindow.transform);
            SetAnchors(ctrlTitle, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -42), new Vector2(1000, 56));
            var tmpCtrlTitle = ctrlTitle.AddComponent<TextMeshProUGUI>();
            tmpCtrlTitle.font = font;
            tmpCtrlTitle.text = "КАК ИГРАТЬ  •  УПРАВЛЕНИЕ";
            tmpCtrlTitle.fontSize = 40;
            tmpCtrlTitle.fontStyle = FontStyles.Bold;
            tmpCtrlTitle.characterSpacing = 3;
            tmpCtrlTitle.textWrappingMode = TextWrappingModes.NoWrap;
            tmpCtrlTitle.alignment = TextAlignmentOptions.Center;
            tmpCtrlTitle.color = new Color(0.95f, 0.98f, 1f, 1f);

            var btnCtrlClose = CreateModalCloseButton("BtnClose", controlsWindow.transform, sBgSmall, sOutlineSmall, font);

            // Controls Body Content
            var ctrlContent = CreateUIObject("Content", controlsWindow.transform);
            SetAnchors(ctrlContent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(1060, 720));
            var tmpCtrlContent = ctrlContent.AddComponent<TextMeshProUGUI>();
            tmpCtrlContent.font = font;
            tmpCtrlContent.fontSize = 28;
            tmpCtrlContent.color = new Color(0.85f, 0.9f, 0.98f, 1f);
            tmpCtrlContent.richText = true;
            tmpCtrlContent.lineSpacing = 8;
            tmpCtrlContent.text =
                "<color=#00F2FE>● УПРАВЛЕНИЕ НА ПК (КЛАВИАТУРА И МЫШЬ):</color>\n" +
                "  • [W][A][S][D] — Перемещение вашего танка\n" +
                "  • [Мышь] — Направление башни и прицеливание\n" +
                "  • [ЛКМ] — Стрельба снарядами\n" +
                "  • [1] ... [5] — Мгновенная прокачка характеристик в бою\n\n" +
                "<color=#FF9500>● УПРАВЛЕНИЕ НА МОБИЛЬНЫХ УСТРОЙСТВАХ:</color>\n" +
                "  • Левый джойстик — Перемещение по арене\n" +
                "  • Правый джойстик — Прицел и автоматический огонь\n\n" +
                "<color=#10B981>● ЦЕЛЬ ИГРЫ И ПРАВИЛА БОЯ:</color>\n" +
                "  • Ваша цель — уничтожить вражеские башни и вражескую Цитадель.\n" +
                "  • За уничтожение крипов и врагов вы получаете опыт и прокачиваете перки.\n" +
                "  • Собирайте нейтральные Руны на арене для временных усилений!";
            tmpCtrlContent.alignment = TextAlignmentOptions.TopLeft;

            controlsModalGo.SetActive(false);

            // ==========================================
            // WIRE UP LobbyManager & MainMenuUI
            // ==========================================
            var lobbyManager = Object.FindAnyObjectByType<LobbyManager>();
            if (lobbyManager != null)
            {
                var lmSo = new SerializedObject(lobbyManager);
                lmSo.FindProperty("btnConnect").objectReferenceValue = btnPlay;
                lmSo.FindProperty("btnLabel").objectReferenceValue = tmpPlayTitle;
                lmSo.FindProperty("inputField").objectReferenceValue = inputNick;
                lmSo.FindProperty("logText").objectReferenceValue = tmpStatus;
                lmSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(lobbyManager);
            }

            var uiSo = new SerializedObject(mainMenuUI);
            uiSo.FindProperty("lobbyManager").objectReferenceValue = lobbyManager;
            uiSo.FindProperty("nicknameInput").objectReferenceValue = inputNick;
            uiSo.FindProperty("serverIpInput").objectReferenceValue = inputIp;
            uiSo.FindProperty("btnPlay").objectReferenceValue = btnPlay;
            uiSo.FindProperty("btnPlayTitle").objectReferenceValue = tmpPlayTitle;
            uiSo.FindProperty("btnPlaySubtitle").objectReferenceValue = tmpPlaySubtitle;
            uiSo.FindProperty("btnSettings").objectReferenceValue = btnSettings;
            uiSo.FindProperty("btnControls").objectReferenceValue = btnControls;
            uiSo.FindProperty("settingsModal").objectReferenceValue = settingsModalGo;
            uiSo.FindProperty("btnSettingsClose").objectReferenceValue = btnSetClose;
            uiSo.FindProperty("volumeSlider").objectReferenceValue = slider;
            uiSo.FindProperty("volumeLabel").objectReferenceValue = tmpVolValue;
            uiSo.FindProperty("fullscreenToggle").objectReferenceValue = toggle;
            uiSo.FindProperty("controlsModal").objectReferenceValue = controlsModalGo;
            uiSo.FindProperty("btnControlsClose").objectReferenceValue = btnCtrlClose;
            uiSo.FindProperty("statusBanner").objectReferenceValue = statusToast;
            uiSo.FindProperty("statusText").objectReferenceValue = tmpStatus;
            uiSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(mainMenuUI);

            Canvas.ForceUpdateCanvases();
            EditorApplication.QueuePlayerLoopUpdate();

            // Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[MenuBuilder] Beautiful Canvas Menu successfully generated and wired!");
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.layer = 5; // UI Layer
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            return go;
        }

        private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private static void SetAnchors(GameObject go, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            SetAnchors(go.GetComponent<RectTransform>(), min, max, pivot, pos, size);
        }

        private static void SetStretchAll(GameObject go, float margin = 0)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(margin, margin);
            rt.offsetMax = new Vector2(-margin, -margin);
        }

        private static void SetStretchMargins(GameObject go, float left, float right, float top, float bottom)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private static void CreateAmbientOrb(Transform parent, UIAmbientParticles system, Sprite sprite, Vector2 pos, Vector2 size, Color color, float speed, float ampX, float ampY, float minA, float maxA)
        {
            var orb = CreateUIObject("AmbientOrb", parent);
            var rt = orb.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = orb.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            system.RegisterParticle(rt, img, speed, ampX, ampY, minA, maxA);
        }

        private static Button CreateStyledNavButton(string name, Transform parent, string label, Sprite sBg, Sprite sOutline, TMP_FontAsset font, Vector2 size)
        {
            var btnGo = CreateUIObject(name, parent);
            var rt = btnGo.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            var imgBg = btnGo.AddComponent<Image>();
            imgBg.sprite = sBg;
            imgBg.type = Image.Type.Sliced;
            imgBg.color = new Color(0.08f, 0.12f, 0.22f, 0.88f);

            var borderGo = CreateUIObject("Border", btnGo.transform);
            SetStretchAll(borderGo);
            var imgBorder = borderGo.AddComponent<Image>();
            imgBorder.sprite = sOutline;
            imgBorder.type = Image.Type.Sliced;
            imgBorder.color = new Color(0.25f, 0.4f, 0.6f, 0.75f);
            imgBorder.raycastTarget = false;

            var textGo = CreateUIObject("Text", btnGo.transform);
            SetStretchAll(textGo);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = label;
            tmp.fontSize = 26;
            tmp.fontStyle = FontStyles.Bold;
            tmp.characterSpacing = 2;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.85f, 0.92f, 1f, 1f);
            tmp.raycastTarget = false;

            var btn = btnGo.AddComponent<Button>();
            btnGo.AddComponent<UIButtonFeedback>();
            return btn;
        }

        private static Button CreateModalCloseButton(string name, Transform parent, Sprite sBg, Sprite sOutline, TMP_FontAsset font)
        {
            var btnGo = CreateUIObject(name, parent);
            SetAnchors(btnGo.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-16, -16), new Vector2(50, 50));

            var imgBg = btnGo.AddComponent<Image>();
            imgBg.sprite = sBg;
            imgBg.type = Image.Type.Sliced;
            imgBg.color = new Color(0.15f, 0.2f, 0.32f, 0.85f);

            var textGo = CreateUIObject("Text", btnGo.transform);
            SetStretchAll(textGo);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = "X";
            tmp.fontSize = 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.9f, 0.95f, 1f, 1f);
            tmp.raycastTarget = false;

            var btn = btnGo.AddComponent<Button>();
            btnGo.AddComponent<UIButtonFeedback>();
            return btn;
        }

        private static void CreatePerkBadge(Transform parent, string iconPath, string name, TMP_FontAsset font, Sprite sBg, Sprite sOutline)
        {
            var badgeGo = CreateUIObject("Perk_" + name, parent);
            var rt = badgeGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(170, 140);

            var iconGo = CreateUIObject("Icon", badgeGo.transform);
            SetAnchors(iconGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -2), new Vector2(86, 86));
            var imgIcon = iconGo.AddComponent<Image>();
            var iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            imgIcon.sprite = iconSprite;
            imgIcon.color = new Color(0.35f, 0.92f, 1f, 0.95f);
            imgIcon.raycastTarget = false;

            var nameGo = CreateUIObject("Name", badgeGo.transform);
            SetAnchors(nameGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(170, 42));
            var tmpName = nameGo.AddComponent<TextMeshProUGUI>();
            tmpName.font = font;
            tmpName.text = name;
            tmpName.fontSize = 32;
            tmpName.fontStyle = FontStyles.Bold;
            tmpName.color = new Color(0.7f, 0.8f, 0.94f, 0.9f);
            tmpName.alignment = TextAlignmentOptions.Center;
            tmpName.raycastTarget = false;
        }

        private static Slider CreateStyledSlider(GameObject sliderGo, Sprite sBg, Sprite sFill, Sprite sHandle)
        {
            var slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            var bg = CreateUIObject("Background", sliderGo.transform);
            SetStretchAll(bg);
            var imgBg = bg.AddComponent<Image>();
            imgBg.sprite = sBg;
            imgBg.type = Image.Type.Sliced;
            imgBg.color = new Color(0.06f, 0.09f, 0.16f, 0.9f);

            var fillArea = CreateUIObject("Fill Area", sliderGo.transform);
            SetStretchAll(fillArea, 4);

            var fill = CreateUIObject("Fill", fillArea.transform);
            SetStretchAll(fill);
            var imgFill = fill.AddComponent<Image>();
            imgFill.sprite = sFill;
            imgFill.type = Image.Type.Sliced;
            imgFill.color = new Color(0f, 0.85f, 1f, 1f);

            var handleArea = CreateUIObject("Handle Slide Area", sliderGo.transform);
            SetStretchAll(handleArea);

            var handle = CreateUIObject("Handle", handleArea.transform);
            SetAnchors(handle.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(0, 1), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(20, 0));
            var imgHandle = handle.AddComponent<Image>();
            imgHandle.sprite = sHandle;
            imgHandle.color = new Color(1f, 1f, 1f, 1f);

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = imgHandle;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private static Toggle CreateStyledToggle(GameObject toggleGo, Sprite sBg, Sprite sOutline, Sprite sCheck)
        {
            var toggle = toggleGo.AddComponent<Toggle>();

            var bg = CreateUIObject("Background", toggleGo.transform);
            SetStretchAll(bg);
            var imgBg = bg.AddComponent<Image>();
            imgBg.sprite = sBg;
            imgBg.type = Image.Type.Sliced;
            imgBg.color = new Color(0.06f, 0.09f, 0.16f, 0.9f);

            var border = CreateUIObject("Border", bg.transform);
            SetStretchAll(border);
            var imgBorder = border.AddComponent<Image>();
            imgBorder.sprite = sOutline;
            imgBorder.type = Image.Type.Sliced;
            imgBorder.color = new Color(0.2f, 0.35f, 0.55f, 0.8f);
            imgBorder.raycastTarget = false;

            var check = CreateUIObject("Checkmark", bg.transform);
            SetStretchAll(check, 6);
            var imgCheck = check.AddComponent<Image>();
            imgCheck.sprite = sCheck;
            imgCheck.color = new Color(0f, 0.95f, 1f, 1f);

            toggle.targetGraphic = imgBg;
            toggle.graphic = imgCheck;
            return toggle;
        }
    }
}
