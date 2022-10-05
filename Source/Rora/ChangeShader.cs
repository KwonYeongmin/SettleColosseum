using UnityEngine;

public class ChangeShader
{
    private Material mat;
    private Shader shaderStandard;
    private Renderer renderer;
    private GameObject characterModel;

    public ChangeShader(GameObject characterModel_)
    {
        characterModel = characterModel_;

        renderer = characterModel.GetComponent<Renderer>();
        mat = renderer.material;
        shaderStandard = Shader.Find("Standard");
        if (!shaderStandard) Debug.Log("shaderStandard not Found");

        if (!characterModel.GetComponent<SkinnedMeshRenderer>())
        {
            MeshRenderer meshrenderer = characterModel.GetComponent<MeshRenderer>();
            meshrenderer.material.shader = shaderStandard;
        }
        else
        {
            SkinnedMeshRenderer meshrenderer = characterModel.GetComponent<SkinnedMeshRenderer>();
            meshrenderer.material.shader = shaderStandard;
        }
    }

    public void ChangeTransparent(float value, int mode)
    {
        mat.SetColor("_Color", new Color(this.mat.color.r, this.mat.color.g, this.mat.color.b, value));

        if (mode == 0) //default
        {
            mat.SetFloat("_Mode", 0); // opaque            
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.renderQueue = -1;
        }
        else if (mode == 1)
        {
            mat.SetFloat("_Mode", 3); // transparent
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
        }

        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
    }
}