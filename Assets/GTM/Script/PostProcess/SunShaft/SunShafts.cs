using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GTM.URP.SunShaft
{
    /// <summary>
    /// A <see cref="VolumeParameter"/> that holds a <c>LayerMask</c> value.
    /// </summary>
    [Serializable, System.Diagnostics.DebuggerDisplay(k_DebuggerDisplay)]
    public class RenderPassEventParameter : VolumeParameter<RenderPassEvent>
    {
        /// <summary>
        /// Creates a new <see cref="RenderPassEventParameter"/> instance.
        /// </summary>
        /// <param name="value">The initial value to store in the parameter.</param>
        /// <param name="overrideState">The initial override state for the parameter.</param>
        public RenderPassEventParameter(RenderPassEvent value, bool overrideState = false)
            : base(value, overrideState) { }
    }

    [Serializable, VolumeComponentMenu("Post-processing/SunShafts")]
    public class SunShafts : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("Enable/Disable")]
        public BoolParameter on = new BoolParameter(false, false);

        [Tooltip("Enable/Disable")]
        public BoolParameter forceOn = new BoolParameter(false, false);

        [Header("Render Pass Event")]
        public BoolParameter useRenderPassEvent = new BoolParameter(false);

        public RenderPassEventParameter renderPassEvent = new RenderPassEventParameter(RenderPassEvent.AfterRenderingSkybox);

        [Header("Use Stencil Mask Tex")]
        public BoolParameter useStencilMaskTex = new BoolParameter(false);

        [Header("Common params")]
        [Tooltip("SunShafts intensity")]
        public ClampedFloatParameter intensity = new ClampedFloatParameter(1.5f, 0f, 5f);

        [Header("Sun params")]
        public BoolParameter useSunLightColor = new BoolParameter(true);
        [ColorUsage(false, true)]
        public ColorParameter shaftsColor = new ColorParameter(Color.black);

        [Header("Sun Position")]
        public BoolParameter useSunPosition = new BoolParameter(false);

        [Tooltip("Can be used for fake Sun Source, which is differ from main Sun Light - source of shadows")]
        public Vector3Parameter sunPosition = new Vector3Parameter(Vector3.zero);

        [Tooltip("SunShafts sunThresholdSky")]
        public ClampedFloatParameter sunThresholdSky = new ClampedFloatParameter(0.75f, 0f, 1f);
        [Tooltip("SunShafts sunThresholdDepth")]
        public ClampedFloatParameter sunThresholdDepth = new ClampedFloatParameter(0.75f, 0f, 1f);

        [Header("Blur params")]
        [Tooltip("SunShafts depthDownscalePow2")]
        public ClampedIntParameter depthDownscalePow2 = new ClampedIntParameter(3, 0, 4);
        [Tooltip("SunShafts blurRadius")]
        public FloatParameter blurRadius = new FloatParameter(1.2f);
        [Tooltip("SunShafts blurStepsCount")]
        public ClampedIntParameter blurStepsCount = new ClampedIntParameter(2, 1, 4);

        public bool IsActive()
        {
            return (bool)on;
        }

        public bool IsTileCompatible() => false;
    }
}

