using UnityEngine;

namespace LR.URP.PPExtensions.common
{
    public static class PPUtils
    {
        public static void GetMaterial(ref Material mat, string shaderName)
        {
            if (mat == null || mat.shader == null)
            {
                if (mat != null)
                {
                    GameObject.DestroyImmediate(mat, true);
                    mat = null;
                }

                Shader shader = Shader.Find(shaderName);
                if (shader == null)
                {
                    return;
                }

                mat = new Material(shader);
            }
        }

        public static Material GetShaderMaterial(Shader shader, string defaultShaderName, out Shader usedShader)
        {
            if (shader)
            {
                usedShader = shader;
                return new Material(shader);
            }

            shader = Shader.Find(defaultShaderName);
            if (!shader)
            {
                usedShader = null;
                //Case of loading scene. On next frame all must be ok
                //Debug.LogError($"Can't find shader {defaultShaderName}");
                return null;
            }

            usedShader = shader;
            return new Material(shader);
        }
    }
}