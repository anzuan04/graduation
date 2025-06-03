// ============================================================================
// 3. PlayerController.cs - 플레이어 전용 컨트롤러 (Unity 6 Input System)
// ============================================================================
using UnityEngine;
using UnityEngine.InputSystem;

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
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        var input = Vector2.zero;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;

        Move(input.normalized);
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
        var mouse = Mouse.current;
        var keyboard = Keyboard.current;

        if (mouse == null || keyboard == null) return;

        // 좌클릭으로 발사
        if (mouse.leftButton.isPressed)
        {
            Fire(GetAimDirection());
        }

        // 스페이스로 대시
        if (keyboard.spaceKey.wasPressedThisFrame)
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

    // 새 Input System용 마우스 월드 좌표 계산
    protected override Vector2 GetMouseWorldPosition()
    {
        var mouse = Mouse.current;
        if (mouse == null) return Vector2.zero;

        var mousePos = mouse.position.ReadValue();
        return playerCamera.ScreenToWorldPoint(mousePos);
    }
}