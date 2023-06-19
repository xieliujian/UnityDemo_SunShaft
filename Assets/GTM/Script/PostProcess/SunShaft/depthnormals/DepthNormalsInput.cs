using System;
using UnityEngine;

namespace LR.URP.PPExtensions.depthnormals
{
    [Serializable]
    public class DepthNormalsInput
    {
        public string textureName;
        public LayerMask layers;        
        public bool debugBlitToColorTarget;
    }
}