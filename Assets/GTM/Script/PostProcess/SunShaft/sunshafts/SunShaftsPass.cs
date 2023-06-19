using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// 优化
// 1. 去掉tmpFullSizeTex，直接使用_TmpBlurTex1参与计算
//    去掉这句话 Blit(cmd, tmpBlurTarget1.Identifier(), tmpFullSizeTex.Identifier());
// 2. 缩放tmpSkyColorTarget的尺寸
////cmd.GetTemporaryRT(tmpSkyColorTarget.id, opaqueDesc.width, opaqueDesc.height, 0, props.filterMode);
//cmd.GetTemporaryRT(tmpSkyColorTarget.id, dstWidth, dstHeight, 0, props.filterMode);


namespace LR.URP.PPExtensions.sunshafts
{
    public class SunShaftsPass : ScriptableRenderPass
    {
        private static readonly int SunPosition = Shader.PropertyToID("_SunPosition");
        private static readonly int DepthValueCutOff = Shader.PropertyToID("_DepthValueCutOff");
        private static readonly int BlurStep = Shader.PropertyToID("_BlurStep");
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int ShaftsColor = Shader.PropertyToID("_ShaftsColor");
        private static readonly int SunThresholdDepth = Shader.PropertyToID("_SunThresholdDepth");
        private static readonly int SunThresholdSky = Shader.PropertyToID("_SunThresholdSky");
        private static readonly int SkyNoiseScale = Shader.PropertyToID("_SkyNoiseScale");

        private static readonly int UseStencilMaskTex = Shader.PropertyToID("_UseStencilMaskTex");

        private static readonly int OutlineThickness = Shader.PropertyToID("_OutlineThickness");
        private static readonly int OutlineMultiplier = Shader.PropertyToID("_OutlineMultiplier");
        private static readonly int OutlineBias = Shader.PropertyToID("_OutlineBias");

        public static readonly int stencilMaskTex = Shader.PropertyToID("_StencilMaskTex");

        const string COMMAND_BUFFER_NAME = "ShaftsRendering";

        private SunShaftsProperties props;

        private RenderTargetIdentifier source;
        private RenderTargetHandle destination;

        private RenderTargetHandle depthNormalsSource;
        private RenderTargetHandle tmpDepthColorTarget;
        private RenderTargetHandle tmpSkyColorTarget;

        private RenderTargetHandle tmpBlurTarget1;
        private RenderTargetHandle tmpBlurTarget2;
        private RenderTargetHandle tmpFullSizeTex;
        private RenderTargetHandle tmpDestination;

        private RenderTargetHandle temHandle;

        public RenderPassEvent originRenderPassEvent { get; set; }

        public SunShaftsPass(SunShaftsProperties props)
        {
            this.props = props;

            depthNormalsSource.Init(SunShaftsFeatureV2.depthNormalsTextureName);

            tmpDepthColorTarget.Init("_TmpDepthColorTex");
            tmpSkyColorTarget.Init("_TmpSkyColorTex");

            tmpFullSizeTex.Init("_ShaftsTex");
            tmpDestination.Init("_TmpDestinationBuffer");

            tmpBlurTarget1.Init("_TmpBlurTex1");
            tmpBlurTarget2.Init("_TmpBlurTex2");
        }

        public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination)
        {
            this.source = source;
            this.destination = destination;
        }

