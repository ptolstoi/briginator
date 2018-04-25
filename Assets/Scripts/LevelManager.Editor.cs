using UnityEngine;
using System;
using System.Collections.Generic;

using static Extensions;

public partial class LevelManager
{
#if UNITY_EDITOR 
    private void OnGUI()
    {
        if (UnityEditor.Selection.activeGameObject != this.gameObject)
        {
            return;
        }

        foreach (var level in levels)
        {
            if (GUILayout.Button($"Load {level.name}"))
            {
                this.level = Level.FromJSON(level.text);
                this.solution = new Solution();
                TransitionTo(GameState.Edit);
            }
        }

        if (GUILayout.Button("Solution"))
        {
            CleanUpSolution();
            GenerateSolution(solution);
        }
    }

    private void _OnDrawGizmosSelected()
    {
        if (level == null || level.FixedAnchors == null || level.FixedAnchors.Count == 0)
        {
            return;
        }

        var size = (Vector3)level.Rect.size;
        Gizmos.DrawWireCube(level.Rect.center, size);
        Gizmos.color = new Color(0.588f, 0.294f, 0);
        Gizmos.DrawCube(level.StartLand - Vector3.up - Vector3.right, Vector3.one * 2);
        Gizmos.DrawCube(level.StartLand - Vector3.right * 2.5f, new Vector3(1, 4, 2));
        Gizmos.DrawCube(level.EndLand - Vector3.up + Vector3.right, Vector3.one * 2);
        Gizmos.DrawCube(level.EndLand + Vector3.right * 2.5f, new Vector3(1, 4, 2));

        var waterSize = new Vector3(level.EndLand.x - level.StartLand.x, 1, 2);
        var startEndCenter = (level.StartLand + level.EndLand) / 2;
        startEndCenter.y = Mathf.Min(level.StartLand.y, level.EndLand.y) - 1.5f;

        Gizmos.color = new Color(0.3f, 0.3f, 1f);
        Gizmos.DrawCube(startEndCenter, waterSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(level.StartPoint - new Vector3(1 / 2, 0.5f / 2, 0), new Vector3(1f, 0.5f, 0));
        Gizmos.color = Color.green;
        Gizmos.DrawCube(level.EndPoint - new Vector3(0.2f / 2, 2 / 2, 0), new Vector3(0.2f, 2, 0));

        Gizmos.color = Color.white;

        var map = new Dictionary<string, Anchor>();

        foreach (var anchor in level.FixedAnchors)
        {
            map.Add(anchor.Id, anchor);
            Gizmos.DrawSphere(anchor, 0.2f);
        }

        if (solution == null)
        {
            return;
        }
        Gizmos.color = Color.grey;
        foreach (var anchor in solution.Anchors)
        {
            map.Add(anchor.Id, anchor);
            Gizmos.DrawSphere(anchor, 0.2f);
        }

        var matrix = Gizmos.matrix;

        foreach (var connection in solution.Connections)
        {
            Anchor anchorA;
            if (!map.TryGetValue(connection.IdA, out anchorA))
            {
                continue;
            }

            Anchor anchorB;
            if (!map.TryGetValue(connection.IdB, out anchorB))
            {
                continue;
            }

            if (anchorA.Position.x > anchorB.Position.x)
            {
                Swap(ref anchorA, ref anchorB);
            }

            var center = ((Vector3)anchorA.Position + anchorB) / 2;
            var blockSize = new Vector3(
                Vector3.Distance(anchorA, anchorB),
                0.25f,
                1f
            );
            var rotation = Quaternion.Euler(
                0, 0,
                Vector3.SignedAngle(anchorB.Position - anchorA.Position, Vector3.right, -Vector3.forward)
            );

            if (connection.Type == ConnectionType.Road)
            {
                Gizmos.color = Color.grey;
            }
            else
            {
                if (connection.Type == ConnectionType.Wood)
                {
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.color = Color.red;
                }
                center += Vector3.forward * (0.5f + 0.25f / 2);
                blockSize.z = 0.25f;
            }

            Gizmos.matrix = Matrix4x4.TRS(center, rotation, blockSize);

            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }

        Gizmos.matrix = matrix;
    }

    private void DrawIcon(GameObject gameObject, int idx)
    {
        var largeIcons = GetTextures("sv_icon_dot", "_pix16_gizmo", 0, 8);
        var icon = largeIcons[idx];
        var egu = typeof(UnityEditor.EditorGUIUtility);
        var flags = System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
        var args = new object[] { gameObject, icon.image };
        var setIcon = egu.GetMethod("SetIconForObject", flags, null, new Type[] { typeof(UnityEngine.Object), typeof(Texture2D) }, null);
        setIcon.Invoke(null, args);
    }
    private GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
    {
        GUIContent[] array = new GUIContent[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = UnityEditor.EditorGUIUtility.IconContent(baseName + (startIndex + i) + postFix);
        }
        return array;
    }
#endif

    void DecorateAnchor(GameObject go, Anchor anchor)
    {
#if UNITY_EDITOR
        DrawIcon(go, 3);
#endif
    }
}