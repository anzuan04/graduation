// ============================================================================
// BotController.cs - 전지적 추적 & 잠복 시스템
// ============================================================================
using UnityEngine;

public class BotController : BaseCharacter
{
    [Header("AI Settings")]
    public BotType botType = BotType.Basic;
    public float detectionRange = 8f;
    public float attackRange = 6f;
    public Color botColor = Color.red;

    [Header("Hunting Settings")]
    public bool omniscientTracking = true; // 전지적 추적
    public float ambushDistance = 12f; // 잠복 거리
    public float ambushDuration = 3f; // 잠복 시간

    private Transform player;
    private Vector2 ambushPosition;
    private Vector2 wanderTarget;
    private float stateTimer;
    private BotState currentState = BotState.Hunt;

    enum BotState { Hunt, Ambush, Attack, Flee }

    protected override void Awake()
    {
        base.Awake();

        // 랜덤 봇 타입 설정
        botType = (BotType)Random.Range(0, System.Enum.GetValues(typeof(BotType)).Length);

        SetBotStats();
        GetComponent<SpriteRenderer>().color = botColor;
        FindPlayer();
    }

    void FindPlayer()
    {
        var playerController = FindAnyObjectByType<PlayerController>();
        if (playerController) player = playerController.transform;
    }

    protected override void HandleMovement()
    {
        if (!player) return;

        switch (currentState)
        {
            case BotState.Hunt:
                // 직접 추적하거나 잠복 위치로 이동
                if (ShouldAmbush())
                {
                    MoveTowards(ambushPosition);
                    if (Vector2.Distance(transform.position, ambushPosition) < 1f)
                        ChangeState(BotState.Ambush);
                }
                else
                {
                    MoveTowards(player.position);
                }
                break;

            case BotState.Ambush:
                // 잠복 중엔 미동하지 않음
                break;

            case BotState.Attack:
                // 공격하면서 근접
                MoveTowards(player.position);
                break;

            case BotState.Flee:
                MoveAwayFrom(player.position);
                break;
        }
    }

    bool ShouldAmbush()
    {
        if (botType == BotType.Sniper || botType == BotType.Aggressive)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            if (distToPlayer > ambushDistance)
            {
                // 플레이어 이동 방향 예측해서 매복
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                Vector2 playerVel = playerRb ? playerRb.linearVelocity : Vector2.zero;
                ambushPosition = (Vector2)player.position + playerVel.normalized * 5f;
                return true;
            }
        }
        return false;
    }

    protected override void HandleAiming()
    {
        if (player && (currentState == BotState.Attack || currentState == BotState.Ambush))
        {
            Vector2 aimDirection = GetPredictiveAimDirection();
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    Vector2 GetPredictiveAimDirection()
    {
        Vector2 targetPos = player.position;

        // 고급 봇은 예측 사격
        if (botType != BotType.Basic)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb)
            {
                Vector2 playerVel = playerRb.linearVelocity;
                float timeToHit = Vector2.Distance(transform.position, targetPos) / bulletSpeed;
                targetPos += playerVel * timeToHit;

                // 봇 타입별 정확도
                float accuracy = botType == BotType.Sniper ? 0.95f :
                               botType == BotType.Aggressive ? 0.8f : 0.6f;
                Vector2 error = Random.insideUnitCircle * (1f - accuracy);
                targetPos += error;
            }
        }

        return (targetPos - (Vector2)transform.position).normalized;
    }

    protected override void HandleActions()
    {
        if (!player) return;

        UpdateHuntingAI();

        if (currentState == BotState.Attack || currentState == BotState.Ambush)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);

            if (distToPlayer <= attackRange)
            {
                Fire(GetPredictiveAimDirection());
            }

            if (botType == BotType.Rusher && distToPlayer < 4f)
            {
                Dash((player.position - transform.position).normalized);
            }
        }
    }

    void UpdateHuntingAI()
    {
        stateTimer += Time.deltaTime;
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case BotState.Hunt:
                if (distToPlayer <= attackRange)
                    ChangeState(BotState.Attack);
                break;

            case BotState.Ambush:
                if (stateTimer > ambushDuration || distToPlayer <= attackRange)
                    ChangeState(BotState.Attack);
                break;

            case BotState.Attack:
                if (currentHealth < maxHealth * 0.25f)
                    ChangeState(BotState.Flee);
                else if (distToPlayer > attackRange * 2f)
                    ChangeState(BotState.Hunt);
                break;

            case BotState.Flee:
                if (stateTimer > 4f || currentHealth > maxHealth * 0.6f)
                    ChangeState(BotState.Hunt);
                break;
        }
    }

    void ChangeState(BotState newState)
    {
        currentState = newState;
        stateTimer = 0f;
    }

    void MoveTowards(Vector2 targetPos)
    {
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        Move(direction);
    }

    void MoveAwayFrom(Vector2 dangerPos)
    {
        Vector2 direction = ((Vector2)transform.position - dangerPos).normalized;
        Move(direction);
    }

    void SetBotStats()
    {
        switch (botType)
        {
            case BotType.Aggressive:
                moveSpeed *= 1.2f;
                fireRate *= 0.7f;
                attackRange *= 1.2f;
                botColor = Color.red;
                break;
            case BotType.Sniper:
                attackRange *= 2f;
                fireRate *= 1.8f;
                moveSpeed *= 0.7f;
                ambushDistance *= 1.5f;
                botColor = Color.green;
                break;
            case BotType.Rusher:
                moveSpeed *= 1.5f;
                dashCooldown *= 0.5f;
                maxHealth *= 0.8f;
                botColor = Color.blue;
                break;
        }
        currentHealth = maxHealth;
    }
}

public enum BotType { Basic, Aggressive, Sniper, Rusher }