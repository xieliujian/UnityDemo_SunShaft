﻿
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GTM.URP.SunShaft
{
    public class SunShaftsFeatureV2 : ScriptableRendererFeature
    {
        public static readonly string buildSkyShaderName = "LingRen/Urp/PPExtensions/BuildSkyForBlurShader";
        public static readonly string mixSkyDepthShaderName = "LingRen/Urp/PPExtensions/MixSkyDepthShader";
        public static readonly string blurShaderName = "LingRen/Urp/PPExtensions/DirectionalBlurShader";
        public static readonly string finalBlendShaderName = "LingRen/Urp/PPExtensions/FinalBlendShader";
        public static readonly string depthNormalsTextureName = "_SunShaftsDepthNormals";

        /// <summary>
        /// 
        /// </summary>
        SunShaftsPass m_ShaftsPass;

        /// <summary>
        /// 
        /// </summary>
        public SunShaftsProperties props;

        /// <summary>
        /// 
        /// </summary>
        public RenderPassEvent shaftsPassEvent = RenderPassEvent.AfterRenderingTransparents;

        /// <summary>
        /// 
        /// </summary>
        public override void Create()
        {
            if (props == null)
            {
                props = new SunShaftsProperties();
            }
            
            m_ShaftsPass = new SunShaftsPass(props)
            {
                renderPassEvent = shaftsPassEvent,
                originRenderPassEvent = shaftsPassEvent,
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (props == null)
                return;

            props.CacheSunShafts();
            if (props.sunShafts == null)
                return;

            props.CopyVolumeProperty();

            var camera = renderingData.cameraData.camera;
            if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
                return;

            RenderTargetIdentifier cameraColorTarget = renderer.cameraColorTarget;
            m_ShaftsPass.Setup(cameraColorTarget, RenderTargetHandle.CameraTarget);
            renderer.EnqueuePass(m_ShaftsPass);
        }
    }
}