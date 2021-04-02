using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballController : MonoBehaviour
{
    public float FireballSpeed = 8f;
    public int Direction = 1;

    private Rigidbody2D _rb;
    private Rigidbody2D rb => _rb == null ? (_rb = GetComponent<Rigidbody2D>()) : _rb;

    void OnEnable()
    {
        Debug.Log("Fireball - OnEnabled");
        rb.velocity = new Vector2(FireballSpeed * Direction, 0);
        transform.localScale = new Vector3(Direction, 1, 1);
    }

    private void Update()
    {
        CheckAndDestroyOOB();
    }

    private void CheckAndDestroyOOB()
    { 
        if (GetComponent<Collider2D>().bounds.max.x < -128f ||
            GetComponent<Collider2D>().bounds.min.x > 128f)
        {
            DestroyImmediate(gameObject);
        }
    }
}
