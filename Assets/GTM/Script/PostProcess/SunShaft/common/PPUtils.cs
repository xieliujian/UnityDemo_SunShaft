using UnityEngine;

namespace LR.URP.PPExtensions.common
{
    public static class PPUtils
    {
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