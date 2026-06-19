using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Utilidades de Editor para revisar colliders en el nivel abierto.
/// Menú: Tools → Mistborn.
/// </summary>
public static class ColliderChecker
{
    [MenuItem("Tools/Mistborn/Seleccionar sprites SIN Collider2D")]
    static void SelectSpritesWithoutCollider()
    {
        Find(requireBox: false);
    }

    [MenuItem("Tools/Mistborn/Seleccionar sprites SIN BoxCollider2D")]
    static void SelectSpritesWithoutBoxCollider()
    {
        Find(requireBox: true);
    }

    static void Find(bool requireBox)
    {
        var result = new List<Object>();
        var sb     = new StringBuilder();

        // Todos los SpriteRenderer de la escena (incluye objetos inactivos).
        SpriteRenderer[] sprites = Object.FindObjectsByType<SpriteRenderer>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (SpriteRenderer sr in sprites)
        {
            bool tieneCollider = requireBox
                ? sr.GetComponent<BoxCollider2D>() != null
                : sr.GetComponent<Collider2D>()    != null;

            if (!tieneCollider)
            {
                result.Add(sr.gameObject);
                sb.AppendLine($"• {GetPath(sr.transform)}");
            }
        }

        // Selecciona todos en la jerarquía (quedan resaltados en la Scene view).
        Selection.objects = result.ToArray();

        string tipo = requireBox ? "BoxCollider2D" : "Collider2D";
        if (result.Count == 0)
            Debug.Log($"[ColliderChecker] Todos los sprites tienen {tipo}. ✓");
        else
            Debug.Log($"[ColliderChecker] {result.Count} sprite(s) SIN {tipo} " +
                      $"(seleccionados en la jerarquía):\n{sb}");
    }

    static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }
}
