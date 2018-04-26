using UnityEngine;
using System;

[Serializable]
public class Connection
{
    [SerializeField]
    private string idA;
    [SerializeField]
    private string idB;
    [SerializeField]
    private ConnectionType type;

    public string IdA { get { return idA; } set { idA = value; } }
    public string IdB { get { return idB; } set { idB = value; } }
    public ConnectionType Type { get { return type; } set { type = value; } }

    public Connection() { }
    public Connection(string idA, string idB, ConnectionType type)
    {
        this.idA = idA;
        this.idB = idB;
        this.type = type;
    }

    public bool IsEqual(Connection other)
    {
        return (idA == other.idA && idB == other.idB) || (idB == other.idA && idA == other.idB);
    }
}

public class ConnectionEx : Connection
{
    public Anchor AnchorA { get; set; }
    public Anchor AnchorB { get; set; }
}