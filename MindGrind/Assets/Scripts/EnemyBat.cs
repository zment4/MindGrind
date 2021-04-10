using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBat : MonoBehaviour, IEnemy
{
    public AudioClip BatGetsHitSound;
    public AudioClip BatDiesSound;

    private GameObject _player = null;
    private GameObject player => _player == null ? (_player = GameObject.Find("Player")) : _player;

    public float Speed = 100f;
    public float DeccelerationSpeed = 50f;

    private Rigidbody2D _rb = null;
    private Rigidbody2D rb => _rb == null ? (_rb = GetComponent<Rigidbody2D>()) : _rb;

    public int MaxHitPoints => Level;

    private int currentHitPoints = 0;
    public int CurrentHitPoints => currentHitPoints;

    public int Damage => Mathf.CeilToInt(Level / 4f);

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

        transform.localScale = new Vector3(-Mathf.Sign(velocityAdd.x), 1, 1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            var playerController = collision.gameObject.GetComponent<PlayerController>();
            playerController?.ProcessEnemyHit(this);
            var playerRigidBody = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRigidBody != null)
                playerRigidBody.velocity += rb.velocity * 0.2f;
            rb.velocity = -rb.velocity * 1.25f;
        }

        if (collision.name.Contains("Fireball"))
        {
            collision.gameObject.GetComponent<FireballController>().ProcessEnemyHit();
            ProcessHit();
        }
    }

    private void ProcessHit()
    {
        rb.velocity = -rb.velocity * 0.5f;
        currentHitPoints--;

        PlayerController.Play(gameObject, BatGetsHitSound);
        
        if (currentHitPoints == 0)
            ProcessDeath();
    }

    private void ProcessDeath()
    {
        var go = new GameObject();
        var audioSrc = PlayerController.Play(go, BatDiesSound);
        PlayerController.WaitForAudioAndDo(audioSrc, () => {
            Destroy(go);
        });

        Destroy(gameObject);
        _gameObject = null;
    }
}
