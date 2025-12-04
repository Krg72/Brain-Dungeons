using System;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    None,
    Health,
    Damage,
    CritChance,
    MovementSpeed,
    // add your types here
}

[Serializable]
public class UpgradeNode
{
    [Tooltip("Stable unique id (generated automatically).")]
    public string id;

    [Tooltip("A short human name for the node.")]
    public string displayName = "New Node";

    [Tooltip("If true this node is the centre (root) node that is visible at start). Only one should be root.")]
    public bool isRoot = false;

    public UpgradeType type = UpgradeType.None;
    public float baseValue = 1f;
    public float incrementValue = 0.5f;
    public bool isPercentage = false;
    public int maxLevel = 5;

    public List<float> levelCosts = new List<float>();

    [Tooltip("Editor placement. Used only by the editor to position the node.")]
    public Vector2 editorPosition;

    [Tooltip("IDs of nodes this node connects to (directional).")]
    public List<string> connections = new List<string>();

    public float GetCostForLevel(int level)
    {
        if (level < 1 || level > levelCosts.Count)
            return 0;
        return levelCosts[level - 1];
    }

}

[CreateAssetMenu(menuName = "Upgrades/UpgradeTree")]
public class UpgradeTree : ScriptableObject
{
    public List<UpgradeNode> nodes = new List<UpgradeNode>();

    public UpgradeNode GetNodeById(string id)
    {
        return nodes.Find(n => n != null && n.id == id);
    }

    public UpgradeNode GetRootNode()
    {
        return nodes.Find(n => n != null && n.isRoot);
    }

    public void EnsureUniqueIds()
    {
        foreach (var n in nodes)
            if (string.IsNullOrEmpty(n.id))
                n.id = Guid.NewGuid().ToString();
    }

#if UNITY_EDITOR
    // editor helper
    public void EnsureOneRoot()
    {
        if (GetRootNode() == null && nodes.Count > 0)
        {
            nodes[0].isRoot = true;
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
