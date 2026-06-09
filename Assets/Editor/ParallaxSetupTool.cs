using UnityEditor;
using UnityEngine;

/// <summary>
/// Construye un fondo con parallax usando los 4 PNG de la carpeta
/// "Assets/Sprites/BackGround/Clouds 1/".
///
/// IMPORTANTE — este tool NO modifica los assets:
///   • No cambia PPU
///   • No cambia Filter Mode
///   • No cambia Wrap Mode
///   • No cambia Sprite Mode
/// Solo crea GameObjects en la escena y los escala para cubrir la cámara.
///
/// Si alguno de esos PNG está siendo usado en la UI, queda intacto.
/// </summary>
public static class ParallaxSetupTool
{
    private const string BackgroundFolder = "Assets/Sprites/BackGround";
    private const string CloudFolderName  = "Clouds 1"; // ← cambia a "Clouds 2".."Clouds 8" para otro set

    // Nombre del GameObject raíz en la escena. Importante: no usar "Background"
    // a secas porque es un nombre típico de paneles de UI — GameObject.Find lo
    // confundiría con un Image del Canvas y rompería la UI.
    private const string RootName = "ParallaxBackground";

    // Tamaño objetivo de la cámara en world units (Orthographic Size 5 a 16:9
    // da ~17.8 × 10). Usamos un poco más para cubrir con margen.
    private const float TargetCameraWidth  = 20f;
    private const float TargetCameraHeight = 12f;

    private static string SourceFolder => $"{BackgroundFolder}/{CloudFolderName}";

    private struct LayerConfig
    {
        public string fileName;
        public string layerName;
        public float  parallaxFactor;
        public bool   infiniteHorizontal;
        public int    tileCount;          // cuántas copias lado a lado
        public int    orderInLayer;

        public LayerConfig(string file, string name, float factor,
                           bool inf, int tiles, int order)
        {
            fileName = file; layerName = name; parallaxFactor = factor;
            infiniteHorizontal = inf; tileCount = tiles; orderInLayer = order;
        }
    }

    // Mapeo: qué PNG va en qué capa, con qué factor, cuántas copias y orden.
    // tileCount=1 → un solo sprite (suficiente si factor=1, no hay drift).
    // tileCount=3 → tres copias lado a lado (-1, 0, +1) para cubrir drift parallax.
    private static readonly LayerConfig[] Layers =
    {
        new LayerConfig("1.png", "Sky",           1.00f, false, 1, -40),
        new LayerConfig("3.png", "Stars",         0.95f, true,  3, -30),
        new LayerConfig("2.png", "MountainsFar",  0.70f, true,  3, -20),
        new LayerConfig("4.png", "MountainsNear", 0.45f, true,  3, -10),
    };

    // ── Menús ─────────────────────────────────────────────────────────────────

    [MenuItem("Mistborn/Parallax/Setup Sky Background (uses Clouds 1)")]
    public static void SetupSkyBackground()
    {
        if (!AssetDatabase.IsValidFolder(SourceFolder))
        {
            EditorUtility.DisplayDialog("Parallax Setup",
                $"No se encontró la carpeta '{SourceFolder}'.\n\n" +
                "Si quieres usar otro set, edita la constante CloudFolderName " +
                "en ParallaxSetupTool.cs.",
                "OK");
            return;
        }

        int built = BuildHierarchy();

        if (built == 0)
        {
            EditorUtility.DisplayDialog("Parallax Setup",
                $"No se pudo cargar ningún PNG de '{SourceFolder}'.\n\n" +
                "Verifica que los archivos 1.png … 4.png existan y estén importados " +
                "como Sprite (Texture Type: Sprite 2D and UI).",
                "OK");
            return;
        }

        EditorUtility.DisplayDialog("Parallax Setup",
            $"Background creado con {built} capa(s) desde '{CloudFolderName}'.\n\n" +
            "No se modificó ningún import setting — los PNGs originales quedan intactos. " +
            "El tamaño se ajustó automáticamente escalando cada capa en la escena.\n\n" +
            "Dale Play y muévete con A/D para ver el parallax.",
            "OK");
    }

