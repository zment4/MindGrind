using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class StartScreen : MonoBehaviour
{

    private void Update()
    {
        if (Keyboard.current.anyKey.isPressed || Gamepad.all.SelectMany(x => x.allControls).Any(x => x is ButtonControl && x.IsPressed()))
        {
            GetComponent<AudioSource>().Play();

            StartCoroutine(LoadAfterSound());
        }
    }

    IEnumerator LoadAfterSound()
    {
        while (GetComponent<AudioSource>().isPlaying)
            yield return null;

        SceneManager.LoadScene("DungeonStart");
    }
}
