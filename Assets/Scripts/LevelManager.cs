﻿using System;
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
    private GameObject CarPrefab;

    [Header("Parents")]
    [SerializeField]
    private Transform EnvironmentParent;
    [SerializeField]
    private Transform BridgeParent;
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

    [Header("Renderer")]
    [SerializeField]
    private Material RoadMaterial;
    [SerializeField]
    private Material WoodMaterial;
    [SerializeField]
    private Material SteelMaterial;


    [HideInInspector] public Level level;
    [HideInInspector] public Solution solution;

    [HideInInspector] public Dictionary<string, Rigidbody2D> AnchorId2Rigidbody;
    [HideInInspector] public Dictionary<Connection, Rigidbody2D> Connection2Rigidbody;

    private BridgeMeshManager bridgeMeshManager;
    private EndZone endZone;
    private Dictionary<string, Rigidbody2D> id2Rigidbody;
    private List<Joint2D> joints;
    private List<Rigidbody2D> rigidbodies;
    private Rigidbody2D carRigidbody;

    private void Start()
    {
        level = new Level()
        {
            Name = "First Level",
            Rect = new Rect(Vector2.left * 4, new Vector2(8, 6)),
            FixedAnchors = new List<Anchor>(new[] { new Anchor(-4, 0), new Anchor(4, 0) }),
            StartPoint = new Vector3(-5, 0.5f, -0.5f),
            EndPoint = new Vector3(5, 0.5f),
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

        bridgeMeshManager = BridgeParent.gameObject.AddComponent<BridgeMeshManager>();
        bridgeMeshManager.LevelManager = this;
        bridgeMeshManager.RoadMaterial = RoadMaterial;
        bridgeMeshManager.WoodMaterial = WoodMaterial;
        bridgeMeshManager.SteelMaterial = SteelMaterial;




        CleanUpLevel(); // Removes everything
        CleanUpSolution(); // Removes only the solution
        GenerateLevel(level); // Generates environment + fixed anchors
        GenerateSolution(solution); // Generates only solution
        // Play();
        // ResetToSolution(solution); // Removes all stuff that changed during runtime + generates solution
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F5))
        {
            Play();
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

        id2Rigidbody = new Dictionary<string, Rigidbody2D>();
    }

    private void CleanUpSolution()
    {
        rigidbodies = new List<Rigidbody2D>();
        joints = new List<Joint2D>();

        // TODO use joints and rigidbodies without fixedAnchors for clean up
        foreach (var item in BridgeParent.GetChildrenArray())
        {
            var rigidbody = item.GetComponent<Rigidbody2D>();

            if (rigidbody != null)
            {
                var id = id2Rigidbody.FirstOrDefault(x => x.Value == rigidbody);

                if (id.Key != null && id.Key != "")
                {
                    if (!level.FixedAnchors.Exists(x => x.Id == id.Key))
                    {
                        id2Rigidbody.Remove(id.Key);
                    }
                    else
                    {
                        var components = item.GetComponents<Component>();
                        rigidbodies.Add(rigidbody);

                        foreach (var component in components)
                        {
                            if (components.GetType().IsSubclassOf(typeof(Joint2D)))
                            {
                                DestroyImmediate(component);
                            }
                        }

                        continue;
                    }
                }
            }

            DestroyImmediate(item.gameObject);
        }

        if (carRigidbody != null)
        {
            // handle car
            joints.AddRange(carRigidbody.GetComponents<WheelJoint2D>());
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
}