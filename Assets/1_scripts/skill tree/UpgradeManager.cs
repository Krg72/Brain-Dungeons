using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public UpgradeTree tree;
    public UpgradeDetailsPanel detailsPanel;

    private Dictionary<string, int> levels = new();
    private Dictionary<UpgradeNode, UpgradeNodeUI> uiNodes = new();
    
    private List<UpgradeConnectorUI> connectors = new();

    public int GetNodeLevel(string id) => levels[id];

    public bool CanAfford(float cost)
    {
        // Replace with your real currency logic
        return true;
    }

    public void RegisterNodeUI(UpgradeNode node, UpgradeNodeUI ui)
    {
        uiNodes[node] = ui;
        levels[node.id] = 0; // default
    }

    public void RegisterConnector(UpgradeConnectorUI conn)
    {
        connectors.Add(conn);
    }

    void Start()
    {
        detailsPanel.Setup(this);
    }

    public void OpenDetailsPanel(UpgradeNodeUI ui)
    {
        if (ui.state == NodeState.Hidden)
        {
            Debug.Log("Trying to open details for hidden node.");
            return;
        }
        detailsPanel.Show(ui);
    }

    public void TryUpgradeNode(UpgradeNodeUI ui)
    {
        var node = ui.data;
        int level = levels[node.id];

        if (level >= node.maxLevel) return;

        float cost = node.levelCosts[level];

        if (!CanAfford(cost)) return;

        levels[node.id]++;

        RefreshAll();
        detailsPanel.Show(ui);
    }

    public void RefreshAll()
    {
        UpgradeNode root = tree.GetRootNode();
        HashSet<UpgradeNode> unlockedNodes = new();

        // root is always visible
        unlockedNodes.Add(root);

        // BFS: unlock nodes whose parents are level >= 1
        Queue<UpgradeNode> q = new Queue<UpgradeNode>();
        q.Enqueue(root);

        while (q.Count > 0)
        {
            var node = q.Dequeue();
            int lvl = levels[node.id];

            if (lvl < 1) continue; // not upgraded yet, so children stay hidden

            foreach (string tid in node.connections)
            {
                var child = tree.GetNodeById(tid);
                unlockedNodes.Add(child);
                q.Enqueue(child);
            }
        }

        // update UI states
        foreach (var pair in uiNodes)
        {
            UpgradeNode node = pair.Key;
            UpgradeNodeUI ui = pair.Value;

            if (!unlockedNodes.Contains(node))
            {
                ui.state = NodeState.Hidden;
            }
            else
            {
                int lvl = levels[node.id];
                if (lvl >= node.maxLevel)
                    ui.state = NodeState.Maxed;
                else if (lvl >= 1)
                    ui.state = NodeState.CanUpgrade;
                else
                    ui.state = NodeState.Locked;
            }

            ui.RefreshVisual();
        }

        foreach (var conn in connectors)
        {
            bool parentVisible = uiNodes[conn.parentNode].state != NodeState.Hidden;
            bool childVisible = uiNodes[conn.childNode].state != NodeState.Hidden;

            bool shouldShow = parentVisible && childVisible;
            conn.Refresh(shouldShow);
        }
    }
}
