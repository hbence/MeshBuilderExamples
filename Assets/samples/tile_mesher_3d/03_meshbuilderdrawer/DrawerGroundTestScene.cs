using System.Collections;

using Unity.Mathematics;
using UnityEngine;

using MeshBuilderTest;
using MeshBuilder;

using DataVolume = MeshBuilder.Volume<MeshBuilder.Tile.Data>;
using Extents = MeshBuilder.Extents;

using RenderInfo = MeshBuilder.MeshBuilderDrawer.RenderInfo;

[RequireComponent(typeof(MeshBuilderDrawerComponent))]
public class DrawerGroundTestScene : MonoBehaviour
{
    private const int GroundIndex = 1;
    private const int WaterIndex = 2;
    private const int RockIndex = 3;

    [Header("render info")]
    [SerializeField]
    private RenderInfo ground = null;
    [SerializeField]
    private RenderInfo grass = null;
    [SerializeField]
    private RenderInfo water = null;
    [SerializeField]
    private RenderInfo rock = null;

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

    private MeshBuilderDrawerComponent drawerComponent;

    void Awake()
    {
        drawerComponent = GetComponent<MeshBuilderDrawerComponent>();

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
        Add(groundMesher, ground);
        if (useGroundNormalAlignment)
        {
     //       builders[builders.Count - 1].AddAligner(cellSize);
        }

        var topGroundMesher = new GridMesher();
        topGroundMesher.Init(dataVolume, GroundIndex, cellSize, 3, GridMesher.UVMode.Normalized);
        topGroundMesher.InitHeightMapScaleFromHeightAvgOffset(groundHeightMap, groundMaxHeight);
        Add(topGroundMesher, grass);

        var waterMesher = new GridMesher();
        waterMesher.Init(dataVolume, WaterIndex, cellSize, 1, GridMesher.UVMode.NoScaling, new float3(0, -0.2f, 0));
        Add(waterMesher, water);

        var rockMesher = new TileMesher3D();
        rockMesher.Init(dataVolume, 1, palette, cellSize);
        Add(rockMesher, rock);

        foreach (var drawer in drawerComponent.Drawers)
        {
            drawer.StartBuilder();
        }

        StartCoroutine(CompleteMesh());
    }

    private IEnumerator CompleteMesh()
    {
        yield return new WaitForEndOfFrame();

        foreach (var drawer in drawerComponent.Drawers)
        {
            if (drawer.IsBuilderGenerating)
            {
                drawer.CompleteBuilder();
            }
        }

        yield return null;
    }

    private void Add(Builder builder, RenderInfo renderInfo)
    {
        drawerComponent.AddDrawer(new MeshBuilderDrawer(renderInfo, builder));
    }

    private void OnDestroy()
    {
        dataVolume.Dispose();

        foreach (var drawer in drawerComponent.Drawers)
        {
            drawer.Dispose();
        }
        drawerComponent.RemoveAll();
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

    private static int3 P(int x, int y, int z) { return new int3(x, y, z); }
    private static int3 S(int x, int y, int z) { return new int3(x, y, z); }
}
