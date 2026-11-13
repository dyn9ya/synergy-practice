using Cinemachine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Собирает тестовую сцену для кейса 5 через меню Case5 -> Build Scene.
// Создает игрока, камеру Cinemachine, бонус с Animator, монеты, бочки, препятствия и UI.
public static class Case5SceneBuilder
{
    private const string Root = "Assets/Case5";
    private const string ScenePath = Root + "/Scenes/Case5.unity";

    [MenuItem("Case5/Build Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        CreateFolder(Root, "Scenes");
        CreateFolder(Root, "Materials");
        CreateFolder(Root, "Animations");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // материалы
        var groundMat = CreateMaterial("Ground", new Color(0.35f, 0.55f, 0.35f));
        var playerMat = CreateMaterial("Player", new Color(0.2f, 0.45f, 0.95f));
        var coinMat = CreateMaterial("Coin", new Color(1f, 0.8f, 0.1f), 0.8f);
        var barrelMat = CreateMaterial("Barrel", new Color(0.8f, 0.1f, 0.1f));
        var obstacleMat = CreateMaterial("Obstacle", new Color(0.95f, 0.5f, 0.1f));
        var bonusMat = CreateMaterial("Bonus", new Color(0.3f, 1f, 0.6f));
        var wallMat = CreateMaterial("Wall", new Color(0.6f, 0.6f, 0.65f));
        var particleMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");

        // земля и стенки, чтобы не упасть
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(4f, 1f, 4f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMat;
        CreateWall("Wall_N", new Vector3(0f, 0.5f, 20f), new Vector3(40f, 1f, 1f), wallMat);
        CreateWall("Wall_S", new Vector3(0f, 0.5f, -20f), new Vector3(40f, 1f, 1f), wallMat);
        CreateWall("Wall_E", new Vector3(20f, 0.5f, 0f), new Vector3(1f, 1f, 40f), wallMat);
        CreateWall("Wall_W", new Vector3(-20f, 0.5f, 0f), new Vector3(1f, 1f, 40f), wallMat);

        // игрок
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1f, -4f);
        player.GetComponent<Renderer>().sharedMaterial = playerMat;
        var playerRb = player.AddComponent<Rigidbody>();
        playerRb.mass = 1f;
        var health = player.AddComponent<Health>();
        player.AddComponent<PlayerController>();

        // "нос", чтобы было видно куда смотрит игрок
        var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nose.name = "Nose";
        Object.DestroyImmediate(nose.GetComponent<Collider>());
        nose.transform.SetParent(player.transform, false);
        nose.transform.localPosition = new Vector3(0f, 0.4f, 0.45f);
        nose.transform.localScale = new Vector3(0.3f, 0.15f, 0.2f);

        // частицы пыли при движении
        var dustGo = new GameObject("DustTrail");
        dustGo.transform.SetParent(player.transform, false);
        dustGo.transform.localPosition = new Vector3(0f, -0.95f, 0f);
        dustGo.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        var dust = dustGo.AddComponent<ParticleSystem>();
        SetupDust(dust, particleMat);
        var trail = dustGo.AddComponent<PlayerTrail>();
        trail.dust = dust;
        trail.player = player.GetComponent<PlayerController>();

        // камера: CinemachineBrain на Main Camera + Virtual Camera, которая следит за игроком
        var mainCam = Camera.main;
        mainCam.gameObject.AddComponent<CinemachineBrain>();
        var vcamGo = new GameObject("CM vcam Player");
        var vcam = vcamGo.AddComponent<CinemachineVirtualCamera>();
        vcam.Follow = player.transform;
        vcam.LookAt = player.transform;
        vcam.m_Lens.FieldOfView = 55f;
        var body = vcam.AddCinemachineComponent<CinemachineTransposer>();
        body.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
        body.m_FollowOffset = new Vector3(0f, 9f, -10f);
        body.m_XDamping = 0.6f;
        body.m_YDamping = 0.6f;
        body.m_ZDamping = 0.6f;
        var aim = vcam.AddCinemachineComponent<CinemachineComposer>();
        aim.m_TrackedObjectOffset = new Vector3(0f, 0.5f, 0f);
        aim.m_HorizontalDamping = 0.5f;
        aim.m_VerticalDamping = 0.5f;
        vcamGo.transform.position = player.transform.position + body.m_FollowOffset;

        // бонус с анимацией
        var controller = CreateBonusAnimator();
        var bonusZone = new GameObject("BonusZone");
        bonusZone.transform.position = new Vector3(10f, 0f, 10f);
        var zoneCol = bonusZone.AddComponent<SphereCollider>();
        zoneCol.isTrigger = true;
        zoneCol.radius = 3f;
        zoneCol.center = new Vector3(0f, 1f, 0f);
        var bonusVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bonusVisual.name = "BonusVisual";
        Object.DestroyImmediate(bonusVisual.GetComponent<Collider>());
        bonusVisual.transform.SetParent(bonusZone.transform, false);
        bonusVisual.transform.localPosition = new Vector3(0f, 1f, 0f);
        bonusVisual.transform.localRotation = Quaternion.Euler(45f, 0f, 45f);
        bonusVisual.transform.localScale = Vector3.one * 0.8f;
        bonusVisual.GetComponent<Renderer>().sharedMaterial = bonusMat;
        var animator = bonusVisual.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        var lightGo = new GameObject("BonusLight");
        lightGo.transform.SetParent(bonusZone.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.3f, 1f, 0.6f);
        light.range = 6f;
        light.intensity = 3f;
        var zone = bonusZone.AddComponent<BonusZone>();
        zone.bonusAnimator = animator;
        zone.bonusLight = light;

        // монеты: круг вокруг центра и дорожка к бонусу
        var coins = new GameObject("Coins");
        for (int i = 0; i < 10; i++)
        {
            float a = i * Mathf.PI * 2f / 10f;
            CreateCoin(coins.transform, new Vector3(Mathf.Cos(a) * 6f, 0.8f, Mathf.Sin(a) * 6f), coinMat);
        }
        for (int i = 1; i <= 4; i++)
            CreateCoin(coins.transform, new Vector3(4f + i * 1.3f, 0.8f, 4f + i * 1.3f), coinMat);

        // бочки со взрывчаткой
        var barrels = new GameObject("Barrels");
        CreateBarrel(barrels.transform, new Vector3(-9f, 0.8f, 6f), barrelMat);
        CreateBarrel(barrels.transform, new Vector3(-11f, 0.8f, 8f), barrelMat);
        CreateBarrel(barrels.transform, new Vector3(-7f, 0.8f, 9f), barrelMat);

        // подвижные препятствия
        var obstacles = new GameObject("Obstacles");
        var slider = CreateObstacle(obstacles.transform, "Slider", new Vector3(-6f, 0.75f, -10f), new Vector3(1.5f, 1.5f, 1.5f), obstacleMat);
        slider.mode = MovingObstacle.MoveMode.PingPong;
        slider.moveOffset = new Vector3(12f, 0f, 0f);
        slider.moveSpeed = 0.35f;
        var spinner = CreateObstacle(obstacles.transform, "Spinner", new Vector3(10f, 0.6f, -9f), new Vector3(8f, 0.6f, 0.6f), obstacleMat);
        spinner.mode = MovingObstacle.MoveMode.Rotate;
        spinner.rotateSpeed = 70f;

        // менеджер, кошелек и UI
        var managerGo = new GameObject("GameManager");
        managerGo.AddComponent<Wallet>();
        var manager = managerGo.AddComponent<GameManager>();
        manager.player = health;
        manager.particleMaterial = particleMat;
        BuildUI(manager);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuild(ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Case5: сцена собрана и сохранена в " + ScenePath);
    }

    // ===== Animator для бонуса: состояния Idle и Active, параметр PlayerInside =====

    private static AnimatorController CreateBonusAnimator()
    {
        string folder = Root + "/Animations/";

        // Idle: медленно крутится
        var idle = new AnimationClip { name = "BonusIdle" };
        SetCurve(idle, "localEulerAnglesRaw.y", AnimationCurve.Linear(0f, 0f, 4f, 360f));
        SetCurve(idle, "localEulerAnglesRaw.x", AnimationCurve.Constant(0f, 4f, 45f));
        SetCurve(idle, "localEulerAnglesRaw.z", AnimationCurve.Constant(0f, 4f, 45f));
        SetCurve(idle, "localPosition.y", new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(2f, 1.2f), new Keyframe(4f, 1f)));
        SetScale(idle, new AnimationCurve(new Keyframe(0f, 0.8f), new Keyframe(4f, 0.8f)));
        SetLoop(idle);

        // Active: быстро крутится, подпрыгивает и пульсирует
        var active = new AnimationClip { name = "BonusActive" };
        SetCurve(active, "localEulerAnglesRaw.y", AnimationCurve.Linear(0f, 0f, 1f, 360f));
        SetCurve(active, "localEulerAnglesRaw.x", AnimationCurve.Constant(0f, 1f, 45f));
        SetCurve(active, "localEulerAnglesRaw.z", AnimationCurve.Constant(0f, 1f, 45f));
        SetCurve(active, "localPosition.y", new AnimationCurve(new Keyframe(0f, 1.6f), new Keyframe(0.5f, 2.3f), new Keyframe(1f, 1.6f)));
        SetScale(active, new AnimationCurve(new Keyframe(0f, 1.1f), new Keyframe(0.25f, 1.35f), new Keyframe(0.5f, 1.1f), new Keyframe(0.75f, 1.35f), new Keyframe(1f, 1.1f)));
        SetLoop(active);

        AssetDatabase.DeleteAsset(folder + "BonusIdle.anim");
        AssetDatabase.DeleteAsset(folder + "BonusActive.anim");
        AssetDatabase.DeleteAsset(folder + "Bonus.controller");
        AssetDatabase.CreateAsset(idle, folder + "BonusIdle.anim");
        AssetDatabase.CreateAsset(active, folder + "BonusActive.anim");

        var controller = AnimatorController.CreateAnimatorControllerAtPath(folder + "Bonus.controller");
        controller.AddParameter("PlayerInside", AnimatorControllerParameterType.Bool);
        var sm = controller.layers[0].stateMachine;
        var idleState = sm.AddState("Idle");
        idleState.motion = idle;
        var activeState = sm.AddState("Active");
        activeState.motion = active;
        sm.defaultState = idleState;

        var toActive = idleState.AddTransition(activeState);
        toActive.hasExitTime = false;
        toActive.duration = 0.25f;
        toActive.AddCondition(AnimatorConditionMode.If, 0f, "PlayerInside");

        var toIdle = activeState.AddTransition(idleState);
        toIdle.hasExitTime = false;
        toIdle.duration = 0.4f;
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "PlayerInside");

        return controller;
    }

