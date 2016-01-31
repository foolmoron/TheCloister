using UnityEngine;
using System.Collections;

public class Shaker : MonoBehaviour {

    public bool Shaking = false;
    [Range(0, 1)]
    public float Strength = 0.05f;
    [Range(0, 1)]
    public float ScaleStrength = 0.15f;
    public bool VerticalShakeOnly;
    [Range(1, 10)]
    public int FrameInterval = 1;
    int frameCount;
    Vector3 previousShake;
    Vector3 originalScale;

    void Start() {
        originalScale = transform.localScale;
    }

    void Update() {
        if (Shaking) {
            frameCount++;
            if (frameCount % FrameInterval == 0) {
                Shake();
            }
        } else {
			if (previousShake != Vector3.zero) {
	            transform.localPosition = transform.localPosition - previousShake;
	            previousShake = Vector3.zero;
			}
        }
    }

    void Shake() {
        transform.localPosition -= previousShake;
        Vector3 shake = Random.insideUnitCircle.normalized * Strength;
        transform.localPosition += shake;
        previousShake = shake;
        if (VerticalShakeOnly) {
            transform.localScale = transform.localScale.withY(originalScale.y * (1 + (Random.value - 0.5f) * ScaleStrength));
        } else {
            transform.localScale = originalScale * (1 + (Random.value - 0.5f) * ScaleStrength);
        }
    }
}
