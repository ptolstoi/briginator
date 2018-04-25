using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public delegate void SolutionChangeDelegate();
    public event SolutionChangeDelegate OnSolutionChange;

    public static Solution FromJSON(string json)
    {
        return JsonUtility.FromJson<Solution>(json);
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }

    public void Add(Anchor anchor)
    {
        anchors.Add(anchor);
        OnSolutionChange?.Invoke();
    }
    public void Add(Connection connection)
    {
        var hasConnection = connections.Any(conn =>
        {
            return
                (conn.IdA == connection.IdA && conn.IdB == connection.IdB) ||
                (conn.IdA == connection.IdB && conn.IdB == connection.IdA);
        });

        if (hasConnection)
        {
            return;
        }

        connections.Add(connection);
        OnSolutionChange?.Invoke();
    }

    public void Remove(Anchor anchor)
    {
        anchors.RemoveAll(x => x.Id == anchor.Id);
        connections.RemoveAll(x => x.IdA == anchor.Id || x.IdB == anchor.Id);
        OnSolutionChange?.Invoke();
    }

    public void Remove(Connection connection)
    {
        connections.RemoveAll(conn =>
        {
            return
                (conn.IdA == connection.IdA && conn.IdB == connection.IdB) ||
                (conn.IdA == connection.IdB && conn.IdB == connection.IdA);
        });
        OnSolutionChange?.Invoke();
    }
}