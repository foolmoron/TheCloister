﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

public class PolygonSolver : MonoBehaviour {

    public class CollisionPoint {
        public int index;
        public Vector2 point;
    }

    public class Path {
        public List<int> indexes;
        public List<Vector2> points;
        public int latestIndex;
        public float sqrLength;
    }

    public class StrayLine {
        public Vector2 Start;
        public Vector2 End;
        public float Degrees { get { return Mathf.Atan2(End.y - Start.y, End.x - Start.x) * Mathf.Rad2Deg; } }
    }

    public LayerMask LayerMask;

    public List<Vector2> Vertexes;
    public Collider2D[] LineColliders;

    public const int MAX_COLLISION_POINTS = 100;
    public List<CollisionPoint> collisionPoints = new List<CollisionPoint>(MAX_COLLISION_POINTS);
    bool[][] adjacencies;
    [Range(0, 1)]
    public float SqrMagnitudeThreshold = 0.1f;
    List<Path> paths = new List<Path>();
    public List<Path> Polygons = new List<Path>();
    public List<StrayLine> StrayLines = new List<StrayLine>();

    GameObject[] collisionDots = new GameObject[MAX_COLLISION_POINTS];
    public GameObject CollisionDotPrefab;

    AudioSource audio;

    PoolOfList<int> intsPool = new PoolOfList<int>(500);
    PoolOfList<Vector2> vec2sPool = new PoolOfList<Vector2>(500);
    PoolOfList<Path> pathsPool = new PoolOfList<Path>(100);
    PoolOfList<CollisionPoint> collisionsPool = new PoolOfList<CollisionPoint>(100);
    //NonGameObjectPool<HashSet<int>> intSetPool = new NonGameObjectPool<HashSet<int>>(100) { OnRelease = set => set.Clear() };

    void Start() {
        adjacencies = new bool[MAX_COLLISION_POINTS][];
        for (int i = 0; i < MAX_COLLISION_POINTS; i++) {
            adjacencies[i] = new bool[MAX_COLLISION_POINTS];
            collisionDots[i] = Instantiate(CollisionDotPrefab);
            collisionDots[i].transform.parent = transform;
        }
        audio = GetComponent<AudioSource>();
    }
    
    void Update() {
        CollisionPointStuff();
        if (Vertexes.Count > 0 && !audio.isPlaying) {
            audio.Play();
        } else if (Vertexes.Count == 0 && audio.isPlaying) {
            audio.Stop();
        }
    }

