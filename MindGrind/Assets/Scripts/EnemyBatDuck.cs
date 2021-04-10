using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBatDuck : MonoBehaviour, IEnemy
{
    public enum BatDuckStateEnum
    {
        Align,       // Vertical position not close enough to player's y
        Advance,     // Go forward 
        Attack,      // Going towards player
        Decelerate,   // Out of attack after going past the player's x
        RunAway,    // After being hit or hitting player, run away and line up another attack
    }

    public int RunAwayXThreshold = 32;
    public float AdvanceSinYSpeed = 10f;
    public float AdvanceSinTimeFactor = 0.2f;
    private BatDuckStateEnum BatDuckState = BatDuckStateEnum.Align;

    public AudioClip BatGetsHitSound;
    public AudioClip BatDiesSound;

    private GameObject _player = null;
    private GameObject player => _player == null ? (_player = GameObject.Find("Player")) : _player;

    public float MaxAdvanceSpeed = 10f;
    public float MaxAttackSpeed = 20f;
    public float Speed = 10f;
    public float AttackAccelerationSpeed = 1f;
    public float RunAwaySpeed = 5f;
    public float DecelerationSpeed = 1f;

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

    private GameObject _gameObject = null;
    public GameObject GameObject => _gameObject;

    public void Start()
    {
        _gameObject = gameObject;

        if (Level == 0) Level = 1;

        StateFunctions = new Action[]
        {
            () => Align(),
            () => Advance(),
            () => Attack(),
            () => Decelerate(),
            () => RunAway(),
        };

        rb.velocity = Vector3.zero;
    }

    private Vector3 attackPosition;
    private void Advance()
    {
        var delta = (player.transform.position - transform.position);
        if (Mathf.Abs(delta.x) < 64)
        {
            BatDuckState = BatDuckStateEnum.Attack;
            attackPosition = player.transform.position;
            return;
        }
        if (Mathf.Abs(delta.y) > 12)
        {
            BatDuckState = BatDuckStateEnum.Align;
            return;
        } 

        var newVelocity = new Vector3(delta.x / Mathf.Abs(delta.x), delta.y / Mathf.Abs(delta.y)) * Speed;

        rb.velocity = new Vector2(newVelocity.x, Mathf.Sin(Time.time * AdvanceSinTimeFactor) * AdvanceSinYSpeed);
        rb.velocity = new Vector2(Math.Abs(rb.velocity.x) > MaxAdvanceSpeed ? Mathf.Sign(rb.velocity.x) * MaxAdvanceSpeed : rb.velocity.x, rb.velocity.y);
    }

    private void Decelerate()
    {
        var delta = (attackPosition - transform.position);
        if (Mathf.Sign(delta.x) == Mathf.Sign(rb.velocity.x))
        {
            accelerationSet = false;
            BatDuckState = BatDuckStateEnum.RunAway;
            return;
        }

        if (!accelerationSet)
            acceleration = new Vector2(delta.x / Mathf.Abs(delta.x), 0f) * AttackAccelerationSpeed;

        rb.velocity += acceleration;

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.99f);
    }

    private Action[] StateFunctions;

    private void RunAway()
    {
        var delta = (player.transform.position - transform.position);
        if (delta.magnitude > 64 && Mathf.Abs(delta.x) > RunAwayXThreshold)
        {
            BatDuckState = BatDuckStateEnum.Align;
            return;
        }

        var newVelocity = -new Vector3(Mathf.Sign(delta.x), Mathf.Sign(delta.y)) * Speed;

        rb.velocity = new Vector2(newVelocity.x, newVelocity.y);
    }

    private bool accelerationSet = false;
    private Vector2 acceleration = Vector2.zero;

    private void Attack()
    {
        var delta = (attackPosition - transform.position);
        if (Mathf.Sign(delta.x) != Mathf.Sign(rb.velocity.x))
        {
            accelerationSet = false;
            BatDuckState = BatDuckStateEnum.Decelerate;
            return;
        }

        if (!accelerationSet)
            acceleration = new Vector2(delta.x / Mathf.Abs(delta.x), 0f) * AttackAccelerationSpeed;

        rb.velocity += acceleration;

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.99f);
    }

    private void Align()
    {
        var delta = (player.transform.position - transform.position);
        if (Mathf.Abs(delta.y) < 1)
        {
            BatDuckState = BatDuckStateEnum.Advance;
            rb.velocity = new Vector2(rb.velocity.x, -AdvanceSinYSpeed * Mathf.Sin((Time.time * AdvanceSinTimeFactor) + Mathf.PI));
            return;
        }
        if (Mathf.Abs(delta.x) < RunAwayXThreshold)
        {
            BatDuckState = BatDuckStateEnum.RunAway;
            return;
        }

        var newVelocity = new Vector3(delta.x / Mathf.Abs(delta.x), delta.y / Mathf.Abs(delta.y)) * Speed;

        rb.velocity = new Vector2(newVelocity.x / 10f, newVelocity.y);
    }

    private Color[] stateColors = new Color[]
    {
        Color.yellow,
        Color.green,
        Color.red,
        Color.blue,
        Color.cyan
    };

    private void Update()
    {
        StateFunctions[(int)BatDuckState]();
        GetComponent<SpriteRenderer>().color = stateColors[(int)BatDuckState];

        GetComponent<SpriteRenderer>().flipX = rb.velocity.x > 0 ? true : false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            player.GetComponent<SpriteRenderer>().color = Color.red;

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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name == "Player")
            player.GetComponent<SpriteRenderer>().color = Color.white;
    }
}
