using UnityEngine;
using System;
using System.Collections.Generic;

public class WaterQuad : MeshPart
{
    private readonly Vector3 position;
    private readonly Vector3 size;

    public WaterQuad(Vector3 position, Vector3 size)
    {
        this.position = position;
        this.size = size;
    }

    protected override IEnumerable<MeshPart> Parts()
    {
        yield break;
    }

    public override void AddToBuffers(ref List<Vector3> v, ref List<int> i)
    {
        var waveHeight = size.y;
        var waveDepth = size.z;
        var waveWidth = 1f;

        var offset = new Vector3(0, 0, 0);

        for (var x = -size.x / 2; x < size.x / 2; x += waveWidth)
        {
            var waterPixel = Vector3.left * x + position;
            var nextPixel = Vector3.left * (x + waveWidth) + position;

            var v1 = new Vector3(waterPixel.x, waterPixel.y, -waveDepth / 2) + offset;
            var v2 = new Vector3(nextPixel.x, nextPixel.y, -waveDepth / 2) + offset;
            var v3 = new Vector3(nextPixel.x, nextPixel.y, waveDepth / 2) + offset;
            var v4 = new Vector3(waterPixel.x, waterPixel.y, waveDepth / 2) + offset;

            var v5 = new Vector3(waterPixel.x, -waveHeight, -waveDepth / 2) + offset;
            var v6 = new Vector3(nextPixel.x, -waveHeight, -waveDepth / 2) + offset;

            AddStrip(ref v, ref i, v1, v2, v3, v4);

            AddQuad(ref v, ref i, v5, v6, v2.NoiseVector(), v1.NoiseVector());
        }

    }

    private void AddStrip(ref List<Vector3> v, ref List<int> i, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        var subdivisions = size.z / 1f;

        var _1 = v1.NoiseVector();
        var _2 = v2.NoiseVector();

        for (var j = 1; j < subdivisions; j++)
        {
            var _3 = Vector3.Lerp(v2, v3, j / subdivisions).NoiseVector();
            var _4 = Vector3.Lerp(v1, v4, j / subdivisions).NoiseVector();

            AddQuad(ref v, ref i, _1, _2, _3, _4);

            _1 = _4;
            _2 = _3;
        }

        AddQuad(ref v, ref i, _1, _2, v3.NoiseVector(), v4.NoiseVector());
    }

    private void AddQuad(ref List<Vector3> v, ref List<int> i, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        AddTriangle(ref v, ref i, v1, v3, v2);
        AddTriangle(ref v, ref i, v1, v4, v3);
    }

    private void AddTriangle(ref List<Vector3> v, ref List<int> i, Vector3 v1, Vector3 v3, Vector3 v2)
    {
        var j = v.Count;

        v.Add(v1);
        v.Add(v2);
        v.Add(v3);

        i.Add(j++);
        i.Add(j++);
        i.Add(j++);
    }
}