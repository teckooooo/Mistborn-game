using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Herramienta para configurar audio en lote.
///
/// Menús (Mistborn → Audio):
///   • Add ButtonClickSound to All Buttons in Scene
///   • Add ButtonClickSound to All Buttons in ALL Scenes (Build Settings)
///   • Remove ButtonClickSound from All Buttons in Scene
/// </summary>
public static class AudioSetupTool
{
    // ── Menús ─────────────────────────────────────────────────────────────────

    [MenuItem("Mistborn/Audio/Add ButtonClickSound to All Buttons in Scene")]
    public static void AddToActiveScene()
    {
        int added = AddClickSoundToScene();
        EditorUtility.DisplayDialog("Audio Setup",
            $"{added} botón(es) en la escena actual ahora tienen ButtonClickSound.\n\n" +
            "Los botones que ya tenían el componente NO fueron modificados.",
            "OK");
    }

    [MenuItem("Mistborn/Audio/Add ButtonClickSound to All Buttons in ALL Scenes")]
    public static void AddToAllBuildScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        if (scenes == null || scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("Audio Setup",
                "No hay escenas en Build Settings. " +
                "Ve a File → Build Profiles y agrégalas primero.",
                "OK");
            return;
        }

        // Guardar la escena actual para volver al final
        var currentScene = UnityEditor.SceneManagement.EditorSceneManager
            .GetActiveScene();
        string currentPath = currentScene.path;

        // Confirmar guardar si hay cambios sin guardar
        if (currentScene.isDirty)
        {
            bool save = EditorUtility.DisplayDialog("Cambios sin guardar",
                "La escena actual tiene cambios sin guardar. ¿Guardar antes de continuar?",
                "Guardar y continuar", "Cancelar");
            if (!save) return;
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(currentScene);
        }

        int totalAdded   = 0;
        int totalScenes  = 0;

        foreach (var bs in scenes)
        {
            if (!bs.enabled) continue;

            var scene = UnityEditor.SceneManagement.EditorSceneManager
                .OpenScene(bs.path);

            int added = AddClickSoundToScene();
            if (added > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                totalAdded  += added;
                totalScenes++;
                Debug.Log($"[AudioSetup] '{scene.name}': {added} botón(es) configurados.");
            }
            else
            {
                Debug.Log($"[AudioSetup] '{scene.name}': sin cambios.");
            }
        }

        // Volver a la escena original
        if (!string.IsNullOrEmpty(currentPath))
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(currentPath);

        EditorUtility.DisplayDialog("Audio Setup",
            $"Listo. {totalAdded} botón(es) configurados en {totalScenes} escena(s).",
            "OK");
    }

    [MenuItem("Mistborn/Audio/Remove ButtonClickSound from All Buttons in Scene")]
    public static void RemoveFromActiveScene()
    {
        if (!EditorUtility.DisplayDialog("Quitar ButtonClickSound",
            "Esto eliminará el componente ButtonClickSound de todos los botones " +
            "en la escena actual. ¿Continuar?",
            "Sí, quitar", "Cancelar"))
            return;

        int removed = 0;
        var clickSounds = Object.FindObjectsByType<ButtonClickSound>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var cs in clickSounds)
        {
            Undo.DestroyObjectImmediate(cs);
            removed++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Audio Setup",
            $"{removed} componente(s) eliminado(s).", "OK");
    }

    // ── Lógica ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Recorre todos los Button de la escena activa (incluso inactivos) y les
    /// agrega ButtonClickSound si no lo tienen. Devuelve cuántos fueron añadidos.
    /// </summary>
    static int AddClickSoundToScene()
    {
        Button[] buttons = Object.FindObjectsByType<Button>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        int added = 0;
        foreach (Button btn in buttons)
        {
            if (btn.GetComponent<ButtonClickSound>() != null) continue;
            Undo.AddComponent<ButtonClickSound>(btn.gameObject);
            EditorUtility.SetDirty(btn.gameObject);
            added++;
            Debug.Log($"[AudioSetup] + ButtonClickSound en '{btn.name}'");
        }

        if (added > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        return added;
    }
}
