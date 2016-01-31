using UnityEngine;
using System.Collections;

public class Torch : MonoBehaviour {
    public Collider2D Collider;

    void Start() {
        Collider = GetComponent<Collider2D>();
	}
	
	void Update() {
    }

    void OnDisable() {
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.green.withAlpha(0.15f);
        var otherTorches = FindObjectsOfType<Torch>();
        for (int i = 0; i < otherTorches.Length; i++) {
            var torch = otherTorches[i];
            if (torch != this) {
                Gizmos.DrawLine(transform.position, torch.transform.position);
                var angle = Mathf.Atan2(torch.transform.position.y - transform.position.y, torch.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
                angle = (angle + 360) % 180;
                drawString(angle.ToString("0.0"), Vector3.Lerp(transform.position, torch.transform.position, 0.15f));
            }
        }
    }

    static void drawString(string text, Vector3 worldPos, Color? colour = null) {
        UnityEditor.Handles.BeginGUI();
        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height, size.x, size.y), text);
        UnityEditor.Handles.EndGUI();
    }
}
