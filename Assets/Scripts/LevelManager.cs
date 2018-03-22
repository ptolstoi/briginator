using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    private GameObject StartLandPrefab;
    [SerializeField]
    private GameObject EndLandPrefab;
    [SerializeField]
    private GameObject CarPrefab;

    [Header("Parents")]
    [SerializeField]
    private Transform Environment;
    [SerializeField]

    private Transform Bridge;
    [Header("Layers")]
    [SerializeField]
    private LayerMask CarLayer;
    [SerializeField]
    private LayerMask BridgeLayer;

    [Header("Misc")]
    [SerializeField]
    private TextAsset[] levels;

    private Level level;
    private Solution solution;

    private void Start()
    {
        level = new Level()
        {
            Name = "First Level",
            Rect = new Rect(Vector2.left * 4, new Vector2(8, 6)),
            FixedAnchors = new List<Anchor>(new[] { new Anchor(-4, 0), new Anchor(4, 0) }),
            StartPoint = new Vector3(-5, 1),
            EndPoint = new Vector3(5, 1),
            StartLand = new Vector3(-4, 0),
            EndLand = new Vector3(4, 0),
        };

        var json = level.ToJSON();

        Debug.Log(json);

        var anchors = new List<Anchor>(new[] { new Anchor(0, 0.1f), new Anchor(-2, 2), new Anchor(2, 2) });

        solution = new Solution()
        {
            Anchors = anchors,
            Connections = new List<Connection>(new[] {
                new Connection(level.FixedAnchors[0].Id, anchors[0].Id, ConnectionType.Road),
                new Connection(anchors[0].Id, level.FixedAnchors[1].Id, ConnectionType.Road),
                new Connection(level.FixedAnchors[0].Id, anchors[1].Id, ConnectionType.Steel),
                new Connection(anchors[0].Id, anchors[1].Id, ConnectionType.Steel),
                new Connection(anchors[1].Id, anchors[2].Id, ConnectionType.Wood),
                new Connection(anchors[0].Id, anchors[2].Id, ConnectionType.Steel),
                new Connection(anchors[2].Id, level.FixedAnchors[1].Id, ConnectionType.Steel)
            })
        };

        json = solution.ToJSON();

        Debug.Log(json);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Load"))
        {
            level = Level.FromJSON(levels[0].text);
        }
    }

    private void OnDrawGizmos()
    {
        if (level == null)
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
            var anchorA = map[connection.IdA];
            var anchorB = map[connection.IdB];

            if (anchorA.Position.x > anchorB.Position.x)
            {
                var tmp = anchorA;
                anchorA = anchorB;
                anchorA = tmp;
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

}
