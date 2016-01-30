using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PolygonSolver : MonoBehaviour {

    class CollisionPoint {
        public int index;
        public Vector2 point;
    }

    public LayerMask LayerMask;
    public Torch[] Torches;

    public const int MAX_COLLISION_POINTS = 100;
    List<CollisionPoint> collisionPoints = new List<CollisionPoint>(MAX_COLLISION_POINTS);
    bool[][] adjacencies;
    [Range(0, 1)]
    public float SqrMagnitudeThreshold = 0.1f;

    GameObject[] collisionDots = new GameObject[MAX_COLLISION_POINTS];
    public GameObject CollisionDotPrefab;

	void Start() {
	    Torches = FindObjectsOfType<Torch>();
        
        adjacencies = new bool[MAX_COLLISION_POINTS][];
        for (int i = 0; i < MAX_COLLISION_POINTS; i++) {
            adjacencies[i] = new bool[MAX_COLLISION_POINTS];
            collisionDots[i] = Instantiate(CollisionDotPrefab);
            collisionDots[i].transform.parent = transform;
        }
	}

    void Update() {
        // reset collision points
        {
            collisionPoints.Clear();
            for (int i = 0; i < MAX_COLLISION_POINTS; i++) {
                collisionDots[i].SetActive(false);
                for (int j = 0; j < MAX_COLLISION_POINTS; j++) {
                    adjacencies[i][j] = false;
                }
            }
        }
        // get all collision points
        {
            for (int t = 0; t < Torches.Length; t++) {
                var torch = Torches[t];
                var vectorToLine = torch.LineEnd - torch.transform.position.to2();
                var distToLine = vectorToLine.magnitude;
                var angleToLine = Mathf.Atan2(vectorToLine.y, vectorToLine.x);
                var results = new RaycastHit2D[MAX_COLLISION_POINTS];
                for (int i = 0; i < Torches.Length; i++) {
                    var lineCollider = Torches[i].LineCollider;
                    var count = Physics2D.RaycastNonAlloc(torch.transform.position, vectorToLine, results, distToLine, LayerMask);
                    CollisionPoint previousPoint = null;
                    for (int j = 0; j < count; j++) {
                        var hit = results[j];
                        if (hit.collider != torch.LineCollider) {
                            CollisionPoint collisionPoint = null;
                            for (int k = 0; k < collisionPoints.Count; k++) {
                                if ((hit.point - collisionPoints[k].point).sqrMagnitude < SqrMagnitudeThreshold) {
                                    collisionPoint = collisionPoints[k];
                                };
                            }
                            if (collisionPoint == null) {
                                collisionPoint = new CollisionPoint {
                                    index = collisionPoints.Count,
                                    point = hit.point,
                                };
                                collisionPoints.Add(collisionPoint);
                            }
                            for (int c = 0; previousPoint != null && c < count; c++) {
                                adjacencies[collisionPoint.index][previousPoint.index] = adjacencies[previousPoint.index][collisionPoint.index] = true;
                            }
                            previousPoint = collisionPoint;
                        }
                    }
                }
            }
        }
        // setup dots based on final points
        {
            for (int i = 0; i < collisionPoints.Count; i++) {
                var dot = collisionDots[i];
                dot.transform.position = collisionPoints[i].point;
                dot.SetActive(true);
            }
        }
        // detect polygons! http://web.ist.utl.pt/alfredo.ferreira/publications/12EPCG-PolygonDetection.pdf
        {
            
        }
    }

    void OnDrawGizmos() {
        for (int i = 0; i < MAX_COLLISION_POINTS; i++) {
            for (int j = 0; j < MAX_COLLISION_POINTS; j++) {
                if (adjacencies[i][j]) {
                    Gizmos.DrawLine(collisionPoints[i].point, collisionPoints[j].point);
                }
            }
        }
    }
}
