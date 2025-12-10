using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;
using System;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.Collections;

public class HelixManager : MonoBehaviour
{
    public static HelixManager Instance;
    void Awake() => Instance = this;

    public HelixGenerator helixGenerator;

    // Instead of a List, we now use a proper QUEUE (TOP → BOTTOM).
    public Queue<Ring> ringQueue = new Queue<Ring>();

    [Header("References")]
    public GameObject ballPrefab;

    [Header("Gameplay")]
    public List<Ball> balls = new List<Ball>();
    public int CurrentMaxLevel = 1;
    public float CurrentTopRingHealth = 10;

    [Header("UI")]
    public Slider progressSlider;      // 0 → 1
     public TMP_Text progressText, prestigeTxt;

    [Header("Spawn settings")]
    public float spawnHeightOffset = 1.5f;
    public float spawnHorizontalRadius = 0.5f;
    public float minSeparation = 0.6f;

    [Header("Merge settings")]
    public float mergeMoveTime = 0.25f;
    public float mergeSpawnAbove = 2.0f;

    public Transform CamTrackingTransform;
    public ParticleSystem breakEfect;

    public Color[] BallLevelColors;

    private bool isMergeAnyInProgress = false;

    private int totalRings = 0;

    [Header("Prestige / Golden Ball")]
    public GameObject goldenBallPrefab;      // assign a golden ball prefab
    public float prestigeMergeHeight = 2.0f; // how far above top ring the merge point is
    public float singleMergeTime = 0.35f;    // each ball's travel time when merging
    public Ease prestigeEase = Ease.InOutSine;

    private bool isPrestiging = false;

    public Ring TopRing
    {
        get
        {
            // Clean any destroyed ring objects
            while (ringQueue.Count > 0 && ringQueue.Peek() == null)
                ringQueue.Dequeue();

            return ringQueue.Count > 0 ? ringQueue.Peek() : null;
        }
    }

    public List<BallSaveData> LoadedballSaveDatas;

