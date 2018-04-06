using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BridgeMeshManager : MonoBehaviour
{
    public enum Parts
    {
        All,
        Left,
        Right
    }

    public LevelManager LevelManager;
    public Material RoadMaterial;
    public Material WoodMaterial;
    public Material SteelMaterial;
    public Material MetalMaterial;


    public void GenerateMeshFor(Connection connection, GameObject atGameObject, Parts with = Parts.All)
    {
        GenerateMesh(
            gameObject: atGameObject,
            meshName: connection + " " + with,
            generator: MeshPartsForConnection(connection, with)
        );
    }

    public void GenerateMeshFor(Anchor anchor, GameObject atGameObject, bool hasRoadConnections = false)
    {
        GenerateMesh(
            gameObject: atGameObject,
            meshName: "Anchor",
            generator: MeshPartsForAnchor(hasRoadConnections)
        );
    }

    private void GenerateMesh(GameObject gameObject, string meshName, IEnumerable<Tuple<Material, MeshPart>> generator)
    {
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        var meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer.sharedMaterials = new[] {
            RoadMaterial,
            WoodMaterial,
            SteelMaterial,
            MetalMaterial
         };

        var mesh = new Mesh();
        mesh.name = "Mesh " + meshName;
        mesh.subMeshCount = meshRenderer.sharedMaterials.Length;

        // generate mesh
        var vertices = new List<Vector3>();
        var indices = new Dictionary<Material, List<int>>();

        foreach (var tuple in generator)
        {
            var material = tuple.Item1;
            var part = tuple.Item2;
            var indexBuffer = indices.GetOrDefault(material, () => new List<int>());
            part.AddToBuffers(ref vertices, ref indexBuffer);
            indices[material] = indexBuffer;
        }

        mesh.SetVertices(vertices);

        foreach (var pair in indices)
        {
            var idx = Array.IndexOf(meshRenderer.sharedMaterials, pair.Key);

            mesh.SetIndices(pair.Value.ToArray(), MeshTopology.Triangles, idx);
        }


        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
    }

    IEnumerable<Tuple<Material, MeshPart>> MeshPartsForConnection(Connection connection, Parts parts)
    {
        var left = Vector3.left / 2;
        var right = Vector3.right / 2;
        var center = Vector3.zero;

        var roadHeight = LevelManager.RoadHeight;
        var roadWidth = 2f;

        var steelHeight = 0.25f;
        var steelWidth = 0.25f;

        var woodHeight = 0.2f;
        var woodWidth = 0.2f;

        var forward = Vector3.forward * (roadWidth / 2 - steelWidth / 2);
        var back = Vector3.back * (roadWidth / 2 - steelWidth / 2);

        if (connection.Type == ConnectionType.Road)
        {
            yield return Tuple.Create(
                RoadMaterial,
                new RoadBox(
                    left: left + Vector3.down * (roadHeight / 2),
                    right: right + Vector3.down * (roadHeight / 2),
                    width: roadWidth,
                    height: roadHeight
                ) as MeshPart
            );
        }
        else if (connection.Type == ConnectionType.Steel)
        {
            if (parts == Parts.All)
            {
                yield return Tuple.Create(
                    SteelMaterial,
                    new SteelBox(
                        left: left + forward,
                        right: right + forward,
                        width: steelWidth,
                        height: steelHeight,
                        innerWidth: steelWidth * 0.5f,
                        innerHeight: steelHeight * 0.5f
                    ) as MeshPart
                );
                yield return Tuple.Create(
                    SteelMaterial,
                    new SteelBox(
                        left: left + back,
                        right: right + back,
                        width: steelWidth,
                        height: steelHeight,
                        innerWidth: steelWidth * 0.5f,
                        innerHeight: steelHeight * 0.5f
                    ) as MeshPart
                );
            }
            else
            {
                var start = parts == Parts.Left ? left : right;
                var end = parts == Parts.Left ? right : left;

                yield return Tuple.Create(
                    SteelMaterial,
                    new SteelBoxPart(
                        left: start + forward,
                        right: end + forward,
                        width: steelWidth,
                        height: steelHeight,
                        innerWidth: steelWidth * 0.5f,
                        innerHeight: steelHeight * 0.5f
                    ) as MeshPart
                );
                yield return Tuple.Create(
                    SteelMaterial,
                    new SteelBoxPart(
                        left: start + back,
                        right: end + back,
                        width: steelWidth,
                        height: steelHeight,
                        innerWidth: steelWidth * 0.5f,
                        innerHeight: steelHeight * 0.5f
                    ) as MeshPart
                );
            }
        }
        else if (connection.Type == ConnectionType.Wood)
        {
            if (parts == Parts.All)
            {
                yield return Tuple.Create(
                    WoodMaterial,
                    new SteelBox(
                        left: left + forward,
                        right: right + forward,
                        width: woodWidth,
                        height: woodHeight,
                        innerWidth: woodWidth * 0.5f,
                        innerHeight: woodHeight * 0.5f
                    ) as MeshPart
                );
                yield return Tuple.Create(
                    WoodMaterial,
                    new SteelBox(
                        left: left + back,
                        right: right + back,
                        width: woodWidth,
                        height: woodHeight,
                        innerWidth: woodWidth * 0.5f,
                        innerHeight: woodHeight * 0.5f
                    ) as MeshPart
                );
            }
            else
            {
                var start = parts == Parts.Left ? left : right;
                var end = parts == Parts.Left ? right : left;

                yield return Tuple.Create(
                    WoodMaterial,
                    new SteelBoxPart(
                        left: start + forward,
                        right: end + forward,
                        width: woodWidth,
                        height: woodHeight,
                        innerWidth: woodWidth * 0.5f,
                        innerHeight: woodHeight * 0.5f
                    ) as MeshPart
                );
                yield return Tuple.Create(
                    WoodMaterial,
                    new SteelBoxPart(
                        left: start + back,
                        right: end + back,
                        width: woodWidth,
                        height: woodHeight,
                        innerWidth: woodWidth * 0.5f,
                        innerHeight: woodHeight * 0.5f
                    ) as MeshPart
                );
            }
        }
    }

    IEnumerable<Tuple<Material, MeshPart>> MeshPartsForAnchor(bool road)
    {
        var steelHeight = 0.25f;
        var steelWidth = steelHeight;
        var roadWidth = 2f;

        var radius = steelHeight;
        var depth = 0.3f;

        var forward = Vector3.forward * (roadWidth / 2 - steelWidth / 2 - depth / 2);
        var back = Vector3.back * (roadWidth / 2 - steelWidth / 2 - depth / 2);

        yield return Tuple.Create(
            MetalMaterial,
            new AnchorMesh(
                position: Vector3.back * (roadWidth / 2 - steelWidth / 2),
                radius: radius,
                depth: depth
            ) as MeshPart
        );
        yield return Tuple.Create(
            MetalMaterial,
            new AnchorMesh(
                position: Vector3.forward * (roadWidth / 2 - steelWidth / 2),
                radius: radius,
                depth: depth
            ) as MeshPart
        );

        if (!road)
        {
            yield return Tuple.Create(
                MetalMaterial,
                new SteelBox(
                    left: back,
                    right: forward,
                    width: radius,
                    height: radius,
                    innerWidth: radius * 0.5f,
                    innerHeight: radius * 0.5f
                ) as MeshPart
            );
        }
    }
}
