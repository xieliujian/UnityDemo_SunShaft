Shader "GTM/PostProcess/SunShaft/DirectionalBlurShader"
{
    Properties
    {
        [NoScaleOffset] _MainTex("InputTexture", 2D) = "black" {}
        _SunPosition("SunPosition", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        LOD 100

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;
                float4 _SunPosition;
                float _BlurStep;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                float3 PosOS = input.positionOS.xyz;
                float3 positionWS = TransformObjectToWorld(PosOS);
                float4 positionCS = TransformWorldToHClip(positionWS);

                output.positionCS = positionCS;
                output.uv = input.uv;

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 uv = input.uv;

                // 
                float2 uvOffset = (_SunPosition.xy - uv.xy) * _BlurStep;

                // 1
                float4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv.xy);

                float2 uv1 = uv + uvOffset;
                float4 mainTexColor1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv1.xy);

                float2 uvOffset2 = uvOffset + uvOffset;
                float2 uv2 = uv + uvOffset2;
                float4 mainTexColor2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv2.xy);

                float2 uvOffset3 = uvOffset2 + uvOffset2;
                float2 uv3 = uv + uvOffset3;
                float4 mainTexColor3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv3.xy);

                float2 uvOffset4 = uvOffset3 + uvOffset3;
                float2 uv4 = uv + uvOffset4;
                float4 mainTexColor4 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv4.xy);

                float2 uvOffset5 = uvOffset4 + uvOffset4;
                float2 uv5 = uv + uvOffset5;
                float4 mainTexColor5 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv5.xy);

                float4 color1 = mainTexColor + mainTexColor1;
                float4 color2 = mainTexColor2 + mainTexColor3;
                float4 color3 = mainTexColor4 + mainTexColor5;

                float4 color4 = color1 + color2;
                float4 color5 = color2 + color3;

                float4 color6 = color4 + color5;

                float4 color = color6 / 6;

                return color;
            }

            ENDHLSL
        }
    }
}
