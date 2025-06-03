// ============================================================================
// BotController.cs - 미래 예측 사격 기능 추가
// ============================================================================
using UnityEngine;

public class BotController : BaseCharacter
{
    [Header("AI Settings")]
    public BotType botType = BotType.Basic;
    public float detectionRange = 8f;
    public float attackRange = 6f;
    public Color botColor = Color.red;

    [Header("Prediction Settings")]
    public bool usePredictiveAiming = true;
    public float predictionAccuracy = 0.8f; // 0~1, 1이 완벽한 예측

    private Transform target;
    private Vector2 wanderTarget;
    private Vector2 lastTargetPosition;
    private Vector2 targetVelocity;
    private float lastTargetSearch;
    private float stateTimer;
    private BotState currentState = BotState.Wander;

    enum BotState { Wander, Chase, Attack, Flee }

    protected override void Awake()
    {
        base.Awake();
        SetBotStats();
        GetComponent<SpriteRenderer>().color = botColor;
        SetRandomWanderTarget();
    }

    void SetBotStats()
    {
        switch (botType)
        {
            case BotType.Aggressive:
                moveSpeed *= 1.2f;
                fireRate *= 0.7f;
                detectionRange *= 1.3f;
                bulletSpeed *= 1.1f;
                predictionAccuracy = 0.9f;
                botColor = Color.red;
                break;
            case BotType.Sniper:
                attackRange *= 1.5f;
                fireRate *= 1.5f;
                moveSpeed *= 0.8f;
                bulletSpeed *= 1.3f;
                predictionAccuracy = 0.95f; // 스나이퍼는 예측이 정확
                botColor = Color.green;
                break;
            case BotType.Rusher:
                moveSpeed *= 1.5f;
                dashCooldown *= 0.5f;
                maxHealth *= 0.8f;
                bulletSpeed *= 0.9f;
                predictionAccuracy = 0.6f; // 러셔는 부정확
                botColor = Color.blue;
                break;
        }
        currentHealth = maxHealth;
    }

    protected override void HandleMovement()
    {
        switch (currentState)
        {
            case BotState.Wander:
                MoveTowards(wanderTarget);
                if (Vector2.Distance(transform.position, wanderTarget) < 1f)
                    SetRandomWanderTarget();
                break;

            case BotState.Chase:
                if (target) MoveTowards(target.position);
                break;

            case BotState.Flee:
                if (target) MoveAwayFrom(target.position);
                break;
        }
    }

    protected override void HandleAiming()
    {
        if (target && currentState == BotState.Attack)
        {
            Vector2 aimDirection = GetAimDirection();
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    Vector2 GetAimDirection()
    {
        if (!target) return Vector2.zero;

        Vector2 targetPos = target.position;

        if (usePredictiveAiming && botType != BotType.Basic)
        {
            // 타겟의 속도 계산
            UpdateTargetVelocity();

            // 예측 사격 계산
            Vector2 predictedPos = CalculatePredictedPosition(targetPos, targetVelocity);

            // 정확도에 따라 오차 추가
            Vector2 aimOffset = Random.insideUnitCircle * (1f - predictionAccuracy) * 2f;
            targetPos = predictedPos + aimOffset;
        }

        return (targetPos - (Vector2)transform.position).normalized;
    }

    void UpdateTargetVelocity()
    {
        if (target)
        {
            Vector2 currentPos = target.position;
            targetVelocity = (currentPos - lastTargetPosition) / Time.deltaTime;
            lastTargetPosition = currentPos;
        }
    }

    Vector2 CalculatePredictedPosition(Vector2 currentPos, Vector2 velocity)
    {
        // 총알이 타겟에 도달하는 시간 계산
        float distanceToTarget = Vector2.Distance(transform.position, currentPos);
        float timeToHit = distanceToTarget / bulletSpeed;

        // 예측 위치 = 현재 위치 + (속도 * 시간)
        return currentPos + velocity * timeToHit;
    }

    protected override void HandleActions()
    {
        UpdateAI();

        if (currentState == BotState.Attack && target)
        {
            Fire(GetAimDirection());

            if (botType == BotType.Rusher && Vector2.Distance(transform.position, target.position) < 4f)
            {
                Dash((target.position - transform.position).normalized);
            }
        }
    }

    void UpdateAI()
    {
        if (Time.time - lastTargetSearch > 0.5f)
        {
            FindTarget();
            lastTargetSearch = Time.time;
        }

        stateTimer += Time.deltaTime;

        var distanceToTarget = target ? Vector2.Distance(transform.position, target.position) : float.MaxValue;

        switch (currentState)
        {
            case BotState.Wander:
                if (target && distanceToTarget < detectionRange)
                    ChangeState(BotState.Chase);
                break;

            case BotState.Chase:
                if (!target || distanceToTarget > detectionRange * 1.5f)
                    ChangeState(BotState.Wander);
                else if (distanceToTarget < attackRange)
                    ChangeState(BotState.Attack);
                break;

            case BotState.Attack:
                if (!target || distanceToTarget > attackRange * 1.2f)
                    ChangeState(BotState.Chase);
                else if (currentHealth < maxHealth * 0.3f)
                    ChangeState(BotState.Flee);
                break;

            case BotState.Flee:
                if (currentHealth > maxHealth * 0.6f || stateTimer > 5f)
                    ChangeState(BotState.Wander);
                break;
        }
    }

    void FindTarget()
    {
        var allCharacters = FindObjectsByType<BaseCharacter>(FindObjectsSortMode.None);
        BaseCharacter closestTarget = null;
        float closestDistance = detectionRange;

        foreach (var character in allCharacters)
        {
            if (character == this || character.IsDead) continue;

            float distance = Vector2.Distance(transform.position, character.transform.position);
            if (distance < closestDistance)
            {
                closestTarget = character;
                closestDistance = distance;
            }
        }

        target = closestTarget?.transform;
        if (target)
        {
            lastTargetPosition = target.position;
        }
    }

    void ChangeState(BotState newState)
    {
        currentState = newState;
        stateTimer = 0f;

        if (newState == BotState.Wander)
            SetRandomWanderTarget();
    }

    void MoveTowards(Vector2 targetPos)
    {
        var direction = (targetPos - (Vector2)transform.position).normalized;
        Move(direction);
    }

    void MoveAwayFrom(Vector2 dangerPos)
    {
        var direction = ((Vector2)transform.position - dangerPos).normalized;
        Move(direction);
    }

    void SetRandomWanderTarget()
    {
        var randomDirection = Random.insideUnitCircle.normalized;
        wanderTarget = (Vector2)transform.position + randomDirection * Random.Range(3f, 8f);
    }
}

public enum BotType { Basic, Aggressive, Sniper, Rusher }