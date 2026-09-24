using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Автоматический сборщик полной сцены HelloTap GameDev Idle:
/// 1. Разметка UI в сцене (SampleScene.unity) - шапка, счетчики, магазин, вкладки.
/// 2. Атмосфера и визуал рабочего места (монитор с бегущим кодом, клавиатура, мышь, кофе, энергетик, кот).
/// 3. Звуковое оформление (AudioManager, клики с рандомизацией питча, криты, релизы, бусты, кнопка звука).
/// </summary>
public static class HelloTapSceneBuilder
{
    private const string VersionMarker = "HelloTap_Scene_v1_BuildDone";

    [InitializeOnLoadMethod]
    private static void AutoInit()
    {
        EditorApplication.delayCall += () =>
        {
            if (!SessionState.GetBool(VersionMarker, false))
            {
                SessionState.SetBool(VersionMarker, true);
                BuildCompleteScene();
            }
        };
    }

    [MenuItem("HelloTap/Build Full Scene")]
    public static void BuildCompleteScene()
    {
        Debug.Log("[HelloTap] Starting Scene Rebuild...");

        // 1. Убеждаемся, что спрайты настроены как 2D Sprite
        SetupSpriteImporters();

        // 2. Открываем SampleScene
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

        // 3. Загружаем ресурсы
        TMP_FontAsset fontAsset = LoadDefaultTMPFont();
        AudioClip[] typingClips = new AudioClip[]
        {
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/click_key1.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/click_key2.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/click_key3.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/click_key4.wav")
        };
        AudioClip critClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/click_crit.wav");
        AudioClip upgradeClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/upgrade_buy.wav");
        AudioClip releaseClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/project_release.wav");
        AudioClip boostClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/boost_activate.wav");

        Sprite sprDeskMat = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spr_desk_mat.png");
        Sprite sprMonitorFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spr_monitor_frame.png");
        Sprite sprMonitorScreen = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spr_monitor_screen.png");
        Sprite sprKeyboard = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spr_keyboard.png");
        Sprite sprMouse = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spr_mouse.png");
        Sprite sprCoffee = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spr_coffee_mug.png");
        Sprite sprEnergy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spr_energy_can.png");
        Sprite sprCat = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spr_cat.png");
        Sprite sprCardBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spr_card_bg.png");
        Sprite sprBtnCyan = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spr_btn_cyan.png");
        Sprite sprBtnOrange = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spr_btn_orange.png");
        Sprite sprBtnGold = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/spr_btn_gold.png");

        // 4. Настраиваем камеру
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.backgroundColor = new Color(0.06f, 0.07f, 0.10f, 1f); // #0F1219
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.orthographic = true;
            mainCam.orthographicSize = 5f;
        }

