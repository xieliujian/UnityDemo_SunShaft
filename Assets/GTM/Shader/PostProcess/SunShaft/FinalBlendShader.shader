Shader "GTM/PostProcess/SunShaft/FinalBlendShader"
{
    Properties
    {
        [NoScaleOffset] _MainTex("Main Texture", 2D) = "black" {}
        [NoScaleOffset] _StencilMaskTex("Stencil Mask Tex", 2D) = "black" {}
        _Intensity("Intensity", Range(0, 1)) = 0.1
        _UseStencilMaskTex("_UseStencilMaskTex", Float) = 0
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

            #pragma enable_d3d11_debug_symbols

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
                float4 _StencilMaskTex_TexelSize;
                float4 _TmpBlurTex1_TexelSize;
                float4 _ShaftsColor;
                float _Intensity;
                float _UseStencilMaskTex;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_StencilMaskTex);
            SAMPLER(sampler_StencilMaskTex);

            TEXTURE2D(_TmpBlurTex1);
            SAMPLER(sampler_TmpBlurTex1);

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
                float4 shaftTexColor = SAMPLE_TEXTURE2D(_TmpBlurTex1, sampler_TmpBlurTex1, uv.xy);
                shaftTexColor *= _Intensity;
                shaftTexColor = saturate(shaftTexColor) * _ShaftsColor;

                //
                float4 maskTexColor = SAMPLE_TEXTURE2D(_StencilMaskTex, sampler_StencilMaskTex, uv.xy);
                float maskVal = saturate(1 - saturate(maskTexColor));
                maskVal = _UseStencilMaskTex * maskVal + (1 - _UseStencilMaskTex);

                shaftTexColor *= maskVal;

                //
                float4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv.xy);

                // 
                float4 color = mainTexColor + shaftTexColor;
                //color = shaftTexColor;

                return color;
            }

            ENDHLSL
        }
    }
}
