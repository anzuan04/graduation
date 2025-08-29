Shader "Custom/FillEffect"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _FillAmount("Fill Amount", Range(0, 1)) = 0.5
        _FillColor("Fill Color", Color) = (0, 0, 1, 1)
        _BackgroundColor("Background Color", Color) = (0, 0, 0, 0)
        _EdgeSoftness("Edge Softness", Range(0, 0.1)) = 0.01
        _BorderWidth("Border Width", Range(0, 0.1)) = 0.02  // �׵θ� �β� �߰�
        _BorderColor("Border Color", Color) = (1, 1, 1, 1)  // �׵θ� ���� �߰�
    }
        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }
            Cull Off
            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _FillAmount;
                fixed4 _FillColor;
                fixed4 _BackgroundColor;
                float _EdgeSoftness;
                float _BorderWidth;
                fixed4 _BorderColor;

                v2f vert(appdata_t IN)
                {
                    v2f OUT;
                    OUT.vertex = UnityObjectToClipPos(IN.vertex);
                    OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                    OUT.color = IN.color;
                    OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);
                    return OUT;
                }

                fixed4 frag(v2f IN) : SV_Target
                {
                    // �ؽ�ó ���ø�
                    fixed4 texColor = tex2D(_MainTex, IN.texcoord);

                // UV ��ǥ ������� fill ��� (�Ʒ����� ����)
                float fillProgress = _FillAmount;
                float uvY = IN.texcoord.y;

                // �׵θ� �˻� - ��������Ʈ ��迡�� �׵θ� �β���ŭ
                float2 uv = IN.texcoord;
                float borderMask = 0;

                // �����¿� ��� �˻�
                if (uv.x < _BorderWidth || uv.x >(1.0 - _BorderWidth) ||
                    uv.y < _BorderWidth || uv.y >(1.0 - _BorderWidth))
                {
                    borderMask = 1.0;
                }

                // Fill ��輱 �˻�
                
                // �ε巯�� ��ȯ�� ���� smoothstep ���
                float fillMask = smoothstep(fillProgress,
                                          fillProgress, uvY);

                fixed4 finalColor;

                // �׵θ� �켱 ó��
                if (borderMask > 0.5 )
                {
                    // �׵θ� ����
                    finalColor = _BorderColor;
                }
                else if (fillMask < 0.5)
                {
                    
                        finalColor = _FillColor;
                    
                }
                else
                {
                    
                        discard; // ������ �����ϰ�
                    
                }

                // ���� �ؽ�ó�� ���� (�ؽ�ó�� ���İ� ���)
                finalColor.rgb *= texColor.rgb;
                finalColor.a *= texColor.a;

                // ���ؽ� �÷� ����
                finalColor *= IN.color;

                // ���� ������Ƽ�ö���
                finalColor.rgb *= finalColor.a;

                return finalColor;
            }
            ENDCG
        }
        }
}