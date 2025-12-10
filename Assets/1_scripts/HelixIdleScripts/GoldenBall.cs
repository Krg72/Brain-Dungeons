// GoldenBall.cs
using System.Collections.Generic;
using UnityEngine;

public class GoldenBall : MonoBehaviour
{
    [Tooltip("Downwards speed of the golden ball (units/sec).")]
    public float fallSpeed = 10f;

    [Tooltip("Distance (world units) at which the golden ball 'hits' a ring.")]
    public float destroyDistance = 0.9f;

    [Tooltip("Small delay after finishing before destroying the golden ball (for FX).")]
    public float destroyDelayAfterDone = 0.6f;

    // Optional: avoid destroying the same ring multiple frames
    HashSet<Ring> destroyedRings = new HashSet<Ring>();

    bool isActive = true;
    float bottomY = -9999f;

    void Start()
    {
        // compute bottomY as a safe fallback using the helix EndBase if present
        var hm = HelixManager.Instance;
        if (hm != null && hm.helixGenerator != null && hm.helixGenerator.EndBase != null)
        {
            bottomY = hm.helixGenerator.EndBase.transform.position.y - 1f;
        }
        else
        {
            // if no endbase, set some large negative - will be checked additionally by ring list empty
            bottomY = -10000f;
        }
    }

    void Update()
    {
        if (!isActive) return;

        // simple linear downward movement (no bouncing)
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // check proximity to each ring in the helix generator
        var hg = HelixManager.Instance?.helixGenerator;
        if (hg != null)
        {
            // iterate over the copy to be safe if underlying list changes
            List<Ring> rings = hg.Rings;
            for (int i = rings.Count - 1; i >= 0; i--)
            {
                Ring r = rings[i];
                if (r == null) continue;
                if (destroyedRings.Contains(r)) continue;

                // distance check (3D) — you can change to Y-only if you prefer
                float d = Vector3.Distance(transform.position, r.transform.position);

                if (d <= destroyDistance)
                {
                    AudioManager.instance.PlayOneShot(AudioClipNames.BallBounce);
                    int coinsReward = Mathf.CeilToInt(HelixManager.Instance.CurrentMaxLevel * GameManager.Instance.GetIncomePerBounce());
                    GameUiManager.Instance.PlayCoinEffect(transform.position, Mathf.CeilToInt(coinsReward / 2));
                    GameManager.Instance.AddCoins(coinsReward);
                    // mark and request HelixManager to destroy it permanently for prestige
                    destroyedRings.Add(r);
                    HelixManager.Instance.OnRingDestroyed(r);
                }
            }

            // If no rings left (all destroyed), finish prestige
            bool anyLiveRing = false;
            for (int i = 0; i < rings.Count; i++)
            {
                if (rings[i] != null) { anyLiveRing = true; break; }
            }
            if (!anyLiveRing)
            {
                FinishPrestige();
                return;
            }
        }

        // safety: if fallen below bottom bound, finish prestige
        if (transform.position.y <= bottomY)
        {
            FinishPrestige();
        }
    }

    void FinishPrestige()
    {
        if (!isActive) return;
        isActive = false;

        // optional FX on finish: you can spawn particles, camera shake, etc. here.

        // tell HelixManager to finalize
        HelixManager.Instance.FinishPrestigeSequence();

        // destroy self after small delay so FX can play
        Destroy(gameObject, destroyDelayAfterDone);
    }
}
