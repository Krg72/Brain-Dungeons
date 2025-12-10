using DG.Tweening;
using TMPro;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Bounce Physics")]
    public float gravity = 9.8f;

    public float verticalVelocity;  // velocity of the ball (positive = rising)

    public float bounceAmplitude = 1.5f; // height above ringY
    public float groundTouchCooldown = 0.12f;
    private float lastBounceTime = 0f;

    public float groundOffset = 0.35f;

    [Header("Data")]
    public int level = 1;
    private float nextBounceVelocity;

    public TMP_Text LevelTxt;
    public Renderer BallRenderer;

    public bool isFrozen = false;

    public Splash SplashObject;

    Tween scaleTween;
    [SerializeField] float squeezeY = 0.85f;     // how much to squeeze in Y (0..1)
    [SerializeField] float squeezeXZ = 1.08f;   // slight stretch on X/Z
    [SerializeField] float squeezeDownDuration = 0.06f;
    [SerializeField] float squeezeUpDuration = 0.12f;
    [SerializeField] Ease squeezeDownEase = Ease.OutCubic;
    [SerializeField] Ease squeezeUpEase = Ease.OutBack;

    void Start()
    {
        gravity = GameManager.Instance.getBuildingValue(BuildingType.Speed);

        float ringY = HelixManager.Instance.TopRing.transform.position.y;
        float minY = ringY + 0.02f;
        if (transform.position.y < minY)
            transform.position = new Vector3(transform.position.x, minY, transform.position.z);

        LevelTxt.text = level.ToString();

        BallRenderer.material.SetColor("_BaseColor", HelixManager.Instance.GetColorForLevel(level));

    }

    void Update()
    {
        if (isFrozen)
            return;

        float ringY = 0;

        if (HelixManager.Instance.TopRing == null)
        {
            ringY = HelixManager.Instance.helixGenerator.transform.position.y;
        }
        else
        {
            ringY = HelixManager.Instance.TopRing.transform.position.y;
        }

        float peakY = ringY + bounceAmplitude;

        // Apply gravity
        verticalVelocity -= gravity * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y += verticalVelocity * Time.deltaTime;


        // Ground bounce
        if (pos.y <= (ringY + groundOffset))
        {
            // Use the correct bounce velocity for the *new* ringY
            verticalVelocity = nextBounceVelocity;

            if (Time.time - lastBounceTime > groundTouchCooldown)
            {
                AudioManager.instance.PlayOneShot(AudioClipNames.BallBounce);
                int coinsReward = Mathf.CeilToInt(level * GameManager.Instance.GetIncomePerBounce());
                GameUiManager.Instance.PlayCoinEffect(transform.position, Mathf.CeilToInt(coinsReward/2));
                GameManager.Instance.AddCoins(coinsReward);

                // Spawn splash effect
                if (SplashObject != null)
                {
                    Splash splash = Instantiate(SplashObject, new Vector3(pos.x, ringY + 0.01f, pos.z), Quaternion.identity);
                    splash.SetColors(HelixManager.Instance.GetColorForLevel(level));
                    splash.transform.SetParent(HelixManager.Instance.TopRing.transform);
                    Destroy(splash, 5f);
                }


                lastBounceTime = Time.time;
                HelixManager.Instance.OnBallBounce(this);

                // Trigger scale squeeze pulse
                TriggerSqueezePulse();
            }
        }
        transform.position = pos;
    }

    void TriggerSqueezePulse()
    {
        if (scaleTween != null)
        {
            scaleTween.Kill();
            scaleTween = null;
        }

        Vector3 downScale = new Vector3(1 * squeezeXZ, 1* squeezeY, 1 * squeezeXZ);

        // Use a DOTween Sequence for down+up
        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(downScale, squeezeDownDuration).SetEase(squeezeDownEase));
        seq.Append(transform.DOScale(Vector3.one, squeezeUpDuration).SetEase(squeezeUpEase));
        seq.OnComplete(() =>
        {
            // ensure exact reset
            transform.localScale = Vector3.one;
            scaleTween = null;
            seq.Kill();
        });

        scaleTween = seq;
    }

    // Called by HelixManager when the top ring changes
    // preserveAmplitude = true → ball keeps exact bounce height
    public void SetRingY(float newY)
    {
        // Precompute the perfect bounce velocity for the new ring
        nextBounceVelocity = Mathf.Sqrt(2f * gravity * bounceAmplitude);
    }

    public void SetVerticalVelocity(float v)
    {
        verticalVelocity = v;
    }

    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;
        if (frozen)
            verticalVelocity = 0f;
    }
}
