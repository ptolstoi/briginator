using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Level
{
    // bounds               Rect
    // fixed anchors        Anchor[]
    // start point          V3
    // end point            V3
    // start land           V3
    // end land             V3
    // name                 string

#pragma warning disable 0414
    [SerializeField] private int version = 1;
#pragma warning restore 0414
    [SerializeField] private string name;
    [SerializeField] private Rect rect;
    [SerializeField] private List<Anchor> fixedAnchors = new List<Anchor>();
    [SerializeField] private Vector3 startPoint;
    [SerializeField] private Vector3 endPoint;
    [SerializeField] private Vector3 startLand;
    [SerializeField] private Vector3 endLand;

    public string Name { get { return name; } set { name = value; } }

    public Rect Rect { get { return rect; } set { rect = value; } }
    public List<Anchor> FixedAnchors { get { return fixedAnchors; } set { fixedAnchors = value; } }
    public Vector3 StartPoint { get { return startPoint; } set { startPoint = value; } }
    public Vector3 EndPoint { get { return endPoint; } set { endPoint = value; } }
    public Vector3 StartLand { get { return startLand; } set { startLand = value; } }
    public Vector3 EndLand { get { return endLand; } set { endLand = value; } }

    public Level()
    {
    }

    public static Level FromJSON(string json)
    {
        return JsonUtility.FromJson<Level>(json);
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }

    public override string ToString()
    {
        return this.ToJSON();
    }
}