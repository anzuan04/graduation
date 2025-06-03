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
    public Transform firePoint;
    public float fireRate = 0.5f;
    public float bulletSpeed = 20f;  // 총알 속도
    public float bulletRange = 15f;  // 총알 사거리
    public float damage = 25f;

    [Header("Layers")]
    public LayerMask obstacleLayer = 1 << 6;
    public LayerMask targetLayer = 1 << 7;

    protected float currentHealth;
    protected bool canDash = true;
    protected float lastFireTime;
    protected Rigidbody2D rb;
    protected Camera playerCamera;

    // 가상 총알 관리
    private List<VirtualBullet> activeBullets = new List<VirtualBullet>();
    private List<TrailSmoke> activeTrails = new List<TrailSmoke>();

    public event Action OnDeath;
    public bool IsDead => currentHealth <= 0;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        playerCamera = Camera.main;
    }

    protected virtual void Update()
    {
        if (IsDead) return;

        UpdateVirtualBullets();
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

            // 총알 위치 업데이트
            bool stillActive = bullet.UpdatePosition(Time.deltaTime);

            if (!stillActive)
            {
                // 사거리 초과
                activeBullets.RemoveAt(i);
                continue;
            }

            // 이전 위치에서 현재 위치까지 레이캐스트
            Vector2 movement = bullet.currentPosition - previousPos;
            float moveDistance = movement.magnitude;

            if (moveDistance > 0.001f)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPos, movement.normalized, moveDistance, obstacleLayer | targetLayer);

                if (hit.collider != null)
                {
                    // 충돌 처리
                    if (IsObstacle(hit.collider))
                    {
                        // 장애물에 충돌 - 총알 제거
                        CreateTrailToPoint(bullet.startPosition, hit.point);
                        activeBullets.RemoveAt(i);
                    }
                    else if (IsTarget(hit.collider))
                    {
                        var character = hit.collider.GetComponent<BaseCharacter>();
                        if (character && character != bullet.shooter)
                        {
                            // 타겟에 충돌 - 피해 적용
                            character.TakeDamage(bullet.damage);
                            CreateTrailToPoint(bullet.startPosition, hit.point);
                            activeBullets.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    // 충돌 없음 - 궤적 연기 업데이트
                    UpdateTrailSmoke(bullet, previousPos);
                }
            }
        }
    }

    void UpdateTrailSmoke(VirtualBullet bullet, Vector2 previousPos)
    {
        // 총알이 지나간 경로에 연기 효과 생성
        if (trailSmokePrefab && Vector2.Distance(previousPos, bullet.currentPosition) > 0.5f)
        {
            var trail = Instantiate(trailSmokePrefab, previousPos, Quaternion.identity);
            var trailComponent = trail.GetComponent<TrailSmoke>();
            trailComponent.CreateTrail(previousPos, bullet.currentPosition);
            activeTrails.Add(trailComponent);

            // 오래된 궤적 정리
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
        if (Time.time - lastFireTime < fireRate) return;

        // 가상 총알 생성
        var bullet = new VirtualBullet(
            firePoint.position,
            direction,
            bulletSpeed,
            damage,
            bulletRange,
            this
        );

        activeBullets.Add(bullet);
        lastFireTime = Time.time;
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
        // 죽을 때 발사한 총알들 정리
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

    // 디버그용 - 가상 총알 위치 표시
    void OnDrawGizmos()
    {
        if (activeBullets == null) return;

        Gizmos.color = Color.yellow;
        foreach (var bullet in activeBullets)
        {
            Gizmos.DrawWireSphere(bullet.currentPosition, 0.1f);
        }
    }
}