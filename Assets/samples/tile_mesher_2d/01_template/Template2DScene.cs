using UnityEngine;
using Unity.Mathematics;

using MeshBuilderTest;
using MeshBuilder;

using DataVolume = MeshBuilder.Volume<MeshBuilder.Tile.Data>;
using Extents = MeshBuilder.Extents;

public class Template2DScene : MonoBehaviour
{
    private const byte VoidValue = 0;
    private const byte FillValue = 1;

    [Header("mesh")]
    [SerializeField]
    private MeshFilter meshFilter = null;

    [Header("generation info")]
    [SerializeField]
    private TileTheme theme = null;

    private Extents dataExtents;
    private DataVolume dataVolume;

    private bool building = true;
    private Mesh mesh;
    private TileMesher2D mesher;

    void Awake()
    {
        dataExtents = new Extents(16, 1, 16);
        dataVolume = new DataVolume(dataExtents);
        FillData(dataVolume);

        mesh = new Mesh();

        mesher = new TileMesher2D();
        mesher.Init(dataVolume, 0, FillValue, theme, new float3(1, 1, 1));
        mesher.Start();
    }

    private void OnDestroy()
    {
        dataVolume.Dispose();
        mesher.Dispose();
    }

    void LateUpdate()
    {
        if (building)
        {
            if (mesher.IsGenerating)
            {
                mesher.Complete(mesh);
                meshFilter.sharedMesh = mesh;
            }
            building = false;
        }
    }

    private void FillData(DataVolume data)
    {
        data.SetRect(FillValue, P(2, 1), 2, 12);

        data.SetRect(FillValue, P(0, 5), 7, 1);
        data.SetRect(FillValue, P(7, 6), 7, 1);

        data.SetRect(FillValue, P(11, 11), 5, 5);
        data.SetRect(VoidValue, P(12, 12), 3, 3);
        data.SetRect(FillValue, P(13, 13), 1, 1);

        data.SetRect(FillValue, P(11, 5), 4, 4);

        data.Set(FillValue, P(5, 15));
        data.Set(FillValue, P(7, 15));
        data.Set(FillValue, P(9, 15));
        data.Set(FillValue, P(6, 14));
        data.Set(FillValue, P(8, 14));
        data.Set(FillValue, P(5, 13));
        data.Set(FillValue, P(7, 13));
        data.Set(FillValue, P(9, 13));

        data.Set(FillValue, P(5, 2));
        data.Set(FillValue, P(7, 2));
        data.Set(FillValue, P(9, 2));
        data.Set(FillValue, P(6, 1));
        data.Set(FillValue, P(8, 1));
        data.Set(FillValue, P(5, 0));
        data.Set(FillValue, P(7, 0));
        data.Set(FillValue, P(9, 0));

        data.SetRect(FillValue, P(10, 0), 2, 5);
    }

    private static int3 P(int x, int z) { return new int3(x, 0, z); }
}