        // 5. Создаем или находим GameManager
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            GameObject gmGo = new GameObject("GameManager");
            gameManager = gmGo.AddComponent<GameManager>();
        }

        // 6. Создаем или находим AudioManager
        AudioManager audioManager = Object.FindFirstObjectByType<AudioManager>();
        if (audioManager == null)
        {
            GameObject amGo = new GameObject("AudioManager");
            audioManager = amGo.AddComponent<AudioManager>();
        }

        // Привязываем аудиоклипы в AudioManager через SerializedObject
        SerializedObject amSo = new SerializedObject(audioManager);
        SerializedProperty typingProp = amSo.FindProperty("typingSounds");
        typingProp.arraySize = typingClips.Length;
        for (int i = 0; i < typingClips.Length; i++)
        {
            typingProp.GetArrayElementAtIndex(i).objectReferenceValue = typingClips[i];
        }
        amSo.FindProperty("critSound").objectReferenceValue = critClip;
        amSo.FindProperty("upgradeSound").objectReferenceValue = upgradeClip;
        amSo.FindProperty("releaseSound").objectReferenceValue = releaseClip;
        amSo.FindProperty("boostSound").objectReferenceValue = boostClip;
        amSo.ApplyModifiedProperties();

        // 7. Подготавливаем Prefab всплывающего текста для ClickJuice
        TMP_Text floatingTextPrefab = CreateFloatingTextPrefab(fontAsset);

        // 8. Настраиваем Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Удаляем устаревшие дочерние объекты из канваса, если это старая лаба
        List<GameObject> toDestroy = new List<GameObject>();
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            toDestroy.Add(canvas.transform.GetChild(i).gameObject);
        }
        foreach (var obj in toDestroy)
        {
            Object.DestroyImmediate(obj);
        }

        // Создаем контейнер для всплывающего текста
        GameObject floatTextParentGo = CreateUIObject("FloatingTextParent", canvas.transform);
        SetFullStretch(floatTextParentGo.GetComponent<RectTransform>());

        // -------------------------------------------------------------
        // СЕКЦИЯ 1: ВЕРХНЯЯ ПАНЕЛЬ (HeaderPanel)
        // -------------------------------------------------------------
        GameObject headerGo = CreateUIObject("HeaderPanel", canvas.transform);
        RectTransform headerRt = headerGo.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0f, 1f);
        headerRt.anchorMax = new Vector2(1f, 1f);
        headerRt.pivot = new Vector2(0.5f, 1f);
        headerRt.anchoredPosition = new Vector2(0, 0);
        headerRt.sizeDelta = new Vector2(0, 110);

        Image headerBg = headerGo.AddComponent<Image>();
        headerBg.color = new Color(0.09f, 0.11f, 0.15f, 0.96f);

        // Блок строк кода (слева)
        GameObject codeBlock = CreateUIObject("CodeBlock", headerGo.transform);
        RectTransform codeBlockRt = codeBlock.GetComponent<RectTransform>();
        codeBlockRt.anchorMin = new Vector2(0f, 0.5f);
        codeBlockRt.anchorMax = new Vector2(0f, 0.5f);
        codeBlockRt.pivot = new Vector2(0f, 0.5f);
        codeBlockRt.anchoredPosition = new Vector2(40, 0);
        codeBlockRt.sizeDelta = new Vector2(420, 90);

        TMP_Text codeCounterText = CreateTMPText("CodeText", codeBlock.transform, fontAsset, "Строк кода: 0", 34, FontStyles.Bold, Color.white);
        codeCounterText.rectTransform.anchoredPosition = new Vector2(0, 16);
        codeCounterText.rectTransform.sizeDelta = new Vector2(420, 42);

        TMP_Text statsText = CreateTMPText("StatsText", codeBlock.transform, fontAsset, "+1.0 за клик | +0.0 строк/сек", 17, FontStyles.Normal, new Color(0.55f, 0.60f, 0.68f));
        statsText.rectTransform.anchoredPosition = new Vector2(0, -22);
        statsText.rectTransform.sizeDelta = new Vector2(420, 28);

        // Блок денег (по центру-слева)
        GameObject moneyBlock = CreateUIObject("MoneyBlock", headerGo.transform);
        RectTransform moneyBlockRt = moneyBlock.GetComponent<RectTransform>();
        moneyBlockRt.anchorMin = new Vector2(0f, 0.5f);
        moneyBlockRt.anchorMax = new Vector2(0f, 0.5f);
        moneyBlockRt.pivot = new Vector2(0f, 0.5f);
        moneyBlockRt.anchoredPosition = new Vector2(480, 0);
        moneyBlockRt.sizeDelta = new Vector2(380, 90);

        TMP_Text moneyText = CreateTMPText("MoneyText", moneyBlock.transform, fontAsset, "Баланс: 0 ₽", 30, FontStyles.Bold, new Color(1.0f, 0.82f, 0.40f));
        moneyText.rectTransform.anchoredPosition = new Vector2(0, 16);
        moneyText.rectTransform.sizeDelta = new Vector2(380, 40);

        TMP_Text moneyPassiveText = CreateTMPText("MoneyPassiveText", moneyBlock.transform, fontAsset, "Пассив: +0 ₽/сек с релизов", 17, FontStyles.Normal, new Color(0.40f, 0.85f, 0.55f));
        moneyPassiveText.rectTransform.anchoredPosition = new Vector2(0, -22);
        moneyPassiveText.rectTransform.sizeDelta = new Vector2(380, 28);

        // Кнопка Бустера «Энергетик»
        GameObject boostBtnGo = CreateUIButton("EnergyBoostButton", headerGo.transform, sprBtnGold, new Vector2(210, 64), new Vector2(1060, 0));
        RectTransform boostRt = boostBtnGo.GetComponent<RectTransform>();
        boostRt.anchorMin = new Vector2(0f, 0.5f);
        boostRt.anchorMax = new Vector2(0f, 0.5f);
        boostRt.pivot = new Vector2(0f, 0.5f);
        TMP_Text boostBtnText = CreateTMPText("Text", boostBtnGo.transform, fontAsset, "⚡ ЭНЕРГЕТИК (x2)", 18, FontStyles.Bold, Color.white);
        SetFullStretch(boostBtnText.rectTransform);
        boostBtnText.alignment = TextAlignmentOptions.Center;

        TMP_Text boostTimerText = CreateTMPText("BoostTimerText", headerGo.transform, fontAsset, "ЭНЕРГЕТИК x2: 00:30", 17, FontStyles.Bold, new Color(1.0f, 0.45f, 0.65f));
        RectTransform boostTimerRt = boostTimerText.rectTransform;
        boostTimerRt.anchorMin = new Vector2(0f, 0.5f);
        boostTimerRt.anchorMax = new Vector2(0f, 0.5f);
        boostTimerRt.pivot = new Vector2(0f, 0.5f);
        boostTimerRt.anchoredPosition = new Vector2(1290, 0);
        boostTimerRt.sizeDelta = new Vector2(200, 30);
        boostTimerText.gameObject.SetActive(false);

        // Кнопка Mute / Звук
        GameObject muteBtnGo = CreateUIButton("AudioMuteButton", headerGo.transform, sprBtnCyan, new Vector2(60, 60), new Vector2(1730, 0));
        RectTransform muteRt = muteBtnGo.GetComponent<RectTransform>();
        muteRt.anchorMin = new Vector2(0f, 0.5f);
        muteRt.anchorMax = new Vector2(0f, 0.5f);
        muteRt.pivot = new Vector2(0f, 0.5f);
        TMP_Text muteTxt = CreateTMPText("Icon", muteBtnGo.transform, fontAsset, "🔊", 26, FontStyles.Normal, Color.white);
        SetFullStretch(muteTxt.rectTransform);
        muteTxt.alignment = TextAlignmentOptions.Center;
        AudioToggleButton audioToggle = muteBtnGo.AddComponent<AudioToggleButton>();
        SerializedObject atSo = new SerializedObject(audioToggle);
        atSo.FindProperty("buttonText").objectReferenceValue = muteTxt;
        atSo.FindProperty("button").objectReferenceValue = muteBtnGo.GetComponent<Button>();
        atSo.ApplyModifiedProperties();

        // Кнопка Сброса (требование лабораторной)
        GameObject resetBtnGo = CreateUIButton("ResetButton", headerGo.transform, sprBtnOrange, new Vector2(100, 60), new Vector2(1810, 0));
        RectTransform resetRt = resetBtnGo.GetComponent<RectTransform>();
        resetRt.anchorMin = new Vector2(0f, 0.5f);
        resetRt.anchorMax = new Vector2(0f, 0.5f);
        resetRt.pivot = new Vector2(0f, 0.5f);
        TMP_Text resetTxt = CreateTMPText("Text", resetBtnGo.transform, fontAsset, "СБРОС", 16, FontStyles.Bold, Color.white);
        SetFullStretch(resetTxt.rectTransform);
        resetTxt.alignment = TextAlignmentOptions.Center;

        // -------------------------------------------------------------
        // СЕКЦИЯ 2: РАБОЧЕЕ МЕСТО И ЗОНА ТАПА (WorkplaceArea)
        // -------------------------------------------------------------
        GameObject workplaceGo = CreateUIObject("WorkplaceArea", canvas.transform);
        RectTransform wpRt = workplaceGo.GetComponent<RectTransform>();
        wpRt.anchorMin = new Vector2(0f, 0f);
        wpRt.anchorMax = new Vector2(0.63f, 0.90f);
        wpRt.pivot = new Vector2(0.5f, 0.5f);
        wpRt.offsetMin = new Vector2(20, 20);
        wpRt.offsetMax = new Vector2(-10, -10);

        // Стол и RGB коврик
        GameObject deskMatGo = CreateUIImage("DeskMat", workplaceGo.transform, sprDeskMat, new Vector2(1150, 680), new Vector2(0, -30));

        // Монитор (Рамка и Подставка)
        GameObject monitorFrameGo = CreateUIImage("MonitorFrame", deskMatGo.transform, sprMonitorFrame, new Vector2(880, 470), new Vector2(0, 100));

        // Экран с бегущим кодом
        GameObject monitorScreenGo = CreateUIImage("MonitorScreen", monitorFrameGo.transform, sprMonitorScreen, new Vector2(800, 320), new Vector2(0, 10));

        // Текст терминала на экране монитора
        TMP_Text terminalText = CreateTMPText("TerminalCodeText", monitorScreenGo.transform, fontAsset, "// HelloTap Terminal v1.0", 14, FontStyles.Normal, new Color(0f, 1f, 0.55f));
        RectTransform termRt = terminalText.rectTransform;
        termRt.anchorMin = new Vector2(0f, 0f);
        termRt.anchorMax = new Vector2(0.58f, 1f);
        termRt.pivot = new Vector2(0f, 1f);
        termRt.offsetMin = new Vector2(15, 10);
        termRt.offsetMax = new Vector2(-10, -25);
        terminalText.alignment = TextAlignmentOptions.TopLeft;

        // Клавиатура (Клик-зона)
        GameObject keyboardGo = CreateUIImage("MechanicalKeyboard", deskMatGo.transform, sprKeyboard, new Vector2(560, 210), new Vector2(0, -195));

        // Подсветка клавиатуры при нажатии
        GameObject keyGlowGo = CreateUIObject("KeyboardGlow", keyboardGo.transform);
        Image keyGlowImg = keyGlowGo.AddComponent<Image>();
        keyGlowImg.color = new Color(0f, 0.89f, 1f, 0.15f);
        SetFullStretch(keyGlowGo.GetComponent<RectTransform>());

        // Мышь
        GameObject mouseGo = CreateUIImage("GamingMouse", deskMatGo.transform, sprMouse, new Vector2(100, 160), new Vector2(360, -195));

        // Кружка кофе
        GameObject coffeeGo = CreateUIImage("CoffeeMug", deskMatGo.transform, sprCoffee, new Vector2(110, 130), new Vector2(-400, -180));

        // Банка энергетика
        GameObject energyGo = CreateUIImage("EnergyCan", deskMatGo.transform, sprEnergy, new Vector2(90, 155), new Vector2(450, -60));

        // Кот-маскот
        GameObject catGo = CreateUIImage("CatMascot", deskMatGo.transform, sprCat, new Vector2(160, 110), new Vector2(-420, -50));

        // Кнопка Тапа (покрывает клавиатуру и зону стола)
        GameObject tapButtonGo = CreateUIObject("TapButton", deskMatGo.transform);
        RectTransform tapBtnRt = tapButtonGo.GetComponent<RectTransform>();
        tapBtnRt.anchoredPosition = new Vector2(0, -180);
        tapBtnRt.sizeDelta = new Vector2(850, 320);
        Button tapBtn = tapButtonGo.AddComponent<Button>();
        Image tapBtnImg = tapButtonGo.AddComponent<Image>();
        tapBtnImg.color = new Color(1f, 1f, 1f, 0.001f); // Прозрачная интерактивная зона

        // Компонент WorkplaceVisuals (анимация монитора, клавиатуры, кофе, кота)
        WorkplaceVisuals visuals = workplaceGo.AddComponent<WorkplaceVisuals>();
        SerializedObject visSo = new SerializedObject(visuals);
        visSo.FindProperty("monitorCodeText").objectReferenceValue = terminalText;
        visSo.FindProperty("keyboardTransform").objectReferenceValue = keyboardGo.transform;
        visSo.FindProperty("keyboardGlowImage").objectReferenceValue = keyGlowImg;
        visSo.FindProperty("coffeeMugTransform").objectReferenceValue = coffeeGo.transform;
        visSo.FindProperty("energyCanTransform").objectReferenceValue = energyGo.transform;
        visSo.FindProperty("catTransform").objectReferenceValue = catGo.transform;
        visSo.ApplyModifiedProperties();

        // Компонент ClickJuice (Squash & stretch, всплывающий текст и мемы)
        ClickJuice clickJuice = workplaceGo.AddComponent<ClickJuice>();
        SerializedObject cjSo = new SerializedObject(clickJuice);
        cjSo.FindProperty("targetTransform").objectReferenceValue = keyboardGo.transform;
        cjSo.FindProperty("punchScaleFactor").floatValue = 0.94f;
        cjSo.FindProperty("bounceDuration").floatValue = 0.14f;
        cjSo.FindProperty("floatingTextPrefab").objectReferenceValue = floatingTextPrefab;
        cjSo.FindProperty("floatingTextParent").objectReferenceValue = floatTextParentGo.transform;
        cjSo.FindProperty("floatDistance").floatValue = 90f;
        cjSo.FindProperty("floatDuration").floatValue = 0.75f;
        cjSo.ApplyModifiedProperties();

        // -------------------------------------------------------------
        // СЕКЦИЯ 3: МАГАЗИН И ПРОЕКТЫ (ShopPanel / DevShopUI)
        // -------------------------------------------------------------
        GameObject shopGo = CreateUIObject("ShopPanel", canvas.transform);
        RectTransform shopRt = shopGo.GetComponent<RectTransform>();
        shopRt.anchorMin = new Vector2(0.64f, 0f);
        shopRt.anchorMax = new Vector2(1f, 0.90f);
        shopRt.pivot = new Vector2(1f, 0.5f);
        shopRt.offsetMin = new Vector2(10, 20);
        shopRt.offsetMax = new Vector2(-20, -10);

        Image shopBg = shopGo.AddComponent<Image>();
        shopBg.color = new Color(0.09f, 0.11f, 0.15f, 0.98f);

        // Заголовок магазина
        GameObject shopHeader = CreateUIObject("ShopHeader", shopGo.transform);
        RectTransform shRt = shopHeader.GetComponent<RectTransform>();
        shRt.anchorMin = new Vector2(0f, 1f);
        shRt.anchorMax = new Vector2(1f, 1f);
        shRt.pivot = new Vector2(0.5f, 1f);
        shRt.anchoredPosition = new Vector2(0, 0);
        shRt.sizeDelta = new Vector2(0, 60);

        TMP_Text shopTitle = CreateTMPText("Title", shopHeader.transform, fontAsset, "💻 DEV-МАРКЕТ", 24, FontStyles.Bold, new Color(0.35f, 0.65f, 1f));
        SetFullStretch(shopTitle.rectTransform);
        shopTitle.alignment = TextAlignmentOptions.Center;

        // Панель вкладок (Железо / Команда / Проекты)
        GameObject tabBarGo = CreateUIObject("TabBar", shopGo.transform);
        RectTransform tbRt = tabBarGo.GetComponent<RectTransform>();
        tbRt.anchorMin = new Vector2(0f, 1f);
        tbRt.anchorMax = new Vector2(1f, 1f);
        tbRt.pivot = new Vector2(0.5f, 1f);
        tbRt.anchoredPosition = new Vector2(0, -60);
        tbRt.sizeDelta = new Vector2(0, 48);

        GameObject tabHwGo = CreateUIButton("TabHardware", tabBarGo.transform, sprBtnCyan, new Vector2(210, 44), new Vector2(-215, 0));
        TMP_Text tabHwTxt = CreateTMPText("Txt", tabHwGo.transform, fontAsset, "💻 ЖЕЛЕЗО", 16, FontStyles.Bold, Color.white);
        SetFullStretch(tabHwTxt.rectTransform); tabHwTxt.alignment = TextAlignmentOptions.Center;

        GameObject tabStGo = CreateUIButton("TabStaff", tabBarGo.transform, sprBtnCyan, new Vector2(210, 44), new Vector2(0, 0));
        TMP_Text tabStTxt = CreateTMPText("Txt", tabStGo.transform, fontAsset, "🤖 КОМАНДА", 16, FontStyles.Bold, Color.white);
        SetFullStretch(tabStTxt.rectTransform); tabStTxt.alignment = TextAlignmentOptions.Center;

        GameObject tabPrGo = CreateUIButton("TabProjects", tabBarGo.transform, sprBtnCyan, new Vector2(210, 44), new Vector2(215, 0));
        TMP_Text tabPrTxt = CreateTMPText("Txt", tabPrGo.transform, fontAsset, "🚀 ПРОЕКТЫ", 16, FontStyles.Bold, Color.white);
        SetFullStretch(tabPrTxt.rectTransform); tabPrTxt.alignment = TextAlignmentOptions.Center;

        // Скроллируемая область контента (ScrollRect)
        GameObject scrollGo = CreateUIObject("ShopScrollView", shopGo.transform);
        RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0f, 0f);
        scrollRt.anchorMax = new Vector2(1f, 1f);
        scrollRt.offsetMin = new Vector2(15, 15);
        scrollRt.offsetMax = new Vector2(-15, -115);

        ScrollRect scrollRect = scrollGo.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 25f;

        // Viewport
        GameObject viewportGo = CreateUIObject("Viewport", scrollGo.transform);
        SetFullStretch(viewportGo.GetComponent<RectTransform>());
        Image vpImg = viewportGo.AddComponent<Image>();
        vpImg.color = Color.white;
        Mask vpMask = viewportGo.AddComponent<Mask>();
        vpMask.showMaskGraphic = false;
        scrollRect.viewport = viewportGo.GetComponent<RectTransform>();

        // Контейнер 1: Железо
        GameObject hwViewGo = CreateCategoryView("HardwareView", viewportGo.transform);
        // Контейнер 2: Персонал
        GameObject stViewGo = CreateCategoryView("StaffView", viewportGo.transform);
        // Контейнер 3: Проекты
        GameObject prViewGo = CreateCategoryView("ProjectsView", viewportGo.transform);

        scrollRect.content = hwViewGo.GetComponent<RectTransform>();

        DevShopUI devShop = shopGo.AddComponent<DevShopUI>();
        List<DevShopUI.UpgradeButtonBinding> upgBindings = new List<DevShopUI.UpgradeButtonBinding>();
        List<DevShopUI.ProjectButtonBinding> prjBindings = new List<DevShopUI.ProjectButtonBinding>();

        // 1. Создаем карточки железа
        var hwData = new[]
        {
            ("hw_keyboard", "Механическая клавиатура", "+1 строка/клик", "50 ₽"),
            ("hw_mouse", "Игровая мышь с макросом", "+3 строки/клик", "250 ₽"),
            ("hw_monitor2", "Второй монитор (для доки)", "+10 строк/клик", "1 200 ₽"),
            ("hw_pc_rig", "Мощный ПК с RTX", "+35 строк/клик", "6 000 ₽"),
            ("hw_chair", "Эргономичное кресло", "+120 строк/клик", "25 000 ₽")
        };
        foreach (var item in hwData)
        {
            var binding = CreateUpgradeCard(hwViewGo.transform, fontAsset, sprCardBg, sprBtnCyan, item.Item1, item.Item2, item.Item3, item.Item4);
            upgBindings.Add(binding);
        }

        // 2. Создаем карточки команды
        var staffData = new[]
        {
            ("staff_script", "Bash / Python скрипт", "+1 строка кода/сек", "100 ₽"),
            ("staff_cat", "Кот на клавиатуре", "+4 строки кода/сек", "500 ₽"),
            ("staff_gpt", "ChatGPT подписка", "+15 строк кода/сек", "2 500 ₽"),
            ("staff_intern", "Студент-стажёр", "+60 строк кода/сек", "12 000 ₽"),
            ("staff_senior", "Senior ментор на час", "+250 строк кода/сек", "60 000 ₽")
        };
        foreach (var item in staffData)
        {
            var binding = CreateUpgradeCard(stViewGo.transform, fontAsset, sprCardBg, sprBtnCyan, item.Item1, item.Item2, item.Item3, item.Item4);
            upgBindings.Add(binding);
        }

        // 3. Создаем карточки проектов
        var projData = new[]
        {
            ("proj_lab", "Лабораторная по Unity", "100 строк кода", "+300 ₽ (+2/сек)"),
            ("proj_tg_bot", "Бот в Telegram", "600 строк кода", "+2 000 ₽ (+10/сек)"),
            ("proj_yandex", "Игра для Яндекс Игр", "3 500 строк кода", "+15 000 ₽ (+50/сек)"),
            ("proj_steam", "Инди-игра в Steam", "20 000 строк кода", "+100 000 ₽ (+250/сек)"),
            ("proj_mmo", "MMO-убийца Ведьмака", "150 000 строк кода", "+1 000 000 ₽ (+2000/сек)")
        };
        foreach (var item in projData)
        {
            var binding = CreateProjectCard(prViewGo.transform, fontAsset, sprCardBg, sprBtnCyan, item.Item1, item.Item2, item.Item3, item.Item4);
            prjBindings.Add(binding);
        }

        // Привязываем все данные к DevShopUI
        SerializedObject dsSo = new SerializedObject(devShop);
        SerializedProperty upgListProp = dsSo.FindProperty("upgradeBindings");
        upgListProp.arraySize = upgBindings.Count;
        for (int i = 0; i < upgBindings.Count; i++)
        {
            var el = upgListProp.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("upgradeId").stringValue = upgBindings[i].upgradeId;
            el.FindPropertyRelative("buyButton").objectReferenceValue = upgBindings[i].buyButton;
            el.FindPropertyRelative("titleText").objectReferenceValue = upgBindings[i].titleText;
            el.FindPropertyRelative("costText").objectReferenceValue = upgBindings[i].costText;
            el.FindPropertyRelative("levelText").objectReferenceValue = upgBindings[i].levelText;
        }

        SerializedProperty prjListProp = dsSo.FindProperty("projectBindings");
        prjListProp.arraySize = prjBindings.Count;
        for (int i = 0; i < prjBindings.Count; i++)
        {
            var el = prjListProp.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("projectId").stringValue = prjBindings[i].projectId;
            el.FindPropertyRelative("releaseButton").objectReferenceValue = prjBindings[i].releaseButton;
            el.FindPropertyRelative("titleText").objectReferenceValue = prjBindings[i].titleText;
            el.FindPropertyRelative("reqText").objectReferenceValue = prjBindings[i].reqText;
            el.FindPropertyRelative("rewardText").objectReferenceValue = prjBindings[i].rewardText;
        }

        dsSo.FindProperty("tabHardwareBtn").objectReferenceValue = tabHwGo.GetComponent<Button>();
        dsSo.FindProperty("tabStaffBtn").objectReferenceValue = tabStGo.GetComponent<Button>();
        dsSo.FindProperty("tabProjectsBtn").objectReferenceValue = tabPrGo.GetComponent<Button>();
        dsSo.FindProperty("hardwareView").objectReferenceValue = hwViewGo;
        dsSo.FindProperty("staffView").objectReferenceValue = stViewGo;
        dsSo.FindProperty("projectsView").objectReferenceValue = prViewGo;
        dsSo.ApplyModifiedProperties();

        // -------------------------------------------------------------
        // СЕКЦИЯ 4: ПРИВЯЗКА TAPCOUNTER (Лабораторная + Idle)
        // -------------------------------------------------------------
        GameObject counterGo = GameObject.Find("CounterScript");
        if (counterGo == null) counterGo = new GameObject("CounterScript");
        TapCounter tapCounter = counterGo.GetComponent<TapCounter>();
        if (tapCounter == null) tapCounter = counterGo.AddComponent<TapCounter>();

        SerializedObject tcSo = new SerializedObject(tapCounter);
        tcSo.FindProperty("counterText").objectReferenceValue = codeCounterText;
        tcSo.FindProperty("tapButton").objectReferenceValue = tapBtn;
        tcSo.FindProperty("resetButton").objectReferenceValue = resetBtnGo.GetComponent<Button>();
        tcSo.FindProperty("scorePrefix").stringValue = "Строк кода: ";
        tcSo.FindProperty("moneyText").objectReferenceValue = moneyText;
        tcSo.FindProperty("statsText").objectReferenceValue = statsText;
        tcSo.FindProperty("boostTimerText").objectReferenceValue = boostTimerText;
        tcSo.FindProperty("energyBoostButton").objectReferenceValue = boostBtnGo.GetComponent<Button>();
        tcSo.ApplyModifiedProperties();

        // 9. Сохраняем сцену
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[HelloTap] Scene Rebuild Complete! SampleScene saved successfully.");
    }

    private static void SetupSpriteImporters()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });
        bool reimportedAny = false;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                reimportedAny = true;
            }
        }
        if (reimportedAny)
        {
            AssetDatabase.Refresh();
        }
    }

    private static TMP_FontAsset LoadDefaultTMPFont()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Packages/com.unity.textmeshpro/Fonts/LiberationSans SDF.asset");
        if (font == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            if (guids.Length > 0)
            {
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
        }
        return font;
    }

    private static TMP_Text CreateFloatingTextPrefab(TMP_FontAsset font)
    {
        string prefabPath = "Assets/Sprites/FloatingTextPrefab.prefab";
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null) return existingPrefab.GetComponent<TMP_Text>();

        GameObject go = new GameObject("FloatingText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        TMP_Text tmp = go.GetComponent<TMP_Text>();
        tmp.font = font;
        tmp.fontSize = 28;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0f, 1f, 0.55f);
        tmp.raycastTarget = false;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
        return prefab.GetComponent<TMP_Text>();
    }

    private static GameObject CreateCategoryView(string name, Transform parent)
    {
        GameObject go = CreateUIObject(name, parent);
        SetFullStretch(go.GetComponent<RectTransform>());

        VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = go.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return go;
    }

    private static DevShopUI.UpgradeButtonBinding CreateUpgradeCard(Transform parent, TMP_FontAsset font, Sprite cardBg, Sprite btnSprite, string id, string title, string power, string cost)
    {
        GameObject cardGo = CreateUIObject("Card_" + id, parent);
        RectTransform rt = cardGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 95);

        Image img = cardGo.AddComponent<Image>();
        img.sprite = cardBg;
        img.type = Image.Type.Sliced;
        img.color = new Color(0.12f, 0.14f, 0.19f, 0.95f);

        // Название
        TMP_Text titleTxt = CreateTMPText("Title", cardGo.transform, font, title, 18, FontStyles.Bold, Color.white);
        titleTxt.rectTransform.anchorMin = new Vector2(0f, 1f);
        titleTxt.rectTransform.anchorMax = new Vector2(0.68f, 1f);
        titleTxt.rectTransform.pivot = new Vector2(0f, 1f);
        titleTxt.rectTransform.anchoredPosition = new Vector2(16, -10);
        titleTxt.rectTransform.sizeDelta = new Vector2(0, 26);

        // Бонус
        TMP_Text powerTxt = CreateTMPText("Power", cardGo.transform, font, power, 15, FontStyles.Normal, new Color(0f, 0.9f, 0.6f));
        powerTxt.rectTransform.anchorMin = new Vector2(0f, 0f);
        powerTxt.rectTransform.anchorMax = new Vector2(0.68f, 0f);
        powerTxt.rectTransform.pivot = new Vector2(0f, 0f);
        powerTxt.rectTransform.anchoredPosition = new Vector2(16, 12);
        powerTxt.rectTransform.sizeDelta = new Vector2(0, 24);

        // Уровень
        TMP_Text levelTxt = CreateTMPText("Level", cardGo.transform, font, "Ур. 0", 14, FontStyles.Normal, new Color(0.6f, 0.65f, 0.75f));
        levelTxt.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        levelTxt.rectTransform.anchorMax = new Vector2(0.68f, 0.5f);
        levelTxt.rectTransform.pivot = new Vector2(0f, 0.5f);
        levelTxt.rectTransform.anchoredPosition = new Vector2(16, -2);
        levelTxt.rectTransform.sizeDelta = new Vector2(0, 22);

        // Кнопка покупки
        GameObject btnGo = CreateUIButton("BuyButton", cardGo.transform, btnSprite, new Vector2(160, 52), new Vector2(-95, 0));
        RectTransform btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(1f, 0.5f);
        btnRt.anchorMax = new Vector2(1f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);

        TMP_Text costTxt = CreateTMPText("CostText", btnGo.transform, font, cost, 16, FontStyles.Bold, Color.white);
        SetFullStretch(costTxt.rectTransform);
        costTxt.alignment = TextAlignmentOptions.Center;

        var binding = new DevShopUI.UpgradeButtonBinding
        {
            upgradeId = id,
            buyButton = btnGo.GetComponent<Button>(),
            titleText = titleTxt,
            costText = costTxt,
            levelText = levelTxt
        };
        return binding;
    }

    private static DevShopUI.ProjectButtonBinding CreateProjectCard(Transform parent, TMP_FontAsset font, Sprite cardBg, Sprite btnSprite, string id, string title, string req, string reward)
    {
        GameObject cardGo = CreateUIObject("ProjCard_" + id, parent);
        RectTransform rt = cardGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 105);

        Image img = cardGo.AddComponent<Image>();
        img.sprite = cardBg;
        img.type = Image.Type.Sliced;
        img.color = new Color(0.12f, 0.14f, 0.19f, 0.95f);

        // Название
        TMP_Text titleTxt = CreateTMPText("Title", cardGo.transform, font, title, 18, FontStyles.Bold, new Color(0.35f, 0.70f, 1f));
        titleTxt.rectTransform.anchorMin = new Vector2(0f, 1f);
        titleTxt.rectTransform.anchorMax = new Vector2(0.68f, 1f);
        titleTxt.rectTransform.pivot = new Vector2(0f, 1f);
        titleTxt.rectTransform.anchoredPosition = new Vector2(16, -10);
        titleTxt.rectTransform.sizeDelta = new Vector2(0, 26);

        // Требуемые строки
        TMP_Text reqTxt = CreateTMPText("Req", cardGo.transform, font, req, 14, FontStyles.Normal, new Color(1.0f, 0.70f, 0.35f));
        reqTxt.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        reqTxt.rectTransform.anchorMax = new Vector2(0.68f, 0.5f);
        reqTxt.rectTransform.pivot = new Vector2(0f, 0.5f);
        reqTxt.rectTransform.anchoredPosition = new Vector2(16, -2);
        reqTxt.rectTransform.sizeDelta = new Vector2(0, 22);

        // Награда
        TMP_Text rewardTxt = CreateTMPText("Reward", cardGo.transform, font, reward, 14, FontStyles.Normal, new Color(0f, 0.9f, 0.6f));
        rewardTxt.rectTransform.anchorMin = new Vector2(0f, 0f);
        rewardTxt.rectTransform.anchorMax = new Vector2(0.68f, 0f);
        rewardTxt.rectTransform.pivot = new Vector2(0f, 0f);
        rewardTxt.rectTransform.anchoredPosition = new Vector2(16, 10);
        rewardTxt.rectTransform.sizeDelta = new Vector2(0, 22);

        // Кнопка релиза
        GameObject btnGo = CreateUIButton("ReleaseButton", cardGo.transform, btnSprite, new Vector2(160, 52), new Vector2(-95, 0));
        RectTransform btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(1f, 0.5f);
        btnRt.anchorMax = new Vector2(1f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);

        TMP_Text btnTxt = CreateTMPText("BtnText", btnGo.transform, font, "РЕЛИЗ", 16, FontStyles.Bold, Color.white);
        SetFullStretch(btnTxt.rectTransform);
        btnTxt.alignment = TextAlignmentOptions.Center;

        var binding = new DevShopUI.ProjectButtonBinding
        {
            projectId = id,
            releaseButton = btnGo.GetComponent<Button>(),
            titleText = titleTxt,
            reqText = reqTxt,
            rewardText = rewardTxt
        };
        return binding;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject CreateUIImage(string name, Transform parent, Sprite sprite, Vector2 size, Vector2 pos)
    {
        GameObject go = CreateUIObject(name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        return go;
    }

    private static GameObject CreateUIButton(string name, Transform parent, Sprite sprite, Vector2 size, Vector2 pos)
    {
        GameObject go = CreateUIObject(name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        return go;
    }

    private static TMP_Text CreateTMPText(string name, Transform parent, TMP_FontAsset font, string text, float size, FontStyles style, Color color)
    {
        GameObject go = CreateUIObject(name, parent);
        TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void SetFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
