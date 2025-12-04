using UnityEngine;

public class UpdateShaderTime : MonoBehaviour
{
    public Material material;

    void Update()
    {
        material.SetFloat("_UnscaledTime", Time.unscaledTime);
    }
}
