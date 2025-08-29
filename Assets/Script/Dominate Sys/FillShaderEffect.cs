using UnityEngine;

public class FillShaderEffect : MonoBehaviour
{
    [Header("External Script Reference")]
    [Tooltip("이미 구현된 스크립트 컴포넌트를 여기에 드래그하세요")]
    public Dominate targetScript;

    [Header("Visual Settings")]
    public Color backgroundColor = Color.clear;
    [Range(0f, 0.1f)] public float edgeSoftness = 0.01f;

    private Material fillMaterial;
    private SpriteRenderer spriteRenderer;

    // 셰이더 프로퍼티 IDs
    
    private static readonly int FillAmountProperty = Shader.PropertyToID("_FillAmount");
    private static readonly int FillColorProperty = Shader.PropertyToID("_FillColor");
    private static readonly int BackgroundColorProperty = Shader.PropertyToID("_BackgroundColor");
    private static readonly int EdgeSoftnessProperty = Shader.PropertyToID("_EdgeSoftness");

    void Start()
    {
        targetScript = GetComponent<Dominate>();
        SetupMaterial();
    }


    void SetupMaterial()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        fillMaterial = spriteRenderer.material;
    }

    void Update()
    {
        UpdateShaderFromExternalScript();
    }

    void UpdateShaderFromExternalScript()
    {

        // 외부 스크립트에서 값 가져오기
        float percentage = targetScript.dm_percent;

        // 퍼센트를 0~1 범위로 정규화
        float normalizedFill = Mathf.Clamp01(percentage / 100f);

        Color fillColor = targetScript.currentColor;

            // 셰이더 프로퍼티 업데이트
            fillMaterial.SetFloat(FillAmountProperty, normalizedFill);
            fillMaterial.SetColor(FillColorProperty, fillColor);
            fillMaterial.SetColor(BackgroundColorProperty, backgroundColor);
            fillMaterial.SetFloat(EdgeSoftnessProperty, edgeSoftness);
        
    }

  

    // 디버그용 - 현재 읽어온 값들 출력
    

    void OnDestroy()
    {
        if (fillMaterial != null)
        {
            DestroyImmediate(fillMaterial);
        }
    }

}