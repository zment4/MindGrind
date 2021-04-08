using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Startup : MonoBehaviour
{
    void Start()
    {
        // fix for weird "native resolution"
        if (Screen.currentResolution.width == 1920 &&
            Screen.currentResolution.height == 1008)
            Screen.SetResolution(1920, 1080, true);

        Debug.Log(Screen.currentResolution);
        SceneManager.LoadScene("DungeonStart");
    }
}
