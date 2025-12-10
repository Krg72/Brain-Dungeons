using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BuildingAbility : MonoBehaviour
{
    public BuildingType BuildingType;

    public GameObject AbilityVFX;

    [SerializeField] float MinTimeInterval = 5f;
    [SerializeField] float MaxTimeInterval = 10f;
    [SerializeField] float ActiveDuration = 3f;

    float timer;
    float nextTriggerTime;
    bool isActive = false;

    [SerializeField] GameObject FillParent;
    [SerializeField] Image FillImage;

    void Start()
    {
        SetNextTriggerTime();
    }

    void SetNextTriggerTime()
    {
        nextTriggerTime = Random.Range(MinTimeInterval, MaxTimeInterval);
    }

    void Update()
    {
        if (GameManager.Instance.BuildingTypeLevels[(int)BuildingType] <= 0)
        {
            FillParent.SetActive(false);
            return;
        }

        // Always show fill parent when level > 0
        FillParent.SetActive(true);

        // ---------------------------------------------------------
        // COOLDOWN PHASE → fill 0 → 1
        // ---------------------------------------------------------
        if (!isActive)
        {
            timer += Time.deltaTime;

            float fill = Mathf.Clamp01(timer / nextTriggerTime);
            FillImage.fillAmount = fill;

            if (timer >= nextTriggerTime)
            {
                StartCoroutine(StartAbility());
            }
        }
    }

    IEnumerator StartAbility()
    {
        isActive = true;
        timer = 0f; // reset cooldown timer

        AbilityVFX.SetActive(true);

        float elapsed = 0f;
        float damageInterval = 0.5f;

        // Start full → drain to 0
        FillImage.fillAmount = 1f;

        while (elapsed < ActiveDuration)
        {
            DealDamage();

            elapsed += damageInterval;

            // ---------------------------------------------------------
            // ACTIVE PHASE → fill goes 1 → 0
            // ---------------------------------------------------------
            float activeFill = 1f - Mathf.Clamp01(elapsed / ActiveDuration);
            FillImage.fillAmount = activeFill;

            yield return new WaitForSeconds(damageInterval);
        }

        AbilityVFX.SetActive(false);

        // Restart cooldown
        isActive = false;
        timer = 0f;
        SetNextTriggerTime();
    }

    void DealDamage()
    {
        Ring top = HelixManager.Instance.TopRing;
        if (top == null) return;

        float dmg = GameManager.Instance.getBuildingValue(BuildingType.Damage);

        // Crit chance
        float chance = GameManager.Instance.getBuildingValue(BuildingType.CriticalChance);
        if (Random.Range(0f, 100f) <= chance)
        {
            dmg *= GameManager.Instance.getBuildingValue(BuildingType.CriticalFactor);
        }

        top.TakeDamage(dmg);
    }
}
