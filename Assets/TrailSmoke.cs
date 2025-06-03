// ============================================================================
// TrailSmoke.cs - 단순화된 궤적 연기 효과
// ============================================================================
using UnityEngine;

public class TrailSmoke : MonoBehaviour
{
    [Header("Trail Settings")]
    public float trailDuration = 1.5f;

    private LineRenderer lineRenderer;
    private ParticleSystem smokeEffect;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        smokeEffect = GetComponent<ParticleSystem>();
        Destroy(gameObject, trailDuration);
    }

    public void CreateTrail(Vector2 startPos, Vector2 endPos)
    {
        // 하나의 긴 궤적 라인 그리기
        if (lineRenderer)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);

            // 라인이 서서히 사라지도록
            StartCoroutine(FadeTrail());
        }

        // 궤적의 중간 지점에 연기 효과
        if (smokeEffect)
        {
            CreateCenterSmoke(startPos, endPos);
        }
    }

    void CreateCenterSmoke(Vector2 start, Vector2 end)
    {
        // 궤적의 중간점과 방향 계산
        Vector2 center = (start + end) * 0.5f;
        Vector2 direction = (end - start).normalized;
        float distance = Vector2.Distance(start, end);

        // 연기를 궤적 중간에 배치
        transform.position = center;

        // 연기가 궤적 방향으로 퍼지도록 설정
        var shape = smokeEffect.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(distance * 0.8f, 0.3f, 1f);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        shape.rotation = new Vector3(0, 0, angle);

        // 파티클 개수를 거리에 비례하게 조정
        var emission = smokeEffect.emission;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, Mathf.RoundToInt(distance * 2f))
        });

        smokeEffect.Play();
    }

    System.Collections.IEnumerator FadeTrail()
    {
        if (!lineRenderer) yield break;

        float elapsed = 0f;
        Color startColor = lineRenderer.startColor;
        Color endColor = lineRenderer.endColor;
        float initialStartAlpha = startColor.a;
        float initialEndAlpha = endColor.a;

        while (elapsed < trailDuration)
        {
            elapsed += Time.deltaTime;
            float fadeProgress = elapsed / trailDuration;

            // 궤적이 뒤쪽부터 서서히 사라지는 효과
            float startAlpha = initialStartAlpha * (1f - fadeProgress);
            float endAlpha = initialEndAlpha * Mathf.Max(0f, 1f - fadeProgress * 1.5f);

            startColor.a = startAlpha;
            endColor.a = endAlpha;
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;

            yield return null;
        }
    }
}