        private void InitTmpTextures(CommandBuffer cmd, RenderTextureDescriptor cameraTargetDescriptor)
        {
            var opaqueDesc = cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;
            opaqueDesc.msaaSamples = 1;

            var dstWidth = opaqueDesc.width >> props.depthDownscalePow2;
            var dstHeight = opaqueDesc.height >> props.depthDownscalePow2;
            cmd.GetTemporaryRT(tmpBlurTarget1.id, dstWidth, dstHeight, 0, props.filterMode, cameraTargetDescriptor.colorFormat);
            cmd.GetTemporaryRT(tmpBlurTarget2.id, dstWidth, dstHeight, 0, props.filterMode, cameraTargetDescriptor.colorFormat);

            bool isgetfullsizetex = false;
            if (props.IsRenderOutlineGeometry())
            {
                isgetfullsizetex = true;
            }
            else if (props.IsRenderSkyOutline() && props.useSkyEdgesForShafts)
            {
                isgetfullsizetex = true;
            }

            if (isgetfullsizetex)
            {
                cmd.GetTemporaryRT(tmpFullSizeTex.id, opaqueDesc.width, opaqueDesc.height, 0, props.filterMode, cameraTargetDescriptor.colorFormat);
            }


            if (props.IsRenderOutlineGeometry())
            {
                cmd.GetTemporaryRT(tmpDepthColorTarget.id, opaqueDesc.width, opaqueDesc.height, 0, props.filterMode, cameraTargetDescriptor.colorFormat);
            }

            if (props.IsRenderSkyOutline())
            {
                //cmd.GetTemporaryRT(tmpSkyColorTarget.id, opaqueDesc.width, opaqueDesc.height, 0, props.filterMode);
                cmd.GetTemporaryRT(tmpSkyColorTarget.id, dstWidth, dstHeight, 0, props.filterMode, cameraTargetDescriptor.colorFormat);
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ExecuteRender(context, ref renderingData);
        }

        private void ExecuteRender(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (props == null)
                return;

            bool canrender = props.CanVolumeRender();
            if (!canrender)
                return;

            if (!props.forceOn)
            {
                bool isvollightopen = props.IsVolumeLightSwitchOpen();
                if (!isvollightopen)
                    return;
            }

            var camera = renderingData.cameraData.camera;
            if (camera == null)
                return;

            Vector3 sunScreenPoint;
            var cansunrender = props.CanSunRender(camera, out sunScreenPoint);
            if (!cansunrender)
                return;

            if (props.useRenderPassEvent)
            {
                renderPassEvent = props.renderPassEvent;
            }
            else
            {
                renderPassEvent = originRenderPassEvent;
            }

            if (props != null)
            {
                props.CacheAllMaterial();
            }

            bool isallmatload = IsAllMaterialLoad();
            if (!isallmatload)
                return;

            var cmd = CommandBufferPool.Get(COMMAND_BUFFER_NAME);
            cmd.Clear();

            var cameraTargetDescriptor = ModifyCameraTargetDescriptor(renderingData.cameraData.cameraTargetDescriptor,
                out bool isModifyDescriptor);

            InitTmpTextures(cmd, cameraTargetDescriptor);

            //1. blit depth as color to color texture
            if (props.IsRenderOutlineGeometry())
            {
                if (props.useDepthNormals)
                {
                    props.buildDepthNormalMaterial.SetVector(SunPosition, sunScreenPoint);
                    props.buildDepthNormalMaterial.SetFloat(SunThresholdDepth, props.sunThresholdDepth);
                    props.buildDepthNormalMaterial.SetFloat(DepthValueCutOff, props.depthValueCutOff);
                    Blit(cmd, depthNormalsSource.Identifier(), tmpDepthColorTarget.Identifier(), props.buildDepthNormalMaterial);
                }
                else
                {
                    props.buildDepthMaterial.SetVector(SunPosition, sunScreenPoint);
                    props.buildDepthMaterial.SetFloat(SunThresholdDepth, props.sunThresholdDepth);
                    props.buildDepthMaterial.SetFloat(DepthValueCutOff, props.depthValueCutOff);
                    Blit(cmd, renderingData.cameraData.renderer.cameraDepthTarget,
                        tmpDepthColorTarget.Identifier(), props.buildDepthMaterial);
                }

                //1.5 run Sobel edge detector for depth texture
                props.outlineMaterial.SetFloat(OutlineThickness, props.depthOutlineThickness);
                props.outlineMaterial.SetFloat(OutlineMultiplier, props.depthOutlineMultiplier);
                props.outlineMaterial.SetFloat(OutlineBias, props.depthOutlineBias);
                Blit(cmd, tmpDepthColorTarget.Identifier(), tmpFullSizeTex.Identifier(), props.outlineMaterial);
                Blit(cmd, tmpFullSizeTex.Identifier(), tmpDepthColorTarget.Identifier());
            }

            //2. Blit sky cutted off by geometry
            if (props.IsRenderSkyOutline())
            {
                props.buildSkyMaterial.SetVector(SunPosition, sunScreenPoint);
                props.buildSkyMaterial.SetFloat(SunThresholdSky, props.sunThresholdSky);
                props.buildSkyMaterial.SetFloat(SkyNoiseScale, props.skyNoiseScale);
                Blit(cmd, source, tmpSkyColorTarget.Identifier(), props.buildSkyMaterial);

                if (props.useSkyEdgesForShafts)
                {
                    //2.5 run Sobel edge detector for sky texture
                    props.outlineMaterial.SetFloat(OutlineThickness, props.skyOutlineThickness);
                    props.outlineMaterial.SetFloat(OutlineMultiplier, props.skyOutlineMultiplier);
                    props.outlineMaterial.SetFloat(OutlineBias, props.skyOutlineBias);
                    Blit(cmd, tmpSkyColorTarget.Identifier(), tmpFullSizeTex.Identifier(), props.outlineMaterial);
                    //TODO: useless blit, can be removed by setting different global textures
                    Blit(cmd, tmpFullSizeTex.Identifier(), tmpSkyColorTarget.Identifier());
                }
            }

            if (props.IsRenderBothOutlines())
            {
                cmd.SetGlobalTexture(tmpDepthColorTarget.id, tmpDepthColorTarget.Identifier());
                cmd.SetGlobalTexture(tmpSkyColorTarget.id, tmpSkyColorTarget.Identifier());
                //tmpFullSizeTex will not be used, just add two textures above to 1st blur target
                Blit(cmd, tmpFullSizeTex.Identifier(), tmpBlurTarget1.Identifier(), props.mixSkyDepthMaterial);
            }
            else if (props.IsRenderSkyOutline())
            {
                Blit(cmd, tmpSkyColorTarget.Identifier(), tmpBlurTarget1.Identifier());
            }
            else if (props.IsRenderOutlineGeometry())
            {
                Blit(cmd, tmpDepthColorTarget.Identifier(), tmpBlurTarget1.Identifier());
            }

            //2. Blur iteratively
            var radius = props.blurRadius / props.radiusDivider;
            const int shaderBlurIterationsCount = 6;
            var iterationScaler = (float)shaderBlurIterationsCount / props.radiusDivider;
            props.blurMaterial.SetVector(SunPosition, sunScreenPoint);

            for (int i = 0; i < props.blurStepsCount; i++)
            {
                props.blurMaterial.SetFloat(BlurStep, radius);
                Blit(cmd, tmpBlurTarget1.Identifier(), tmpBlurTarget2.Identifier(), props.blurMaterial);

                radius = props.blurRadius * (i * 2f + 1f) * iterationScaler;
                props.blurMaterial.SetFloat(BlurStep, radius);
                Blit(cmd, tmpBlurTarget2.Identifier(), tmpBlurTarget1.Identifier(), props.blurMaterial);

                radius = props.blurRadius * (i * 2f + 2f) * iterationScaler;
            }

            props.finalBlendMaterial.SetFloat(Intensity, props.intensity);
            var shaftsColor = props.useSunLightColor && RenderSettings.sun
                ? RenderSettings.sun.color
                : props.shaftsColor;
            props.finalBlendMaterial.SetColor(ShaftsColor, shaftsColor);

            // 设置是否使用Mask Tex
            props.finalBlendMaterial.SetFloat(UseStencilMaskTex, props.useStencilMaskTex ? 1f : 0f);

            // 设置角色模板材质
            cmd.SetGlobalTexture(stencilMaskTex, stencilMaskTex);

            if (destination == RenderTargetHandle.CameraTarget)
            {
                var cameraDesc = cameraTargetDescriptor;
                cameraDesc.depthBufferBits = 0;

                cmd.GetTemporaryRT(tmpDestination.id, cameraDesc, props.filterMode);

                temHandle = tmpDestination;

                Blit(cmd, source, temHandle.Identifier(), props.finalBlendMaterial);
                Blit(cmd, temHandle.Identifier(), source);
            }
            else
            {
                Blit(cmd, source, destination.Identifier(), props.finalBlendMaterial);
            }

            CleanupTmpTextures(cmd);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void CleanupTmpTextures(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tmpBlurTarget1.id);
            cmd.ReleaseTemporaryRT(tmpBlurTarget2.id);
            cmd.ReleaseTemporaryRT(tmpFullSizeTex.id);

            if (props.IsRenderOutlineGeometry())
            {
                cmd.ReleaseTemporaryRT(tmpDepthColorTarget.id);
            }

            if (props.IsRenderSkyOutline())
            {
                cmd.ReleaseTemporaryRT(tmpSkyColorTarget.id);
            }
        }

        bool IsAllMaterialLoad()
        {
            if (props == null)
                return false;

            if (props.buildDepthNormalMaterial == null)
                return false;

            if (props.buildDepthMaterial == null)
                return false;

            if (props.buildSkyMaterial == null)
                return false;

            if (props.outlineMaterial == null)
                return false;

            if (props.mixSkyDepthMaterial == null)
                return false;

            if (props.blurMaterial == null)
                return false;

            if (props.finalBlendMaterial == null)
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
    }
}