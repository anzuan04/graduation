Shader "CustomRenderTexture/Enemy_shadow"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (255,255,255,0)
    }
        SubShader
        {
            Tags { "QUEUE" = "Transparent" "RenderType" = "Transparent" }
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                float4 _Color;

                float4 _PlayerPos;
                float4 _PlayerDir;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float3 worldPos : TEXCOORD1;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    return o;
                }

                float frag_angle(float2 dirA, float2 dirB)
                {
                    float dotProd = dot(dirA, dirB);
                    float crossProd = dirA.x * dirB.y - dirA.y * dirB.x;
                    return atan2(crossProd, dotProd);
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 toObj = normalize(i.worldPos.xy - _PlayerPos.xy);
                    float2 forward = normalize(_PlayerDir.xy);

                    
                    float angle = frag_angle(forward, toObj); 

                    float absAngleDeg = abs(degrees(angle)); 

                   
                    float fade = 1.0 - smoothstep(60, 100, absAngleDeg);

                    float4 texColor = tex2D(_MainTex, i.uv) * _Color;

                    texColor.rgb *= fade;

                    return texColor;
                }
                ENDCG
            }
        }
}
