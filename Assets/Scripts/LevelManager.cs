using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using static Extensions;

public partial class LevelManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    private GameObject StartLandPrefab;
    [SerializeField]
    private GameObject EndLandPrefab;
    [SerializeField]
    private GameObject WaterPrefab;
    [SerializeField]
    private GameObject EndZonePrefab;
    [SerializeField]
    private GameObject CarPrefab;

    [Header("Parents")]
    [SerializeField]
    private Transform EnvironmentParent;
    [SerializeField]
    private Transform BridgeParent;
    [SerializeField]
    public Transform MeshParent;
    [Header("Layers")]
    [SerializeField, Layer]
    private int CarLayer;
    [SerializeField, Layer]
    private int BridgeLayer;
    [SerializeField, Layer]
    private int EnvironmentLayer;

    [Header("Misc")]
    [SerializeField]
    private TextAsset[] levels;

    [SerializeField]
    private float ConnectionSpringFrequency = 25f;
    [SerializeField]
    private float ConnectionSpringDampingRatio = 0.5f;
    [SerializeField]
    private float MaximalForce = 4000;
    [SerializeField]
    public float RoadHeight = 0.1f;
    public float gridSize = 0.5f;

    [Header("Wiring")]
    [SerializeField]
    private BridgeMeshManager bridgeMeshManager;
    [SerializeField]
    private LevelEditorManager levelEditorManager;


    [HideInInspector]
    public Level level;
    [HideInInspector] public Solution solution;

    public Dictionary<string, Rigidbody2D> AnchorId2Rigidbody;
    public Dictionary<Connection, Rigidbody2D> Connection2Rigidbody;

    public float hexHeight => Mathf.Sqrt(3) / 2 * gridSize;

    private EndZone endZone;
    private Dictionary<string, Rigidbody2D> id2Rigidbody;
    private List<Joint2D> joints;
    private List<Rigidbody2D> rigidbodies;
    private Rigidbody2D carRigidbody;

    private void Start()
    {
        animator = GetComponent<Animator>();

        level = new Level()
        {
            Name = "First Level",
            Rect = new Rect(Vector2.left * 4, new Vector2(8, 6)),
            FixedAnchors = new List<Anchor>(new[] { new Anchor(-4, 0), new Anchor(4, 0) }),
            StartPoint = new Vector3(-5, 0.25f, -0.5f),
            EndPoint = new Vector3(5, 0),
            StartLand = new Vector3(-4, 0),
            EndLand = new Vector3(4, 0),
        };

        var json = level.ToJSON();

        Debug.Log(json);

        level = Level.FromJSON(levels[0].text);

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

        solution = new Solution();

        TransitionTo(GameState.Edit);

        CleanUpLevel(); // Removes everything
        // CleanUpSolution(); // Removes only the solution
        GenerateLevel(level); // Generates environment + fixed anchors
        GenerateSolution(solution); // Generates only solution
        // Play();
        // ResetToSolution(solution); // Removes all stuff that changed during runtime + generates solution
    }

    private void Update()
    {
        if (Mode == GameState.Play)
        {
            if (Input.GetKeyUp(KeyCode.F5) || Input.GetKeyUp(KeyCode.Space))
            {
                TransitionTo(GameState.Run);
            }
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                TransitionTo(GameState.Edit);
            }
        }
        else if (Mode == GameState.Edit)
        {
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                TransitionTo(GameState.Play);
            }
        }
        else if (Mode == GameState.Run)
        {
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                TransitionTo(GameState.Edit);
            }
        }
    }

    private void CleanUpLevel()
    {
        foreach (var item in EnvironmentParent.GetChildrenArray())
        {
            DestroyImmediate(item.gameObject);
        }

        foreach (var item in BridgeParent.GetChildrenArray())
        {
            DestroyImmediate(item.gameObject);
        }

        foreach (var item in MeshParent.GetChildrenArray())
        {
            DestroyImmediate(item.gameObject);
        }

        id2Rigidbody = new Dictionary<string, Rigidbody2D>();
    }

    private void CleanUpSolution()
    {
        CleanUpLevel();
        GenerateLevel(level);
    }

    public void ModeChanged(GameState? prev, GameState mode, GameState? next)
    {
        if (mode == GameState.Run)
        {
            Play();
        }
        else if (mode == GameState.Transition && next == GameState.Edit)
        {
            CleanUpLevel();
            GenerateLevel(level);
            GenerateSolution(solution);
        }
    }

    private void Play()
    {
        rigidbodies.ForEach(x => x.isKinematic = false);
        joints.ForEach(x => x.enabled = true);
        var rigidbodiesInCar = carRigidbody.GetComponentsInChildren<Rigidbody2D>();
        foreach (var r in rigidbodiesInCar)
        {
            r.isKinematic = false;
        }
        carRigidbody.isKinematic = false;
    }

    public void OnEnterEndZone()
    {
        var rigidbodiesInCar = carRigidbody.GetComponentsInChildren<Rigidbody2D>();
        foreach (var r in rigidbodiesInCar)
        {
            r.isKinematic = true;
        }

        carRigidbody.velocity = Vector2.zero;
        carRigidbody.angularVelocity = 0;
    }

    private void OnSolutionChangeHandler()
    {
        var solution = this.solution;
        var level = this.level;
        CleanUpLevel();
        GenerateLevel(level);
        GenerateSolution(solution);
    }

    Anchor getNearest(List<Anchor> list, Vector2 pos) =>
        list
            ?.Select(anchor => new { anchor, distance = Vector2.Distance(anchor.Position, pos) })
            ?.Where(t => t.distance < this.gridSize)
            ?.OrderBy(y => y.distance)
            ?.FirstOrDefault()
            ?.anchor;

    public Anchor GetNearestAnchor(Vector2 pos)
    {
        if (level != null)
        {
            var nearest = getNearest(level.FixedAnchors, pos);
            if (nearest != null)
            {
                return nearest;
            }
        }

        return getNearest(solution?.Anchors, pos);
    }
}
