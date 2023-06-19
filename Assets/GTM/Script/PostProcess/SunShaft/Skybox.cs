using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LR.URP.PPExtensions
{
    [Serializable, VolumeComponentMenu("Post-processing/Skybox")]
    public class Skybox : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Enable/Disable")]
        public BoolParameter on = new BoolParameter(false, false);

        //public ClampedFloatParameter tiling = new ClampedFloatParameter(0.1f, 0f, 1f);

        public ColorParameter skyColor = new ColorParameter(new Color(13f / 255f, 
            103f / 255f, 230f / 255f, 1f), true, true, false);

        public ColorParameter cloudColor1 = new ColorParameter(new Color(149f / 255f, 
            190f / 255f, 231f / 255f, 0f), true, true, false);

        public ClampedFloatParameter cloudTranStrength1 = new ClampedFloatParameter(1f, 0f, 1f);

        public ColorParameter cloudColor2 = new ColorParameter(new Color(197f / 255f, 
            217f / 255f, 237f / 255f, 0f), true, true, false);

        public ClampedFloatParameter cloudTranStrength2 = new ClampedFloatParameter(0.81f, 0f, 1f);

        public ColorParameter cloudColor3 = new ColorParameter(new Color(196f / 255f,
            227f / 255f, 255f / 255f, 0f), true, true, false);

        public ClampedFloatParameter cloudTranStrength3 = new ClampedFloatParameter(0.902f, 0f, 1f);

        //public Vector4Parameter wind = new Vector4Parameter(new Vector4(0.3f, 0f, 0f, 0f));

        public FloatParameter noiseCloud = new FloatParameter(1.5f);

        public FloatParameter noiseCloudness = new FloatParameter(0.99f);

        public FloatParameter cloudHeightFalloff = new FloatParameter(1.97f);

        public ColorParameter topColor = new ColorParameter(new Color(85f / 255f,
            200f / 255f, 255f / 255f, 0f), true, true, false);

        public ColorParameter middleColor = new ColorParameter(new Color(1f,
            0.3349057f, 0.3349057f, 0f), true, true, false);

        public ColorParameter cloudHeightColor = new ColorParameter(new Color(0f / 255f,
            0f / 255f, 0f / 255f, 0f), true, true, false);

        // 云层渐变添加参数
        public ClampedFloatParameter topRangeMove = new ClampedFloatParameter(1f, -1f, 1f);

        public ClampedFloatParameter topRange = new ClampedFloatParameter(2.49f, 0f, 10f);

        public ClampedFloatParameter bottomRangeMove = new ClampedFloatParameter(-0.001f, -1f, 1f);

        public ClampedFloatParameter bottomRange = new ClampedFloatParameter(2.11f, 0f, 10f);

        // Azure
        public FloatParameter azure_SunTextureSize = new FloatParameter(1f);

        public FloatParameter azure_MoonTextureSize = new FloatParameter(5f);

        public ClampedFloatParameter azure_MoonIntensity = new ClampedFloatParameter(0f, 0f, 1f);

        public ClampedFloatParameter azure_StarIntensity = new ClampedFloatParameter(0f, 0f, 1f);

        public ClampedFloatParameter azure_SunIntensity = new ClampedFloatParameter(0f, 0f, 1f);

        public FloatParameter azure_MoonTextureIntensity = new FloatParameter(1f);

        public FloatParameter azure_StarsTextureIntensity = new FloatParameter(1f);

        public FloatParameter azure_MilkyWayIntensity = new FloatParameter(1f);

        [Header("星星被云层的遮罩强度")]
        public FloatParameter azure_StarsMaskIntensity = new FloatParameter(8f);

        public FloatParameter azure_SunTextureIntensity = new FloatParameter(1f);

        public ColorParameter azure_MoonTextureColor = new ColorParameter(Color.white);

        public ColorParameter azure_SunTextureColor = new ColorParameter(Color.white);

        [Header("光晕效果1")]
        public FloatParameter azure_MieColorIntensity = new FloatParameter(0f);

        public ColorParameter azure_MieColor = new ColorParameter(Color.white);

        // 
        [Header("光晕效果2")]
        public ColorParameter moonColor = new ColorParameter(Color.black, true, true, false);

        public ClampedFloatParameter moonkuangdu = new ClampedFloatParameter(0.4462404f, 0f, 1f);

        public ClampedFloatParameter moonbanjing = new ClampedFloatParameter(0.7789788f, 0f, 1f);

        public ClampedFloatParameter moonrouhedu = new ClampedFloatParameter(0.7165735f, 0f, 1f);

        public bool IsActive()
        {
            return (bool)on;
        }

        public bool IsTileCompatible() => false;
    }

}