    public void CollisionPointStuff() {
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
            for (int v = 1; v < Vertexes.Count; v++) {
                var currentVertex = Vertexes[v];
                var prevVertex = Vertexes[v - 1];
                var currentLineCollider = LineColliders[v - 1];
                var vectorToLine = currentVertex - prevVertex;
                var distToLine = vectorToLine.magnitude;
                var angleToLine = Mathf.Atan2(vectorToLine.y, vectorToLine.x);
                var results = new RaycastHit2D[MAX_COLLISION_POINTS];
                var count = Physics2D.RaycastNonAlloc(prevVertex, vectorToLine, results, distToLine, LayerMask);
                CollisionPoint previousPoint = null;
                for (int k = 0; k < collisionPoints.Count; k++) {
                    if ((prevVertex - collisionPoints[k].point).sqrMagnitude < SqrMagnitudeThreshold) {
                        previousPoint = collisionPoints[k];
                    };
                }
                if (previousPoint == null) {
                    previousPoint = new CollisionPoint {
                        index = collisionPoints.Count,
                        point = prevVertex,
                    };
                    collisionPoints.Add(previousPoint);
                }
                for (int j = 0; j < count; j++) {
                    var hit = results[j];
                    if (hit.collider != currentLineCollider) {
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
                CollisionPoint lastPoint = null;
                for (int k = 0; k < collisionPoints.Count; k++) {
                    if ((currentVertex - collisionPoints[k].point).sqrMagnitude < SqrMagnitudeThreshold) {
                        lastPoint = collisionPoints[k];
                    };
                }
                if (lastPoint == null) {
                    lastPoint = new CollisionPoint {
                        index = collisionPoints.Count,
                        point = currentVertex,
                    };
                    collisionPoints.Add(lastPoint);
                }
                for (int c = 0; previousPoint != null && c < count; c++) {
                    adjacencies[lastPoint.index][previousPoint.index] = adjacencies[previousPoint.index][lastPoint.index] = true;
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
    }

    public void Solve() {
        CollisionPointStuff();
        // reset paths
        {
            paths.Clear();
        }
        // find shortest cycle for each point
        var finalCyclesWithDupes = pathsPool.Obtain();
        {
            for (int startingPoint = 0; startingPoint < collisionPoints.Count; startingPoint++) {
                var cycles = pathsPool.Obtain();
                var pathsToBuild = pathsPool.Obtain();
                pathsToBuild.Add(new Path {
                    indexes = intsPool.Obtain().WithAdd(startingPoint),
                    points = vec2sPool.Obtain().WithAdd(collisionPoints[startingPoint].point),
                    latestIndex = startingPoint,
                    sqrLength = 0,
                });
                while(pathsToBuild.Count > 0) {
                    var currentPath = pathsToBuild[pathsToBuild.Count - 1];
                    pathsToBuild.RemoveAt(pathsToBuild.Count - 1);
                    for (int i = 0; i < collisionPoints.Count; i++) {
                        if (adjacencies[currentPath.latestIndex][i] && i == startingPoint && currentPath.indexes.Count > 2) {
                            cycles.Add(new Path {
                                indexes = intsPool.Obtain().WithAddRange(currentPath.indexes).WithAdd(i),
                                points = vec2sPool.Obtain().WithAddRange(currentPath.points).WithAdd(collisionPoints[i].point),
                                latestIndex = i,
                                sqrLength = currentPath.sqrLength + (collisionPoints[i].point - collisionPoints[currentPath.latestIndex].point).sqrMagnitude,
                            });
                        } else if (adjacencies[currentPath.latestIndex][i] && !currentPath.indexes.Contains(i)) {
                            pathsToBuild.Add(new Path {
                                indexes = intsPool.Obtain().WithAddRange(currentPath.indexes).WithAdd(i),
                                points = vec2sPool.Obtain().WithAddRange(currentPath.points).WithAdd(collisionPoints[i].point),
                                latestIndex = i,
                                sqrLength = currentPath.sqrLength + (collisionPoints[i].point - collisionPoints[currentPath.latestIndex].point).sqrMagnitude,
                            });
                        }
                    }
                    intsPool.Release(currentPath.indexes);
                    vec2sPool.Release(currentPath.points);
                }
                if (cycles.Count > 0) {
                    cycles.Sort((c1, c2) => c1.sqrLength.CompareTo(c2.sqrLength));
                    finalCyclesWithDupes.Add(new Path {
                        indexes = intsPool.Obtain().WithAddRange(cycles[0].indexes),
                        points = vec2sPool.Obtain().WithAddRange(cycles[0].points),
                        latestIndex = cycles[0].latestIndex,
                        sqrLength = cycles[0].sqrLength,
                    });
                }
                foreach (var c in cycles) {
                    intsPool.Release(c.indexes);
                    vec2sPool.Release(c.points);
                }
                pathsPool.Release(cycles);
                foreach (var p in pathsToBuild) {
                    intsPool.Release(p.indexes);
                    vec2sPool.Release(p.points);
                }
                pathsPool.Release(pathsToBuild);
            }
        }
        // remove duplicates in final cycles
        foreach (var p in Polygons) {
            intsPool.Release(p.indexes);
            vec2sPool.Release(p.points);
        }
        Polygons.Clear();
        {
            var vertsToCycles = new Dictionary<HashSet<int>, List<Path>>(new SetComparer());
            for (int i = 0; i < finalCyclesWithDupes.Count; i++) {
                var verts = new HashSet<int>();
                foreach (var index in finalCyclesWithDupes[i].indexes) {
                    verts.Add(index);
                }
                if (!vertsToCycles.ContainsKey(verts)) {
                    vertsToCycles[verts] = new List<Path>();
                }
                vertsToCycles[verts].Add(finalCyclesWithDupes[i]);
                //intSetPool.Release(verts);
            }
            foreach (var pair in vertsToCycles) {
                pair.Value.Sort((c1, c2) => c1.sqrLength.CompareTo(c2.sqrLength));
                Polygons.Add(new Path {
                    indexes = intsPool.Obtain().WithAddRange(pair.Value[0].indexes),
                    points = vec2sPool.Obtain().WithAddRange(pair.Value[0].points),
                    latestIndex = pair.Value[0].latestIndex,
                    sqrLength = pair.Value[0].sqrLength,
                });
            }
        }
        // find non-polygon points
        var nonPolygonPoints = collisionsPool.Obtain();
        {
            var polygonPoints = new HashSet<int>();
            for (int i = 0; i < Polygons.Count; i++) {
                for (int j = 0; j < Polygons[i].indexes.Count; j++) {
                    polygonPoints.Add(Polygons[i].indexes[j]);
                }
            }
            for (int i = 0; i < collisionPoints.Count; i++) {
                if (!polygonPoints.Contains(i)) {
                    nonPolygonPoints.Add(collisionPoints[i]);
                }
            }
            //intSetPool.Release(polygonPoints);
        }
        // find stray lines from non-polygon points
        {
            StrayLines.Clear();
            for (int i = 0; i < nonPolygonPoints.Count; i++) {
                var startPoint = nonPolygonPoints[i];
                for (int j = 0; j < adjacencies[startPoint.index].Length; j++) {
                    if (adjacencies[startPoint.index][j]) {
                        if (startPoint.point != collisionPoints[j].point) {
                            StrayLines.Add(new StrayLine {
                                Start = startPoint.point,
                                End = collisionPoints[j].point,
                            });
                        }
                    }
                }
            }
        }
        // remove stray line duplicates
        {
            for (int i = 0; i < StrayLines.Count; i++) {
                var firstLine = StrayLines[i];
                for (int j = i + 1; j < StrayLines.Count; j++) {
                    var otherLine = StrayLines[j];
                    if ((firstLine.Start == otherLine.Start && firstLine.End == otherLine.End) ||
                        (firstLine.Start == otherLine.End && firstLine.End == otherLine.Start)) {
                        StrayLines.RemoveAt(j);
                        j--;
                    }
                }
            }
        }
        foreach (var c in finalCyclesWithDupes) {
            intsPool.Release(c.indexes);
            vec2sPool.Release(c.points);
        }
        pathsPool.Release(finalCyclesWithDupes);
        collisionsPool.Release(nonPolygonPoints);
    }

    class SetComparer : IEqualityComparer<HashSet<int>> {
        public bool Equals(HashSet<int> x, HashSet<int> y) {
            return x.SetEquals(y);
        }

        public int GetHashCode(HashSet<int> obj) {
            return obj.Count;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        if (Application.isPlaying) {
            for (int i = 0; i < MAX_COLLISION_POINTS; i++) {
                for (int j = 0; j < MAX_COLLISION_POINTS; j++) {
                    if (adjacencies[i][j]) {
                        Gizmos.DrawLine(collisionPoints[i].point, collisionPoints[j].point);
                    }
                }
            }
            for (int i = 0; i < collisionPoints.Count; i++) {
                drawString(i.ToString(), collisionPoints[i].point, Color.white);
            }
            try {
                for (int i = 0; i < Polygons.Count; i++) {
                    Gizmos.color = new HSBColor(Mathf.Lerp(0.25f, 0.8f, (float)i / (Polygons.Count - 1)), 1, 1).ToColor();
                    var polygon = Polygons[i];
                    for (int j = 1; j < polygon.indexes.Count; j++) {
                        Gizmos.DrawLine(collisionPoints[polygon.indexes[j]].point, collisionPoints[polygon.indexes[j - 1]].point);
                    }
                }
            } catch {

            }
        }
    }

    static void drawString(string text, Vector3 worldPos, Color? colour = null)
    {
        UnityEditor.Handles.BeginGUI();
        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height, size.x, size.y), text);
        UnityEditor.Handles.EndGUI();
    }
#endif
}
