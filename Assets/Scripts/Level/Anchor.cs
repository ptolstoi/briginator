using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Anchor
{
    [SerializeField]
    private string id = Guid.NewGuid().ToString();
    [SerializeField]
    private Vector2 position;

    public string Id { get { return id; } protected set { this.id = value; } }
    public Vector2 Position { get { return position; } set { this.position = value; } }

    public Anchor() { }
    public Anchor(Vector2 pos) { this.position = pos; }
    public Anchor(float x, float y) { this.position = new Vector2(x, y); }

    public static implicit operator Vector2(Anchor a)
    {
        return a.position;
    }

    public static implicit operator Vector3(Anchor a)
    {
        return a.position;
    }
}