using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceDot : MonoBehaviour
{
    public float Speed = 100f;
    public AnimationCurve SpeedCurve;

    private GameObject _player = null;
    private GameObject player => _player == null ? (_player = GameObject.Find("Player")) : _player;

    private float timeToTarget;
    private float timeSpawned;
    private Vector3 originalPosition;
    private void Start()
    {
        originalPosition = transform.position;
        timeSpawned = Time.time;
        timeToTarget = 100f / Speed;
    }

    public Vector3 TargetPosition = new Vector3(17, 12);
    void Update()
    {
        transform.position = Vector3.Lerp(originalPosition, TargetPosition, SpeedCurve.Evaluate((Time.time - timeSpawned) / timeToTarget));
        if ((transform.position - TargetPosition).magnitude < 1)
        {
            var playerController = player.GetComponent<PlayerController>();
            playerController?.IncreaseExperience();
            Destroy(gameObject);
        }
    }
}
