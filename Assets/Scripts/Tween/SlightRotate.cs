using UnityEngine;
using System.Collections;

public class SlightRotate : MonoBehaviour {

    public float ZAmount;
    [Range(0, 5)]
    public float Duration = 1f;
    float originalZ;
    bool negativeToggle;

    void Start() {
        originalZ = transform.localRotation.eulerAngles.z;
        Duration = Duration * (0.8f + Random.value * 0.3f);
        negativeToggle = Random.value > 0.5f;
        RotateToNewSpot(gameObject);
    }

    void RotateToNewSpot(GameObject obj) {
        negativeToggle = !negativeToggle;
        Tween.RotateTo(obj, obj.transform.localRotation.eulerAngles.withZ(originalZ + (ZAmount * (0.2f + Random.value * 0.8f) * (negativeToggle ? 1 : -1))), Duration, Interpolate.EaseType.EaseInOutSine, RotateToNewSpot);
    }
}
