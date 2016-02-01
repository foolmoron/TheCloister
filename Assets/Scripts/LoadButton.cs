using UnityEngine;
using System.Collections;

public class LoadButton : MonoBehaviour {

    public bool Next;

    Collider2D collider;
    SpriteRenderer sprite;
    Loader loader;

    public AudioClip Sound;

    void Start() {
        loader = FindObjectOfType<Loader>();
        collider = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update() {
        var level = loader.Levels[loader.currentLevel];
        var canPress = level != null && (Next && level.Solved && loader.currentLevel < loader.LevelCount - 1) || (!Next && loader.currentLevel > 0);
        for (int i = 0; i < loader.Levels.Length; i++) {
            canPress &= loader.Levels[i] != null;
        }
        sprite.enabled = canPress;
        if (canPress) {
            if (Input.GetMouseButtonDown(0) && (collider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))) {
                if (Next) {
                    loader.NextLevel();
                    AudioSource.PlayClipAtPoint(Sound, Camera.main.transform.position);
                } else {
                    loader.PreviousLevel();
                    AudioSource.PlayClipAtPoint(Sound, Camera.main.transform.position);
                }
            }
        }
    }
}
