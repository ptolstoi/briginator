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
    private float RoadHeight = 0.1f;


    public Level level { get; private set; }
    public Solution solution { get; private set; }

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
            StartPoint = new Vector3(-5, 0.5f),
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

    private void GenerateLevel(Level level)
    {
        Debug.Assert(id2Rigidbody == null || id2Rigidbody.Count == 0, "Previous level not cleaned up");

        if (id2Rigidbody == null || id2Rigidbody.Count != 0)
        {
            id2Rigidbody = new Dictionary<string, Rigidbody2D>();
        }

        rigidbodies = new List<Rigidbody2D>();
        joints = new List<Joint2D>();

        var startLand = Instantiate(StartLandPrefab, level.StartLand, Quaternion.identity) as GameObject;
        startLand.name = "Start Land";
        var startLandTransform = startLand.transform;
        startLandTransform.SetParent(EnvironmentParent);
        startLand.layer = EnvironmentLayer;

        var endLand = Instantiate(EndLandPrefab, level.EndLand, Quaternion.identity) as GameObject;
        endLand.name = "End Land";
        var endLandTransform = endLand.transform;
        endLandTransform.SetParent(EnvironmentParent);
        endLand.layer = EnvironmentLayer;

        foreach (var anchor in level.FixedAnchors)
        {
            GenerateAnchor(anchor, "Fixed Anchor").
                constraints = RigidbodyConstraints2D.FreezeAll;
        }

        var endZoneGo = new GameObject("End Zone");
        var endZoneTransform = endZoneGo.transform;
        endZoneTransform.position = level.EndPoint;
        endZoneTransform.localScale = new Vector3(0.2f, 5, 1);
        endZoneTransform.SetParent(EnvironmentParent);
        var endZoneCollider = endZoneGo.AddComponent<BoxCollider2D>();
        endZoneCollider.isTrigger = true;
        endZone = endZoneGo.AddComponent<EndZone>();
        endZone.LevelManager = this;

        var carGO = Instantiate(CarPrefab, level.StartPoint, Quaternion.identity) as GameObject;
        carRigidbody = carGO.GetComponent<Rigidbody2D>();
        rigidbodies.Add(carRigidbody);
        carGO.transform.SetParent(EnvironmentParent);
        carGO.layer = CarLayer;
        joints.AddRange(carGO.GetComponents<WheelJoint2D>());
        var rigidbodiesInCar = carGO.GetComponentsInChildren<Rigidbody2D>();
        foreach (var r in rigidbodiesInCar)
        {
            r.isKinematic = true;
        }
        rigidbodies.AddRange(rigidbodiesInCar);
        endZone.CarCollider = carGO.GetComponent<Collider2D>();
    }

    Rigidbody2D GenerateAnchor(Anchor anchor, string name = "Anchor")
    {
        var go = new GameObject(name);
        var anchorTransform = go.transform;
        anchorTransform.position = anchor;
        anchorTransform.SetParent(BridgeParent);

        var rigidbody = go.AddComponent<Rigidbody2D>();

        id2Rigidbody.Add(anchor.Id, rigidbody);
        rigidbodies.Add(rigidbody);

        DecorateAnchor(go, anchor);

        return rigidbody;
    }

    private void GenerateSolution(Solution solution)
    {
        solution.Anchors.ForEach(x =>
        {
            var rb = GenerateAnchor(x);
            rb.isKinematic = true;
        });

        foreach (var Connection in solution.Connections)
        {
            var rbA = id2Rigidbody[Connection.IdA];
            var rbB = id2Rigidbody[Connection.IdB];
            var transformA = rbA.transform;
            var transformB = rbB.transform;

            if (transformA.position.x > transformB.position.x)
            {
                Swap(ref rbA, ref rbB);
                Swap(ref transformA, ref transformB);
            }

            var goA = rbA.gameObject;
            var goB = rbB.gameObject;

            var anchorDistance = Vector3.Distance(transformA.position, transformB.position);

            var springJoint = goA.AddComponent<SpringJoint2D>();
            springJoint.enabled = false;
            springJoint.autoConfigureDistance = false;
            springJoint.connectedBody = rbB;

            springJoint.frequency = ConnectionSpringFrequency;
            // springJoint.breakForce = maxForce;

            springJoint.distance = anchorDistance;
            springJoint.dampingRatio = ConnectionSpringDampingRatio;
#if UNITY_EDITOR
            if (goA.GetComponent<SpringVisualizer>() == null)
            {
                goA.AddComponent<SpringVisualizer>();
            }
#endif

            joints.Add(springJoint);


            if (Connection.Type == ConnectionType.Road)
            {
                var slopeAngle = Vector3.SignedAngle(transformB.position - transformA.position, Vector3.right, -Vector3.forward);
                var roadGameObject = new GameObject("Road");
                var roadTransform = roadGameObject.transform;
                roadTransform.position = (transformA.position + transformB.position) / 2;
                roadTransform.SetParent(BridgeParent);
                roadGameObject.layer = CarLayer;
                roadTransform.eulerAngles = new Vector3(
                    0,
                    0,
                    slopeAngle
                );
                roadTransform.localScale = new Vector3(
                    anchorDistance,
                    RoadHeight,
                    1
                );
                var collider = roadGameObject.AddComponent<BoxCollider2D>();
                collider.offset += Vector2.down / 2;
                var rbRoad = roadGameObject.AddComponent<Rigidbody2D>();
                rbRoad.isKinematic = true;
                rigidbodies.Add(rbRoad);

                var hingeSpring = roadGameObject.AddComponent<HingeJoint2D>();
                joints.Add(hingeSpring);
                hingeSpring.enabled = false;
                hingeSpring.connectedBody = rbA;
                hingeSpring.anchor = Vector2.left / 2;

                hingeSpring = roadGameObject.AddComponent<HingeJoint2D>();
                joints.Add(hingeSpring);
                hingeSpring.enabled = false;
                hingeSpring.connectedBody = rbB;
                hingeSpring.anchor = Vector2.right / 2;
            }
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
