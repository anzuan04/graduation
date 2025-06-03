// ============================================================================
// 3. PlayerController.cs - 플레이어 전용 컨트롤러
// ============================================================================
using UnityEngine;

public class PlayerController : BaseCharacter
{
    [Header("Player UI")]
    public GameObject crosshair;
    public UnityEngine.UI.Slider healthBar;
    public UnityEngine.UI.Button dashButton;

    protected override void Awake()
    {
        base.Awake();
        if (dashButton) dashButton.onClick.AddListener(() => Dash(GetAimDirection()));
    }

    protected override void Update()
    {
        base.Update();
        UpdateUI();
        UpdateCrosshair();
    }

    protected override void HandleMovement()
    {
        var input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        Move(input);
    }

    protected override void HandleAiming()
    {
        var aimDirection = GetAimDirection();
        if (aimDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    protected override void HandleActions()
    {
        if (Input.GetMouseButton(0)) // 좌클릭으로 발사
        {
            Fire(GetAimDirection());
        }

        if (Input.GetKeyDown(KeyCode.Space)) // 스페이스로 대시
        {
            Dash(GetAimDirection());
        }
    }

    Vector2 GetAimDirection()
    {
        var mousePos = GetMouseWorldPosition();
        return (mousePos - (Vector2)transform.position).normalized;
    }

    void UpdateUI()
    {
        if (healthBar) healthBar.value = currentHealth / maxHealth;
        if (dashButton) dashButton.interactable = canDash;
    }

    void UpdateCrosshair()
    {
        if (crosshair)
        {
            crosshair.transform.position = GetMouseWorldPosition();
        }
    }
}