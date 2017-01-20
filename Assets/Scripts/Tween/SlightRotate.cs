using UnityEngine;
using System.Collections;

public class SlightRotate : MonoBehaviour {

    public float ZAmount;
    [Range(0, 5)]
    public float Duration = 1f;
    float originalZ;

    void Start() {
        originalZ = transform.localRotation.eulerAngles.z;
        RotateToNewSpot(gameObject);
    }

    void RotateToNewSpot(GameObject obj) {
        Tween.RotateTo(obj, obj.transform.localRotation.eulerAngles.withZ(originalZ + (ZAmount * (Random.value * 2 - 1))), Duration, Interpolate.EaseType.Linear, RotateToNewSpot);
    }
}
