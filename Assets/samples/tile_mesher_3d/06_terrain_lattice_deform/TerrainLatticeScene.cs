using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using MeshBuilderTest;
using MeshBuilder;

using DataVolume = MeshBuilder.Volume<MeshBuilder.Tile.Data>;
using Extents = MeshBuilder.Extents;

public class TerrainLatticeScene : MonoBehaviour
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

    [Header("lattice")]
    [SerializeField]
    private LatticeGrid groundLattice = null;
    [SerializeField]
    private LatticeGrid grassLattice = null;
    [SerializeField]
    private VertexGridAsset startGrid = null;
    [SerializeField]
    private VertexGridAsset endGrid = null;

    private Extents dataExtents;
    private DataVolume dataVolume;

    private List<BuiltMesh> builders;
    private int generationStep = 0;

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

        foreach (var built in builders)
        {
            built.Start();
        }
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

    void LateUpdate()
    {
        switch(generationStep)
        {
            case 0: CompleteBuilders(); break;
            case 1:
                {
                    FirstGridSnapshot(groundLattice, groundMeshFilter);
                    FirstGridSnapshot(grassLattice, groundTopMeshFilter);
                    break;
                }
            case 2:
                {
                    SecondGridEvaluation(groundLattice, groundMeshFilter);
                    SecondGridEvaluation(grassLattice, groundTopMeshFilter);
                    break;
                }
        }

        if (generationStep < 3)
        {
            ++generationStep;
        }
    }

    private void FillData(DataVolume data)
    {
        data.SetLayer(GroundIndex, 0);
        data.SetRect(GroundIndex, P(2, 1, 2), 3, 11);
        data.SetCube(GroundIndex, P(4, 0, 3), Column);
        data.SetCube(GroundIndex, P(12, 0, 4), Column);
        data.SetCube(GroundIndex, P(2, 0, 10), Column);

        data.SetRect(WaterIndex, P(10, 0, 10), 4, 5);
        data.SetRect(WaterIndex, P(6, 0, 7), 5, 4);
        data.SetRect(WaterIndex, P(6, 0, 2), 1, 8);
        data.SetRect(WaterIndex, P(10, 0, 3), 1, 8);

        data.SetCube(GroundIndex, P(8, 0, 10), Column);

        data.SetCube(GroundIndex, P(12, 1, 2), S(1, 1, 7));
        data.SetCube(GroundIndex, P(14, 1, 2), S(1, 2, 7));
    }

    private void CompleteBuilders()
    {
        foreach (var builder in builders)
        {
            if (builder.IsGenerating)
            {
                builder.Complete();
            }
        }
    }

    private void FirstGridSnapshot(LatticeGrid latticeControl, MeshFilter meshFilter)
    {
        if (latticeControl != null)
        {
            latticeControl.target = meshFilter;
            latticeControl.CopyFrom(startGrid.Grid);
            latticeControl.TakeTargetSnapshot();
        }
    }

    private void SecondGridEvaluation(LatticeGrid latticeControl, MeshFilter meshFilter)
    {
        if (latticeControl != null)
        {
            latticeControl.CopyFrom(endGrid.Grid);
            latticeControl.UpdateTargetSnapshotVertices();
        }
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
