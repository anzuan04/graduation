// ============================================================================
// 4. BotController.cs - AI 봇 컨트롤러
// ============================================================================
using UnityEngine;

public class BotController : BaseCharacter
{
    [Header("AI Settings")]
    public BotType botType = BotType.Basic;
    public float detectionRange = 8f;
    public float attackRange = 6f;
    public Color botColor = Color.red;

    private Transform target;
    private Vector2 wanderTarget;
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
                botColor = Color.red;
                break;
            case BotType.Sniper:
                attackRange *= 1.5f;
                fireRate *= 1.5f;
                moveSpeed *= 0.8f;
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
            var direction = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    protected override void HandleActions()
    {
        UpdateAI();

        if (currentState == BotState.Attack && target)
        {
            Fire((target.position - transform.position).normalized);

            if (botType == BotType.Rusher && Vector2.Distance(transform.position, target.position) < 4f)
            {
                Dash((target.position - transform.position).normalized);
            }
        }
    }

    void UpdateAI()
    {
        if (Time.time - lastTargetSearch > 0.5f) // 0.5초마다 타겟 검색
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
                else if (currentHealth < maxHealth * 0.3f) // 체력 30% 이하시 도망
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