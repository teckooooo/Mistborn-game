// Archivo: Assets/Editor/AnimatorSetup.cs
// Este script crea un menú en Unity: Tools → Setup Player Animator
// Solo se ejecuta en el Editor, no afecta el build final.

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

public class AnimatorSetup : EditorWindow
{
    // ── Configuración ──────────────────────────────────────────────
    // Ajusta estos nombres para que coincidan con tus sprites.
    // El script busca sprites cuyo nombre CONTENGA estas palabras clave.
    private static readonly AnimationDef[] Animations = new[]
    {
        new AnimationDef("Idle",      new[]{"idle"},               loop: true,  fps: 8),
        new AnimationDef("Walk",      new[]{"walk","run"},          loop: true,  fps: 10),
        new AnimationDef("Jump",      new[]{"jump"},                loop: false, fps: 10),
        new AnimationDef("Fall",      new[]{"fall"},                loop: true,  fps: 8),
        new AnimationDef("ThrowCoin", new[]{"throw","coin"},        loop: false, fps: 12),
        new AnimationDef("Push",      new[]{"push"},                loop: false, fps: 12),
        new AnimationDef("Hurt",      new[]{"hurt","hit","damage"}, loop: false, fps: 10),
    };

    private AnimatorController targetController;
    private string spritesFolder = "Assets/Sprites";

