using UnityEngine;
using System.Collections;

public class Torch : MonoBehaviour {

    public Collider2D Collider;
    public bool Active;
    bool prevActive;

    public Sprite InactiveSprite;
    public Sprite ActiveSprite;

    public AudioClip FireOnSound;
    public AudioClip FireOffSound;

    ParticleSystem particles;
    SpriteRenderer sprite;
    Shaker shaker;

    void Start() {
        Collider = GetComponent<Collider2D>();
        particles = GetComponentInChildren<ParticleSystem>();
        sprite = transform.FindChild("Square").GetComponent<SpriteRenderer>();
        shaker = GetComponentInChildren<Shaker>();
	}
	
	void Update() {
        particles.enableEmission = Active;
        shaker.gameObject.SetActive(Active);
        sprite.sprite = Active ? ActiveSprite : InactiveSprite;
        if (prevActive != Active) {
            AudioSource.PlayClipAtPoint(Active ? FireOnSound : FireOffSound, transform.position);
        }
        prevActive = Active;
    }

    void OnDisable() {
    }

#if UNITY_EDITOR
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
#endif
}
