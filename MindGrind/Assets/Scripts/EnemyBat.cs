using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBat : MonoBehaviour, IEnemy
{
    private GameObject _player = null;
    private GameObject player => _player == null ? (_player = GameObject.Find("Player")) : _player;

    public float Speed = 100f;
    public float DeccelerationSpeed = 50f;

    private Rigidbody2D _rb = null;
    private Rigidbody2D rb => _rb == null ? (_rb = GetComponent<Rigidbody2D>()) : _rb;

    public int MaxHitPoints => Mathf.RoundToInt(Level * Level * 0.75f);

    private int currentHitPoints = 0;
    public int CurrentHitPoints => currentHitPoints;

    public int Damage => Mathf.RoundToInt(Level * Level * 0.75f);

    private int level = 0;
    public int Level
    {
        get => level;
        set
        {
            level = value;
            currentHitPoints = MaxHitPoints;
        }
    }

    public GameObject _gameObject = null;
    public GameObject GameObject => _gameObject;
    public void Start()
    {
        _gameObject = gameObject;

        if (Level == 0) Level = 1;
    }

    private void Update()
    {
        var velocityAdd = (player.transform.position - transform.position).normalized * Time.deltaTime * Speed;
        velocityAdd = velocityAdd - new Vector3(rb.velocity.x, rb.velocity.y, 0).normalized * Time.deltaTime * DeccelerationSpeed;

        rb.velocity += new Vector2(velocityAdd.x, velocityAdd.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            collision.gameObject.GetComponent<PlayerController>().ProcessEnemyHit(this);
        }

        if (collision.name.Contains("Fireball"))
        {
            collision.gameObject.GetComponent<FireballController>().ProcessEnemyHit();
            ProcessDeath();
        }
    }

    private void ProcessDeath()
    {
        // Play animation and sound
        Destroy(gameObject);
        _gameObject = null;
    }
}
