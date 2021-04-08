using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private int _maxHealth;
    private int maxHealth { get { return _maxHealth; } 
        set {
            _maxHealth = value;
            if (StatusBarUpdater != null)
            {
                StatusBarUpdater.MaxHealth = _maxHealth;
            }
        } 
    }
    private int _currentHealth;
    private int currentHealth { get { return _currentHealth; } 
        set
        {
            _currentHealth = value;
            if (StatusBarUpdater != null)
            {
                StatusBarUpdater.CurrentHealth = _currentHealth;
            }
        }
    }
    private int _maxMagic;
    private int maxMagic
    {
        get { return _maxMagic; }
        set
        {
            _maxMagic = value;
            if (StatusBarUpdater != null)
            {
                StatusBarUpdater.MaxMagic = _maxMagic;
            }
        }
    }
    private int _currentMagic;
    private int currentMagic
    {
        get { return _currentMagic; }
        set
        {
            _currentMagic = value;
            if (StatusBarUpdater != null)
            {
                StatusBarUpdater.CurrentMagic = _currentMagic;
            }
        }
    }

    private int _experience;
    private int experience { get { return _experience; } 
        set
        {
            _experience = value;
            DungeonPersistentData.Instance.PlayerExperience = value;
            if (StatusBarUpdater != null)
            {
                StatusBarUpdater.Experience = _experience;
            }
        }
    }
    public StatusBarUpdater StatusBarUpdater;

    [System.Serializable]
    public class InputsContainer
    {
        public InputAction Shoot;
        public InputAction Jump;
        public InputAction Move;
    }
    public InputsContainer Inputs;

    public GameObject FireballPrefab;
    private Transform fireballPoint;

    public float MagicRegenTime = 1f;
    public float MaxTimeToHoldJump = 0.75f;
    public float JumpHoldStrength = 15f;
    public float EndJumpStrength = 4f;
    public float MaxVerticalSpeed = 15f;
    public float MaxHorizontalSpeed = 15f;
    public float FloorControlSpeed = 0.5f;
    public float AirControlSpeed = 0.5f;
    public float FloorDragSpeed = 0.8f;
    public float GravityScale = 32f;
    public float CoyoteJumpWindowTime = 0.2f;

    private float lastRegenTime;

    private Rigidbody2D _rb;
    private Rigidbody2D rb => _rb == null ? (_rb = GetComponent<Rigidbody2D>()) : _rb;
    private Collider2D _coll;
    private Collider2D coll => _coll == null ? (_coll = GetComponent<Collider2D>()) : _coll;
    private bool jumpButtonDown = false;

    public bool IsDead => currentHealth <= 0;

    private int direction = 1;

    private int frameIsOnFloorChecked;
    private bool _isOnFloor;
    private bool isOnFloor
    {
        get
        {
            if (frameIsOnFloorChecked < Time.frameCount)
            {
                _isOnFloor = CheckIsOnFloor();
                frameIsOnFloorChecked = Time.frameCount;
            }

            return _isOnFloor;
        }
    }
    enum JumpingStateEnum
    {
        Start,
        Jumping,
        NotJumping
    };

    internal void IncreaseExperience()
    {
        experience++;
    }

    internal void ProcessEnemyHit(IEnemy enemy)
    {
        if (IsDead)
            return;

        currentHealth -= enemy.Damage;
        if (IsDead)
        {
            transform.rotation = Quaternion.Euler(0, 0, -90);
            StartCoroutine(ExitDungeonSceneDelay(3f));
        }
    }

    IEnumerator ExitDungeonSceneDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("DungeonStart");
    }

    JumpingStateEnum JumpingState;

    private float timeStartedJumping;
    private float currentJumpStrength;
    private float gravityDecreasePerSecond =>
         (JumpHoldStrength - EndJumpStrength) / MaxTimeToHoldJump;

    private float lastTimeOnFloor;
    private bool canCoyoteJump => Time.time - lastTimeOnFloor <= CoyoteJumpWindowTime;

    private void Awake()
    {
        experience = DungeonPersistentData.Instance.PlayerExperience;
        maxHealth = DungeonPersistentData.Instance.PlayerHealth;
        currentHealth = maxHealth;
        maxMagic = DungeonPersistentData.Instance.PlayerMagic;
        currentMagic = maxMagic;

        Inputs.Jump.started += _ =>
        {
            jumpButtonDown = true;
            Jump();
        };

        Inputs.Jump.canceled += _ => jumpButtonDown = false;

        Inputs.Shoot.performed += _ => Shoot();

        Inputs.Jump.Enable();
        Inputs.Shoot.Enable();
        Inputs.Move.Enable();

        fireballPoint = transform.Find("FireballPoint");
    }

    private void Update()
    {
        if (currentMagic < maxMagic && (Time.time - lastRegenTime) > MagicRegenTime)
        {
            currentMagic++;
            lastRegenTime += MagicRegenTime;
        }

        lastTimeOnFloor = isOnFloor ? Time.time : lastTimeOnFloor;

        HandleJumpState();

        if (Mathf.Abs(rb.velocity.y) > MaxVerticalSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Sign(rb.velocity.y) * MaxVerticalSpeed);
        }

        if (IsDead) return;

        var moveInput = GetAndSanitizeMoveInput();

        Move(moveInput.x);

        ChangeDirection(moveInput.x);
    }

    private void ChangeDirection(float x)
    {
        if (x < 0) direction = -1;
        if (x > 0) direction = 1;

        transform.localScale = new Vector3(direction, 1, 1);
    }

    private void Move(float moveInput)
    {
        rb.velocity += new Vector2(
            moveInput * 
            (isOnFloor ? FloorControlSpeed : AirControlSpeed) * 
            Time.deltaTime, 
            0);

        if (isOnFloor)
        {
            var drag = Mathf.Sign(rb.velocity.x) * (FloorDragSpeed * Time.deltaTime);
            if (Mathf.Abs(drag) > Mathf.Abs(rb.velocity.x))
                drag = rb.velocity.x;

            rb.velocity = new Vector2(rb.velocity.x - drag, rb.velocity.y);
        }

        if (Mathf.Abs(rb.velocity.x) > MaxHorizontalSpeed)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * MaxHorizontalSpeed, rb.velocity.y);
        }
    }

    private Vector2 GetAndSanitizeMoveInput()
    {
        var moveInput = Inputs.Move.ReadValue<Vector2>();
        float x = 0f;
        float y = 0f;

        if (Mathf.Abs(moveInput.x) > 0.3)
        {
            x = Mathf.Sign(moveInput.x);
        }

        if (Mathf.Abs(moveInput.y) > 0.3)
        {
            y = Mathf.Sign(moveInput.y);
        }

        return new Vector2(x, y);
    }
    private void HandleJumpState()
    {
        CheckAndBonk();

        if (JumpingState == JumpingStateEnum.Start)
        {
            KeepJumping();
        }

        if (JumpingState == JumpingStateEnum.Jumping)
        {
            if (isOnFloor)
            {
                JumpingState = JumpingStateEnum.NotJumping;
            }
        }
    }

    private void CheckAndBonk()
    {
        if (JumpingState == JumpingStateEnum.Start &&
            CheckIsUnderCeiling())
        {
            JumpingState = JumpingStateEnum.Jumping;
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.position = new Vector2(rb.position.x, Mathf.Round(rb.position.y) - 2f);
        }
    }

    public void Jump()
    {
        if (IsDead) return;

        var canJump = (isOnFloor && JumpingState == JumpingStateEnum.NotJumping) || 
            (canCoyoteJump && !isOnFloor);

        if (!canJump)
            return;

        JumpingState = JumpingStateEnum.Start;
        timeStartedJumping = Time.time;
        currentJumpStrength = JumpHoldStrength;
    }

    private void KeepJumping()
    {
        currentJumpStrength -= (gravityDecreasePerSecond * Time.deltaTime);

        rb.velocity = new Vector2(rb.velocity.x, currentJumpStrength);

        if (!jumpButtonDown ||
            Time.time - timeStartedJumping >= MaxTimeToHoldJump || IsDead)
        {
            JumpingState = JumpingStateEnum.Jumping;
        }
    }

    private bool CheckIsOnFloor()
    {
        var colliderMax = new Vector2(coll.bounds.max.x - 1f, coll.bounds.min.y + 1f - (coll as BoxCollider2D).edgeRadius);
        var colliderMin = new Vector2(coll.bounds.min.x + 1f, coll.bounds.min.y - 2f - (coll as BoxCollider2D).edgeRadius);
        var coll2d = Physics2D.OverlapArea(colliderMin, colliderMax, LayerMask.GetMask("Ground"));

        return coll2d != null;
    }

    private bool CheckIsUnderCeiling()
    {
        var colliderMax = new Vector2(coll.bounds.max.x - 1f, coll.bounds.max.y + 0.5f + (coll as BoxCollider2D).edgeRadius);
        var colliderMin = new Vector2(coll.bounds.min.x + 1f, coll.bounds.max.y - 0.5f + (coll as BoxCollider2D).edgeRadius);
        var coll2d = Physics2D.OverlapArea(colliderMin, colliderMax, LayerMask.GetMask("Ground"));

        return coll2d != null;
    }

    private void Shoot()
    {
        if (currentMagic == 0 || IsDead)
            return;

        var fireball = GameObject.Instantiate(FireballPrefab, fireballPoint.transform.position, Quaternion.identity);
        fireball.GetComponent<FireballController>().Direction = direction;
        fireball.SetActive(true);
        currentMagic -= 1;
        lastRegenTime = Time.time;
    }
}
