using UnityEngine;
using Unity.Mathematics;

using MeshBuilderTest;
using MeshBuilder;

using DataVolume = MeshBuilder.Volume<MeshBuilder.Tile.Data>;
using Extents = MeshBuilder.Extents;

public class LatticeScene : MonoBehaviour
{
    private const byte VoidValue = 0;
    private const byte FillValue = 1;

    [Header("mesh")]
    [SerializeField]
    private MeshFilter meshFilter = null;

    [Header("generation info")]
    [SerializeField]
    private TileTheme theme = null;

    [SerializeField]
    private Vector3 cellSize = new Vector3(1, 1, 1);

    [Header("lattice")]
    [SerializeField]
    private LatticeGrid latticeControl = null;
    [SerializeField]
    private VertexGridAsset startLattice = null;
    [SerializeField]
    private VertexGridAsset endLattice = null;

    private Extents dataExtents;
    private DataVolume dataVolume;

    private Mesh mesh;
    private TileMesher3D mesher;

    private int generationStep = 0;

    void Awake()
    {
        dataExtents = new Extents(16, 8, 16);
        dataVolume = new DataVolume(dataExtents);
        FillData(dataVolume);

        mesh = new Mesh();

        mesher = new TileMesher3D();
        mesher.Init(dataVolume, FillValue, theme, cellSize);
        mesher.Start();
    }

    private void OnDestroy()
    {
        dataVolume.Dispose();
        mesher.Dispose();
    }

    void LateUpdate()
    {
        switch (generationStep)
        {
            case 0: CompleteMesher(); break;
            case 1: FirstGridSnapshot(); break;
            case 2: SecondGridEvaluation(); break;
        }

        if (generationStep < 3)
        {
            ++generationStep;
        }
    }

    private void FillData(DataVolume data)
    {
        data.SetLayer(FillValue, 0);
        data.SetRect(VoidValue, P(8, 0, 0), 1, 16);

        data.SetCube(FillValue, P(10, 0, 10), S(2, 2, 2));
        data.SetCube(FillValue, P(12, 0, 12), S(2, 2, 2));

        data.SetRect(FillValue, P(2, 1, 2), 3, 11);
        data.SetRect(FillValue, P(2, 2, 2), 3, 11);

        data.SetRect(VoidValue, P(3, 1, 2), 1, 11);
        data.SetRect(VoidValue, P(1, 0, 5), 11, 1);
        data.SetRect(VoidValue, P(1, 1, 5), 11, 1);

        data.SetRect(VoidValue, P(1, 1, 8), 11, 3);

        data.SetRect(FillValue, P(12, 1, 2), 2, 7);
        data.SetCube(FillValue, P(4, 0, 3), Column);
        data.SetCube(FillValue, P(12, 0, 4), Column);
        data.SetCube(FillValue, P(2, 0, 10), Column);

        data.SetRect(FillValue, P(1, 4, 6), 6, 6);
        data.SetRect(VoidValue, P(2, 4, 7), 4, 4);
    }

    private void CompleteMesher()
    {
        if (mesher.IsGenerating)
        {
            mesher.Complete(mesh);
            meshFilter.sharedMesh = mesh;
        }
    }

    private void FirstGridSnapshot()
    {
        if (latticeControl != null)
        {
            latticeControl.target = meshFilter;
            latticeControl.CopyFrom(startLattice.Grid);
            latticeControl.TakeTargetSnapshot();
        }
    }

    private void SecondGridEvaluation()
    {
        if (latticeControl)
        {
            latticeControl.CopyFrom(endLattice.Grid);
            latticeControl.UpdateTargetSnapshotVertices();
        }
    }

    private int3 Column = new int3(1, 5, 1);

    private static int3 P(int x, int y, int z) { return new int3(x, y, z); }
    private static int3 S(int x, int y, int z) { return new int3(x, y, z); }
}
