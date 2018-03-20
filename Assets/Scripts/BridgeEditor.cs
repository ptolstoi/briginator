using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(BoxCollider2D))]
public class BridgeEditor : MonoBehaviour
{
    [SerializeField]
    Material RoadMaterial;
    [SerializeField]
    Transform[] FixedAnchors;

    [SerializeField]
    float maxForce = 4000f;

    [SerializeField]
    float gridSize = 0.5f;
    [SerializeField]
    float sigma = 0.9f;
    float maxGuassianSize => Mathf.Exp(-0.5f * Mathf.Pow((gridSize * 0.75f) / sigma, 2));
    float hexHeight => Mathf.Sqrt(3) / 2 * gridSize;
    float roadHeight = 0.1f;


    private Transform lastPoint = null;
    private List<GameObject> anchors = new List<GameObject>();

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mouseWorldPosition = GetMouseWorldPosition();
            if (!mouseWorldPosition.HasValue)
            {
                return;
            }

            if (!lastPoint)
            {
                lastPoint = GetOrCreateAnchorAt(mouseWorldPosition.Value).transform;
            }
            else
            {
                var newPoint = GetOrCreateAnchorAt(mouseWorldPosition.Value).transform;

                CreateRoad(lastPoint, newPoint.transform);

                lastPoint = newPoint;
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            lastPoint = null;
        }

        if (Input.GetKeyUp(KeyCode.F5))
        {
            anchors.ForEach(x =>
            {
                var rb = x?.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }

                foreach (var spring in x?.GetComponents<Joint2D>())
                {
                    spring.enabled = true;
                }
            });

