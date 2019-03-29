using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadSceneOnButton : MonoBehaviour {

    public KeyCode Key = KeyCode.F5;
    public string Scene = "main";

    void Update() {
        if (Input.GetKeyDown(Key)) {
            SceneManager.LoadScene(Scene);
        }
    }
}
