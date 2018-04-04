﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using static Extensions;

public partial class LevelManager : MonoBehaviour
{
    private void GenerateLevel(Level level)
    {
        Debug.Assert(id2Rigidbody == null || id2Rigidbody.Count == 0, "Previous level not cleaned up");

        if (id2Rigidbody == null || id2Rigidbody.Count != 0)
        {
            id2Rigidbody = new Dictionary<string, Rigidbody2D>();
        }

        rigidbodies = new List<Rigidbody2D>();
        joints = new List<Joint2D>();
        AnchorId2Rigidbody = new Dictionary<string, Rigidbody2D>();
        Connection2Rigidbody = new Dictionary<Connection, Rigidbody2D>();

        Create(
            name: "Start Land",

            prefab: StartLandPrefab,
            position: level.StartLand,
            parent: EnvironmentParent,
            layer: EnvironmentLayer
        );

        Create(
            name: "End Land",

            prefab: EndLandPrefab,
            position: level.EndLand,
            parent: EnvironmentParent,
            layer: EnvironmentLayer
        );

        foreach (var anchor in level.FixedAnchors)
        {
            GenerateAnchor(anchor, fixedAnchor: true);
        }

        Create(
            name: "End Zone",

            position: level.EndPoint,
            scale: new Vector3(0.2f, 5, 1),
            parent: EnvironmentParent,

            afterCreate: (go) =>
            {
                var endZoneCollider = go.AddComponent<BoxCollider2D>();
                endZoneCollider.isTrigger = true;
                endZone = go.AddComponent<EndZone>();
                endZone.LevelManager = this;
            }
        );

        Create(
            name: "Car",

            prefab: CarPrefab,
            position: level.StartPoint,
            layer: CarLayer,
            parent: EnvironmentParent,

            afterCreate: (go) =>
            {
                carRigidbody = go.GetComponent<Rigidbody2D>();
                rigidbodies.Add(carRigidbody);
                joints.AddRange(go.GetComponents<WheelJoint2D>());

                var rigidbodiesInCar = go.GetComponentsInChildren<Rigidbody2D>();

                foreach (var r in rigidbodiesInCar)
                {
                    r.isKinematic = true;
                }

                rigidbodies.AddRange(rigidbodiesInCar);

                endZone.CarCollider = go.GetComponentInChildren<Collider2D>();
            }
        );
    }

    Rigidbody2D GenerateAnchor(Anchor anchor, string name = "Anchor", bool fixedAnchor = false)
    {
        return Create(
            name: fixedAnchor ? "Fixed " : "" + name,

            position: anchor,
            parent: BridgeParent,
            afterCreate: go =>
            {
                var rigidbody = go.AddComponent<Rigidbody2D>();

                if (fixedAnchor)
                {
                    rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
                }

                AnchorId2Rigidbody[anchor.Id] = rigidbody;

                id2Rigidbody.Add(anchor.Id, rigidbody);
                rigidbodies.Add(rigidbody);

                DecorateAnchor(go, anchor);

                bridgeMeshManager.GenerateMeshFor( // TODO bin ich richtig hier? xD
                    anchor: anchor,
                    atGameObject: go
                );
            }
        ).GetComponent<Rigidbody2D>();
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
            springJoint.breakForce = MaximalForce;

            springJoint.distance = anchorDistance;
            springJoint.dampingRatio = ConnectionSpringDampingRatio;

            var connectionManager = goA.AddComponent<ConnectionManager>();
            connectionManager.meshGenerator = bridgeMeshManager;
            connectionManager.connectionJoint = springJoint;
            connectionManager.connection = Connection;


#if UNITY_EDITOR
            goA.EnsureComponent<SpringVisualizer>();
#endif

            joints.Add(springJoint);


            if (Connection.Type == ConnectionType.Road)
            {
                var slopeAngle = Vector3.SignedAngle(transformB.position - transformA.position, Vector3.right, -Vector3.forward);

                Create(
                    name: "Road",
                    position: (transformA.position + transformB.position) / 2,
                    rotation: Quaternion.Euler(0, 0, slopeAngle),
                    scale: new Vector3(anchorDistance, RoadHeight, 1),
                    parent: BridgeParent,
                    layer: BridgeLayer,

                    afterCreate: go =>
                    {
                        var collider = go.AddComponent<BoxCollider2D>();
                        collider.offset += Vector2.down / 2;

                        var rbRoad = go.AddComponent<Rigidbody2D>();
                        rbRoad.isKinematic = true;
                        rigidbodies.Add(rbRoad);

                        var hingeSpring = go.AddComponent<HingeJoint2D>();
                        joints.Add(hingeSpring);
                        hingeSpring.enabled = false;
                        hingeSpring.connectedBody = rbA;
                        hingeSpring.anchor = Vector2.left / 2;

                        hingeSpring = go.AddComponent<HingeJoint2D>();
                        joints.Add(hingeSpring);
                        hingeSpring.enabled = false;
                        hingeSpring.connectedBody = rbB;
                        hingeSpring.anchor = Vector2.right / 2;

                        Connection2Rigidbody[Connection] = rbRoad;

                        connectionManager.connectedBlock = go;
                    }
                );
            }
        }
    }

    GameObject Create(
            GameObject prefab = null,
            string name = null,
            Vector3 position = default(Vector3),
            Vector3? scale = null,
            Quaternion rotation = default(Quaternion),
            int layer = -1,
            Transform parent = null,
            Action<GameObject> afterCreate = null,
            params Type[] components
        )
    {
        Transform trnsfrm = null;
        GameObject obj = null;

        if (prefab == null)
        {
            obj = new GameObject(name);
        }
        else
        {
            obj = Instantiate(prefab, position, rotation);
        }

        if (name != null) obj.name = name;

        trnsfrm = obj.transform;
        trnsfrm.position = position;
        if (scale.HasValue)
        {
            trnsfrm.localScale = scale.Value;
        }
        trnsfrm.rotation = rotation;

        if (layer > 0) obj.SetLayerRecursive(layer);

        if (parent != null) trnsfrm.SetParent(parent);

        foreach (var type in components)
        {
            obj.AddComponent(type);
        }

        if (afterCreate != null) afterCreate(obj);

        return obj;
    }
}