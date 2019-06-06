using Unity.Mathematics;
using UnityEngine;

using MeshBuilderTest;
using MeshBuilder;

using DataVolume = MeshBuilder.Volume<MeshBuilder.Tile.Data>;
using Extents = MeshBuilder.Extents;

using RenderInfo = MeshBuilder.MeshBuilderDrawer.RenderInfo;

[RequireComponent(typeof(MeshBuilderDrawerComponent))]
public class DungeonScene: MonoBehaviour
{
    private const int WallFill = 1;
    private const int WalkwayFill = 2;
    private const int WaterFill = 3;

    [Header("render info")]
    [SerializeField]
    private RenderInfo wall = null;
    [SerializeField]
    private RenderInfo walkway = null;
    [SerializeField]
    private RenderInfo walkwayBg = null;
    [SerializeField]
    private RenderInfo water = null;

    [Header("generation info")]
    [SerializeField]
    private TileTheme walkwayTheme = null;

    [SerializeField]
    private TileTheme wallTheme = null;

    [SerializeField]
    private Texture2D groundHeightMap = null;

    [SerializeField]
    private float groundMaxHeight = 1f;

    private float3 cellSize = new float3(1, 1, 1);

    private Extents dataExtents;
    private DataVolume dataVolume;

    private MeshBuilderDrawerComponent drawerComponent;
    private bool building = true;

    void Awake()
    {
        drawerComponent = GetComponent<MeshBuilderDrawerComponent>();

        dataExtents = new Extents(16, 4, 16);
        dataVolume = new DataVolume(dataExtents);
        FillData(dataVolume);

        var walkwayMesher = new TileMesher2D();
        walkwayMesher.Init(dataVolume, 0, WalkwayFill, walkwayTheme, new TileMesher2D.Settings { centerRandomRotation = true });
        Add(walkwayMesher, walkway);

        var walkwayBgMesher = new GridMesher();
        walkwayBgMesher.Init(dataVolume, WalkwayFill, cellSize, 3, GridMesher.UVMode.Normalized, new float3(0, -1f, 0));
        walkwayBgMesher.InitHeightMapScaleFromHeightAvgOffset(groundHeightMap, groundMaxHeight);
        Add(walkwayBgMesher, walkwayBg);

        var waterMesher = new GridMesher();
        waterMesher.Init(dataVolume, WaterFill, cellSize, 1, GridMesher.UVMode.NoScaling, new float3(0, -1.2f, 0));
        Add(waterMesher, water);

        var wallMesher = new TileMesher2D();
        wallMesher.Init(dataVolume, 0, WallFill, wallTheme, new TileMesher2D.Settings() { emptyBoundaries = Tile.Direction.None });
        Add(wallMesher, wall);

        foreach (var drawer in drawerComponent.Drawers)
        {
            drawer.StartBuilder();
        }
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

    void LateUpdate()
    {
        if (building)
        {
            foreach (var drawer in drawerComponent.Drawers)
            {
                if (drawer.IsBuilderGenerating)
                {
                    drawer.CompleteBuilder();
                }
            }
            building = false;
        }
    }

    private void FillData(DataVolume data)
    {
        data.SetLayer(WallFill, 0);

        data.SetRect(WaterFill, P(1, 1), 14, 14);

        data.SetRect(WalkwayFill, P(2, 2), 5, 5);
        data.SetRect(WalkwayFill, P(7, 6), 5, 1);

        data.SetRect(WalkwayFill, P(2, 8), 10, 1);
        data.SetRect(WallFill, P(0, 9), 7, 6);
        data.SetRect(WalkwayFill, P(1, 10), 5, 5);
        data.SetRect(WalkwayFill, P(4, 9), 1, 2);

        data.SetRect(WalkwayFill, P(9, 9), 5, 5);
        data.SetRect(WalkwayFill, P(9, 4), 3, 4);

        data.SetRect(WallFill, P(9, 13), 4, 1);
        data.SetRect(WallFill, P(13, 6), 1, 8);
    }

    private static int3 P(int x, int z) { return new int3(x, 0, z); }
}
