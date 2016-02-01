using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    IEnumerator Start() {
        var audio = GetComponent<AudioSource>();
        audio.PlayOneShot(audio.clip);
        while (true) {
            yield return new WaitForSeconds(audio.clip.length);
            audio.PlayOneShot(audio.clip);
        }
    }
}
