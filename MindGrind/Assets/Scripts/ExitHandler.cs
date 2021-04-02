using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitHandler : MonoBehaviour
{
    public RoomGenerator RoomGenerator;

    public void TriggerExit(int exitNumber)
    {
        Debug.Log($"Exit {exitNumber} triggered.");
        RoomGenerator?.ActivateExit(exitNumber);
    }
}
