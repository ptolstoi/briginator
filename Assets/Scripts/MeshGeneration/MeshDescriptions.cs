using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class MeshPart
{
    protected abstract IEnumerable<MeshPart> Parts();
    public virtual void AddToBuffers(ref List<Vector3> vertexBuffer, ref List<int> indexBuffer)
    {
        foreach (var part in Parts())
        {
            part.AddToBuffers(ref vertexBuffer, ref indexBuffer);
        }
    }

    public virtual int UpdateBuffers(int offset, ref List<Vector3> vertexBuffer)
    {
        foreach (var part in Parts())
        {
            offset = part.UpdateBuffers(offset, ref vertexBuffer);
        }
        return offset;
    }
}

public class Triangle : MeshPart
{
    private Vector3 v1;
    private Vector3 v2;
    private Vector3 v3;

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
    }
    protected override IEnumerable<MeshPart> Parts()
    {
        yield break;
    }

    public override void AddToBuffers(ref List<Vector3> vertexBuffer, ref List<int> indexBuffer)
    {
        var i = vertexBuffer.Count;

        vertexBuffer.Add(v1);
        vertexBuffer.Add(v3);
        vertexBuffer.Add(v2);

        indexBuffer.Add(i++);
        indexBuffer.Add(i++);
        indexBuffer.Add(i++);
    }

    public override int UpdateBuffers(int offset, ref List<Vector3> vertexBuffer)
    {
        vertexBuffer[offset + 0] = v1;
        vertexBuffer[offset + 1] = v3;
        vertexBuffer[offset + 2] = v2;

        return offset + 3;
    }
}

public class Quad : MeshPart
{
    public readonly Vector3 v1;
    public readonly Vector3 v2;
    public readonly Vector3 v3;
    public readonly Vector3 v4;

    public Quad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
        this.v4 = v4;
    }

    public Quad(Vector3 position, Vector3 up, Vector3 right, float width, float height)
        : this(
            position + up * height / 2 - right * width / 2,
            position - up * height / 2 - right * width / 2,
            position - up * height / 2 + right * width / 2,
            position + up * height / 2 + right * width / 2
        )
    { }

    protected override IEnumerable<MeshPart> Parts()
    {
        yield return new Triangle(v1, v2, v3);
        yield return new Triangle(v1, v3, v4);
    }
}

public class Box : MeshPart
{
    public readonly Vector3 left;
    public readonly Vector3 right;

    public readonly float width;
    public readonly float height;

    protected Vector3 up;
    protected Vector3 forward;

    public Box(Vector3 left, Vector3 right, float width, float height)
    {
        this.left = left;
        this.right = right;
        this.width = width;
        this.height = height;
        var tmp = right - left;
        Vector3.OrthoNormalize(ref tmp, ref this.up);
        this.forward = Vector3.Cross(tmp, this.up).normalized;
    }

    protected override IEnumerable<MeshPart> Parts()
    {
        var v1 = left + forward * (width / 2) + up * (height / 2);
        var v2 = left - forward * (width / 2) + up * (height / 2);
        var v3 = right - forward * (width / 2) + up * (height / 2);
        var v4 = right + forward * (width / 2) + up * (height / 2);
        var v5 = left + forward * (width / 2) - up * (height / 2);
        var v6 = left - forward * (width / 2) - up * (height / 2);
        var v7 = right - forward * (width / 2) - up * (height / 2);
        var v8 = right + forward * (width / 2) - up * (height / 2);

        yield return new Quad(v1, v2, v3, v4);
        yield return new Quad(v6, v5, v8, v7);
        yield return new Quad(v2, v6, v7, v3);
        yield return new Quad(v1, v4, v8, v5);
        yield return new Quad(v1, v5, v6, v2);
        yield return new Quad(v4, v3, v7, v8);
    }
}

public class RoadBox : Box
{
    public RoadBox(Vector3 left, Vector3 right, float width, float height)
       : base(left, right, width, height)
    {
        this.up = Vector3.up;
        this.forward = Vector3.forward;
    }
}

public class SteelBoxPart : MeshPart
{
    public readonly Vector3 left;
    public readonly Vector3 right;

    public readonly float width;
    public readonly float height;

