using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighting : MonoBehaviour
{
    [Header("Lighting/Shader"), Range(0f, 1f)]
    public float globalLightLevel;

    public bool fog = true;
    [Range(0, 1000f)]
    public float _fogRadius;

    public float minLightLevel = .15f;
    public float maxLightLevel = .9f;
    public float lightFallOff = 0.08f;

    public Color day, night, skyTint;

    public void UpdateShader()
    {
        Shader.SetGlobalFloat("globalLightLevel", globalLightLevel);
        if (fog)
            Shader.SetGlobalFloat("fogRadius", _fogRadius);
        else
            Shader.SetGlobalFloat("fogRadius", -1);

        Shader.SetGlobalFloat("minGlobalLightLevel", minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", maxLightLevel);

        Shader.SetGlobalColor("skyTint", skyTint);

        Shader.SetGlobalColor("daySkyColor", Color.Lerp(night, day, globalLightLevel));

        foreach (Camera camera in FindObjectsOfType<Camera>())
        {
            camera.backgroundColor = Color.Lerp(night, day, globalLightLevel);
        }
    }

    private void OnValidate()
    {
        UpdateShader();
    }
}
