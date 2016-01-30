using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour {

    public float[] SolutionLineAngles;
    [Range(0, 5)]
    public int SolutionTriangles;
    [Range(0, 5)]
    public int SolutionSquares;
    
    void Awake() {
        var solutionCount = SolutionLineAngles.Length + SolutionTriangles + SolutionSquares;
    }
}