    private void Start()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }
    }

    public void SetRings(List<Ring> generated)
    {
        ringQueue.Clear();

        // HelixGenerator spawns from top → bottom, so enqueue in order
        foreach (var r in generated)
            ringQueue.Enqueue(r);

        totalRings = ringQueue.Count;   // remember how many rings in total

        UpdateTopRingLayer();
        UpdateProgressUI();

        if (LoadedballSaveDatas != null && LoadedballSaveDatas.Count > 0)
        {
            StartCoroutine(SpawneLoadedBalls(0.15f));
        }
    }

    IEnumerator SpawneLoadedBalls(float delay)
    {
        if (LoadedballSaveDatas != null && LoadedballSaveDatas.Count > 0)
        {
            // Spawn balls from saved data
            foreach (var ballData in LoadedballSaveDatas)
            {
                for (int i = 0; i < ballData.count; i++)
                {
                    SpawnBall(ballData.level);
                    yield return new WaitForSeconds(delay);
                }
            }
            LoadedballSaveDatas = null; // clear after loading
        }
    }

    private void UpdateProgressUI()
    {
        int remaining = GameManager.Instance.DestroyedRings;

        // 0 when no progress, 1 when all rings are gone
        float progress01 = ((float)remaining / (float)GameManager.Instance.GetRequiredPrestigeRings());
        progress01 = Mathf.Clamp01(progress01);   // <-- Clamp to 0–1

        progressSlider.value = progress01;

        progressText.text = (progress01 * 100f).ToString("F1") + "%";
        prestigeTxt.text = "PRESTIGE " + (GameManager.Instance.CurrentPrestige + 1);

        GameUiManager.Instance.PrestigeBtn.SetActive(progress01 >= 1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            SpawnBall();

        if (Input.GetKeyDown(KeyCode.M))
            MergeAny();

        CamTrackingTransform.position = new Vector3(0f, TopRing ? TopRing.transform.position.y : helixGenerator.EndBase.transform.position.y, 0f);
    }

    public void OnClickSpawnBall()
    {
        SpawnBall();
    }

    public Ball SpawnBall(int level = 1)
    {
        Ring top = TopRing;
        if (!top)
            return null;

        float topY = top.transform.position.y;

        Vector3 pos = Vector3.zero;
        bool foundValid = false;

        // Try multiple variations before settling
        for (int i = 0; i < 10; i++)
        {
            Vector2 rand = Random.insideUnitCircle * spawnHorizontalRadius;
            pos = new Vector3(rand.x, topY + spawnHeightOffset, rand.y);
            pos = EnsureSeparation(pos);

            //if (IsFarEnoughFromAllBalls(pos))
            //{
            //    foundValid = true;
            //    break;
            //}
        }

        GameObject go = Instantiate(ballPrefab, pos, Quaternion.identity);
        Ball ball = go.GetComponent<Ball>();
        ball.level = level;
        ball.SetRingY(topY);
        ball.SetVerticalVelocity(-Random.Range(1f, 3f));

        balls.Add(ball);
        GameUiManager.Instance.RefreshUpgradeButtons();

        if (level > CurrentMaxLevel && level != 1)
        {
            GameUiManager.Instance.ShowMergedNewBallPanel(level);
        }
        CurrentMaxLevel = Mathf.Max(CurrentMaxLevel, level);


        return ball;
    }

    Vector3 EnsureSeparation(Vector3 pos)
    {
        for (int iteration = 0; iteration < 8; iteration++)
        {
            bool moved = false;

            foreach (Ball b in balls)
            {
                if (b == null) continue;

                Vector2 a = new Vector2(pos.x, pos.z);
                Vector2 c = new Vector2(b.transform.position.x, b.transform.position.z);

                float dist = Vector2.Distance(a, c);

                if (dist < minSeparation && dist > 0.001f)
                {
                    Vector2 pushDir = (a - c).normalized;
                    float push = minSeparation - dist;
                    a += pushDir * push;

                    pos.x = a.x;
                    pos.z = a.y;
                    moved = true;
                }
            }

            if (!moved) break;
        }

        return pos;
    }

    void UpdateTopRingLayer()
    {
        // Assign the special layer to the top ring
        if (TopRing != null)
            TopRing.setLayer("topring");
    }

    public void OnBallBounce(Ball ball)
    {
        if (ball == null) return;

        Ring top = TopRing;
        if (top != null)
        {
            //add critical chance 
            float finalDmg = GameManager.Instance.GetBallLevelDamage(ball.level) * GameManager.Instance.getBuildingValue(BuildingType.Damage);

            var ran = Random.Range(0f, 1f) * 100f;
            if (ran <= GameManager.Instance.getBuildingValue(BuildingType.CriticalChance))
            {
                float critDamage = GameManager.Instance.getBuildingValue(BuildingType.CriticalFactor);
                finalDmg = finalDmg * critDamage;
            }

            top.TakeDamage(finalDmg);

            GameUiManager.Instance.SpawnFloatingText(GameUiManager.Instance.textData_damage, finalDmg.ToString(), ball.transform.position);
        }

        GameUiManager.Instance.RefreshUpgradeButtons();
    }


    public void OnRingDestroyed(Ring destroyed)
    {
        // Only remove it if it's truly the top entry
        if (ringQueue.Count > 0 && ringQueue.Peek() == destroyed)
            ringQueue.Dequeue();

        UpdateProgressUI();

        if (!isPrestiging)
        {
            helixGenerator.RecycleRing(destroyed);

            ringQueue.Enqueue(destroyed);
            GameManager.Instance.DestroyedRings++;
        }
        else
        {
            destroyed.gameObject.SetActive(false);
        }

        Ring newTop = TopRing;
        if (newTop == null)
            return;

        AudioManager.instance.PlayOneShot(AudioClipNames.DestroyRing);

        UpdateTopRingLayer();   // ← Add this

        CurrentTopRingHealth = newTop.health;
        float newY = newTop.transform.position.y;

        breakEfect.Play();
        GameUiManager.Instance.PlayGemEffect(newTop.transform.position);
        GameManager.Instance.AddGems((int)1);

        // Rebind all balls to new top Y WITHOUT snapping or clamping
        foreach (var b in balls)
        {
            if (b == null) continue;
            b.SetRingY(newY);   // ball maintains its amplitude and continues its motion smoothly
        }

        if (!isPrestiging)
            SaveLoadManager.instance.SaveGame();
    }

    public void MergeBalls(Ball a, Ball b, Vector3 spawnPos)
    {
        if (!a || !b) return;
        if (a.level != b.level) return;

        int newLevel = a.level;

        // Clean up old balls
        balls.Remove(a);
        balls.Remove(b);

        Destroy(a.gameObject);
        Destroy(b.gameObject);

        // Spawn the new merged ball right at the merge position

        AudioManager.instance.PlayOneShot(AudioClipNames.BallMerge);
        Ball merged = SpawnBall(newLevel + 1);
        if (merged)
        {
            merged.transform.position = spawnPos;
            merged.transform.localScale = Vector3.one * 0.1f; // tiny at start

            // Little pop-in animation
            Sequence appearSeq = DOTween.Sequence();
            appearSeq.Append(merged.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
            appearSeq.Append(merged.transform.DOScale(1.0f, 0.1f).SetEase(Ease.InOutQuad));

            // Give the new ball a clean downward start after the pop
            appearSeq.OnComplete(() =>
            {
                merged.SetVerticalVelocity(-2.5f);
            });
        }

        SaveLoadManager.instance.SaveGame();
    }

    public void MergeAny()
    {
        if (isMergeAnyInProgress)
            return;

        Ball first = null;
        Ball second = null;

        // Find first pair with same level
        for (int i = 0; i < balls.Count; i++)
        {
            for (int j = i + 1; j < balls.Count; j++)
            {
                if (balls[i] == null || balls[j] == null) continue;
                if (balls[i].level == balls[j].level)
                {
                    first = balls[i];
                    second = balls[j];
                    break;
                }
            }
            if (first != null) break;
        }

        if (first == null || second == null) return;

        isMergeAnyInProgress = true;

        // ❄️ Freeze both so their Update() stops moving them
        first.SetFrozen(true);
        second.SetFrozen(true);

        float ringY = TopRing ? TopRing.transform.position.y : 0f;

        // Where we want them to finally meet
        float mergeHeight = ringY + mergeSpawnAbove;

        Vector3 aStart = first.transform.position;
        Vector3 bStart = second.transform.position;

        Vector3 center = (aStart + bStart) * 0.5f;
        center.y = mergeHeight;   // final merge position

        // ------- CURVED PATHS -------

        float curveHeight = 1.3f;       // how high the arc goes above the line
        float moveTime = 0.25f;      // total travel time

        // Control point for A: halfway + some upward offset
        Vector3 controlA = (aStart + center) * 0.5f + Vector3.up * curveHeight;
        // Control point for B
        Vector3 controlB = (bStart + center) * 0.5f + Vector3.up * curveHeight;

        // Simple 3-point paths (start → control → center)
        Vector3[] pathA = new Vector3[] { aStart, controlA, center };
        Vector3[] pathB = new Vector3[] { bStart, controlB, center };

        Sequence seq = DOTween.Sequence();

        // 1) Both balls follow a smooth curved path to the center
        var tweenA = first.transform.DOPath(pathA, moveTime, PathType.CatmullRom)
            .SetEase(Ease.InOutSine);
        var tweenB = second.transform.DOPath(pathB, moveTime, PathType.CatmullRom)
            .SetEase(Ease.InOutSine);

        seq.Join(tweenA);
        seq.Join(tweenB);

        // 2) Add a subtle scale “breath” during movement
        seq.Join(first.transform.DOScale(1.15f, moveTime * 0.5f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo));
        seq.Join(second.transform.DOScale(1.15f, moveTime * 0.5f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo));

        // 3) Punch + shrink at the very end
        float punchTime = 0.1f;
        float squashTime = 0.1f;

        seq.Append(first.transform.DOPunchScale(Vector3.one * 0.35f, punchTime, 10, 0.6f));
        seq.Join(second.transform.DOPunchScale(Vector3.one * 0.35f, punchTime, 10, 0.6f));

        seq.Append(first.transform.DOScale(0.0f, squashTime).SetEase(Ease.InBack));
        seq.Join(second.transform.DOScale(0.0f, squashTime).SetEase(Ease.InBack));

        // 4) Finish with the actual merge + spawn new ball at the center
        seq.OnComplete(() =>
        {
            MergeBalls(first, second, center);
            isMergeAnyInProgress = false;
        });

        // (Optional) safety in case tween gets killed externally
        seq.OnKill(() =>
        {
            isMergeAnyInProgress = false;
        });
    }

    public Color GetColorForLevel(int level)
    {
        if (BallLevelColors == null || BallLevelColors.Length == 0)
            return Color.white;

        int idx = Mathf.Clamp(level - 1, 0, BallLevelColors.Length - 1);
        return BallLevelColors[idx];
    }

    public bool CanMerge()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            for (int j = i + 1; j < balls.Count; j++)
            {
                if (balls[i] == null || balls[j] == null) continue;
                if (balls[i].level == balls[j].level)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ResetHelix()
    {
        foreach (var ring in helixGenerator.Rings)
        {
            Destroy(ring.gameObject);
        }

        foreach (var ball in balls)
        {
            Destroy(ball.gameObject);
        }

        balls.Clear();

        CurrentMaxLevel = 1;
        CurrentTopRingHealth = helixGenerator.baseHealth;

        helixGenerator.Generate();
        UpdateProgressUI();
    }

    public void StartPrestigeSequence()
    {
        if (isPrestiging) return;
        isPrestiging = true;
        GameUiManager.Instance.GameCanvas.gameObject.SetActive(false);
        // find top ring Y
        Ring top = TopRing;

        float ringY = top.transform.position.y;
        Vector3 mergeCenter = new Vector3(0f, ringY + prestigeMergeHeight, 0f);

        // Copy current balls list to avoid mutation while tweening
        var ballsToMerge = new List<Ball>();
        foreach (var b in balls)
            if (b != null)
                ballsToMerge.Add(b);

        // Freeze all balls so their Update() stops interfering
        foreach (var b in ballsToMerge)
            b.SetFrozen(true);

        // Build DOTween sequence that joins all balls moving to the mergeCenter and shrinking
        Sequence seq = DOTween.Sequence();

        // We'll join all movements in parallel
        foreach (var b in ballsToMerge)
        {
            // move to center and scale down
            seq.Join(b.transform.DOMove(mergeCenter, singleMergeTime).SetEase(prestigeEase));
            seq.Join(b.transform.DOScale(0.05f, singleMergeTime).SetEase(prestigeEase));
        }

        // small punch and wait
        seq.AppendInterval(0.08f);
        seq.OnComplete(() =>
        {
            // destroy old balls visuals and clear list
            foreach (var b in ballsToMerge)
            {
                if (b == null) continue;
                // optionally spawn small pop FX here
                Destroy(b.gameObject);
            }
            // remove them from master list
            foreach (var b in ballsToMerge)
                balls.Remove(b);

            // spawn golden ball at mergeCenter
            if (goldenBallPrefab != null)
            {
                GameObject go = Instantiate(goldenBallPrefab, mergeCenter, Quaternion.identity);
                // ensure it has GoldenBall script
                GoldenBall gb = go.GetComponent<GoldenBall>();
                if (gb == null)
                    go.AddComponent<GoldenBall>();

                // optional: set visual scale/pulse then let it start falling
            }
        });
    }

    public void FinishPrestigeSequence()
    {
        // ensure we only run once
        if (!isPrestiging) return;

        isPrestiging = false;
        GameUiManager.Instance.GameCanvas.gameObject.SetActive(true);

        // Clean up any leftover balls (should be none)
        foreach (var b in new List<Ball>(balls))
        {
            if (b != null) Destroy(b.gameObject);
        }
        balls.Clear();

        // Destroy any remaining rings (safety) then regenerate helix
        // Note: If you want a little delay for FX, you can start a coroutine here.
        StartCoroutine(FinishAndGenerateNextHelix());
    }

    System.Collections.IEnumerator FinishAndGenerateNextHelix()
    {
        // small delay for polish (play an effect, etc.)
        yield return new WaitForSeconds(0.6f);

        ResetHelix();

        // optionally restore camera, UI
        yield return null;
    }
    public void SpawnBallsFromSaveData(List<BallSaveData> ballSaveDatas)
    {
        LoadedballSaveDatas = ballSaveDatas;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Vector3.zero, spawnHorizontalRadius);
    }
}

[Serializable]
public class BallSaveData
{
    public int level;
    public int count;
}
