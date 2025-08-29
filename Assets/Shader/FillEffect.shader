Shader "Custom/FillEffect"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _FillAmount("Fill Amount", Range(0, 1)) = 0.5
        _FillColor("Fill Color", Color) = (0, 0, 1, 1)
        _BackgroundColor("Background Color", Color) = (0, 0, 0, 0)
        _EdgeSoftness("Edge Softness", Range(0, 0.1)) = 0.01
        _BorderWidth("Border Width", Range(0, 0.1)) = 0.02  // 테두리 두께 추가
        _BorderColor("Border Color", Color) = (1, 1, 1, 1)  // 테두리 색상 추가
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
                    // 텍스처 샘플링
                    fixed4 texColor = tex2D(_MainTex, IN.texcoord);

                // UV 좌표 기반으로 fill 계산 (아래에서 위로)
                float fillProgress = _FillAmount;
                float uvY = IN.texcoord.y;

                // 테두리 검사 - 스프라이트 경계에서 테두리 두께만큼
                float2 uv = IN.texcoord;
                float borderMask = 0;

                // 상하좌우 경계 검사
                if (uv.x < _BorderWidth || uv.x >(1.0 - _BorderWidth) ||
                    uv.y < _BorderWidth || uv.y >(1.0 - _BorderWidth))
                {
                    borderMask = 1.0;
                }

                // Fill 경계선 검사
                
                // 부드러운 전환을 위한 smoothstep 사용
                float fillMask = smoothstep(fillProgress,
                                          fillProgress, uvY);

                fixed4 finalColor;

                // 테두리 우선 처리
                if (borderMask > 0.5 )
                {
                    // 테두리 영역
                    finalColor = _BorderColor;
                }
                else if (fillMask < 0.5)
                {
                    
                        finalColor = _FillColor;
                    
                }
                else
                {
                    
                        discard; // 완전히 투명하게
                    
                }

                // 원본 텍스처와 블렌딩 (텍스처의 알파값 고려)
                finalColor.rgb *= texColor.rgb;
                finalColor.a *= texColor.a;

                // 버텍스 컬러 적용
                finalColor *= IN.color;

                // 알파 프리멀티플라이
                finalColor.rgb *= finalColor.a;

                return finalColor;
            }
            ENDCG
        }
        }
}