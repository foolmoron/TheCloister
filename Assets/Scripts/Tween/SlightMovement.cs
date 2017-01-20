using UnityEngine;
using System.Collections;

public class SlightMovement : MonoBehaviour {

    public Vector2 Offsets;
    [Range(0, 5)]
    public float Duration = 2f;
    Vector3 originalPos;

    void Start() {
        originalPos = transform.localPosition;
        MoveToNewSpot(gameObject);
    }

    void MoveToNewSpot(GameObject obj) {
        Tween.MoveTo(obj, originalPos + Vector2.Scale(Offsets, Random.insideUnitCircle).to3(), Duration, Interpolate.EaseType.Linear, MoveToNewSpot);
    }
}
