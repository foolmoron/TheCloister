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
}
