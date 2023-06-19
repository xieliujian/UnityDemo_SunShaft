using LR.Scene;
using LR.URP.PPExtensions.common;
using LR.URP.PPExtensions.depthnormals;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LR.URP.PPExtensions.sunshafts
{
    public class SunShaftsFeatureV2 : ScriptableRendererFeature
    {
        public static readonly string depthNormalsShaderName = "LingRen/Urp/PPExtensions/DepthNormalsShader";
        public static readonly string buildDepthNormalsShaderName = "LingRen/Urp/PPExtensions/BuildDepthNormalsForBlurShader";
        public static readonly string buildDepthShaderName = "LingRen/Urp/PPExtensions/BuildDepthForBlurShader";
        public static readonly string outlineShaderName = "LingRen/Urp/PPExtensions/OutlineShader";
        public static readonly string buildSkyShaderName = "LingRen/Urp/PPExtensions/BuildSkyForBlurShader";
        public static readonly string mixSkyDepthShaderName = "LingRen/Urp/PPExtensions/MixSkyDepthShader";
        public static readonly string blurShaderName = "LingRen/Urp/PPExtensions/DirectionalBlurShader";
        public static readonly string finalBlendShaderName = "LingRen/Urp/PPExtensions/FinalBlendShader";


        public static readonly string depthNormalsTextureName = "_SunShaftsDepthNormals";
        
        public SunShaftsProperties props;
        public RenderPassEvent normalsPassEvent = RenderPassEvent.BeforeRenderingPrepasses;
        public RenderPassEvent shaftsPassEvent = RenderPassEvent.AfterRenderingTransparents;
        
        private DepthNormalsPass depthNormalsPass;
        private SunShaftsPass shaftsPass;

        public override void Create()
        {
            if (props == null)
            {
                props = new SunShaftsProperties();
            }
            
            InitOnPropertiesUpdate(props);

            shaftsPass = new SunShaftsPass(props)
            {
                renderPassEvent = shaftsPassEvent,
                originRenderPassEvent = shaftsPassEvent,
            };
        }

        public void InitOnPropertiesUpdate(SunShaftsProperties props)
        {
            if (props.useDepthNormals)
            {
                depthNormalsPass = new DepthNormalsPass(new DepthNormalsInput()
                {
                    layers = props.normalsLayerMask,
                    textureName = depthNormalsTextureName,
                    debugBlitToColorTarget = false,
                }, props)
                {
                    renderPassEvent = normalsPassEvent,
                };
            }
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
            {
                return;
            }

            RenderTargetIdentifier cameraColorTarget = renderer.cameraColorTarget;

            if (props.useDepthNormals)
            {
                depthNormalsPass.Setup(cameraColorTarget);
                renderer.EnqueuePass(depthNormalsPass);
            }

            shaftsPass.Setup(cameraColorTarget, RenderTargetHandle.CameraTarget);
            renderer.EnqueuePass(shaftsPass);
        }
    }
}