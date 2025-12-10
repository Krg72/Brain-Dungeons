using UnityEngine;
using System.Collections.Generic;

public class UpgradeTreeSpawner : MonoBehaviour
{
    public UpgradeTree tree;

    public RectTransform content;            // ScrollView content
    public UpgradeNodeUI nodePrefab;
    public RectTransform connectorPrefab;

    public float connectorWidth = 5;

    [Header("Layout Scaling")]
    public float spacingX = 1f;              // Compress or expand horizontal space
    public float spacingY = 1f;              // Compress or expand vertical space
    public float globalScale = 1f;           // Final scaling factor

    public Vector2 padding = new Vector2(300, 300);

    private Dictionary<UpgradeNode, RectTransform> uiLookup = new();

    void Start()
    {
        SpawnUsingEditorLayout();
    }

    void SpawnUsingEditorLayout()
    {
        if (tree == null)
        {
            Debug.LogError("Tree missing.");
            return;
        }

        // ------------------------------
        // 1. Calculate bounds of editor layout
        // ------------------------------
        Vector2 min = Vector2.one * 999999f;
        Vector2 max = Vector2.one * -999999f;

        foreach (var n in tree.nodes)
        {
            min.x = Mathf.Min(min.x, n.editorPosition.x);
            min.y = Mathf.Min(min.y, n.editorPosition.y);

            max.x = Mathf.Max(max.x, n.editorPosition.x);
            max.y = Mathf.Max(max.y, n.editorPosition.y);
        }

        Vector2 size = max - min;
        Vector2 centerOffset = -(min + size * 0.5f);

        // ------------------------------
        // 2. Spawn nodes at scaled positions
        // ------------------------------
        foreach (var node in tree.nodes)
        {
            var ui = Instantiate(nodePrefab, content);
            ui.Setup(node, GameManager.Instance.UpgradeManager);
            GameManager.Instance.UpgradeManager.RegisterNodeUI(node, ui);

            Vector2 pos = node.editorPosition + centerOffset;

            // Apply spacing + scaling
            pos.x *= spacingX * globalScale;
            pos.y *= spacingY * globalScale;

            RectTransform rect = ui.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;

            uiLookup[node] = rect;
        }

        // ------------------------------
        // 3. Spawn connectors BEHIND nodes
        // ------------------------------
        foreach (var n in tree.nodes)
        {
            foreach (var tid in n.connections)
            {
                var target = tree.GetNodeById(tid);
                if (target == null) continue;

                var conn = CreateConnector(uiLookup[n], uiLookup[target]);
                conn.SetSiblingIndex(0); // push behind nodes

                var ui = conn.GetComponent<UpgradeConnectorUI>();
                ui.parentNode = n;
                ui.childNode = target;
                GameManager.Instance.UpgradeManager.RegisterConnector(ui);
            }
        }

        GameManager.Instance.UpgradeManager.RefreshAll();

        // ------------------------------
        // 4. Resize scroll content to fit
        // ------------------------------
        content.sizeDelta = (size * new Vector2(spacingX, spacingY) * globalScale) + padding;
    }

    RectTransform CreateConnector(RectTransform from, RectTransform to)
    {
        var conn = Instantiate(connectorPrefab, content);

        Vector2 a = from.anchoredPosition;
        Vector2 b = to.anchoredPosition;
        Vector2 dir = b - a;

        float dist = dir.magnitude;

        conn.sizeDelta = new Vector2(dist, connectorWidth);
        conn.anchoredPosition = a + dir * 0.5f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        conn.localRotation = Quaternion.Euler(0, 0, angle);

        return conn;
    }
}
