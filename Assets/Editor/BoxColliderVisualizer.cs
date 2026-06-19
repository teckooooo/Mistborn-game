using UnityEngine;
using UnityEditor;

/// <summary>
/// Dibuja TODOS los BoxCollider2D del nivel siempre visibles en la Scene view
/// (no solo el seleccionado). Útil para revisar de un vistazo qué tiene collider.
///
/// Activar/desactivar desde: Tools → Mistborn → Mostrar BoxColliders siempre.
/// Requiere que el botón 'Gizmos' del Scene view esté encendido.
/// </summary>
public static class BoxColliderVisualizer
{
    const string MenuPath = "Tools/Mistborn/Mostrar BoxColliders siempre";
    const string PrefKey  = "Mistborn_ShowBoxColliders";

    static bool Enabled => EditorPrefs.GetBool(PrefKey, true);

    [MenuItem(MenuPath)]
    static void Toggle()
    {
        EditorPrefs.SetBool(PrefKey, !Enabled);
        SceneView.RepaintAll();
    }

    [MenuItem(MenuPath, true)]
    static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Active)]
    static void DrawBox(BoxCollider2D box, GizmoType gizmoType)
    {
        if (!Enabled || box == null || !box.enabled) return;

        Transform t   = box.transform;
        Matrix4x4 old = Gizmos.matrix;

        // Verde = tiene tag "Ground" ; Rojo = NO lo tiene.
        bool  isGround = box.CompareTag("Ground");
        Color baseCol  = isGround ? new Color(0.1f, 1f, 0.3f) : new Color(1f, 0.2f, 0.2f);

        // Respeta posición, rotación, escala del objeto + offset/size del collider.
        Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);

        // Relleno tenue
        Gizmos.color = new Color(baseCol.r, baseCol.g, baseCol.b, 0.10f);
        Gizmos.DrawCube(box.offset, box.size);

        // Borde brillante
        Gizmos.color = new Color(baseCol.r, baseCol.g, baseCol.b, 0.9f);
        Gizmos.DrawWireCube(box.offset, box.size);

        Gizmos.matrix = old;
    }
}
