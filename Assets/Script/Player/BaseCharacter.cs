// ============================================================================
// BaseCharacter.cs - 가상 총알 시뮬레이션 시스템
// ============================================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public abstract class BaseCharacter : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float dashDistance = 3f;
    public float dashCooldown = 2f;

    [Header("Combat")]
    public GameObject trailSmokePrefab;
    public GameObject bullet;
    public Transform firePoint;
    public float fireRate = 0.5f;
    public float bulletSpeed = 20f;
    public float bulletRange = 15f;
    public float damage = 25f;

    [Header("Layers")]
    public LayerMask obstacleLayer = 1 << 6;
    public LayerMask targetLayer = 1 << 7;

    protected float currentHealth;
    protected bool canDash = true;
    protected float lastFireTime;
    protected Rigidbody2D rb;
    protected Camera playerCamera;

    private List<VirtualBullet> activeBullets = new List<VirtualBullet>();
    private List<TrailSmoke> activeTrails = new List<TrailSmoke>();
    private MuzzleFlash muzzleFlash;

    public event Action OnDeath;
    public bool IsDead => currentHealth <= 0;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        playerCamera = Camera.main;

        if (firePoint)
        {
            muzzleFlash = firePoint.GetComponent<MuzzleFlash>();
            if (!muzzleFlash)
                muzzleFlash = firePoint.gameObject.AddComponent<MuzzleFlash>();
        }
    }

    protected virtual void Update()
    {
        if (IsDead) return;

        HandleMovement();
        HandleAiming();
        HandleActions();
    }

    void UpdateVirtualBullets()
    {
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            var bullet = activeBullets[i];
            Vector2 previousPos = bullet.currentPosition;

            bool stillActive = bullet.UpdatePosition(Time.deltaTime);
            if (!stillActive)
            {
                activeBullets.RemoveAt(i);
                continue;
            }

            Vector2 movement = bullet.currentPosition - previousPos;
            float moveDistance = movement.magnitude;

            if (moveDistance > 0.001f)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPos, movement.normalized, moveDistance, obstacleLayer | targetLayer);
                if (hit.collider != null)
                {
                    if (IsObstacle(hit.collider))
                    {
                        CreateTrailToPoint(bullet.startPosition, hit.point);
                        activeBullets.RemoveAt(i);
                    }
                    else if (IsTarget(hit.collider))
                    {
                        var character = hit.collider.GetComponent<BaseCharacter>();
                        if (character && character != bullet.shooter)
                        {
                            character.TakeDamage(bullet.damage);
                            CreateTrailToPoint(bullet.startPosition, hit.point);
                            activeBullets.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    UpdateTrailSmoke(bullet, previousPos); 
                }
            }
        }
    }

    void UpdateTrailSmoke(VirtualBullet bullet, Vector2 previousPos)
    {   
        
        if (trailSmokePrefab)
        {
            
            var trail = Instantiate(trailSmokePrefab, previousPos, Quaternion.identity);
            var trailComponent = trail.GetComponent<TrailSmoke>();
            trailComponent.CreateTrail(previousPos, bullet.currentPosition);
            activeTrails.Add(trailComponent);

            if (activeTrails.Count > 20)
            {
                if (activeTrails[0] != null)
                    Destroy(activeTrails[0].gameObject);
                activeTrails.RemoveAt(0);
            }
        }
    }

    void CreateTrailToPoint(Vector2 start, Vector2 end)
    {
        if (trailSmokePrefab)
        {
            var trail = Instantiate(trailSmokePrefab, start, Quaternion.identity);
            trail.GetComponent<TrailSmoke>().CreateTrail(start, end);
        }
    }

    protected abstract void HandleMovement();
    protected abstract void HandleAiming();
    protected abstract void HandleActions();

    protected void Move(Vector2 direction)
    {
        rb.linearVelocity = direction * moveSpeed;
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
        /*if (Time.time - lastFireTime < fireRate) return;

        if (muzzleFlash)
            muzzleFlash.Flash(direction);

        var bullet = new VirtualBullet(
            firePoint.position,
            direction,
            bulletSpeed,
            damage,
            bulletRange,
            this
        );

        activeBullets.Add(bullet);
        lastFireTime = Time.time;*/
        var bulletObj = BulletPool.instance.Pool.Get();
        if (bulletObj != null)
        {
            if (muzzleFlash)
                muzzleFlash.Flash(direction);
            BulletSys bs = bulletObj.GetComponent<BulletSys>();
            bs.Fire(firePoint.position,direction);
        }
    }

    bool IsObstacle(Collider2D collider)
    {
        return (obstacleLayer.value & (1 << collider.gameObject.layer)) != 0;
    }

    bool IsTarget(Collider2D collider)
    {
        return (targetLayer.value & (1 << collider.gameObject.layer)) != 0;
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
        activeBullets.Clear();
        OnDeath?.Invoke();
        gameObject.SetActive(false);
    }

    protected virtual Vector2 GetMouseWorldPosition()
    {
        var mouse = Mouse.current;
        if (mouse == null) return Vector2.zero;

        var mousePos = mouse.position.ReadValue();
        return playerCamera.ScreenToWorldPoint(mousePos);
    }
    
}