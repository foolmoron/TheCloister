using UnityEngine;
using System.Collections;

public class Torch : MonoBehaviour {

    public bool IsDrawing;
    public Vector2 LineEnd;

    public Collider2D Collider;

    void Start() {
        Collider = GetComponent<Collider2D>();

        LineEnd = transform.position.to2();
	}
	
	void Update() {
	    if (Input.GetMouseButtonDown(0) && Collider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition))) {
            IsDrawing = true;
        } else if (Input.GetMouseButtonUp(0)) {
            IsDrawing = false;
        }

        if (IsDrawing) {
            LineEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition).to2();
        }
    }

    void OnDisable() {
        LineEnd = transform.position;
    }
}
