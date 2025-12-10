using AssetKits.ParticleImage;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUiManager : MonoBehaviour
{
    public static GameUiManager Instance;

    public Sprite[] ResourceTypeIcons;
    public Sprite[] UpgradeTypeIcons;
    public Sprite[] BuildingTypeIcons;

    [Header("References")]
    public DynamicTextData textData_damage;

    public ParticleImage CoinParticle;

    public ParticleImage GemParticle;

    public Canvas CoinAnimCanvas, GameCanvas;

    public BallUpgradeBtn[] AllUpgradeBtns;

    public BuildingPanel BuildingPanel;

    public GameObject PrestigeBtn;

    [SerializeField] public TMP_Text MaxBallsTxt;

    [SerializeField] GameObject PrestigeConfirmPanel;
    [SerializeField] TMP_Text PrestigeRewardCoinsTxt, PrestigeRewardPointsTxt;

    [SerializeField] GameObject NewMergePanel;
    [SerializeField] TMP_Text MergeLevelTxt, MergeDamageTxt;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
    }

    void Update()
    {
        int maxBalls = (int)GameManager.Instance.getBuildingValue(BuildingType.MaxBalls);
        MaxBallsTxt.text = $"{HelixManager.Instance.balls.Count}/{maxBalls}";
    }

    public void RefreshUpgradeButtons()
    {
        foreach (var btn in AllUpgradeBtns)
        {
            btn.SetData();
        }
    }

    public void OpenPrestige()
    {
        AudioManager.instance.OnButtonClickSound();
        int rewardCoins = (GameManager.Instance.CurrentPrestige + 1) * 1000;
        int rewardPoints = (GameManager.Instance.CurrentPrestige + 1) * 5;
        PrestigeRewardCoinsTxt.text = rewardCoins.ToString("F0");
        PrestigeRewardPointsTxt.text = rewardPoints.ToString("F0");
        PrestigeConfirmPanel.SetActive(true);
    }

    public void OnConfirmPrestige()
    {
        AudioManager.instance.OnButtonClickSound();
        PrestigeConfirmPanel.SetActive(false);
        GameManager.Instance.DoPrestige();
    }

    public void ShowMergedNewBallPanel(int newlevel)
    {
        int prevDmg = GameManager.Instance.GetBallLevelDamage(newlevel - 1);
        int newDmg = GameManager.Instance.GetBallLevelDamage(newlevel);

        MergeLevelTxt.text = "LEVEL " + newlevel;
        MergeDamageTxt.text = $"DMG : {prevDmg} >  <color=green>{newDmg}</color>";

        NewMergePanel.SetActive(true);
    }

    public void PlayCoinEffect(Vector3 worldPos, int amount)
    {
        amount = Mathf.Clamp(amount, 1, 10);

        // 1. World → Screen
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // 2. Screen → Canvas local pos
        RectTransform canvasRect = CoinAnimCanvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            CoinAnimCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPos
        );

        // 3. Instantiate a new particle instance
        ParticleImage particleInstance = Instantiate(CoinParticle, CoinAnimCanvas.transform);

        // 4. Move instance to position
        RectTransform rect = particleInstance.GetComponent<RectTransform>();
        rect.anchoredPosition = localPos;

        // 5. OPTIONAL: Set burst (AssetKits.ParticleImage)
        //    SetBurst(index, time, count)
        particleInstance.SetBurst(0, 0, amount);

        // 6. Play effect
        particleInstance.Play();

        // 7. Auto destroy after duration
        float life = particleInstance.main.duration;
        Destroy(particleInstance.gameObject, life + 0.1f);
    }
    
    public void PlayGemEffect(Vector3 worldPos)
    {
        // 1. World → Screen
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // 2. Screen → Canvas local pos
        RectTransform canvasRect = CoinAnimCanvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            CoinAnimCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPos
        );

        // 3. Instantiate a new particle instance
        ParticleImage particleInstance = Instantiate(GemParticle, CoinAnimCanvas.transform);

        // 4. Move instance to position
        RectTransform rect = particleInstance.GetComponent<RectTransform>();
        rect.anchoredPosition = localPos;

        // 5. OPTIONAL: Set burst (AssetKits.ParticleImage)
        //    SetBurst(index, time, count)
        particleInstance.SetBurst(0, 0, 1);

        // 6. Play effect
        particleInstance.Play();

        // 7. Auto destroy after duration
        float life = particleInstance.main.duration;
        Destroy(particleInstance.gameObject, life + 0.1f);
    }


    public void SpawnFloatingText(DynamicTextData data, string DataString, Vector3 position)
    {
        if (!SettingManager.instance.isFloatTextOn) return;

        if (transform != null)
        {
            position.x += (Random.value - 0.5f);
            position.y += (Random.value);
            position.z += (Random.value - 0.5f);
            DynamicTextManager.CreateText(position, DataString, data);
        }
    }

    public static string FormatNumber(float num)
    {
        if (num >= 1_000_000_000)
            return (num / 1_000_000_000f).ToString("0.#") + "B";
        if (num >= 1_000_000)
            return (num / 1_000_000f).ToString("0.#") + "M";
        if (num >= 1_000)
            return (num / 1_000f).ToString("0.#") + "K";

        return num.ToString("f0");
    }

}