    public readonly float innerWidth;
    public readonly float innerHeight;
    public readonly bool hasBox;

    public SteelBoxPart(Vector3 left, Vector3 right, float width, float height, float innerWidth, float innerHeight, bool hasBox = true)
    {
        this.left = left;
        this.right = right;
        this.width = width;
        this.height = height;
        this.innerWidth = innerWidth;
        this.innerHeight = innerHeight;
        this.hasBox = hasBox;
    }

    protected override IEnumerable<MeshPart> Parts()
    {
        var n = 2;
        var diff = left - right;
        var center = right + diff / n * (n - 1);
        var up = Vector3.zero;
        var tmp = diff;
        Vector3.OrthoNormalize(ref tmp, ref up);
        var forward = Vector3.Cross(right - left, up).normalized;

        var v1 = left + forward * (width / 2) + up * (height / 2);
        var v2 = left - forward * (width / 2) + up * (height / 2);
        var v3 = center - forward * (innerWidth / 2) + up * (innerHeight / 2);
        var v4 = center + forward * (innerWidth / 2) + up * (innerHeight / 2);

        var v5 = left + forward * (width / 2) - up * (height / 2);
        var v6 = left - forward * (width / 2) - up * (height / 2);

        var v7 = center - forward * (innerWidth / 2) - up * (innerHeight / 2);
        var v8 = center + forward * (innerWidth / 2) - up * (innerHeight / 2);

        yield return new Quad(v1, v5, v6, v2); // left left
        yield return new Quad(v1, v2, v3, v4); // left top
        yield return new Quad(v5, v8, v7, v6); // left bottom
        yield return new Quad(v2, v6, v7, v3); // left front
        yield return new Quad(v1, v4, v8, v5); // left back

        if (hasBox)
        {
            yield return new Box(center, right, innerWidth, innerHeight);
        }
    }
}

public class SteelBox : MeshPart
{

    public readonly Vector3 left;
    public readonly Vector3 right;

    public readonly float width;
    public readonly float height;

    public readonly float innerWidth;
    public readonly float innerHeight;
    public readonly bool hasBox;

    public SteelBox(Vector3 left, Vector3 right, float width, float height, float innerWidth, float innerHeight, bool hasBox = true)
    {
        this.left = left;
        this.right = right;
        this.width = width;
        this.height = height;
        this.innerWidth = innerWidth;
        this.innerHeight = innerHeight;
        this.hasBox = hasBox;
    }

    protected override IEnumerable<MeshPart> Parts()
    {
        var center = (left + right) / 2;
        yield return new SteelBoxPart(
            left: left,
            right: center,
            width: width,
            height: height,
            innerWidth: innerWidth,
            innerHeight: innerHeight,
            hasBox: hasBox
        );
        yield return new SteelBoxPart(
            left: right,
            right: center,
            width: width,
            height: height,
            innerWidth: innerWidth,
            innerHeight: innerHeight,
            hasBox: hasBox
        );
    }
}

public class AnchorMesh : MeshPart
{
    private Vector3 position;
    private float radius;
    private float depth;
    private int n;

    public AnchorMesh(Vector3 position, float radius, float depth, int n = 10)
    {
        this.position = position;
        this.radius = radius;
        this.depth = depth;
        this.n = n;
    }

    protected override IEnumerable<MeshPart> Parts()
    {
        var centerFront = position + Vector3.back * (depth / 2);
        var centerBack = position + Vector3.forward * (depth / 2);

        for (float i = 0f; i < n; i++)
        {
            var angle = 1f / n * 2 * Mathf.PI;
            var v1 = Vector3.up * Mathf.Cos(i * angle) * radius +
                     Vector3.right * Mathf.Sin(i * angle) * radius;
            var v2 = Vector3.up * Mathf.Cos((i + 1) * angle) * radius +
                    Vector3.right * Mathf.Sin((i + 1) * angle) * radius;
            yield return new Triangle(v1 + centerFront, centerFront, v2 + centerFront);
            yield return new Quad(v1 + centerFront, v2 + centerFront, v2 + centerBack, v1 + centerBack);
            yield return new Triangle(v1 + centerBack, v2 + centerBack, centerBack);
        }
    }
}