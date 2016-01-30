using UnityEngine;
using System.Collections;

public class Torch : MonoBehaviour {

    public bool IsDrawing;
    public Vector2 LineEnd;

    new Collider2D collider;

    public GameObject LinePrefab;
    GameObject line;
    GameObject lineSprite;
    public Collider2D LineCollider { get; set; }

    void Start() {
        collider = GetComponent<Collider2D>();

        LineEnd = transform.position.to2();

        line = Instantiate(LinePrefab);
        line.transform.parent = transform;
        line.transform.localPosition = Vector3.zero;
        lineSprite = line.GetComponentInChildren<SpriteRenderer>().gameObject;
        LineCollider = line.GetComponentInChildren<Collider2D>();
	}
	
	void Update() {
	    if (Input.GetMouseButtonDown(0) && collider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition))) {
            IsDrawing = true;
        } else if (Input.GetMouseButtonUp(0)) {
            IsDrawing = false;
        }

        if (IsDrawing) {
            LineEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition).to2();
        }

        var vectorToLine = LineEnd - transform.position.to2();
        var distToLine = vectorToLine.magnitude;
        var angleToLine = Mathf.Atan2(vectorToLine.y, vectorToLine.x);
        line.transform.rotation = Quaternion.Euler(0, 0, angleToLine * Mathf.Rad2Deg);
        lineSprite.transform.localScale = new Vector3(distToLine, 0.4f, 0.4f);
        lineSprite.transform.localPosition = new Vector3(distToLine/2, 0, 0);
    }

    void OnDisable() {
        LineEnd = transform.position;
    }
}
