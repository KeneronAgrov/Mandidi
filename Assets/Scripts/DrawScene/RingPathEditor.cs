using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RingPathEditor : MonoBehaviour {
    [Header("Config")]
    public MandalaData mandalaData;
    public RectTransform canvasRect;

    [Header("Paths por Ring")]
    public RingWaypointSet[] ringPaths;

#if UNITY_EDITOR
    [ContextMenu("Bake All Rings to MandalaData")]
    public void BakeAll() {
        if (mandalaData == null) { Debug.LogError("Asigná MandalaData."); return; }
        if (canvasRect == null) { Debug.LogError("Asigná CanvasRect."); return; }

        mandalaData.ringPaths = new DrawRingPath[ringPaths.Length];
        Vector2 canvasSize = canvasRect.rect.size;

        for (int r = 0; r < ringPaths.Length; r++) {
            var set = ringPaths[r];
            if (set.waypoints == null || set.waypoints.Length == 0) continue;

            Vector2[] uvPoints = new Vector2[set.waypoints.Length];
            for (int i = 0; i < set.waypoints.Length; i++) {
                if (set.waypoints[i] == null) continue;
                Vector2 localPos = set.waypoints[i].anchoredPosition;
                uvPoints[i] = new Vector2(
                    (localPos.x + canvasSize.x * 0.5f) / canvasSize.x,
                    (localPos.y + canvasSize.y * 0.5f) / canvasSize.y
                );
            }

            mandalaData.ringPaths[r] = new DrawRingPath { waypoints = uvPoints };
        }

        EditorUtility.SetDirty(mandalaData);
        AssetDatabase.SaveAssets();
        Debug.Log($"Baked {ringPaths.Length} rings en {mandalaData.name}");
    }

    void OnDrawGizmos() {
        if (ringPaths == null) return;
        Color[] colors = { Color.cyan, Color.yellow, Color.green, Color.magenta, Color.red };
        for (int r = 0; r < ringPaths.Length; r++) {
            Gizmos.color = colors[r % colors.Length];
            var wps = ringPaths[r].waypoints;
            if (wps == null) continue;
            for (int i = 0; i < wps.Length; i++) {
                if (wps[i] == null) continue;
                Gizmos.DrawSphere(wps[i].position, 10f);
                if (i < wps.Length - 1 && wps[i + 1] != null)
                    Gizmos.DrawLine(wps[i].position, wps[i + 1].position);
            }
        }
    }
#endif
}

[System.Serializable]
public class RingWaypointSet {
    public string label; // solo para identificar en el Inspector, ej "Ring 0"
    public RectTransform[] waypoints;
}