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

    void Awake() {
        var solutionCount = SolutionLineAngles.Length + SolutionTriangles + SolutionSquares;

        torches = GetComponentsInChildren<Torch>();
        
        for (int i = 0; i < MAX_LINES; i++) {
            lines[i] = Instantiate(LinePrefab);
            lines[i].transform.parent = transform;
            lines[i].transform.localPosition = Vector3.zero;
            lineSprites[i] = lines[i].GetComponentInChildren<SpriteRenderer>().gameObject;
            LineColliders[i] = lines[i].GetComponentInChildren<Collider2D>();
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
                    if (Vertexes.Count == 0 || Vertexes[Vertexes.Count - 1] == torchPos) {
                        Vertexes.Add(torchPos);
                        Vertexes.Add(mouseWorldPos); // will be updated on drag
                        isDrawing = true;
                    }
                }
            } else if (Input.GetMouseButtonUp(0)) {
                // stop drawing and remove the mouse vertex
                isDrawing = false;
                Vertexes.RemoveAt(Vertexes.Count - 1);
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
