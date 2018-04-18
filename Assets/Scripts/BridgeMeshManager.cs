using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

public class BridgeMeshManager : MonoBehaviour
{
    public enum Parts
    {
        All,
        Left,
        Right
    }

    [Header("Materials")]
    public Material RoadMaterial;
    public Material WoodMaterial;
    public Material SteelMaterial;
    public Material MetalMaterial;
    public Material LoadMaterial;
    public Material MarkerMaterial;
    public Material WaterMaterial;

    [Header("Misc")]
    public bool ShowLoad = true;
    [Header("Wiring")]
    public LevelManager LevelManager;

    public event Action NeedMeshRegeneration;



    public void GenerateMeshFor(Connection connection, GameObject atGameObject, Parts with = Parts.All, bool useLoadMaterial = false)
    {
        GenerateMesh(
            gameObject: atGameObject,
            meshName: connection + " " + with,
            generator: MeshPartsForConnection(connection, with, useLoadMaterial)
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

    public void GenerateWaterMesh(GameObject atGameObject, Vector3 position, Vector3 size)
    {
        GenerateMesh(
            gameObject: atGameObject,
            meshName: "Water",
            generator: MeshPartsForWater(position, size)
        );
    }

    private void GenerateMesh(GameObject gameObject, string meshName, IEnumerable<Tuple<Material, MeshPart>> generator)
    {
        var meshRenderer = gameObject.EnsureComponent<MeshRenderer>();
        meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        var meshFilter = gameObject.EnsureComponent<MeshFilter>();

        var mesh = new Mesh();
        mesh.name = "Mesh " + meshName;

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

        meshRenderer.sharedMaterials = indices.Keys.ToArray();
        mesh.subMeshCount = meshRenderer.sharedMaterials.Length;

        foreach (var pair in indices)
        {
            var idx = Array.IndexOf(meshRenderer.sharedMaterials, pair.Key);

            mesh.SetIndices(pair.Value.ToArray(), MeshTopology.Triangles, idx);
        }


        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
    }

    IEnumerable<Tuple<Material, MeshPart>> MeshPartsForConnection(Connection connection, Parts parts, bool useLoadMaterial = false)
    {
        var left = Vector3.left / 2;
        var right = Vector3.right / 2;
        var center = Vector3.zero;

        var roadHeight = LevelManager.RoadHeight;
        var roadDepth = roadHeight;
        var roadWidth = 2f;

        var steelHeight = 0.25f;
        var steelWidth = 0.25f;

        var woodHeight = 0.2f;
        var woodWidth = 0.2f;

        var forward = Vector3.forward * (roadWidth / 2 - steelWidth / 2);
        var back = Vector3.back * (roadWidth / 2 - steelWidth / 2);

        var overwriteMaterial = useLoadMaterial ? LoadMaterial : null;

        if (connection.Type == ConnectionType.Road)
        {
            #region Road Parts
            yield return Tuple.Create(
                RoadMaterial,
                new RoadBox(
                    left: left + Vector3.down * (roadHeight / 2),
                    right: right + Vector3.down * (roadHeight / 2),
                    width: roadWidth,
                    height: roadHeight
                ) as MeshPart
            );
            yield return Tuple.Create(
                overwriteMaterial ?? MarkerMaterial,
                new RoadBox(
                    left: left + forward,
                    right: right + forward,
                    width: roadDepth,
                    height: roadHeight
                ) as MeshPart
            );
            yield return Tuple.Create(
                overwriteMaterial ?? MarkerMaterial,
                new RoadBox(
                    left: left + back,
                    right: right + back,
                    width: roadHeight,
                    height: roadHeight
                ) as MeshPart
            );
            #endregion
        }
        else if (connection.Type == ConnectionType.Steel)
        {
            #region Steel Parts
            if (parts == Parts.All)
            {
                yield return Tuple.Create(
                    overwriteMaterial ?? SteelMaterial,
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
                    overwriteMaterial ?? SteelMaterial,
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
                    overwriteMaterial ?? SteelMaterial,
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
                    overwriteMaterial ?? SteelMaterial,
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
            #endregion
        }
        else if (connection.Type == ConnectionType.Wood)
        {
            #region Wood Parts
            if (parts == Parts.All)
            {
                yield return Tuple.Create(
                    overwriteMaterial ?? WoodMaterial,
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
                    overwriteMaterial ?? WoodMaterial,
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
                    overwriteMaterial ?? WoodMaterial,
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
                    overwriteMaterial ?? WoodMaterial,
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
            #endregion
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

    private IEnumerable<Tuple<Material, MeshPart>> MeshPartsForWater(Vector3 position, Vector3 size)
    {
        yield return Tuple.Create(
            WaterMaterial,
            new WaterQuad(
                position: position,
                size: size
            ) as MeshPart
        );
    }
}
