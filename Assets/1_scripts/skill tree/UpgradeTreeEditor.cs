using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class UpgradeTreeEditor : EditorWindow
{
    private UpgradeTree tree;
    private Vector2 panOffset = Vector2.zero;
    private Vector2 drag = Vector2.zero;

    private const float NODE_WIDTH = 180f;
    private const float NODE_HEIGHT = 120f;
    private GUIStyle nodeStyle;
    private GUIStyle selectedNodeStyle;
    private GUIStyle titleStyle;

    private UpgradeNode selectedNode = null;
    private string dragConnectionFromNodeId = null;
    private Vector2 dragConnectionToPos;

    private bool showLegend = true;
    private readonly Vector2 legendSize = new Vector2(300, 150);

    [MenuItem("Tools/Upgrade Tree Editor")]
    public static void OpenWindow()
    {
        var w = GetWindow<UpgradeTreeEditor>("Upgrade Tree");
        w.minSize = new Vector2(600, 400);
    }

    private void OnEnable()
    {
        nodeStyle = new GUIStyle(EditorStyles.helpBox);
        nodeStyle.alignment = TextAnchor.UpperLeft;
        nodeStyle.padding = new RectOffset(8, 8, 8, 8);

        selectedNodeStyle = new GUIStyle(nodeStyle);
        selectedNodeStyle.normal.background = MakeTex(2, 2, new Color(0.22f, 0.5f, 0.85f, 0.2f));

        titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.alignment = TextAnchor.UpperCenter;
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (tree == null)
        {
            EditorGUILayout.HelpBox("Please select an UpgradeTree asset in the toolbar to begin.", MessageType.Info);
            return;
        }

        tree.EnsureUniqueIds();
        tree.EnsureOneRoot();

        HandleInput(Event.current);

        DrawGrid(20, 0.08f);
        DrawGrid(100, 0.12f);

        DrawConnections();
        DrawNodes();

        DrawDraggingConnection();
        
        DrawLegendBox();

        if (GUI.changed) Repaint();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        var newTree = (UpgradeTree)EditorGUILayout.ObjectField(tree, typeof(UpgradeTree), false, GUILayout.Width(250));
        if (newTree != tree)
        {
            tree = newTree;
            selectedNode = null;
        }

        if (GUILayout.Button("Create New Tree", EditorStyles.toolbarButton))
        {
            var path = EditorUtility.SaveFilePanelInProject("Create UpgradeTree", "UpgradeTree", "asset", "Create UpgradeTree asset");
            if (!string.IsNullOrEmpty(path))
            {
                var newAsset = ScriptableObject.CreateInstance<UpgradeTree>();
                AssetDatabase.CreateAsset(newAsset, path);
                AssetDatabase.SaveAssets();
                Selection.activeObject = newAsset;
                tree = newAsset;
            }
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Add Node", EditorStyles.toolbarButton))
        {
            AddNodeAtCenter();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void AddNodeAtCenter()
    {
        if (tree == null) return;
        Undo.RecordObject(tree, "Add Node");
        var n = new UpgradeNode
        {
            id = System.Guid.NewGuid().ToString(),
            displayName = "Node " + (tree.nodes.Count + 1),
            editorPosition = (position.size / 2f) - panOffset - new Vector2(NODE_WIDTH / 2, NODE_HEIGHT / 2),
            type = UpgradeType.None
        };

        // if no root yet, make first node root
        if (tree.GetRootNode() == null)
            n.isRoot = true;

        tree.nodes.Add(n);
        EditorUtility.SetDirty(tree);
    }

    private void DrawGrid(float spacing, float opacity)
    {
        int widthDivs = Mathf.CeilToInt(position.width / spacing);
        int heightDivs = Mathf.CeilToInt(position.height / spacing);

        Handles.BeginGUI();
        Color oldColor = Handles.color;
        Handles.color = new Color(0.5f, 0.5f, 0.5f, opacity);

        Vector3 offset = new Vector3(panOffset.x % spacing, panOffset.y % spacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(spacing * i, -spacing, 0) + offset, new Vector3(spacing * i, position.height + spacing, 0) + offset);
        }
        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-spacing, spacing * j, 0) + offset, new Vector3(position.width + spacing, spacing * j, 0) + offset);
        }

        Handles.color = oldColor;
        Handles.EndGUI();
    }

    private void DrawNodes()
    {
        if (tree.nodes == null) return;

        // ensure root is centered if requested (optional behaviour)
        // draw nodes
        for (int i = 0; i < tree.nodes.Count; i++)
        {
            var n = tree.nodes[i];
            // visibility: only root visible start; editor shows all nodes but we can visually dim locked ones
            Rect nodeRect = new Rect(n.editorPosition + panOffset, new Vector2(NODE_WIDTH, NODE_HEIGHT));

            bool isSelected = selectedNode == n;
            var style = isSelected ? selectedNodeStyle : nodeStyle;

            GUI.Box(nodeRect, "", style);

            GUILayout.BeginArea(nodeRect);
            GUILayout.Space(4);
            GUILayout.Label(n.displayName + (n.isRoot ? " (Root)" : ""), titleStyle);
            GUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Type:", GUILayout.Width(40));
            n.type = (UpgradeType)EditorGUILayout.EnumPopup(n.type);
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select", GUILayout.Width(72)))
            {
                selectedNode = n;
            }
            if (GUILayout.Button("Connect", GUILayout.Width(72)))
            {
                // begin connection from this node
                dragConnectionFromNodeId = n.id;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.EndArea();

            // handle events for dragging the node
            HandleNodeEvents(n, nodeRect);
        }

        DrawInspectorPanel();
    }

    private void HandleNodeEvents(UpgradeNode node, Rect rect)
    {
        var e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
        {
            selectedNode = node;
            e.Use();
        }

        // node dragging with left mouse & alt/ctrl or when selected and dragging
        if (e.type == EventType.MouseDrag && e.button == 0 && selectedNode == node && rect.Contains(e.mousePosition))
        {
            Undo.RecordObject(tree, "Move Node");
            node.editorPosition += e.delta;
            EditorUtility.SetDirty(tree);
            e.Use();
        }

        // right-click context
        if (e.type == EventType.ContextClick && rect.Contains(e.mousePosition))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Set as Root"), node.isRoot, () =>
            {
                Undo.RecordObject(tree, "Set Root");
                foreach (var n in tree.nodes) n.isRoot = false;
                node.isRoot = true;
                EditorUtility.SetDirty(tree);
            });
            menu.AddItem(new GUIContent("Delete Node"), false, () =>
            {
                if (EditorUtility.DisplayDialog("Delete Node", "Delete node '" + node.displayName + "'? This will remove any connections to it.", "Delete", "Cancel"))
                {
                    Undo.RecordObject(tree, "Delete Node");
                    // remove references
                    foreach (var other in tree.nodes)
                    {
                        other.connections.RemoveAll(id => id == node.id);
                    }
                    tree.nodes.Remove(node);
                    selectedNode = null;
                    EditorUtility.SetDirty(tree);
                }
            });
            menu.ShowAsContext();
            e.Use();
        }
    }

    private void DrawConnections()
    {
        Handles.BeginGUI();

        foreach (var n in tree.nodes)
        {
            foreach (var tid in n.connections)
            {
                var target = tree.GetNodeById(tid);
                if (target == null) continue;

                // Compute dynamic endpoints
                Vector2 start = GetConnectionPoint(n, target.editorPosition + panOffset + new Vector2(NODE_WIDTH / 2f, NODE_HEIGHT / 2f));
                Vector2 end = GetConnectionPoint(target, n.editorPosition + panOffset + new Vector2(NODE_WIDTH / 2f, NODE_HEIGHT / 2f));

                // Tangents based on direction
                Vector2 dir = (end - start).normalized;
                Vector2 startTan = start + dir * 60f;
                Vector2 endTan = end - dir * 60f;

                Handles.DrawBezier(start, end, startTan, endTan, Color.white, null, 3f);

                DrawArrow(end, dir);
            }
        }

        Handles.EndGUI();
    }

    private void DrawArrow(Vector2 pos, Vector2 dir)
    {
        float size = 10f;
        Vector2 perp = new Vector2(-dir.y, dir.x) * 0.5f * size;

        Vector3 p1 = pos;
        Vector3 p2 = pos - dir * size + perp;
        Vector3 p3 = pos - dir * size - perp;

        Handles.DrawAAConvexPolygon(p1, p2, p3);
    }


    private void DrawDraggingConnection()
    {
        if (!string.IsNullOrEmpty(dragConnectionFromNodeId))
        {
            Handles.BeginGUI();
            Vector2 fromPos = Vector2.zero;
            var fromNode = tree.GetNodeById(dragConnectionFromNodeId);
            if (fromNode != null)
                fromPos = fromNode.editorPosition + panOffset + new Vector2(NODE_WIDTH, NODE_HEIGHT / 2f - 8);

            Vector2 toPos = dragConnectionToPos;
            Vector2 startTan = fromPos + Vector2.right * 80f;
            Vector2 endTan = toPos + Vector2.left * 80f;

            Handles.DrawBezier(fromPos, toPos, startTan, endTan, Color.yellow, null, 3f);
            Handles.EndGUI();
        }
    }

    private void DrawLegendBox()
    {
        float margin = 10f;

        Rect toggleRect = new Rect(
            margin,
            position.height - margin - 22,
            80,
            22
        );

        // Toggle button
        if (GUI.Button(toggleRect, showLegend ? "Hide Help" : "Show Help"))
            showLegend = !showLegend;

        if (!showLegend) return;

        Rect legendRect = new Rect(
            margin,
            position.height - margin - legendSize.y - 30,
            legendSize.x,
            legendSize.y
        );

        GUI.Box(legendRect, "");

        GUILayout.BeginArea(legendRect);
        GUILayout.Label("Editor Legend", EditorStyles.boldLabel);

        GUILayout.Label("• Middle Mouse empty space = Pan canvas");
        GUILayout.Label("• Left-click + drag node = Move node");
        GUILayout.Label("• Click 'Connect' then click another node = Create link");
        GUILayout.Label("• Right-click node = Context menu (root/delete)");
        GUILayout.Label("• Mouse up during connection drag = Finish link");
        GUILayout.Label("• Alt + click on connection midpoint = Remove link");

        GUILayout.EndArea();
    }


    private void HandleInput(Event e)
    {
        // panning with middle mouse or alt+left-drag
        if (e.type == EventType.MouseDrag && (e.button == 2 || (e.button == 0 && e.alt)))
        {
            panOffset += e.delta;
            e.Use();
        }

        // update dragConnectionToPos while dragging
        if (!string.IsNullOrEmpty(dragConnectionFromNodeId))
        {
            dragConnectionToPos = e.mousePosition;
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                // attempt to connect to node under mouse
                UpgradeNode hit = GetNodeAtPosition(e.mousePosition - panOffset);
                if (hit != null && hit.id != dragConnectionFromNodeId)
                {
                    Undo.RecordObject(tree, "Add Connection");
                    var from = tree.GetNodeById(dragConnectionFromNodeId);
                    if (!from.connections.Contains(hit.id))
                    {
                        from.connections.Add(hit.id);
                        EditorUtility.SetDirty(tree);
                    }
                }
                dragConnectionFromNodeId = null;
                e.Use();
            }
        }

        // delete connection by alt+click on a connection midpoint (nice-to-have)
        if (e.type == EventType.MouseDown && e.button == 0 && e.alt)
        {
            var hitConn = GetConnectionUnderMouse(e.mousePosition);
            if (hitConn != null)
            {
                var from = hitConn.Value.from;
                var to = hitConn.Value.to;
                if (EditorUtility.DisplayDialog("Delete Connection", $"Remove connection from '{from.displayName}' to '{to.displayName}'?", "Remove", "Cancel"))
                {
                    Undo.RecordObject(tree, "Remove Connection");
                    from.connections.RemoveAll(id => id == to.id);
                    EditorUtility.SetDirty(tree);
                }
            }
        }
    }

    private (UpgradeNode from, UpgradeNode to)? GetConnectionUnderMouse(Vector2 mouse)
    {
        // rudimentary: check distance to mid point of each connection
        foreach (var n in tree.nodes)
        {
            Vector2 start = n.editorPosition + panOffset + new Vector2(NODE_WIDTH, NODE_HEIGHT / 2f - 8);
            foreach (var tid in n.connections)
            {
                var toNode = tree.GetNodeById(tid);
                if (toNode == null) continue;
                Vector2 end = toNode.editorPosition + panOffset + new Vector2(0, NODE_HEIGHT / 2f - 8);
                Vector2 mid = (start + end) * 0.5f;
                if (Vector2.Distance(mouse, mid) < 8f)
                    return (n, toNode);
            }
        }
        return null;
    }

    private UpgradeNode GetNodeAtPosition(Vector2 localPos)
    {
        // localPos is mouse - panOffset; nodes use editorPosition
        foreach (var n in tree.nodes)
        {
            Rect r = new Rect(n.editorPosition, new Vector2(NODE_WIDTH, NODE_HEIGHT));
            if (r.Contains(localPos))
                return n;
        }
        return null;
    }

    private void DrawInspectorPanel()
    {
        GUILayout.BeginArea(new Rect(position.width - 300, 20, 280, position.height - 40), EditorStyles.helpBox);

        GUILayout.Label("Inspector", EditorStyles.boldLabel);
        if (selectedNode == null)
        {
            GUILayout.Label("Select a node to edit.");
        }
        else
        {
            EditorGUI.BeginChangeCheck();

            selectedNode.displayName = EditorGUILayout.TextField("Name", selectedNode.displayName);
            selectedNode.isRoot = EditorGUILayout.Toggle("Is Root", selectedNode.isRoot);
            selectedNode.type = (UpgradeType)EditorGUILayout.EnumPopup("Upgrade Type", selectedNode.type);
            selectedNode.baseValue = EditorGUILayout.FloatField("Base Value", selectedNode.baseValue);
            selectedNode.incrementValue = EditorGUILayout.FloatField("Increment", selectedNode.incrementValue);
            selectedNode.isPercentage = EditorGUILayout.Toggle("IsPercentage", selectedNode.isPercentage);
            selectedNode.maxLevel = EditorGUILayout.IntField("Max Level", Mathf.Max(1, selectedNode.maxLevel));
            selectedNode.editorPosition = EditorGUILayout.Vector2Field("Position", selectedNode.editorPosition);

            GUILayout.Space(8);
            GUILayout.Label("Level Costs:", EditorStyles.boldLabel);

            int newCount = Mathf.Max(1, selectedNode.maxLevel);
            while (selectedNode.levelCosts.Count < newCount)
                selectedNode.levelCosts.Add(0f);
            while (selectedNode.levelCosts.Count > newCount)
                selectedNode.levelCosts.RemoveAt(selectedNode.levelCosts.Count - 1);

            for (int i = 0; i < selectedNode.levelCosts.Count; i++)
            {
                selectedNode.levelCosts[i] = EditorGUILayout.FloatField(
                    $"Level {i + 1} Cost",
                    selectedNode.levelCosts[i]
                );
            }


            GUILayout.Space(8);
            GUILayout.Label("Connections:");
            if (selectedNode.connections == null) selectedNode.connections = new List<string>();

            for (int i = 0; i < selectedNode.connections.Count; i++)
            {
                var id = selectedNode.connections[i];
                var node = tree.GetNodeById(id);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(node != null ? node.displayName : "<missing>", GUILayout.Width(150));
                if (GUILayout.Button("Go", GUILayout.Width(40)))
                {
                    selectedNode = node;
                }
                if (GUILayout.Button("X", GUILayout.Width(24)))
                {
                    Undo.RecordObject(tree, "Remove Connection");
                    selectedNode.connections.RemoveAt(i);
                    EditorUtility.SetDirty(tree);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(6);
            if (GUILayout.Button("Add connection to..."))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var n in tree.nodes)
                {
                    if (n.id == selectedNode.id) continue;
                    var target = n;
                    menu.AddItem(new GUIContent(target.displayName), false, () =>
                    {
                        Undo.RecordObject(tree, "Add Connection");
                        if (!selectedNode.connections.Contains(target.id))
                        {
                            selectedNode.connections.Add(target.id);
                            EditorUtility.SetDirty(tree);
                        }
                    });
                }
                menu.ShowAsContext();
            }

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(tree);

            GUILayout.Space(12);
            if (GUILayout.Button("Frame Selected"))
            {
                // center selected node in view
                panOffset = -selectedNode.editorPosition + (position.size / 2f) - new Vector2(NODE_WIDTH / 2, NODE_HEIGHT / 2);
            }
        }

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
        {
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }
        if (GUILayout.Button("Auto-arrange"))
        {
            AutoArrange();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private void AutoArrange()
    {
        // Simple radial layout around root for first-degree nodes, then cascades.
        var root = tree.GetRootNode();
        if (root == null) return;
        Undo.RecordObject(tree, "Auto Arrange");
        float radius = 220f;
        root.editorPosition = Vector2.zero;

        var level1 = new List<UpgradeNode>();
        foreach (var id in root.connections)
        {
            var n = tree.GetNodeById(id);
            if (n != null) level1.Add(n);
        }

        for (int i = 0; i < level1.Count; i++)
        {
            float angle = (i / (float)Mathf.Max(1, level1.Count)) * Mathf.PI * 2f;
            level1[i].editorPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        // second layer (children of level1) -- basic placement outward from parent
        float radius2 = 420f;
        foreach (var parent in level1)
        {
            var children = new List<UpgradeNode>();
            foreach (var id in parent.connections)
            {
                var n = tree.GetNodeById(id);
                if (n != null && n != root && !level1.Contains(n)) children.Add(n);
            }
            for (int i = 0; i < children.Count; i++)
            {
                float angle = (i / (float)Mathf.Max(1, children.Count)) * Mathf.PI * 2f;
                children[i].editorPosition = parent.editorPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 160f;
            }
        }

        EditorUtility.SetDirty(tree);
    }

    private Vector2 GetConnectionPoint(UpgradeNode node, Vector2 targetPos)
    {
        Vector2 nodePos = node.editorPosition + panOffset;
        Rect rect = new Rect(nodePos, new Vector2(NODE_WIDTH, NODE_HEIGHT));

        Vector2 center = rect.center;
        Vector2 dir = (targetPos - center).normalized;

        // Absolute direction
        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);

        // Horizontal vs vertical dominance
        if (absX > absY)
        {
            // LEFT or RIGHT side
            if (dir.x > 0)
                return new Vector2(rect.xMax, rect.center.y); // right
            else
                return new Vector2(rect.xMin, rect.center.y); // left
        }
        else
        {
            // TOP or BOTTOM side
            if (dir.y > 0)
                return new Vector2(rect.center.x, rect.yMax); // top
            else
                return new Vector2(rect.center.x, rect.yMin); // bottom
        }
    }


    private Texture2D MakeTex(int width, int height, Color col)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = col;

        tex.SetPixels(pixels);
        tex.Apply();

        return tex;
    }

}
