using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using MeshBuilderTest;
using MeshBuilder;

using DataVolume = MeshBuilder.Volume<MeshBuilder.Tile.Data>;
using Extents = MeshBuilder.Extents;

public class GroundTestScene : MonoBehaviour
{
    private const int GroundIndex = 1;
    private const int WaterIndex = 2;
    private const int RockIndex = 3;

    [Header("mesh filters")]
    [SerializeField]
    private MeshFilter groundMeshFilter = null;
    [SerializeField]
    private MeshFilter groundTopMeshFilter = null;
    [SerializeField]
    private MeshFilter waterMeshFilter = null;
    [SerializeField]
    private MeshFilter rockMeshFilter = null;

    [Header("generation info")]
    [SerializeField]
    private TileThemePalette palette = null;

    [SerializeField]
    private Vector3 cellSize = new Vector3(1, 1, 1);

    [SerializeField]
    private Texture2D groundHeightMap = null;

    [SerializeField]
    private float groundMaxHeight = 1f;

    [SerializeField]
    private bool useGroundNormalAlignment = true;

    private Extents dataExtents;
    private DataVolume dataVolume;

    private List<BuiltMesh> builders;

    void Awake()
    {
        builders = new List<BuiltMesh>();

        dataExtents = new Extents(16, 4, 16);
        dataVolume = new DataVolume(dataExtents);
        FillData(dataVolume);

        var groundMesher = new TileMesher3D();
        groundMesher.Init(dataVolume, 0, palette, cellSize, 
            new TileMesher3D.Settings()
            {
                filledBoundaries = Tile.Direction.None,
                skipDirections = Tile.Direction.YAxis,
            });
        Add(groundMesher, groundMeshFilter);
        if (useGroundNormalAlignment)
        {
            builders[builders.Count - 1].AddAligner(cellSize);
        }

        var topGroundMesher = new GridMesher();
        topGroundMesher.Init(dataVolume, GroundIndex, cellSize, 3, GridMesher.UVMode.Normalized);
        topGroundMesher.InitHeightMapScaleFromHeightAvgOffset(groundHeightMap, groundMaxHeight);
        Add(topGroundMesher, groundTopMeshFilter);

        var waterMesher = new GridMesher();
        waterMesher.Init(dataVolume, WaterIndex, cellSize, 1, GridMesher.UVMode.NoScaling, new float3(0, -0.2f, 0));
        Add(waterMesher, waterMeshFilter);

        var rockMesher = new TileMesher3D();
        rockMesher.Init(dataVolume, 1, palette, cellSize);
        Add(rockMesher, rockMeshFilter);

        foreach (var built in builders)
        {
            built.Start();
        }

        StartCoroutine(CompleteMesh());
    }

    private IEnumerator CompleteMesh()
    {
        yield return new WaitForEndOfFrame();

        foreach (var builder in builders)
        {
            if (builder.IsGenerating)
            {
                builder.Complete();
            }
        }

        yield return null;
    }

    private void Add(Builder builder, MeshFilter filter)
    {
        builders.Add(new BuiltMesh(builder, filter));
    }

    private void OnDestroy()
    {
        dataVolume.Dispose();

        foreach (var builder in builders)
        {
            builder.Dispose();
        }
        builders.Clear();
    }

    private void FillData(DataVolume data)
    {
        data.SetLayer(GroundIndex, 0);
        data.SetRect(GroundIndex, P(2, 1, 2), 3, 11);
        data.SetRect(GroundIndex, P(12, 1, 2), 2, 7);
        data.SetCube(GroundIndex, P(4, 0, 3), Column);
        data.SetCube(GroundIndex, P(12, 0, 4), Column);
        data.SetCube(GroundIndex, P(2, 0, 10), Column);

        data.SetRect(WaterIndex, P(10, 0, 10), 4, 5);
        data.SetRect(WaterIndex, P(6, 0, 7), 5, 4);
        data.SetRect(WaterIndex, P(6, 0, 2), 1, 8);
        data.SetRect(WaterIndex, P(10, 0, 3), 1, 8);

        data.SetCube(GroundIndex, P(8, 0, 10), Column);

        data.SetCube(RockIndex, P(2, 0, 1), S(4, 2, 2));
        data.SetCube(RockIndex, P(2, 0, 1), S(2, 4, 1));
        data.SetCube(RockIndex, P(12, 0, 5), S(4, 2, 4));
        data.SetCube(RockIndex, P(12, 0, 7), S(4, 4, 2));
        data.SetCube(RockIndex, P(12, 0, 15), Column);
    }

    private int3 Column = new int3(1, 4, 1);

    private class BuiltMesh
    {
        private Builder builder;
        private Mesh mesh;
        private MeshFilter filter;

        private AlignNormals aligner;

        public BuiltMesh(Builder builder, MeshFilter filter)
        {
            this.builder = builder;
            this.filter = filter;
            mesh = new Mesh();
        }

        public void AddAligner(float3 cellSize)
        {
            aligner = new AlignNormals();
            aligner.Init(0.01f, cellSize, cellSize * 0.975f);
        }

        public void Start()
        {
            builder.Start();
        }

        public void Complete()
        {
            builder.Complete(mesh);
            aligner?.Start(mesh);
            aligner?.Complete();
            filter.sharedMesh = mesh;
        }

        public bool IsGenerating { get => builder.IsGenerating; }

        public void Dispose()
        {
            builder.Dispose();
            aligner?.Dispose();
        }
    }

    private static int3 P(int x, int y, int z) { return new int3(x, y, z); }
    private static int3 S(int x, int y, int z) { return new int3(x, y, z); }
}
