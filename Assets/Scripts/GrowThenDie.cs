using UnityEngine;
using System.Collections;

public class GrowThenDie : MonoBehaviour {

    [Range(0, 100)]
    public float FinalScale = 10f;
    [Range(0, 5)]
    public float Duration = 1f;

    void Start() {
        Tween.ScaleTo(gameObject, new Vector3(FinalScale, FinalScale, 1), Duration, Interpolate.EaseType.EaseOutQuad);
        Tween.ColorTo(gameObject, GetComponent<SpriteRenderer>().color.withAlpha(0), Duration, Interpolate.EaseType.EaseInQuad);
        Destroy(gameObject, Duration);
    }
}
