using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    public Vector3 GetSpawnPoint(int spawnNumber)
    {
        return transform.Find($"Spawn {spawnNumber}").position;
    }
}
