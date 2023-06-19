using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LR.URP.PPExtensions.outline
{
    public class OutlineBlitPass : ScriptableRenderPass
    {
        private static readonly int OutlineThickness = Shader.PropertyToID("_OutlineThickness");
        private static readonly int OutlineMultiplier = Shader.PropertyToID("_OutlineMultiplier");
        private static readonly int OutlineBias = Shader.PropertyToID("_OutlineBias");

        private OutlineInput props;
        private RenderTargetIdentifier source;
        private RenderTargetIdentifier destination;
        
        private RenderTargetHandle tmpColorBuffer;

        public OutlineBlitPass(OutlineInput props)
        {
            this.props = props;
        }

        public void Setup(RenderTargetIdentifier source, RenderTargetIdentifier destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("OutlineBlitPass");
            
            props.outlineMaterial.SetFloat(OutlineThickness, props.thickness);
            props.outlineMaterial.SetFloat(OutlineMultiplier, props.multiplier);
            props.outlineMaterial.SetFloat(OutlineBias, props.bias);

            if (source == destination)
            {
                var spec = renderingData.cameraData.cameraTargetDescriptor;
                spec.depthBufferBits = 0;
                cmd.GetTemporaryRT(tmpColorBuffer.id, spec, FilterMode.Bilinear);
                
                Blit(cmd, source, tmpColorBuffer.Identifier(), props.outlineMaterial);
                Blit(cmd, tmpColorBuffer.Identifier(), destination);
            }
            else
            {
                Blit(cmd, source, destination, props.outlineMaterial);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (source == destination)
            {
                cmd.ReleaseTemporaryRT(tmpColorBuffer.id);
            }
        }
    }
}