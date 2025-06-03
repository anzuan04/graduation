// ============================================================================
// 5. Bullet.cs - 총알 시스템
// ============================================================================
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 15f;
    public float damage = 25f;
    public float lifetime = 3f;

    private BaseCharacter shooter;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 direction, BaseCharacter shooter)
    {
        this.shooter = shooter;
        rb.linearVelocity = direction.normalized * speed; // Unity 6 새 API

        // 총알 방향으로 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var character = other.GetComponent<BaseCharacter>();
        if (character && character != shooter)
        {
            character.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}