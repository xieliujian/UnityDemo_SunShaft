using System;
using UnityEngine;

namespace LR.URP.PPExtensions.outline
{
    [Serializable]
    public class OutlineInput
    {
        public string sourceTextureName;
        public Shader outlineShader;
        public float thickness = 1;
        public float multiplier = 3;
        public float bias = 5;

        [NonSerialized]
        public Material outlineMaterial;
    }
}