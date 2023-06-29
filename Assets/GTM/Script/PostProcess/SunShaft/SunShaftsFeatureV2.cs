
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GTM.URP.SunShaft
{
    public class SunShaftsFeatureV2 : ScriptableRendererFeature
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly string BUILD_SKY_SHADER_NAME = "GTM/PostProcess/SunShaft/BuildSkyForBlurShader";

        /// <summary>
        /// 
        /// </summary>
        public static readonly string BLUR_SHADER_NAME = "GTM/PostProcess/SunShaft/DirectionalBlurShader";

        /// <summary>
        /// 
        /// </summary>
        public static readonly string FINAL_BLEND_SHADER_NAME = "GTM/PostProcess/SunShaft/FinalBlendShader";
        //public static readonly string FINAL_BLEND_SHADER_NAME = "LingRen/Urp/PPExtensions/FinalBlendShader";

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