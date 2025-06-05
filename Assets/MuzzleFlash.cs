// ============================================================================
// MuzzleFlash.cs - 총구 화염 전용 컴포넌트
// ============================================================================
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    private ParticleSystem flashEffect;

    void Awake()
    {
        flashEffect = GetComponent<ParticleSystem>();
        if (!flashEffect)
        {
            flashEffect = gameObject.AddComponent<ParticleSystem>();
        }

        SetupFlash();
    }

    void SetupFlash()
    {
        var main = flashEffect.main;
        main.startLifetime = 0.08f;
        main.startSpeed = 4f;
        main.startSize = 0.4f;
        main.startColor = new Color(1f, 0.8f, 0.3f); // 주황빛 화염
        main.maxParticles = 20;

        var emission = flashEffect.emission;
        emission.enabled = false;

        var shape = flashEffect.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.05f;
    }

    public void Flash(Vector2 direction)
    {
        // 화염 방향 설정
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        flashEffect.Emit(20);
    }
}