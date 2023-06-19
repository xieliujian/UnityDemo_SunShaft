using LR.Scene;
using LR.URP.PPExtensions.common;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LR.URP.PPExtensions.sunshafts
{
    [Serializable]
    public class SunShaftsProperties
    {
        /// <summary>
        /// 摄像机与灯光的角度
        /// </summary>
        const float CAN_VISIBLE_RENDER_LIGHT_ANGLE = 30.0f;

        /// <summary>
        /// 摄像机与向上方向向量的角度
        /// </summary>
        const float CAN_VISIBLE_RENDER_UP_ANGLE = 70.0f;

        [HideInInspector]
        public bool isOn = false;

        [HideInInspector]
        public bool forceOn = false;

        [Header("Common params")]
        public FilterMode filterMode = FilterMode.Bilinear;
        [Range(0, 5)]

        public float intensity = 0.2f;

        [Tooltip("LayerMask to render to depth normals texture")]
        public LayerMask normalsLayerMask = -1; //All by default
        [Tooltip("What parts to use for outline: only sky bounded by geometry or both sky + geometry")]
        public OutlineMode outlineMode = OutlineMode.OnlySky;

        [Header("Sun params")] 
        public bool useSunLightColor = true;
        [ColorUsage(false, true)]
        public Color shaftsColor;
        //public bool setSunByScript;

        [HideInInspector]
        [Header("Sun Position")]
        public bool useSunPosition = false;

        [Tooltip("Can be used for fake Sun Source, which is differ from main Sun Light - source of shadows")]
        public Vector3 sunPosition;

        [Range(0, 1)]
        public float sunThresholdSky = 0.75f;
        [Range(0, 1)]
        public float sunThresholdDepth = 0.75f;
        
        //[Header("Depth texture params - for geometry/normals texture")]
        //[Range(0, 1)]
        //public float depthValueCutOff = 0.5f;
        //public int depthOutlineThickness = 1;
        //public float depthOutlineMultiplier = 5f;
        //public float depthOutlineBias = 25f;

        [Header("Sky params - for SkyOnly mode or both")]
        [Tooltip("Highlight only edges, not the whole sky")]
        public bool useSkyEdgesForShafts;
        public float skyNoiseScale = 75f;
        public int skyOutlineThickness = 1;
        public float skyOutlineMultiplier = 5f;
        public float skyOutlineBias = 25f;

        [Header("Blur params")]
        [Range(0, 4)]
        public int depthDownscalePow2 = 0;
        public float blurRadius = 0.15f;
        [NonSerialized]
        public float radiusDivider = 750f;
        [Range(1, 4)]
        public int blurStepsCount = 2;

        [HideInInspector]
        public bool useRenderPassEvent = false;

        [HideInInspector]
        public RenderPassEvent renderPassEvent = new RenderPassEvent();

        [HideInInspector]
        public bool useStencilMaskTex = false;

        [NonSerialized]
        public Shader depthNormalsShader;
        [NonSerialized]
        public Shader buildDepthNormalsShader;
        [NonSerialized]
        public Shader buildDepthShader;
        [NonSerialized]
        public Shader outlineShader;
        [NonSerialized]
        public Shader buildSkyShader;
        [NonSerialized]
        public Shader mixSkyDepthShader;
        [NonSerialized]
        public Shader blurShader;
        [NonSerialized]
        public Shader finalBlendShader;
        
        [NonSerialized]
        public Material renderDepthNormalsMaterial;
        [NonSerialized]
        public Material buildDepthNormalMaterial;
        [NonSerialized]
        public Material buildDepthMaterial;
        [NonSerialized]
        public Material buildSkyMaterial;
        [NonSerialized]
        public Material outlineMaterial;
        [NonSerialized]
        public Material mixSkyDepthMaterial;
        [NonSerialized]
        public Material blurMaterial;
        [NonSerialized]
        public Material finalBlendMaterial;

        SunShafts m_SunShafts;

        public SunShafts sunShafts
        {
            get { return m_SunShafts; }
        }

        public bool IsRenderOutlineGeometry()
        {
            return outlineMode == OutlineMode.OnlyDepthGeometry
                   || outlineMode == OutlineMode.SkyAndDepthGeometry;
        }

        public bool IsRenderSkyOutline()
        {
            return outlineMode == OutlineMode.OnlySky
                   || outlineMode == OutlineMode.SkyAndDepthGeometry;
        }

        public bool IsRenderBothOutlines()
        {
            return outlineMode == OutlineMode.SkyAndDepthGeometry;
        }

        public bool CanVolumeRender()
        {
            if (!forceOn)
            {
                return isOn;
            }

            return true;
        }

        public void CopyVolumeProperty()
        {
            isOn = m_SunShafts.IsActive();
            forceOn = m_SunShafts.forceOn.value;

            useRenderPassEvent = m_SunShafts.useRenderPassEvent.value;
            renderPassEvent = m_SunShafts.renderPassEvent.value;

            useStencilMaskTex = m_SunShafts.useStencilMaskTex.value;

            intensity = m_SunShafts.intensity.value;

            useSunLightColor = m_SunShafts.useSunLightColor.value;
            shaftsColor = m_SunShafts.shaftsColor.value;

            useSunPosition = m_SunShafts.useSunPosition.value;
            sunPosition = m_SunShafts.sunPosition.value;

            sunThresholdSky = m_SunShafts.sunThresholdSky.value;
            sunThresholdDepth = m_SunShafts.sunThresholdDepth.value;

            //depthValueCutOff = m_SunShafts.depthValueCutOff.value;
            //depthOutlineThickness = m_SunShafts.depthOutlineThickness.value;
            //depthOutlineMultiplier = m_SunShafts.depthOutlineMultiplier.value;
            //depthOutlineBias = m_SunShafts.depthOutlineBias.value;

            //useSkyEdgesForShafts = m_SunShafts.useSkyEdgesForShafts.value;
            //skyNoiseScale = m_SunShafts.skyNoiseScale.value;
            //skyOutlineThickness = m_SunShafts.skyOutlineThickness.value;
            //skyOutlineMultiplier = m_SunShafts.skyOutlineMultiplier.value;
            //skyOutlineBias = m_SunShafts.skyOutlineBias.value;

            depthDownscalePow2 = m_SunShafts.depthDownscalePow2.value;
            blurRadius = m_SunShafts.blurRadius.value;
            blurStepsCount = m_SunShafts.blurStepsCount.value;
        }

        public void CacheSunShafts()
        {
            var stack = VolumeManager.instance.stack;
            if (stack != null)
            {
                m_SunShafts = stack.GetComponent<SunShafts>();
            }
        }

        /// <summary>
        /// 加载所有的材质
        /// </summary>
        public void CacheAllMaterial()
        {
            //PPUtils.GetMaterial(ref renderDepthNormalsMaterial, SunShaftsFeatureV2.depthNormalsShaderName);
            //PPUtils.GetMaterial(ref buildDepthNormalMaterial, SunShaftsFeatureV2.buildDepthNormalsShaderName);
            //PPUtils.GetMaterial(ref buildDepthMaterial, SunShaftsFeatureV2.buildDepthShaderName);
            //PPUtils.GetMaterial(ref outlineMaterial, SunShaftsFeatureV2.outlineShaderName);
            PPUtils.GetMaterial(ref buildSkyMaterial, SunShaftsFeatureV2.buildSkyShaderName);
            PPUtils.GetMaterial(ref mixSkyDepthMaterial, SunShaftsFeatureV2.mixSkyDepthShaderName);
            PPUtils.GetMaterial(ref blurMaterial, SunShaftsFeatureV2.blurShaderName);
            PPUtils.GetMaterial(ref finalBlendMaterial, SunShaftsFeatureV2.finalBlendShaderName);
        }

        /// <summary>
        /// 是否太阳被渲染
        /// </summary>
        /// <returns></returns>
        public bool CanSunRender(Camera camera, out Vector3 _sunScreenPoint)
        {
            _sunScreenPoint = Vector3.zero;
            if (camera == null)
                return false;

            var lightpos = GetSunLightWorldPosition(camera);

            // 摄像机看不到太阳光, pass
            var sunScreenPoint = camera.WorldToViewportPoint(lightpos);
            _sunScreenPoint = sunScreenPoint;
            if (sunScreenPoint.z < 0f)
                return false;

            if (!forceOn)
            {
                // 摄像机方向和水平面大于30度才看见体积光
                var isinanglerender = IsInAngleRender(camera);
                if (!isinanglerender)
                    return false;
            }

            return true;
        }
        
        public bool IsVolumeLightSwitchOpen()
        {
            return true;
        }

        Vector3 GetLightDir()
        {
            Vector3 dir = Vector3.forward;

            if (RenderSettings.sun != null)
            {
                dir = RenderSettings.sun.gameObject.transform.forward;
            }

            return dir;
        }

        Vector3 GetSunLightWorldPosition(Camera camera)
        {
            if (useSunPosition)
            {
                return sunPosition;
            }
            else
            {
                if (!RenderSettings.sun)
                {
                    return sunPosition;
                }
                else
                {
                    var dir = GetLightDir();

                    var fardis = 10000f;

                    var pos = -dir * fardis;
                    return pos;
                }
            }
        }

        /// <summary>
        /// 是否在角度中渲染
        /// </summary>
        /// <returns></returns>
        bool IsInAngleRender(Camera camera)
        {
            var lightdir = GetLightDir();
            var camforward = camera.transform.forward;
            float angle1 = Mathf.Abs(Vector3.Angle(-camforward, lightdir));
            float angle2 = Mathf.Abs(Vector3.Angle(camforward, Vector3.up));

            bool isInAngle1 = angle1 <= CAN_VISIBLE_RENDER_LIGHT_ANGLE;
            bool isInAngle2 = angle2 <= CAN_VISIBLE_RENDER_UP_ANGLE;
            return isInAngle1 || isInAngle2;
        }
    }
}