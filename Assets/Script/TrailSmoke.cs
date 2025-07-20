// ============================================================================
// TrailSmoke.cs - 레이저 직선 궤적
// ============================================================================
using UnityEngine;

public class TrailSmoke : MonoBehaviour
{
    [Header("Trail Settings")]
    public float trailDuration = 0.8f;
    public float lineWidth = 0.1f;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (!lineRenderer)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        SetupLineRenderer();
        Destroy(gameObject, trailDuration);
    }

    void SetupLineRenderer()
    {
        lineRenderer.material = CreateLaserMaterial();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = 1;
    }

    Material CreateLaserMaterial()
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = Color.yellow;
        return mat;
    }

    public void CreateTrail(Vector2 startPos, Vector2 endPos)
    {
        if (lineRenderer)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);

            StartCoroutine(FadeTrail());
        }
    }

    System.Collections.IEnumerator FadeTrail()
    {
        float elapsed = 0f;
        Color originalColor = lineRenderer.material.color;

        while (elapsed < trailDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / trailDuration);

            Color newColor = originalColor;
            newColor.a = alpha;
            lineRenderer.material.color = newColor;

            yield return null;
        }
    }
}