    [MenuItem("Mistborn/Parallax/Reset Background (Delete from Scene)")]
    public static void ResetBackground()
    {
        GameObject existing = FindParallaxRoot();
        if (existing == null)
        {
            EditorUtility.DisplayDialog("Parallax Setup",
                $"No hay un GameObject '{RootName}' en la escena (con SpriteRenderer; " +
                "se ignoran objetos de UI con el mismo nombre).", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Reset Background",
            $"Borrar el GameObject '{RootName}' de la escena. ¿Continuar?",
            "Sí, borrar", "Cancelar"))
            return;

        Undo.DestroyObjectImmediate(existing);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    /// <summary>
    /// Busca el root de parallax en la escena ignorando cualquier GameObject de
    /// UI (con RectTransform). Devuelve null si no existe.
    /// </summary>
    static GameObject FindParallaxRoot()
    {
        // FindObjectsByType porque GameObject.Find devuelve el primero por nombre
        // y podría caer en un UI Image llamado igual.
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go.name != RootName) continue;
            if (go.GetComponent<RectTransform>() != null) continue; // es UI
            return go;
        }
        return null;
    }

    // ── Construcción de la jerarquía ──────────────────────────────────────────

    static int BuildHierarchy()
    {
        GameObject root = FindParallaxRoot();
        if (root == null)
        {
            root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create ParallaxBackground");
        }
        Undo.RecordObject(root.transform, "Reset ParallaxBackground Transform");
        root.transform.position   = Vector3.zero;
        root.transform.localScale = Vector3.one;

        int built = 0;

        foreach (LayerConfig cfg in Layers)
        {
            string path   = $"{SourceFolder}/{cfg.fileName}";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"[ParallaxSetup] Capa '{cfg.layerName}' omitida — " +
                                 $"no se pudo cargar '{path}'.");
                continue;
            }

            // Contenedor para esta capa (agrupa todas las copias lado a lado)
            Transform existing = root.transform.Find(cfg.layerName);
            GameObject layerGroup;
            if (existing == null)
            {
                layerGroup = new GameObject(cfg.layerName);
                Undo.RegisterCreatedObjectUndo(layerGroup, "Create Layer Group");
                layerGroup.transform.SetParent(root.transform);
            }
            else
            {
                layerGroup = existing.gameObject;
                // Limpiar copias previas para reconstruir limpio
                for (int i = layerGroup.transform.childCount - 1; i >= 0; i--)
                    Undo.DestroyObjectImmediate(layerGroup.transform.GetChild(i).gameObject);
            }

            Undo.RecordObject(layerGroup.transform, "Layer Group Transform");
            layerGroup.transform.localPosition = Vector3.zero;
            layerGroup.transform.localScale    = Vector3.one;

            // Calcular escala (igual para todas las copias de esta capa)
            float spriteW = sprite.bounds.size.x;
            float spriteH = sprite.bounds.size.y;
            float scale   = 1f;
            if (spriteW > 0f && spriteH > 0f)
            {
                float scaleX = TargetCameraWidth  / spriteW;
                float scaleY = TargetCameraHeight / spriteH;
                scale = Mathf.Max(scaleX, scaleY);
            }

            float scaledWidth = spriteW * scale;
            int   tiles       = Mathf.Max(1, cfg.tileCount);
            int   halfTiles   = (tiles - 1) / 2;

            // Crear N copias lado a lado centradas en 0:
            //  tiles=1 → solo offset 0
            //  tiles=3 → offsets -1, 0, +1 (× scaledWidth)
            //  tiles=5 → offsets -2, -1, 0, +1, +2
            for (int i = 0; i < tiles; i++)
            {
                int offset = i - halfTiles;
                string copyName = tiles == 1 ? "Sprite" : $"Sprite_{offset:+0;-0;0}";

                GameObject copy = new GameObject(copyName);
                Undo.RegisterCreatedObjectUndo(copy, "Create Layer Copy");
                copy.transform.SetParent(layerGroup.transform);
                copy.transform.localPosition = new Vector3(offset * scaledWidth, 0f, 0f);
                copy.transform.localScale    = new Vector3(scale, scale, 1f);

                SpriteRenderer sr = Undo.AddComponent<SpriteRenderer>(copy);
                sr.sprite       = sprite;
                sr.color        = Color.white;
                sr.sortingOrder = cfg.orderInLayer;

                ParallaxBackground pb = Undo.AddComponent<ParallaxBackground>(copy);
                pb.parallaxFactor     = cfg.parallaxFactor;
                pb.infiniteHorizontal = cfg.infiniteHorizontal;
                pb.tileCount          = tiles;
                pb.verticalParallax   = false;
                pb.targetCamera       = null;

                EditorUtility.SetDirty(copy);
            }

            EditorUtility.SetDirty(layerGroup);
            built++;

            Debug.Log($"[ParallaxSetup] '{cfg.layerName}' ← {cfg.fileName} " +
                      $"(factor={cfg.parallaxFactor}, scale={scale:F2}, " +
                      $"tiles={tiles}, order={cfg.orderInLayer})");
        }

        EditorUtility.SetDirty(root);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        return built;
    }
}
