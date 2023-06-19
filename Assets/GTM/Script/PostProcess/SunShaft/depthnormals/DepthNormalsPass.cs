using LR.URP.PPExtensions.sunshafts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LR.URP.PPExtensions.depthnormals
{
    public class DepthNormalsPass : ScriptableRenderPass
    {
        private DepthNormalsInput props;
        private SunShaftsProperties mainProps;
        private RenderTargetHandle destination;
        private RenderTargetIdentifier source;

        private List<ShaderTagId> shaderTagIds = new List<ShaderTagId>
        {
            new ShaderTagId("DepthOnly")
        };

        public DepthNormalsPass(DepthNormalsInput props, SunShaftsProperties mainProps)
        {
            this.props = props;
            this.mainProps = mainProps;

            if (string.IsNullOrEmpty(props.textureName))
            {
                destination = RenderTargetHandle.CameraTarget;
            }
            else
            {
                destination.Init(props.textureName);
            }
        }

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var cameraSpec = cameraTextureDescriptor;
            cameraSpec.depthBufferBits = 32;
            cameraSpec.colorFormat = RenderTextureFormat.ARGB32;
            cmd.GetTemporaryRT(destination.id, cameraSpec, FilterMode.Point);
            
            ConfigureTarget(destination.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (mainProps == null)
                return;

            bool canrender = mainProps.CanVolumeRender();
            if (!canrender)
                return;
            
            if (!mainProps.forceOn)
            {
                bool isvollightopen = mainProps.IsVolumeLightSwitchOpen();
                if (!isvollightopen)
                    return;
            }

            var camera = renderingData.cameraData.camera;
            if (camera == null)
                return;

            Vector3 sunScreenPoint;
            var cansunrender = mainProps.CanSunRender(camera, out sunScreenPoint);
            if (!cansunrender)
                return;

            bool isallmatload = IsAllMaterialLoad();
            if (!isallmatload)
                return;

            var drawingSettings = CreateDrawingSettings(
                shaderTagIds,
                ref renderingData, 
                renderingData.cameraData.defaultOpaqueSortFlags
            );

            drawingSettings.overrideMaterial = mainProps.renderDepthNormalsMaterial;
            drawingSettings.overrideMaterialPassIndex = 0;
                
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque, props.layers);
            var rsb = new RenderStateBlock(RenderStateMask.Nothing);

            var cmd = CommandBufferPool.Get("DepthNormalsPass");
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            using (new ProfilingScope(cmd, new ProfilingSampler("render scene")))
            {
                context.DrawRenderers(renderingData.cullResults, 
                    ref drawingSettings,
                    ref filterSettings,
                    ref rsb);
                cmd.SetGlobalTexture(destination.id, destination.Identifier());
            }

            if (props.debugBlitToColorTarget)
            {
                Blit(cmd, destination.Identifier(), source);
            }
            else
            {
                cmd.SetRenderTarget(source);
            }

            context.ExecuteCommandBuffer(cmd);

            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (destination != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(destination.id);
            }
        }

        private bool IsAllMaterialLoad()
        {
            if (mainProps == null)
                return false;

            if (mainProps.renderDepthNormalsMaterial == null)
                return false;

            return true;
        }
    }
}