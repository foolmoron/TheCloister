using System;
using UnityEngine;
using System.Collections;

public class PolygonSolver : MonoBehaviour {

    public LayerMask LayerMask;
    public Torch[] Torches;

    Vector2[] collisionPoints = new Vector2[100];
    GameObject[] collisionDots = new GameObject[100];
    public GameObject CollisionDotPrefab;
    [Range(0, 1)]
    public float SqrMagnitudeThreshold = 0.1f;

	void Start() {
	    Torches = FindObjectsOfType<Torch>();

	    for (int i = 0; i < collisionDots.Length; i++) {
	        collisionDots[i] = Instantiate(CollisionDotPrefab);
	        collisionDots[i].transform.parent = transform;
	    }
	}

    void Update() {
        // reset collision points
        for (int i = 0; i < collisionPoints.Length; i++) {
            collisionPoints[i] = Vector2.zero;
            collisionDots[i].SetActive(false);
        }
        // get all collision points
        var collisions = 0;
        for (int t = 0; t < Torches.Length; t++) {
            var torch = Torches[t];
            var vectorToLine = torch.LineEnd - torch.transform.position.to2();
            var distToLine = vectorToLine.magnitude;
            var angleToLine = Mathf.Atan2(vectorToLine.y, vectorToLine.x);
            var results = new RaycastHit2D[100];
            for (int i = 0; i < Torches.Length; i++) {
                var lineCollider = Torches[i].LineCollider;
                var count = Physics2D.RaycastNonAlloc(torch.transform.position, vectorToLine, results, distToLine, LayerMask);
                for (int j = 0; j < count; j++) {
                    var hit = results[j];
                    if (hit.collider != torch.LineCollider) {
                        collisionPoints[collisions] = hit.point;
                        collisions++;
                    }
                }
            }
        }
        // sort points from highest to lowest
        Array.Sort(collisionPoints, (p1, p2) => -Math.Abs(p1.x * 100000 + p1.y).CompareTo(Math.Abs(p2.x * 100000 + p2.y)));
        // set dots based on collision points
        var previousPoint = Vector2.zero;
        var drawn = 0;
        for (int i = 0; i < collisionPoints.Length; i++) {
            var point = collisionPoints[i];
            if ((point - previousPoint).sqrMagnitude < SqrMagnitudeThreshold) {
                continue;
            }
            if (point == Vector2.zero) {
                break;
            }
            var dot = collisionDots[drawn];
            dot.transform.position = point;
            dot.SetActive(true);
            drawn++;
            previousPoint = point;
        }

        Debug.Log("points=" + collisions + " drawn=" + drawn);
    }
}
