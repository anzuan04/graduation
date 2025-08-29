Shader "Custom/TilemapBackground"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _Brightness("Brightness", Range(0, 2)) = 1
        _Contrast("Contrast", Range(0, 2)) = 1
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

        SubShader
        {
            Tags
            {
                "RenderType" = "Opaque"
                "Queue" = "Background"
                "IgnoreProjector" = "True"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Pass
            {
                Name "TilemapLit"

                Blend Off
                ZWrite On
                ZTest LEqual
                Cull Off

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma multi_compile_instancing
                #pragma multi_compile_local _ PIXELSNAP_ON
                #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
                #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                TEXTURE2D(_AlphaTex);
                SAMPLER(sampler_AlphaTex);

                CBUFFER_START(UnityPerMaterial)
                    float4 _MainTex_ST;
                    half4 _Color;
                    half _Brightness;
                    half _Contrast;
                    half _EnableExternalAlpha;
                CBUFFER_END

                struct Attributes
                {
                    float4 positionOS : POSITION;
                    float2 uv : TEXCOORD0;
                    float4 color : COLOR;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 color : COLOR;
                    float3 positionWS : TEXCOORD1;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                Varyings vert(Attributes input)
                {
                    Varyings output = (Varyings)0;
                    UNITY_SETUP_INSTANCE_ID(input);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                    output.positionCS = vertexInput.positionCS;
                    output.positionWS = vertexInput.positionWS;

                    output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                    output.color = input.color * _Color;

                    #ifdef PIXELSNAP_ON
                        output.positionCS = UnityPixelSnap(output.positionCS);
                    #endif

                    return output;
                }

                half4 frag(Varyings input) : SV_Target
                {
                    // 메인 텍스처 샘플링
                    half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                    // External Alpha 처리 (타일맵에서 사용)
                    #if ETC1_EXTERNAL_ALPHA
                        half4 alpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, input.uv);
                        color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
                    #endif

                        // 버텍스 컬러와 틴트 컬러 적용
                        color *= input.color;

                        // 라이팅 없이 기본 색상만
                        color.a = 1.0; // 배경은 완전 불투명

                        return color;
                    }
                    ENDHLSL
                }

                Pass
                {
                    Name "TilemapUnlit"
                    Tags { "LightMode" = "UniversalForward" }

                    Blend Off
                    ZWrite On
                    ZTest LEqual
                    Cull Off

                    HLSLPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma target 2.0
                    #pragma multi_compile_instancing
                    #pragma multi_compile_local _ PIXELSNAP_ON
                    #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                    TEXTURE2D(_MainTex);
                    SAMPLER(sampler_MainTex);
                    TEXTURE2D(_AlphaTex);
                    SAMPLER(sampler_AlphaTex);

                    CBUFFER_START(UnityPerMaterial)
                        float4 _MainTex_ST;
                        half4 _Color;
                        half _Brightness;
                        half _Contrast;
                        half _EnableExternalAlpha;
                    CBUFFER_END

                    struct Attributes
                    {
                        float4 positionOS : POSITION;
                        float2 uv : TEXCOORD0;
                        float4 color : COLOR;
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                    };

                    struct Varyings
                    {
                        float4 positionCS : SV_POSITION;
                        float2 uv : TEXCOORD0;
                        float4 color : COLOR;
                        UNITY_VERTEX_OUTPUT_STEREO
                    };

                    Varyings vert(Attributes input)
                    {
                        Varyings output = (Varyings)0;
                        UNITY_SETUP_INSTANCE_ID(input);
                        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                        output.positionCS = vertexInput.positionCS;
                        output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                        output.color = input.color * _Color;

                        #ifdef PIXELSNAP_ON
                            output.positionCS = UnityPixelSnap(output.positionCS);
                        #endif

                        return output;
                    }

                    half4 frag(Varyings input) : SV_Target
                    {
                        half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                        #if ETC1_EXTERNAL_ALPHA
                            half4 alpha = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, input.uv);
                            color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
                        #endif

                        color *= input.color;
                        color.rgb = ((color.rgb - 0.5) * _Contrast + 0.5) * _Brightness;
                        color.a = 1.0;

                        return color;
                    }
                    ENDHLSL
                }
        }

            Fallback "Sprites/Default"
}