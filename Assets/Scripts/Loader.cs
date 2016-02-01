using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Loader : MonoBehaviour {

    public static readonly string[] ROMAN_NUMERALS = {
        "I", "II", "III", "IV", "V",
        "VI", "VII", "VIII", "IX", "X",
        "XI", "XII", "XIII", "XIV", "XV",
        "XVI", "XVII", "XVIII", "XIX", "XX",
        "", "XXII", "XXIII", "XXIV", "XXV",
        "XXVI", "XXVII", "XXVIII", "XXIX", "XXX",
    };

    public string LevelPrefix = "Level";
    [Range(0, 50)]
    public int LevelCount = 5;
    [Range(0, 20)]
    public float LevelGap = 9.6f;

    public Level[] Levels;
    public int currentLevel { get; set; }

    public TextMesh LoadingText;
    public TextMesh LoadingText2;

    public AudioClip WinSound;

    public GameObject FlashPrefab;

    PolygonSolver polygonSolver;

    IEnumerator Start() {
        polygonSolver = FindObjectOfType<PolygonSolver>();

        Levels = new Level[LevelCount];
        for (int i = 0; i < LevelCount; i++) {
            yield return SceneManager.LoadSceneAsync(LevelPrefix + i, LoadSceneMode.Additive);
            Levels[i] = FindObjectOfType<Level>();
            Levels[i].gameObject.SetActive(false);
            Levels[i].transform.position = transform.position.plusX(LevelGap * i);
            Levels[i].Solved = PlayerPrefs.GetInt("solved") > i;
        }
        ActivateLevel(0, true);
	}

    public void ActivateLevel(int level, bool isInitial = false) {
        if (level < 0 || level >= Levels.Length) {
            return;
        }
        for (int i = 0; i < Levels.Length; i++) {
            Levels[i].gameObject.SetActive(true);
        }
        currentLevel = level;
        System.Action<GameObject> finishLevelTransition = obj => {
            for (int i = 0; i < Levels.Length; i++) {
                if (i != currentLevel) {
                    Levels[i].gameObject.SetActive(false);
                }
            }
            LoadingText.gameObject.SetActive(false);
            polygonSolver.Vertexes = Levels[currentLevel].Vertexes;
            polygonSolver.LineColliders = Levels[currentLevel].LineColliders;
        };
        if (!isInitial) {
            LoadingText.gameObject.SetActive(true);
            LoadingText2.gameObject.SetActive(true);
            LoadingText.text = ROMAN_NUMERALS[currentLevel];
            LoadingText2.text = ROMAN_NUMERALS[currentLevel];
            Tween.MoveTo(Camera.main.gameObject, Camera.main.transform.position.withX(LevelGap * currentLevel), 1f, Interpolate.EaseType.EaseOutQuart, finishLevelTransition);
        } else {
            finishLevelTransition(null);
        }
    }

    public void Win(Torch[] torches) {
        PlayerPrefs.SetInt("solved", Mathf.Max(PlayerPrefs.GetInt("solved"), currentLevel + 1));
        AudioSource.PlayClipAtPoint(WinSound, Camera.main.transform.position.withZ(-7));
        for (int i = 0; i < torches.Length; i++) {
            var flash = (GameObject) Instantiate(FlashPrefab, torches[i].transform.position.withZ(-5), Quaternion.identity);
        }
    }

    public void NextLevel() {
        ActivateLevel(currentLevel + 1);
    }

    public void PreviousLevel() {
        ActivateLevel(currentLevel - 1);
    }

    void Update() {
#if DEBUG
        if (Input.GetKeyDown(KeyCode.P)) {
            PlayerPrefs.SetInt("solved", 0);
            for (int i = 0; i < Levels.Length; i++) {
                Levels[i].Solved = false;
                Levels[i].Vertexes.Clear();
            }
        }
        if (Input.GetKeyDown(KeyCode.U)) {
            PlayerPrefs.SetInt("solved", 999);
            for (int i = 0; i < Levels.Length; i++) {
                Levels[i].Solved = true;
                Levels[i].Vertexes.Clear();
            }
        }
#endif
        if (Input.GetKeyDown(KeyCode.M)) {
            AudioListener.volume = AudioListener.volume == 0 ? 1 : 0;
        }
    }
}
