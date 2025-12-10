using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonScaler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float scaleDownFactor = 0.9f; // How much to scale down
    public float animationDuration = 0.1f; // Duration of the scale animation
    private Vector3 originalScale;
    public bool playSound = false;

    private void Start()
    {
        // Save the original scale of the button
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Scale down the button
        ScaleButton(originalScale * scaleDownFactor);
        //if (playSound) AudioManager.instance.OnButtonClickSound();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Scale back to the original size
        ScaleButton(originalScale);
    }

    private void ScaleButton(Vector3 targetScale)
    {
        // Animate the scale change
        //LeanTween.scale(gameObject, targetScale, animationDuration).setEase(LeanTweenType.easeOutQuad);
        // Alternatively, use DOTween if you prefer:
         transform.DOScale(targetScale, animationDuration).SetEase(Ease.OutQuad);
    }
}