using AssetKits.ParticleImage;
using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [Header("Volume & Settings")]
    public Volume volume;

    private ChromaticAberration chromaticAberration;

    [Header("Chromatic Aberration Settings")]
    public float maxIntensity = 1f;
    public float fadeSpeed = 2f;

    private Coroutine currentFade;

    //[Header("Effects")]

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (volume == null)
        {
            Debug.LogError("Volume not assigned in EffectManager.");
            enabled = false;
            return;
        }

        if (!volume.profile.TryGet<ChromaticAberration>(out chromaticAberration))
        {
            Debug.LogError("No ChromaticAberration found in Volume profile.");
            enabled = false;
            return;
        }

        // Initialize effect disabled with zero intensity
        chromaticAberration.active = true;
        chromaticAberration.intensity.value = 0f;
    }

    public void FadeInChromaticAberration()
    {
        StartFade(maxIntensity);
    }

    public void FadeOutChromaticAberration()
    {
        StartFade(0f);
    }

    private void StartFade(float target)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeChromatic(target));
    }

    private IEnumerator FadeChromatic(float target)
    {
        while (!Mathf.Approximately(chromaticAberration.intensity.value, target))
        {
            chromaticAberration.intensity.value = Mathf.MoveTowards(
                chromaticAberration.intensity.value,
                target,
                fadeSpeed * Time.unscaledDeltaTime);

            yield return null;
        }
        chromaticAberration.intensity.value = target;
    }

    public void SpawnEffect(GameObject effect, Vector3 position, Quaternion? rotation = null, bool giveZoffset = true)
    {
        if (effect == null)
        {
            Debug.LogError("Effect prefab is null.");
            return;
        }

        if (giveZoffset)
            position += new Vector3(0, 0, -0.1f);

        // Use provided rotation or default to identity
        Quaternion finalRotation = rotation ?? Quaternion.identity;

        GameObject spawnedEffect = Instantiate(effect, position, finalRotation);
        Destroy(spawnedEffect, 2f); // Destroy after 2 seconds
    }

    

}
