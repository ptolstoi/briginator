using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class SpringVisualizer : MonoBehaviour
{
    public bool showSum;
    Dictionary<Joint2D, float> max_springses = new Dictionary<Joint2D, float>();
    Dictionary<Joint2D, Color> acab = new Dictionary<Joint2D, Color>();

    Stack<Color> colors = new Stack<Color>(new[] { Color.red, Color.green, Color.blue, Color.magenta });

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            return;
        }

        var springs = gameObject.GetComponents<Component>()
            .Where(x => x.GetType() == typeof(SpringJoint2D))
            .Select(x => x as SpringJoint2D);

        foreach (var spring in springs)
        {
            var from = transform.position + (Vector3)spring.anchor;
            var to = (Vector3)spring.connectedAnchor;
            if (spring.connectedBody != null)
            {
                to += spring.connectedBody.transform.position;
            }
            var color = Color.grey;
            if (spring.enabled)
            {
                color = Color.white;
            }
            Debug.DrawLine(from, to, color);
        }
    }

    private void OnGUI()
    {
        if (UnityEditor.Selection.activeGameObject == this.gameObject)
        {
            GUILayout.BeginVertical();

            foreach (var i in max_springses)
            {
                var color = acab[i.Key];
                GUILayout.BeginHorizontal();
                GUI.color = color;
                GUILayout.Button("", GUILayout.MinWidth(32), GUILayout.MaxWidth(32));
                GUI.color = Color.black;
                GUILayout.Label(" " + i.Value, GUILayout.MinWidth(100), GUILayout.MaxWidth(100));
                try
                {
                    GUILayout.Label(" " + i.Key.reactionForce.sqrMagnitude);
                }
                catch (Exception) { }
                GUILayout.EndHorizontal();
            }
        }
    }

    void FixedUpdate()
    {
        var joints = gameObject.GetComponents<Component>()
            .Where(x => x.GetType().IsSubclassOf(typeof(AnchoredJoint2D)))
            .Select(x => x as AnchoredJoint2D);

        var sumSum = 0f;

        foreach (var joint in joints)
        {
            var from = transform.position + (Vector3)joint.anchor;
            var to = (Vector3)joint.connectedAnchor;
            if (joint.connectedBody != null)
            {
                to = joint.connectedBody.transform.position;
            }
            var color = Color.grey;
            if (joint.enabled)
            {
                if (!max_springses.ContainsKey(joint))
                {
                    max_springses.Add(joint, 0);
                    acab.Add(joint, colors.PopOrDefault());
                }
                max_springses[joint] = Mathf.Max(max_springses[joint], joint.reactionForce.sqrMagnitude);

                sumSum += joint.reactionForce.sqrMagnitude;

                if (float.IsInfinity(joint.breakForce))
                {
                    color = acab.ContainsKey(joint) ? acab[joint] : Color.white;
                }
                else
                {
                    var prop = joint.reactionForce.sqrMagnitude / joint.breakForce;
                    color = Color.Lerp(Color.green, Color.red, prop);

                    if (prop > 1)
                    {
                        joint.breakForce = 0;
                    }
                }
            }

            from += Vector3.forward * -0.5f;
            to += Vector3.forward * -0.5f;

            if (UnityEditor.Selection.activeGameObject != gameObject &&
                UnityEditor.Selection.activeGameObject != null &&
                UnityEditor.Selection.activeGameObject.GetComponent<SpringVisualizer>() != null)
            {
                color = Color.Lerp(Color.black, color, 0.4f);
            }
            else
            {
                var dir = (to - from).normalized;
                var x = Vector3.Cross(-Vector3.forward, dir).normalized * 0.02f;
                var y = -Vector3.forward * 0.02f;

                for (int i = 0, n = 6; i < n; ++i)
                {
                    var angle = i * 2 * Mathf.PI / n;
                    var displ = x * Mathf.Sin(angle) + y * Mathf.Cos(angle);
                    Debug.DrawLine(from + displ, to + displ, color);
                }


            }

            from += Vector3.forward;
            to += Vector3.forward;

            color = acab.ContainsKey(joint) ? acab[joint] : Color.white;

            Debug.DrawLine(from, to, color);
        }

        // if (showSum && false)
        // {
        //     Debug.DrawLine(
        //         transform.position - Vector3.right * transform.localScale.x / 2 - Vector3.forward * 0.5f,
        //         transform.position + Vector3.right * transform.localScale.x / 2 - Vector3.forward * 0.5f,
        //         Color.Lerp(Color.green, Color.red, sumSum / 10000f)
        //     );
        // }
    }

    internal void CleanUp()
    {
        this.max_springses = new Dictionary<Joint2D, float>();
        this.acab = new Dictionary<Joint2D, Color>();

        this.colors = new Stack<Color>(new[] { Color.red, Color.green, Color.blue, Color.magenta });
    }
}