            foreach (var fixedAnchor in FixedAnchors)
            {
                foreach (var spring in fixedAnchor.GetComponents<Component>()
                                                    .Where(x => x.GetType().IsSubclassOf(typeof(Joint2D)))
                                                    .Select(x => x as Joint2D))
                {
                    spring.enabled = true;
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.F2))
        {
            anchors.Clear();
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform fixedAnchor in FixedAnchors)
            {
                foreach (var spring in fixedAnchor.GetComponents<Component>()
                                                    .Where(x => x.GetType().IsSubclassOf(typeof(Joint2D)))
                                                    .Select(x => x as Joint2D))
                {
                    DestroyImmediate(spring);
                }

                fixedAnchor.GetComponent<SpringVisualizer>()?.CleanUp();
            }
        }
    }

    private GameObject GetOrCreateAnchorAt(Vector2 position)
    {
        var existingAnchors = Physics2D.OverlapCircleAll(position, gridSize / 2, 1 << LayerMask.NameToLayer("NoSelfIntersection"))
            .Where(x => x is CircleCollider2D);


        if (existingAnchors.Count() == 0)
        {
            var go = new GameObject("Anchor");
            anchors.Add(go);
            DrawIcon(go, 1);
            go.layer = LayerMask.NameToLayer("NoSelfIntersection");
            go.transform.position = position;
            go.transform.SetParent(transform);
            var circle = go.AddComponent<CircleCollider2D>();
            circle.radius = 0.1f;
            circle.isTrigger = true;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.drag = 0.3f;
            rb.isKinematic = true;
            go.AddComponent<SpringVisualizer>();

            return go;
        }
        else
        {
            return existingAnchors.First().gameObject;
        }
    }

    private void CreateRoad(Transform tA, Transform tB)
    {
        if (tA.position.x > tB.position.x)
        {
            var tmp = tA;
            tA = tB;
            tB = tmp;
        }

        var goA = tA.gameObject;
        var goB = tB.gameObject;
        var rbA = goA.GetComponent<Rigidbody2D>();
        var rbB = goB.GetComponent<Rigidbody2D>();
        var anchorDistance = Vector3.Distance(tA.position, tB.position);
        var slopeAngle = Vector3.SignedAngle(tB.position - tA.position, Vector3.right, -Vector3.forward);
        var isSlope = Mathf.Abs(slopeAngle) >= 11 || Mathf.Abs(tA.position.y) > hexHeight * 2;


        var anchorJoint = goA.AddComponent<HingeJoint2D>();
        anchorJoint.enabled = false;
        // anchorJoint.autoConfigureDistance = false;

        // anchorJoint.distance = anchorDistance;
        anchorJoint.connectedBody = rbB;

        // anchorJoint.dampingRatio = 0.25f;
        // anchorJoint.frequency = 25;
        anchorJoint.breakForce = maxForce;

        var goRoadBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var vizu = goRoadBlock.AddComponent<SpringVisualizer>();
        vizu.showSum = true;
        goRoadBlock.transform.SetParent(transform);
        goRoadBlock.layer = LayerMask.NameToLayer("NoSelfIntersection");
        DestroyImmediate(goRoadBlock.GetComponent<BoxCollider>());
        var ridBo = goRoadBlock.AddComponent<Rigidbody2D>();
        ridBo.isKinematic = true;
        goRoadBlock.GetComponent<MeshRenderer>().sharedMaterial = RoadMaterial;
        var trRoadBlock = goRoadBlock.transform;
        trRoadBlock.position = (tA.position + tB.position) / 2 + Vector3.down * roadHeight / 2;
        trRoadBlock.localScale = new Vector3(
            anchorDistance,
            roadHeight,
            1
        );
        trRoadBlock.localEulerAngles = new Vector3(0, 0, slopeAngle);

        var AnDiWa = goA.AddComponent<AnchorDistructionWatcher>();
        AnDiWa.joint = anchorJoint;
        AnDiWa.connectedBlock = goRoadBlock;

        var anchorYOffset = Vector2.zero;

        if (isSlope)
        {

            var attachedToAnchor = goRoadBlock.AddComponent<AttachedToAnchors>();
            attachedToAnchor.Anchors = new[] {
                tA,
                tB
            };
            trRoadBlock.position += Vector3.forward * 0.5f - Vector3.forward * 0.25f / 2
                                    + Vector3.up * roadHeight / 2;
            trRoadBlock.localScale += -Vector3.forward + Vector3.forward * 0.25f;
            goRoadBlock.name = "Beam";
            return;
        }
        else
        {
            anchors.Add(goRoadBlock);
            var boxi = goRoadBlock.AddComponent<BoxCollider2D>();
            boxi.size = new Vector2(0.99f, 1);
            anchorYOffset.y = 0.5f;
            goRoadBlock.name = "Road";
        }

        var spring = goRoadBlock.AddComponent<HingeJoint2D>();
        // spring.autoConfigureDistance = false;
        spring.enabled = false;
        spring.connectedBody = rbA;

        // spring.distance = 0.0075f;
        spring.anchor = anchorYOffset + Vector2.left * 0.5f;
        // spring.anchor = anchorYOffset + Vector2.left * (0.5f - (spring.distance) / trRoadBlock.localScale.x);
        // spring.dampingRatio = 1f;
        // spring.frequency = 1000;


        spring = goRoadBlock.AddComponent<HingeJoint2D>();
        // spring.autoConfigureDistance = false;
        spring.enabled = false;
        spring.connectedBody = rbB;

        // spring.distance = 0.0075f;
        spring.anchor = anchorYOffset + Vector2.right * 0.5f;
        // spring.anchor = anchorYOffset + Vector2.right * (0.5f - (spring.distance) / trRoadBlock.localScale.x);
        // spring.dampingRatio = 1f;
        // spring.frequency = 1000;
    }

    private void OnDrawGizmos()
    {
        if (gridSize < 0.01f)
        {
            return;
        }

        var mouseWorldPosition = GetMouseWorldPosition();
        if (!Application.isPlaying)
        {
            mouseWorldPosition = transform.position;
        }

        var bounds = GetComponent<BoxCollider2D>().bounds;

        if (mouseWorldPosition.HasValue)
        {
            var color = Gizmos.color;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(mouseWorldPosition.Value, gridSize / 2);
            Gizmos.color = color;

            if (lastPoint != null)
            {
                Gizmos.DrawLine(lastPoint.position, mouseWorldPosition.Value);
            }
        }


        var xx = 0;
        for (var x = bounds.min.x; x <= bounds.max.x; x += gridSize)
        {
            var yEven = true;
            for (var y = bounds.min.y; y <= bounds.max.y; y += hexHeight)
            {
                yEven = !yEven;

                var gridPoint = Vector3.up * y + Vector3.right * x;

                if (yEven)
                {
                    gridPoint += Vector3.right * gridSize / 2.0f;
                }

                if (gridPoint.x > bounds.max.x)
                {
                    continue;
                }


                var distance = mouseWorldPosition.HasValue ?
                    Vector3.Distance(mouseWorldPosition.Value, gridPoint)
                    : float.MaxValue;

                if (distance < 0.1f) continue;

                var gaussScale = Mathf.Exp(-0.5f * Mathf.Pow(distance / sigma, 2)) / maxGuassianSize;
                var gridPointSize = Mathf.Max(0.025f, gridSize / 3 * gaussScale);

                // gridPointSize = gridSize / 2;

                Gizmos.DrawSphere(gridPoint, gridPointSize);
            }
            xx++;
        }
    }

    Vector3? GetMouseWorldPosition()
    {
        var bounds = GetComponent<BoxCollider2D>().bounds;
        var mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z;

        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition) - bounds.min;

        mouseWorldPosition.y = Mathf.Round(mouseWorldPosition.y / hexHeight) * hexHeight;

        var posY = Mathf.RoundToInt(mouseWorldPosition.y / hexHeight);
        if (posY % 2 != 0)
        {
            mouseWorldPosition.x = Mathf.Round(mouseWorldPosition.x / gridSize - 0.5f) * gridSize + gridSize / 2;
        }
        else
        {
            mouseWorldPosition.x = Mathf.Round(mouseWorldPosition.x / gridSize) * gridSize;
        }

        mouseWorldPosition = mouseWorldPosition + bounds.min;

        if (bounds.Contains(mouseWorldPosition))
        {
            return mouseWorldPosition;
        }

        return null;
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
}
