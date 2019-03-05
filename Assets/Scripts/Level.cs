using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Level : MonoBehaviour {

    [Range(0, 179)]
    public float[] SolutionLineAngles;
    [Range(0, 5)]
    public int SolutionTriangles;
    [Range(0, 5)]
    public int SolutionSquares;
    [Range(0, 5)]
    public int SolutionPentagons;

    public bool Solved;

    public GameObject IconLinePrefab;
    public GameObject IconTrianglePrefab;
    public GameObject IconSquarePrefab;
    public GameObject IconPentagonPrefab;
    [Range(0, 3)]
    public float IconGap = 0.6f;
    public float IconY = 2.5f;

    public List<Vector2> Vertexes = new List<Vector2>();

    public const int MAX_LINES = 20;
    public GameObject LinePrefab;
    GameObject[] lines = new GameObject[MAX_LINES];
    public Collider2D[] LineColliders = new Collider2D[MAX_LINES];

    public Config Config;
    GameObject[] indicators = new GameObject[MAX_LINES];
    SpriteRenderer[] indicatorSprites = new SpriteRenderer[MAX_LINES];

    bool isDrawing;
    Torch[] torches;

    Loader loader;
    PolygonSolver polygonSolver;

    static int AngleToStrayLineType(float angle) {
        angle += 22.5f;
        angle += 360f;
        var corner = (int)(angle / 45f);
        corner %= 4;
        return corner;
    }

    void Awake() {
        // setup icons
        {
            var solutionCount = SolutionLineAngles.Length + SolutionTriangles + SolutionSquares + SolutionPentagons;
            var x = (float)(solutionCount - 1) * -IconGap / 2;
            for (int i = 0; i < solutionCount; i++) {
                GameObject newIcon = null;
                if (i < SolutionLineAngles.Length) {
                    newIcon = Instantiate(IconLinePrefab);
                } else if (i < SolutionLineAngles.Length + SolutionTriangles) {
                    newIcon = Instantiate(IconTrianglePrefab);
                } else if (i < SolutionLineAngles.Length + SolutionTriangles + SolutionSquares) {
                    newIcon = Instantiate(IconSquarePrefab);
                } else if (i < SolutionLineAngles.Length + SolutionTriangles + SolutionSquares + SolutionPentagons) {
                    newIcon = Instantiate(IconPentagonPrefab);
                }
                newIcon.transform.parent = transform;
                newIcon.transform.localPosition = new Vector3(x + i * IconGap, IconY, 10);
                if (i < SolutionLineAngles.Length) {
                    newIcon.transform.rotation = Quaternion.Euler(0, 0, SolutionLineAngles[i]);
                }
            }
        }

        loader = FindObjectOfType<Loader>();
        polygonSolver = FindObjectOfType<PolygonSolver>();
        torches = GetComponentsInChildren<Torch>();
        
        for (int i = 0; i < MAX_LINES; i++) {
            lines[i] = Instantiate(LinePrefab);
            lines[i].transform.parent = transform;
            lines[i].transform.localPosition = Vector3.zero;
            LineColliders[i] = lines[i].GetComponentInChildren<Collider2D>();
            indicators[i] = Instantiate(Config.IndicatorPrefab);
            indicators[i].transform.parent = transform;
            indicators[i].transform.localPosition = Vector3.zero;
            indicatorSprites[i] = indicators[i].GetComponentInChildren<SpriteRenderer>();
        }
    }

    public bool CheckVictory() {
        var allTorches = true;
        for (int i = 0; i < torches.Length; i++) {
            var pos = torches[i].transform.position.to2();
            allTorches &= Vertexes.Contains(pos);
        }

        var matchingStrayLines = 0;
        var alreadyMatchedLines = new List<int>();
        for (int i = 0; i < SolutionLineAngles.Length; i++) {
            var angle = SolutionLineAngles[i];
            var matched = false;
            for (int j = 0; j < polygonSolver.StrayLines.Count; j++) {
                var tempAngle = polygonSolver.StrayLines[j].Degrees;
                if (tempAngle < 0f && tempAngle >= -5f) {
                    tempAngle = 0;
                }
                if (!alreadyMatchedLines.Contains(j) && AngleToStrayLineType(angle) == AngleToStrayLineType(polygonSolver.StrayLines[j].Degrees)) {
                    matched = true;
                    alreadyMatchedLines.Add(j);
                }
            }
            if (matched) {
                matchingStrayLines++;
            }
        }

        var matchingTriangles = 0;
        var matchingSquares = 0;
        var matchingPentagons = 0;
        for (int i = 0; i < polygonSolver.Polygons.Count; i++) {
            if (polygonSolver.Polygons[i].indexes.Count == 4) {
                matchingTriangles++;
            } else if (polygonSolver.Polygons[i].indexes.Count == 5) {
                matchingSquares++;
            } else if (polygonSolver.Polygons[i].indexes.Count == 6) {
                matchingPentagons++;
            }
        }

        var solved = 
            allTorches 
            && (matchingStrayLines == SolutionLineAngles.Length) 
            && (matchingTriangles == SolutionTriangles) 
            && (matchingSquares == SolutionSquares)
            && (matchingPentagons == SolutionPentagons)
            ;
        return solved;
    }

    void Update() {
        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // vertexes
        {
            if (Input.GetMouseButtonDown(0)) {
                Torch torch = null;
                for (int i = 0; i < torches.Length; i++) {
                    if (torches[i].Collider.OverlapPoint(mouseWorldPos)) {
                        torch = torches[i];
                        break;
                    }
                }
                if (torch != null) {
                    var torchPos = torch.transform.position.to2();
                    Vertexes.Clear();
                    Vertexes.Add(torchPos);
                    Vertexes.Add(mouseWorldPos); // will be updated on drag
                    isDrawing = true;
                }
            } else if (Input.GetMouseButtonUp(0)) {
                if (isDrawing) {
                    // stop drawing and remove the mouse vertex
                    isDrawing = false;
                    Vertexes.RemoveAt(Vertexes.Count - 1);
                    // then check for victory
                    polygonSolver.Solve();
                    if (CheckVictory()) {
                        // handle victory
                        Solved = true;
                        loader.Win(torches);
                    } else {
                        // handle failure
                        Vertexes.Clear();
                    }
                }
            } else if (Input.GetMouseButton(0) && isDrawing) {
                // check for collision with new torch
                Torch torch = null;
                for (int i = 0; i < torches.Length; i++) {
                    if (torches[i].Collider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition))) {
                        torch = torches[i];
                        break;
                    }
                }
                if (torch != null) {
                    var existingTorch = false;
                    var torchPos = torch.transform.position.to2();
                    for (int i = 0; i < Vertexes.Count && !existingTorch; i++) {
                        existingTorch |= torchPos == Vertexes[i];
                    }
                    if (!existingTorch) {
                        Vertexes[Vertexes.Count - 1] = torchPos;
                        Vertexes.Add(mouseWorldPos); // becomes the new mouse vertex
                    }
                }
                Vertexes[Vertexes.Count - 1] = (Vertexes.Count <= torches.Length) ? mouseWorldPos.to2() : Vertexes[Vertexes.Count - 2];
            }
            // always solve
            polygonSolver.Solve();
        }
        // reset lines
        {
            for (int i = 0; i < MAX_LINES; i++) {
                lines[i].gameObject.SetActive(false);
            }
        }
        // draw lines
        {
            for (int i = 1; i < Vertexes.Count; i++) {
                var thisVert = Vertexes[i];
                var prevVert = Vertexes[i - 1];
                var vectorToLine = thisVert - prevVert;
                var distToLine = vectorToLine.magnitude;
                var angleToLine = Mathf.Atan2(vectorToLine.y, vectorToLine.x);
                var line = lines[i - 1];
                var lineCollider = LineColliders[i - 1];
                line.SetActive(true);
                line.transform.position = prevVert;
                line.transform.rotation = Quaternion.Euler(0, 0, angleToLine * Mathf.Rad2Deg);
                lineCollider.transform.localScale = lineCollider.transform.localScale.withX(distToLine);
                lineCollider.transform.localPosition = new Vector3(distToLine / 2, 0, 5f);
            }
        }
        // draw shape indicators
        {
            foreach (var indicator in indicators) {
                indicator.SetActive(false);
            }
            var indicatorIndex = 0;
            foreach (var line in polygonSolver.StrayLines) {
                var thisVert = line.Start;
                var prevVert = line.End;
                var vectorToLine = thisVert - prevVert;
                var distToLine = vectorToLine.magnitude;
                var angleToLine = Mathf.Atan2(vectorToLine.y, vectorToLine.x);
                var indicator = indicators[indicatorIndex];
                switch (AngleToStrayLineType(line.Degrees)) {
                    case 0: indicatorSprites[indicatorIndex].color = Config.HorizLineColor; break;
                    case 1: indicatorSprites[indicatorIndex].color = Config.RightyLineColor; break;
                    case 2: indicatorSprites[indicatorIndex].color = Config.VertLineColor; break;
                    case 3: indicatorSprites[indicatorIndex].color = Config.LeftyLineColor; break;
                }
                indicatorIndex++;
                indicator.SetActive(true);
                indicator.transform.position = prevVert;
                indicator.transform.rotation = Quaternion.Euler(0, 0, angleToLine * Mathf.Rad2Deg);
                indicator.transform.localScale = new Vector3(distToLine, 1, 1);
            }
            foreach (var polygon in polygonSolver.Polygons) {
                for (int i = 1; i < polygon.points.Count; i++) {
                    var thisVert = polygon.points[i];
                    var prevVert = polygon.points[i - 1];
                    var vectorToLine = thisVert - prevVert;
                    var distToLine = vectorToLine.magnitude;
                    var angleToLine = Mathf.Atan2(vectorToLine.y, vectorToLine.x);
                    var indicator = indicators[indicatorIndex];
                    switch (polygon.indexes.Count) {
                        case 4: indicatorSprites[indicatorIndex].color = Config.TriangleColor; break;
                        case 5: indicatorSprites[indicatorIndex].color = Config.SquareColor; break;
                        case 6: indicatorSprites[indicatorIndex].color = Config.PentagonColor; break;
                        default: indicatorSprites[indicatorIndex].color = Config.HexOrMoreColor; break;
                    }
                    indicatorIndex++;
                    indicator.SetActive(true);
                    indicator.transform.position = prevVert;
                    indicator.transform.rotation = Quaternion.Euler(0, 0, angleToLine * Mathf.Rad2Deg);
                    indicator.transform.localScale = new Vector3(distToLine, 1, 1);
                }
            }
        }
        // set torch state based on current vertexes
        {
            for (int t = 0; t < torches.Length; t++) {
                torches[t].Active = false;
            }
            for (int i = 0; i < Vertexes.Count; i++) {
                for (int t = 0; t < torches.Length; t++) {
                    if (torches[t].transform.position.to2() == Vertexes[i]) {
                        torches[t].Active = true;
                    }
                }
            }
        }
    }
}
