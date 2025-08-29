using UnityEngine;

public class FillShaderEffect : MonoBehaviour
{
    [Header("External Script Reference")]
    [Tooltip("�̹� ������ ��ũ��Ʈ ������Ʈ�� ���⿡ �巡���ϼ���")]
    public Dominate targetScript;

    [Header("Visual Settings")]
    public Color backgroundColor = Color.clear;
    [Range(0f, 0.1f)] public float edgeSoftness = 0.01f;

    private Material fillMaterial;
    private SpriteRenderer spriteRenderer;

    // ���̴� ������Ƽ IDs
    
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

        // �ܺ� ��ũ��Ʈ���� �� ��������
        float percentage = targetScript.dm_percent;

        // �ۼ�Ʈ�� 0~1 ������ ����ȭ
        float normalizedFill = Mathf.Clamp01(percentage / 100f);

        Color fillColor = targetScript.currentColor;

            // ���̴� ������Ƽ ������Ʈ
            fillMaterial.SetFloat(FillAmountProperty, normalizedFill);
            fillMaterial.SetColor(FillColorProperty, fillColor);
            fillMaterial.SetColor(BackgroundColorProperty, backgroundColor);
            fillMaterial.SetFloat(EdgeSoftnessProperty, edgeSoftness);
        
    }

  

    // ����׿� - ���� �о�� ���� ���
    

    void OnDestroy()
    {
        if (fillMaterial != null)
        {
            DestroyImmediate(fillMaterial);
        }
    }

}