using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerInspector : Editor
{
    Rect targetRect;
    Rect sourceRect;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        try
        {
            var manager = target as LevelManager;
            var level = manager.level;

            if (level == null || level.FixedAnchors == null || level.FixedAnchors.Count == 0)
            {
                return;
            }

            var solution = manager.solution;

            sourceRect = level.Rect;

            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(level.Name);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            var currentWidth = GUILayoutUtility.GetLastRect().width;

            using (new GUILayout.HorizontalScope())
            {
                var width = currentWidth * 16;
                var ratio = sourceRect.height / sourceRect.width;
                var height = width * ratio * 16;

                targetRect = GUILayoutUtility.GetRect(width, height);
            }

            foreach (var anchor in level.FixedAnchors)
            {
                ExtendRectToContainPoint(ref sourceRect, anchor);
            }

            foreach (var anchor in solution.Anchors)
            {
                ExtendRectToContainPoint(ref sourceRect, anchor);
            }

            ExtendRectToContainPoint(ref sourceRect, level.StartLand);
            ExtendRectToContainPoint(ref sourceRect, level.StartPoint);
            ExtendRectToContainPoint(ref sourceRect, level.EndLand);
            ExtendRectToContainPoint(ref sourceRect, level.EndPoint);

            sourceRect.width += 0.5f;
            sourceRect.height += 0.5f;

            sourceRect.xMin -= 0.5f;
            sourceRect.yMin -= 0.5f;

            Handles.DrawSolidRectangleWithOutline(
                targetRect,
                new Color(0.15f, 0.15f, 0.15f, 1f),
                new Color(0.15f, 0.15f, 0.15f, 1f)
            );

            Handles.DrawSolidRectangleWithOutline(new[] {
            Project(level.Rect.xMin, level.Rect.yMin),
            Project(level.Rect.xMin, level.Rect.yMax),
            Project(level.Rect.xMax, level.Rect.yMax),
            Project(level.Rect.xMax, level.Rect.yMin)
        }, new Color(0.75f, 0.75f, 0.75f, 0.05f), new Color(0.75f, 0.75f, 0.75f, 1f));

            var prjPt = Project(level.StartLand);
            var landRect = Rect.MinMaxRect(targetRect.xMin, prjPt.y, prjPt.x, targetRect.yMax);
            Handles.DrawSolidRectangleWithOutline(landRect, new Color(0.588f, 0.294f, 0, 1), Color.clear);
            prjPt = Project(level.EndLand);
            landRect = Rect.MinMaxRect(prjPt.x, prjPt.y, targetRect.xMax, targetRect.yMax);
            Handles.DrawSolidRectangleWithOutline(landRect, new Color(0.588f, 0.294f, 0, 1), Color.clear);

            Handles.color = Handles.zAxisColor;
            Handles.DrawSolidDisc(Project(level.StartPoint), Vector3.forward, 4);
            Handles.DrawSolidDisc(Project(level.EndPoint), Vector3.forward, 4);
            Handles.ArrowHandleCap(0, Project(level.StartPoint), Quaternion.Euler(0, 90, 0), 15, EventType.Repaint);

            var lookup = new Dictionary<string, Vector2>();

            Handles.color = Handles.xAxisColor;
            foreach (var anchor in level.FixedAnchors)
            {
                Handles.DrawSolidDisc(Project(anchor), Vector3.forward, 4);
                lookup.Add(anchor.Id, anchor);
            }

            Handles.color = Handles.yAxisColor;
            foreach (var anchor in solution.Anchors)
            {
                Handles.DrawSolidDisc(Project(anchor), Vector3.forward, 4);
                lookup.Add(anchor.Id, anchor);
            }

            foreach (var con in solution.Connections)
            {
                Vector2 anchorA;
                if (!lookup.TryGetValue(con.IdA, out anchorA))
                {
                    continue;
                }

                Vector2 anchorB;
                if (!lookup.TryGetValue(con.IdB, out anchorB))
                {
                    continue;
                }

                if (con.Type == ConnectionType.Steel)
                {
                    Handles.color = Color.red;
                }
                else if (con.Type == ConnectionType.Wood)
                {
                    Handles.color = Color.yellow;
                }
                else
                {
                    Handles.color = Color.grey;
                }
                Handles.DrawLine(Project(anchorA), Project(anchorB));
            }

            GUILayout.Space(2);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    Vector3 Project(Vector3 v)
    {
        return new Vector3(
            Mathf.Lerp(targetRect.xMin, targetRect.xMax, (v.x - sourceRect.xMin) / sourceRect.width),
            Mathf.Lerp(targetRect.yMax, targetRect.yMin, (v.y - sourceRect.yMin) / sourceRect.height)
        );
    }
    Vector3 Project(float x, float y)
    {
        return Project(new Vector3(x, y));
    }

    void ExtendRectToContainPoint(ref Rect rect, Vector2 point)
    {
        if (rect.Contains(point))
        {
            return;
        }

        rect = Rect.MinMaxRect(
            Mathf.Min(rect.xMin, point.x),
            Mathf.Min(rect.yMin, point.y),
            Mathf.Max(rect.xMax, point.x),
            Mathf.Max(rect.yMax, point.y)
        );
    }
}