    [MenuItem("Tools/Setup Player Animator")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorSetup>("Animator Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Player Animator Setup", EditorStyles.boldLabel);
        GUILayout.Space(8);

        targetController = (AnimatorController)EditorGUILayout.ObjectField(
            "Animator Controller", targetController, typeof(AnimatorController), false);

        GUILayout.BeginHorizontal();
        spritesFolder = EditorGUILayout.TextField("Sprites Folder", spritesFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Sprites Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // Convertir ruta absoluta a relativa
                if (path.StartsWith(Application.dataPath))
                    spritesFolder = "Assets" + path.Substring(Application.dataPath.Length);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(12);

        EditorGUILayout.HelpBox(
            "El script busca sprites cuyos nombres contengan las palabras clave definidas.\n" +
            "Si tus sprites tienen otros nombres, edita el array 'Animations' arriba en el código.",
            MessageType.Info);

        GUILayout.Space(8);

        GUI.enabled = targetController != null;
        if (GUILayout.Button("Generar Animator", GUILayout.Height(36)))
        {
            BuildAnimator();
        }
        GUI.enabled = true;

        GUILayout.Space(8);

        if (GUILayout.Button("Solo agregar parámetros"))
        {
            if (targetController != null) AddParameters(targetController);
        }
    }

    void BuildAnimator()
    {
        var controller = targetController;

        // 1. Agregar parámetros
        AddParameters(controller);

        // 2. Cargar todos los sprites de la carpeta
        var allSprites = LoadAllSprites(spritesFolder);
        Debug.Log($"[AnimatorSetup] Sprites encontrados: {allSprites.Count}");

        // 3. Crear clips y estados
        var createdStates = new Dictionary<string, AnimatorState>();

        foreach (var def in Animations)
        {
            var matchingSprites = FindSprites(allSprites, def.keywords);

            if (matchingSprites.Count == 0)
            {
                Debug.LogWarning($"[AnimatorSetup] No se encontraron sprites para '{def.name}' " +
                                 $"(palabras clave: {string.Join(", ", def.keywords)})");
                continue;
            }

            // Crear AnimationClip
            var clip = CreateClip(def.name, matchingSprites, def.fps, def.loop);
            var clipPath = $"Assets/Animations/{def.name}.anim";
            System.IO.Directory.CreateDirectory("Assets/Animations");
            AssetDatabase.CreateAsset(clip, clipPath);

            // Agregar estado al Animator
            var state = controller.layers[0].stateMachine.AddState(def.name);
            state.motion = clip;
            createdStates[def.name] = state;

            Debug.Log($"[AnimatorSetup] Estado '{def.name}' creado con {matchingSprites.Count} frames");
        }

        // 4. Estado inicial = Idle
        if (createdStates.ContainsKey("Idle"))
        {
            controller.layers[0].stateMachine.defaultState = createdStates["Idle"];
        }

        // 5. Crear transiciones automáticas
        CreateTransitions(controller, createdStates);

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("¡Listo!", 
            "Animator configurado.\n\nRevisa las transiciones en el Animator y ajusta los umbrales si es necesario.", 
            "OK");
    }

    void AddParameters(AnimatorController controller)
    {
        AddFloatParam(controller, "Speed");
        AddBoolParam(controller,  "IsGrounded");
        AddBoolParam(controller,  "IsJumping");
        AddBoolParam(controller,  "IsFalling");
        AddTriggerParam(controller, "ThrowCoin");
        AddTriggerParam(controller, "Push");
        AddTriggerParam(controller, "Hurt");
        Debug.Log("[AnimatorSetup] Parámetros agregados.");
    }

    void CreateTransitions(AnimatorController ctrl, Dictionary<string, AnimatorState> states)
    {
        var sm = ctrl.layers[0].stateMachine;

        // Idle ↔ Walk
        if (states.ContainsKey("Idle") && states.ContainsKey("Walk"))
        {
            // Idle → Walk
            var t = states["Idle"].AddTransition(states["Walk"]);
            t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            t.hasExitTime = false;

            // Walk → Idle
            var t2 = states["Walk"].AddTransition(states["Idle"]);
            t2.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            t2.hasExitTime = false;
        }

        // Cualquier estado → Jump
        if (states.ContainsKey("Jump"))
        {
            var t = sm.AddAnyStateTransition(states["Jump"]);
            t.AddCondition(AnimatorConditionMode.If, 0, "IsJumping");
            t.hasExitTime = false;
            t.canTransitionToSelf = false;
        }

        // Jump → Fall
        if (states.ContainsKey("Jump") && states.ContainsKey("Fall"))
        {
            var t = states["Jump"].AddTransition(states["Fall"]);
            t.AddCondition(AnimatorConditionMode.If, 0, "IsFalling");
            t.hasExitTime = false;
        }

        // Fall → Idle (al aterrizar)
        if (states.ContainsKey("Fall") && states.ContainsKey("Idle"))
        {
            var t = states["Fall"].AddTransition(states["Idle"]);
            t.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
            t.hasExitTime = false;
        }

        // AnyState → ThrowCoin, Push, Hurt (triggers)
        foreach (var triggerAnim in new[] { "ThrowCoin", "Push", "Hurt" })
        {
            if (states.ContainsKey(triggerAnim))
            {
                var t = sm.AddAnyStateTransition(states[triggerAnim]);
                t.AddCondition(AnimatorConditionMode.If, 0, triggerAnim);
                t.hasExitTime = false;
                t.canTransitionToSelf = false;

                // Volver a Idle al terminar
                if (states.ContainsKey("Idle"))
                {
                    var back = states[triggerAnim].AddTransition(states["Idle"]);
                    back.hasExitTime = true;
                    back.exitTime = 1f;
                    back.hasFixedDuration = false;
                }
            }
        }

        Debug.Log("[AnimatorSetup] Transiciones creadas.");
    }

    // ─── Helpers ────────────────────────────────────────────────────

    List<Sprite> LoadAllSprites(string folder)
    {
        var sprites = new List<Sprite>();
        var guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            // Cargar todos los sub-sprites de un spritesheet
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is Sprite s) sprites.Add(s);
            }
        }
        return sprites;
    }

    List<Sprite> FindSprites(List<Sprite> all, string[] keywords)
    {
        var result = new List<Sprite>();
        foreach (var s in all)
        {
            string nameLower = s.name.ToLower();
            foreach (var kw in keywords)
            {
                if (nameLower.Contains(kw.ToLower()))
                {
                    result.Add(s);
                    break;
                }
            }
        }
        // Ordenar por nombre para que los frames queden en orden
        result.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        return result;
    }

    AnimationClip CreateClip(string clipName, List<Sprite> sprites, int fps, bool loop)
    {
        var clip = new AnimationClip { name = clipName, frameRate = fps };

        var binding = new UnityEditor.EditorCurveBinding
        {
            type         = typeof(SpriteRenderer),
            path         = "",
            propertyName = "m_Sprite"
        };

        var keyframes = new ObjectReferenceKeyframe[sprites.Count];
        float frameDuration = 1f / fps;

        for (int i = 0; i < sprites.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time      = i * frameDuration,
                value     = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        // Configurar loop
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        return clip;
    }

    void AddFloatParam(AnimatorController c, string name)
    {
        foreach (var p in c.parameters) if (p.name == name) return;
        c.AddParameter(name, AnimatorControllerParameterType.Float);
    }

    void AddBoolParam(AnimatorController c, string name)
    {
        foreach (var p in c.parameters) if (p.name == name) return;
        c.AddParameter(name, AnimatorControllerParameterType.Bool);
    }

    void AddTriggerParam(AnimatorController c, string name)
    {
        foreach (var p in c.parameters) if (p.name == name) return;
        c.AddParameter(name, AnimatorControllerParameterType.Trigger);
    }

    // ─── Data class ─────────────────────────────────────────────────
    class AnimationDef
    {
        public string   name;
        public string[] keywords;
        public bool     loop;
        public int      fps;

        public AnimationDef(string name, string[] keywords, bool loop, int fps)
        {
            this.name     = name;
            this.keywords = keywords;
            this.loop     = loop;
            this.fps      = fps;
        }
    }
}