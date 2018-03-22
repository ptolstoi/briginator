using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Solution
{
    // anchors              Anchor[]
    // connections          Connection[]

    [SerializeField]
    private List<Anchor> anchors = new List<Anchor>();

    [SerializeField]
    private List<Connection> connections = new List<Connection>();

    public List<Anchor> Anchors { get { return anchors; } set { anchors = value; } }
    public List<Connection> Connections { get { return connections; } set { connections = value; } }

    public static Solution FromJSON(string json)
    {
        return JsonUtility.FromJson<Solution>(json);
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }
}