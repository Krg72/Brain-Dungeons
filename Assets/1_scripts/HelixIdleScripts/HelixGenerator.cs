using UnityEngine;
using System.Collections.Generic;

public class HelixGenerator : MonoBehaviour
{
    public GameObject ringPrefab;
    public int ringCount = 10;
    public float ringSpacing = 1f;
    public float startY = 5f;
    public float baseHealth = 5f;
    public float healthStep = 2f;
    
    public List<Ring> Rings { get; private set; } = new List<Ring>();

    public Gradient HelixGradient;

    public GameObject EndBase;

    float currentBottomY;
    int nextLogicalIndex;

    void Start()
    {
        baseHealth = HelixManager.Instance.CurrentTopRingHealth;
        Generate();
    }

    public void Generate()
    {
        Rings.Clear();

        for (int i = 0; i < ringCount; i++)
        {
            float y = startY - i * ringSpacing;
            var r = Instantiate(ringPrefab, new Vector3(0, y, 0), Quaternion.identity, transform);
            var ring = r.GetComponent<Ring>();

            float t = (float)i / Mathf.Max(1, ringCount - 1);
            Color ringColor = HelixGradient.Evaluate(t);
            ring.ringRenderer.material.SetColor("_BaseColor", ringColor);

            ring.Init(i, baseHealth + i * healthStep);
            Rings.Add(ring);
        }

        currentBottomY = startY - (ringCount - 1) * ringSpacing;
        nextLogicalIndex = ringCount;

        if (EndBase != null)
            EndBase.transform.position = new Vector3(0, currentBottomY - ringSpacing, 0);

        HelixManager.Instance.SetRings(Rings);
    }

    public void RecycleRing(Ring ring)
    {
        // move a step further down
        currentBottomY -= ringSpacing;

        ring.transform.position = new Vector3(0, currentBottomY, 0);

        // new infinite index, use this for scaling difficulty
        float newHealth = baseHealth + nextLogicalIndex * healthStep;
        ring.Init(nextLogicalIndex, newHealth);

        // optional: cycle gradient using index
        float t = Mathf.Repeat(nextLogicalIndex * 0.05f, 1f);
        Color ringColor = HelixGradient.Evaluate(t);
        ring.ringRenderer.material.SetColor("_BaseColor", ringColor);

        ring.setLayer("ring");

        nextLogicalIndex++;

        if (EndBase != null)
            EndBase.transform.position = new Vector3(0, currentBottomY - ringSpacing, 0);
    }
}