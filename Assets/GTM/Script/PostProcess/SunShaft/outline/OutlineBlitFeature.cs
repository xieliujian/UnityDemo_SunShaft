using LR.URP.PPExtensions.common;
using UnityEngine.Rendering.Universal;

namespace LR.URP.PPExtensions.outline
{
    public class OutlineBlitFeature : ScriptableRendererFeature
    {
        public OutlineInput props;
        public RenderPassEvent renderPassEvent;
        
        private RenderTargetHandle sourceTexture;
        private OutlineBlitPass pass;

        public override void Create()
        {
            if (props == null)
            {
                return;
            }
            
            InitMaterials();
            
            pass = new OutlineBlitPass(props)
            {
                renderPassEvent = renderPassEvent
            };

            if (!string.IsNullOrEmpty(props.sourceTextureName))
            {
                sourceTexture.Init(props.sourceTextureName);
            }
        }

        private void InitMaterials()
        {
            props.outlineMaterial = PPUtils.GetShaderMaterial(props.outlineShader, 
                "Shader Graphs/OutlineShader",
                out props.outlineShader);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (props == null)
            {
                return;
            }
            
            //Case of scene destroyed
            if (!props.outlineMaterial)
            {
                InitMaterials();
                return;
            }
            
            var sourceIdentifier = string.IsNullOrEmpty(props.sourceTextureName)
                ? renderer.cameraColorTarget
                : sourceTexture.Identifier();
            
            pass.Setup(sourceIdentifier, renderer.cameraColorTarget);
            renderer.EnqueuePass(pass);
        }
    }
}