    private static void SetCurve(AnimationClip clip, string property, AnimationCurve curve)
    {
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), property), curve);
    }

    private static void SetScale(AnimationClip clip, AnimationCurve curve)
    {
        SetCurve(clip, "localScale.x", curve);
        SetCurve(clip, "localScale.y", curve);
        SetCurve(clip, "localScale.z", curve);
    }

    private static void SetLoop(AnimationClip clip)
    {
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
    }

    // ===== объекты сцены =====

    private static void CreateCoin(Transform parent, Vector3 pos, Material mat)
    {
        var coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = "Coin";
        coin.transform.SetParent(parent, false);
        coin.transform.position = pos;
        coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        coin.transform.localScale = new Vector3(0.7f, 0.06f, 0.7f);
        coin.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(coin.GetComponent<Collider>());
        var trigger = coin.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        coin.AddComponent<Coin>();
    }

    private static void CreateBarrel(Transform parent, Vector3 pos, Material mat)
    {
        var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.name = "ExplosiveBarrel";
        barrel.transform.SetParent(parent, false);
        barrel.transform.position = pos;
        barrel.transform.localScale = new Vector3(0.9f, 0.8f, 0.9f);
        barrel.GetComponent<Renderer>().sharedMaterial = mat;
        var rb = barrel.AddComponent<Rigidbody>();
        rb.mass = 3f;
        barrel.AddComponent<ExplosiveBarrel>();
    }

    private static MovingObstacle CreateObstacle(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        return go.AddComponent<MovingObstacle>();
    }

    private static void CreateWall(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private static void SetupDust(ParticleSystem ps, Material mat)
    {
        var main = ps.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startColor = new Color(0.85f, 0.78f, 0.6f, 0.8f);
        main.gravityModifier = -0.05f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;

        var emission = ps.emission;
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 35f;
        shape.radius = 0.25f;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        var color = ps.colorOverLifetime;
        color.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) });
        color.color = g;

        ps.GetComponent<ParticleSystemRenderer>().sharedMaterial = mat;
    }

    private static void BuildUI(GameManager manager)
    {
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        canvasGo.AddComponent<GraphicRaycaster>();

        manager.coinsText = CreateText(canvasGo.transform, "CoinsText", new Vector2(0f, 1f), new Vector2(20f, -20f), TextAnchor.UpperLeft, 28);
        manager.hpText = CreateText(canvasGo.transform, "HPText", new Vector2(0f, 1f), new Vector2(20f, -60f), TextAnchor.UpperLeft, 28);
        manager.gameOverText = CreateText(canvasGo.transform, "GameOverText", new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.MiddleCenter, 44);
        manager.gameOverText.color = new Color(1f, 0.35f, 0.3f);
        manager.gameOverText.rectTransform.sizeDelta = new Vector2(900f, 200f);
    }

    private static Text CreateText(Transform parent, string name, Vector2 anchor, Vector2 pos, TextAnchor align, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(600f, 40f);
        var text = go.AddComponent<Text>();
        text.font = LoadFont();
        text.fontSize = fontSize;
        text.alignment = align;
        text.color = Color.white;
        var shadow = go.AddComponent<Shadow>();
        shadow.effectDistance = new Vector2(2f, -2f);
        return text;
    }

    private static Font LoadFont()
    {
        // в новых версиях Unity встроенный шрифт называется LegacyRuntime.ttf, в старых Arial.ttf
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return font;
    }

    // ===== служебное =====

    private static Material CreateMaterial(string name, Color color, float metallic = 0f)
    {
        string path = Root + "/Materials/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Glossiness", metallic > 0f ? 0.7f : 0.3f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void CreateFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
    }

    private static void AddSceneToBuild(string path)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
            if (s.path == path) return;
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes);
        list.Insert(0, new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}
