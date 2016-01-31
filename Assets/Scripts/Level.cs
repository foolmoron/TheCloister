using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Level : MonoBehaviour {

    public float[] SolutionLineAngles;
    [Range(0, 5)]
    public int SolutionTriangles;
    [Range(0, 5)]
    public int SolutionSquares;

    public List<Vector2> Vertexes = new List<Vector2>();

    public const int MAX_LINES = 100;
    public GameObject LinePrefab;
    GameObject[] lines = new GameObject[MAX_LINES];
    GameObject[] lineSprites = new GameObject[MAX_LINES];
    public Collider2D[] LineColliders = new Collider2D[MAX_LINES];

    bool isDrawing;
    Torch[] torches;

    Loader loader;
    PolygonSolver polygonSolver;

    void Awake() {
        var solutionCount = SolutionLineAngles.Length + SolutionTriangles + SolutionSquares;

        loader = FindObjectOfType<Loader>();
        polygonSolver = FindObjectOfType<PolygonSolver>();
        torches = GetComponentsInChildren<Torch>();
        
        for (int i = 0; i < MAX_LINES; i++) {
            lines[i] = Instantiate(LinePrefab);
            lines[i].transform.parent = transform;
            lines[i].transform.localPosition = Vector3.zero;
            lineSprites[i] = lines[i].GetComponentInChildren<SpriteRenderer>().gameObject;
            LineColliders[i] = lines[i].GetComponentInChildren<Collider2D>();
        }
    }

    public void CheckVictory() {
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
                if (!alreadyMatchedLines.Contains(j) && Mathf.Abs(angle - polygonSolver.StrayLines[j].Degrees % 180) < 20f) {
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
        for (int i = 0; i < polygonSolver.Polygons.Count; i++) {
            if (polygonSolver.Polygons[i].indexes.Count == 4) {
                matchingTriangles++;
            } else if (polygonSolver.Polygons[i].indexes.Count == 5) {
                matchingSquares++;
            }
        }

        var solved = allTorches && (matchingStrayLines == SolutionLineAngles.Length) && (matchingTriangles == SolutionTriangles) && (matchingSquares == SolutionSquares);
        if (solved) {
            loader.Win();
        }
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
                    if (Vertexes.Count != 0) {
                        while (Vertexes.Count > 0) {
                            if (Vertexes[Vertexes.Count - 1] == torchPos) {
                                break;
                            }
                            Vertexes.RemoveAt(Vertexes.Count - 1);
                        }
                    }
                    if (Vertexes.Count == 0) {
                        Vertexes.Add(torchPos);
                    }
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
                    CheckVictory();
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
                // set the mouse vertex
                Vertexes[Vertexes.Count - 1] = mouseWorldPos;
            }
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
                var lineSprite = lineSprites[i - 1];
                line.SetActive(true);
                line.transform.position = prevVert;
                line.transform.rotation = Quaternion.Euler(0, 0, angleToLine * Mathf.Rad2Deg);
                lineSprite.transform.localScale = new Vector3(distToLine, 0.4f, 0.4f);
                lineSprite.transform.localPosition = new Vector3(distToLine / 2, 0, 0);
            }
        }
    }
}
