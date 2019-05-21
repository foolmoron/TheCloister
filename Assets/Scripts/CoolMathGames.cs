using System.Runtime.InteropServices;
using UnityEngine;

public static class CoolMathGames {
    // Import the javascript function that redirects to another URL
    [DllImport("__Internal")]
    public static extern void RedirectTo(string url);
    // Import the javascript function that redirects to another URL
    [DllImport("__Internal")]
    public static extern void StartGameEvent();
    // Import the javascript function that redirects to another URL
    [DllImport("__Internal")]
    public static extern void StartLevelEvent(int level);
    // Import the javascript function that redirects to another URL
    [DllImport("__Internal")]
    public static extern void ReplayEvent();

#if UNITY_WEBGL && !UNITY_EDITOR
    // Check right away if the domain is valid
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void BeforeSceneLoad() {
        CheckDomains();
    }

    public static readonly string[] DOMAINS = {
        "https://www.coolmath-games.com",
        "www.coolmath-games.com",
        "edit.coolmath-games.com",
        "www.stage.coolmath-games.com",
        "edit-stage.coolmath-games.com",
        "dev.coolmath-games.com",
        "m.coolmath-games.com",
        "https://www.coolmathgames.com",
        "www.coolmathgames.com",
        "edit.coolmathgames.com",
        "www.stage.coolmathgames.com",
        "edit-stage.coolmathgames.com",
        "dev.coolmathgames.com",
        "m.coolmathgames.com",
        "http://localhost",
    };

    static void CheckDomains() {
        if (!IsValidHost(DOMAINS)) {
            RedirectTo("www.coolmathgames.com");
        }
    }
    static bool IsValidHost(string[] hosts) {
        foreach (string host in hosts)
            if (Application.absoluteURL.IndexOf(host) == 0)
                return true;
        return false;
    }
#endif
}