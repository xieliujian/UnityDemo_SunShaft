using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GTM.URP.SunShaft
{
    public class SunShaftsPass : ScriptableRenderPass
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly int SunPosition = Shader.PropertyToID("_SunPosition");

        /// <summary>
        /// 
        /// </summary>
        private static readonly int BlurStep = Shader.PropertyToID("_BlurStep");
        
        /// <summary>
        /// 
        /// </summary>
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");

        /// <summary>
        /// 
        /// </summary>
        private static readonly int ShaftsColor = Shader.PropertyToID("_ShaftsColor");

        /// <summary>
        /// 
        /// </summary>
        private static readonly int SunThresholdSky = Shader.PropertyToID("_SunThresholdSky");

        /// <summary>
        /// 
        /// </summary>
        private static readonly int SkyNoiseScale = Shader.PropertyToID("_SkyNoiseScale");

        /// <summary>
        /// 
        /// </summary>
        private static readonly int UseStencilMaskTex = Shader.PropertyToID("_UseStencilMaskTex");

        /// <summary>
        /// 
        /// </summary>
        public static readonly int stencilMaskTex = Shader.PropertyToID("_StencilMaskTex");

        /// <summary>
        /// 
        /// </summary>
        const string COMMAND_BUFFER_NAME = "ShaftsRendering";

        /// <summary>
        /// 
        /// </summary>
        SunShaftsProperties m_Props;

        /// <summary>
        /// 
        /// </summary>
        RenderTargetIdentifier m_Source;

        /// <summary>
        /// 
        /// </summary>
        RenderTargetHandle m_Destination;

        /// <summary>
        /// 
        /// </summary>
        RenderTargetHandle m_TmpSkyColorTarget;

        /// <summary>
        /// 
        /// </summary>
        RenderTargetHandle m_TmpBlurTarget1;

        /// <summary>
        /// 
        /// </summary>
        RenderTargetHandle m_TmpBlurTarget2;

        /// <summary>
        /// 
        /// </summary>
        RenderTargetHandle m_TmpDestination;

        /// <summary>
        /// 
        /// </summary>
        RenderTargetHandle m_TemHandle;

        /// <summary>
        /// 
        /// </summary>
        public RenderPassEvent originRenderPassEvent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SunShaftsPass(SunShaftsProperties props)
        {
            m_Props = props;

            m_TmpSkyColorTarget.Init("_TmpSkyColorTex");

            m_TmpDestination.Init("_TmpDestinationBuffer");

            m_TmpBlurTarget1.Init("_TmpBlurTex1");
            m_TmpBlurTarget2.Init("_TmpBlurTex2");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination)
        {
            m_Source = source;
            m_Destination = destination;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ExecuteRender(context, ref renderingData);
        }

        /// <summary>
        /// 
        /// </summary>
        void ExecuteRender(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Props == null)
                return;

            bool canrender = m_Props.CanVolumeRender();
            if (!canrender)
                return;

            if (!m_Props.forceOn)
            {
                bool isvollightopen = m_Props.IsVolumeLightSwitchOpen();
                if (!isvollightopen)
                    return;
            }

            var camera = renderingData.cameraData.camera;
            if (camera == null)
                return;

            Vector3 sunScreenPoint;
            var cansunrender = m_Props.CanSunRender(camera, out sunScreenPoint);
            if (!cansunrender)
                return;

            if (m_Props.useRenderPassEvent)
            {
                renderPassEvent = m_Props.renderPassEvent;
            }
            else
            {
                renderPassEvent = originRenderPassEvent;
            }

            if (m_Props != null)
            {
                m_Props.CacheAllMaterial();
            }

            bool isallmatload = IsAllMaterialLoad();
            if (!isallmatload)
                return;

            var cmd = CommandBufferPool.Get(COMMAND_BUFFER_NAME);
            cmd.Clear();

            var cameraTargetDescriptor = ModifyCameraTargetDescriptor(renderingData.cameraData.cameraTargetDescriptor,
                out bool isModifyDescriptor);

            InitTmpTextures(cmd, cameraTargetDescriptor);

            //2. Blit sky cutted off by geometry
            m_Props.buildSkyMaterial.SetVector(SunPosition, sunScreenPoint);
            m_Props.buildSkyMaterial.SetFloat(SunThresholdSky, m_Props.sunThresholdSky);
            m_Props.buildSkyMaterial.SetFloat(SkyNoiseScale, m_Props.skyNoiseScale);
            Blit(cmd, m_Source, m_TmpBlurTarget1.Identifier(), m_Props.buildSkyMaterial);

            //Blit(cmd, m_TmpSkyColorTarget.Identifier(), m_TmpBlurTarget1.Identifier());

            //2. Blur iteratively
            var radius = m_Props.blurRadius / m_Props.radiusDivider;
            const int shaderBlurIterationsCount = 6;
            var iterationScaler = (float)shaderBlurIterationsCount / m_Props.radiusDivider;
            m_Props.blurMaterial.SetVector(SunPosition, sunScreenPoint);

            for (int i = 0; i < m_Props.blurStepsCount; i++)
            {
                m_Props.blurMaterial.SetFloat(BlurStep, radius);
                Blit(cmd, m_TmpBlurTarget1.Identifier(), m_TmpBlurTarget2.Identifier(), m_Props.blurMaterial);

                radius = m_Props.blurRadius * (i * 2f + 1f) * iterationScaler;
                m_Props.blurMaterial.SetFloat(BlurStep, radius);
                Blit(cmd, m_TmpBlurTarget2.Identifier(), m_TmpBlurTarget1.Identifier(), m_Props.blurMaterial);

                radius = m_Props.blurRadius * (i * 2f + 2f) * iterationScaler;
            }

            m_Props.finalBlendMaterial.SetFloat(Intensity, m_Props.intensity);
            var shaftsColor = m_Props.useSunLightColor && RenderSettings.sun ? RenderSettings.sun.color : m_Props.shaftsColor;
            m_Props.finalBlendMaterial.SetColor(ShaftsColor, shaftsColor);

            // 设置是否使用Mask Tex
            m_Props.finalBlendMaterial.SetFloat(UseStencilMaskTex, m_Props.useStencilMaskTex ? 1f : 0f);

            // 设置角色模板材质
            cmd.SetGlobalTexture(stencilMaskTex, stencilMaskTex);

            if (m_Destination == RenderTargetHandle.CameraTarget)
            {
                var cameraDesc = cameraTargetDescriptor;
                cameraDesc.depthBufferBits = 0;

                cmd.GetTemporaryRT(m_TmpDestination.id, cameraDesc, m_Props.filterMode);

                m_TemHandle = m_TmpDestination;

                Blit(cmd, m_Source, m_TemHandle.Identifier(), m_Props.finalBlendMaterial);
                Blit(cmd, m_TemHandle.Identifier(), m_Source);
            }
            else
            {
                Blit(cmd, m_Source, m_Destination.Identifier(), m_Props.finalBlendMaterial);
            }

            CleanupTmpTextures(cmd);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <summary>
        /// 
        /// </summary>
        void CleanupTmpTextures(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_TmpBlurTarget1.id);
            cmd.ReleaseTemporaryRT(m_TmpBlurTarget2.id);
            cmd.ReleaseTemporaryRT(m_TmpSkyColorTarget.id);
        }

        /// <summary>
        /// 
        /// </summary>
        bool IsAllMaterialLoad()
        {
            if (m_Props == null)
                return false;

            if (m_Props.buildSkyMaterial == null)
                return false;

            if (m_Props.blurMaterial == null)
                return false;

            if (m_Props.finalBlendMaterial == null)
                return false;

            return true;
        }

        /// <summary>
        /// 修改RenderTextureDescriptor
        /// </summary>
        /// <param name="cameraTargetDescriptor"></param>
        /// <returns></returns>
        RenderTextureDescriptor ModifyCameraTargetDescriptor(RenderTextureDescriptor cameraTargetDescriptor, out bool isModifyDescriptor)
        {
            isModifyDescriptor = false;

            float renderScale = 1f;
            var urpAsset = UniversalRenderPipeline.asset;
            if (urpAsset != null)
            {
                if (urpAsset.renderScale > 1)
                {
                    isModifyDescriptor = true;
                    renderScale = urpAsset.renderScale;

                    cameraTargetDescriptor.width = (int)(cameraTargetDescriptor.width / renderScale);
                    cameraTargetDescriptor.height = (int)(cameraTargetDescriptor.height / renderScale);

                    if (cameraTargetDescriptor.width % 2 != 0)
                    {
                        cameraTargetDescriptor.width += 1;
                    }

                    if (cameraTargetDescriptor.height % 2 != 0)
                    {
                        cameraTargetDescriptor.height += 1;
                    }
                }
            }

            return cameraTargetDescriptor;
        }

        /// <summary>
        /// 
        /// </summary>
        void InitTmpTextures(CommandBuffer cmd, RenderTextureDescriptor cameraTargetDescriptor)
        {
            var opaqueDesc = cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;
            opaqueDesc.msaaSamples = 1;

            var dstWidth = opaqueDesc.width >> m_Props.depthDownscalePow2;
            var dstHeight = opaqueDesc.height >> m_Props.depthDownscalePow2;
            cmd.GetTemporaryRT(m_TmpBlurTarget1.id, dstWidth, dstHeight, 0, m_Props.filterMode, cameraTargetDescriptor.colorFormat);
            cmd.GetTemporaryRT(m_TmpBlurTarget2.id, dstWidth, dstHeight, 0, m_Props.filterMode, cameraTargetDescriptor.colorFormat);

            cmd.GetTemporaryRT(m_TmpSkyColorTarget.id, dstWidth, dstHeight, 0, m_Props.filterMode, cameraTargetDescriptor.colorFormat);
        }
    }
}