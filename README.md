
# SunShaft光线

![github](https://github.com/xieliujian/UnityDemo_SunShaft/blob/main/Video/1.png?raw=true)

## 第一步

采样光线颜色

![github](https://github.com/xieliujian/UnityDemo_SunShaft/blob/main/Video/2.png?raw=true)

Shader片段

```cs

float4 frag(Varyings input) : SV_Target
{
    float4 uv = input.uv;
    float4 screenPos = input.ScreenPosition;

    // Distance from Sun
    float disFromSun = length(_SunPosition.xy - uv.xy);

    // Limit for SkyBox by Sun Distance
    float limitSkyBySunDis = saturate(_SunThresholdSky - disFromSun);

    // 
    float sceneDepth = Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(screenPos.xy / screenPos.w), _ZBufferParams);
    float sceneDepthComp = (sceneDepth >= 0.99) ? 1 : 0;

    limitSkyBySunDis *= sceneDepthComp;

    // 
    float4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv.xy);
    float noiseVal;
    Unity_SimpleNoise_float(uv.xy, _SkyNoiseScale, noiseVal);
    mainTexColor *= noiseVal;

    float4 color = mainTexColor * limitSkyBySunDis;

    return color;
}

```

## 第二步

径向模糊

![github](https://github.com/xieliujian/UnityDemo_SunShaft/blob/main/Video/3.png?raw=true)

Shader片段

```cs

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

```

## 第三步

和场景图混合

![github](https://github.com/xieliujian/UnityDemo_SunShaft/blob/main/Video/4.png?raw=true)

Shader片段

```cs

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

```
