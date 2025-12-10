using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using TMPro;
using UnityEngine;

public class Ring : MonoBehaviour
{
    public float health;
    public int index;   // no longer used for lookup, safe to keep for debugging

    public Renderer ringRenderer;

    public TMP_Text healthTxt;

    [Header("Flash Effect")]
    private Material[] originalMaterials;
    public float flashDuration = 0.2f;
    Coroutine flashCoroutine;
    bool isFlashing = false;

    Vector3 baseScale;
    Tween scaleTween;
    [SerializeField] float hitScale = 0.9f;
    [SerializeField] float scaleDownDuration = 0.06f;
    [SerializeField] float scaleUpDuration = 0.12f;
    [SerializeField] Ease scaleDownEase = Ease.OutCubic;
    [SerializeField] Ease scaleUpEase = Ease.OutBack; // nice pop

    void Awake()
    {
        baseScale = transform.localScale;
    }

    public void Init(int idx, float hp)
    {
        index = idx;
        health = hp;
    }

    protected virtual void OnEnable()
    {
        if (originalMaterials != null && originalMaterials.Length > 0)
            ringRenderer.materials = originalMaterials;
        originalMaterials = ringRenderer.materials;

        if (baseScale == Vector3.zero) baseScale = transform.localScale;
        transform.localScale = baseScale;
    }

    private void Update()
    {
        healthTxt.text = Mathf.CeilToInt(health).ToString();
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;

        //if (!isFlashing)
        //    flashCoroutine = StartCoroutine(FlashEffect());

        // start scale pulse (stop previous so no overlap)
        StartScalePulse();

        if (health <= 0f)
        {
            if (scaleTween != null)
            {
                scaleTween.Kill();
                scaleTween = null;
            }

            StopAllCoroutines();
            ringRenderer.materials = originalMaterials;
            isFlashing = false;
            // Inform HelixManager *before* destroying
            HelixManager.Instance.OnRingDestroyed(this);
        }
    }

    void StartScalePulse()
    {
        // Kill existing tween if running — prevents overlapping pulses
        if (scaleTween != null)
        {
            scaleTween.Kill();
            scaleTween = null;
        }

        // Compose a two-step tween: scale down then scale up
        Vector3 targetDown = baseScale * hitScale;

        // Immediately tween to targetDown, then tween back to baseScale
        scaleTween = transform
            .DOScale(targetDown, scaleDownDuration)
            .SetEase(scaleDownEase)
            .OnComplete(() =>
            {
                // tween back to baseScale with pop ease
                scaleTween = transform
                    .DOScale(baseScale, scaleUpDuration)
                    .SetEase(scaleUpEase)
                    .OnComplete(() => scaleTween = null);
            });
    }

    public void setLayer(string layername)
    {
        gameObject.layer = LayerMask.NameToLayer(layername);
        ringRenderer.gameObject.layer = LayerMask.NameToLayer(layername);

    }

    private IEnumerator FlashEffect()
    {
        isFlashing = true;
        Material[] flashMaterials = new Material[originalMaterials.Length];
        for (int i = 0; i < flashMaterials.Length; i++)
            flashMaterials[i] = GameManager.Instance.flashMaterial;

        ringRenderer.materials = flashMaterials;
        yield return new WaitForSeconds(flashDuration);
        ringRenderer.materials = originalMaterials;
        isFlashing = false;
    }
}
