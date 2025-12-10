using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Splash : MonoBehaviour
{
    public ParticleSystem splashEffect;
    public DecalProjector splashDecal;

    private Material decalInstance;   // store unique material

    public void SetColors(Color color)
    {
        // HDR intensity
        Color hdrColor = color * 2.5f;

        // --- PARTICLE SYSTEM ---
        var main = splashEffect.main;
        main.startColor = new ParticleSystem.MinMaxGradient(Color.white, hdrColor);

        // --- DECAL UNIQUE MATERIAL ---
        if (decalInstance == null)
        {
            // Create independent material instance
            decalInstance = new Material(splashDecal.material);
            splashDecal.material = decalInstance;
        }

        decalInstance.SetColor("_Color", hdrColor);

        //set random y rotation of splash decal
        splashDecal.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
     
        //set random width and height of splash decal
        float randomScale = Random.Range(1, 1.3f);
        splashDecal.size = new Vector3(randomScale, randomScale, 1);
    }
}
