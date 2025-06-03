// ============================================================================
// 2. BaseCharacter.cs - 플레이어/봇 공통 베이스 클래스
// ============================================================================
using UnityEngine;
using System;

public abstract class BaseCharacter : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float dashDistance = 3f;
    public float dashCooldown = 2f;

    [Header("Combat")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;

    protected float currentHealth;
    protected bool canDash = true;
    protected float lastFireTime;
    protected Rigidbody2D rb;
    protected Camera playerCamera;

    public event Action OnDeath;
    public bool IsDead => currentHealth <= 0;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        playerCamera = Camera.main; // Unity 6에서 최적화됨
    }

    protected virtual void Update()
    {
        if (IsDead) return;

        HandleMovement();
        HandleAiming();
        HandleActions();
    }

    protected abstract void HandleMovement();
    protected abstract void HandleAiming();
    protected abstract void HandleActions();

    protected void Move(Vector2 direction)
    {
        rb.linearVelocity = direction * moveSpeed; // Unity 6 새 API
    }

    protected void Dash(Vector2 direction)
    {
        if (!canDash) return;

        rb.AddForce(direction.normalized * dashDistance, ForceMode2D.Impulse);
        canDash = false;
        Invoke(nameof(ResetDash), dashCooldown);
    }

    void ResetDash() => canDash = true;

    protected void Fire(Vector2 direction)
    {
        if (Time.time - lastFireTime < fireRate) return;

        var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().Initialize(direction, this);
        lastFireTime = Time.time;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke();
        gameObject.SetActive(false);
    }

    protected Vector2 GetMouseWorldPosition()
    {
        var mousePos = Input.mousePosition;
        return playerCamera.ScreenToWorldPoint(mousePos);
    }
}