using UnityEngine;
using MeshBuilder;

using TileInfo = ChunkDrawerInfo.TileInfo;

public class ChunkDrawer : MeshBuilderDrawerComponent
{
    [SerializeField]
    private ChunkDrawerInfo info = null;
    public ChunkDrawerInfo Info 
    { 
        get => info; 
        set
        {
            info = value;
            InitTileDrawers();
        }
    }

    private TileDrawer[] tileDrawers;

    private Volume<Tile.Data> tileData = null;
    public Volume<Tile.Data> TileData
    {
        get => tileData;
        set
        {
            tileData = value;
            Rebuild();
        }
    }

    void Awake()
    {
        if (info != null)
        {
            InitTileDrawers();
        }
    }

    private void InitTileDrawers()
    {
        Dispose();

        if (info != null)
        {
            int length = info.TileInfos.Length;
            tileDrawers = new TileDrawer[length];
            for (int i = 0; i < tileDrawers.Length; ++i)
            {
                tileDrawers[i] = new TileDrawer(info.TileInfos[i], this, info.CellSize);
            }
        }

        Rebuild();
    }

    private bool updateDrawers = false;

    public void Rebuild()
    {
        updateDrawers = true;
    }

    private void Update()
    {
        if (updateDrawers && tileData != null)
        {
            bool couldUpdateDrawers = UpdateDrawers();
            updateDrawers = !couldUpdateDrawers;
        }
    }

    private bool UpdateDrawers()
    {
        if (tileDrawers != null)
        {
            bool canUpdateTileData = true;
            foreach (var drawer in tileDrawers)
            {
                if (drawer.IsGenerating)
                {
                    canUpdateTileData = false;
                    break;
                }
            }

            if (canUpdateTileData)
            {
                foreach (var drawer in tileDrawers)
                {
                    drawer.UpdateTileData(tileData);
                }
                return true;
            }
        }

        return false;
    }

    private void LateUpdate()
    {
        if (tileDrawers != null)
        {
            foreach (var drawer in tileDrawers)
            {
                drawer.EndFrame();
            }
        }
    }

    private void OnDestroy()
    {
        Dispose();
    }
    
    public void Dispose()
    {
        if (tileDrawers != null)
        {
            for (int i = 0; i < tileDrawers.Length; ++i)
            {
                tileDrawers[i].Dispose();
            }

            tileDrawers = null;
        }
    }

    /// <summary>
    /// /////////////////////////////
    /// </summary>

    private class TileDrawer
    {
        private Vector3 CellSize { get;  }
        private TileInfo Info { get; }

        public TileMesher2D TileMesher { get; }
        private AlignNormals alignNormals;

        private LatticeGridModifier latticeModifier;

        private MeshBuilderDrawer tileDrawer;
        private MeshBuilderDrawer gridDrawer;

        public TileDrawer(TileInfo info, MeshBuilderDrawerComponent drawerComponent, Vector3 cellSize)
        {
            CellSize = cellSize;
            Info = info;
            
            if (info.HasTileMesher)
            {
                tileDrawer = MeshBuilderDrawer.Create<TileMesher2D>(info.TileRenderInfo);
                TileMesher = tileDrawer.Get<TileMesher2D>();
                drawerComponent.AddDrawer(tileDrawer);
                
                if (info.AlignNormals)
                {
                   alignNormals = new AlignNormals();
                   alignNormals.Init(0.01f, CellSize, CellSize * 0.95f);
                }

                if (info.AllowRandomLatticeMod)
                {
                    latticeModifier = new LatticeGridModifier();
                }
            }
            if (info.HasGridMesher)
            {
                gridDrawer = MeshBuilderDrawer.Create<GridMesher>(info.GridRenderInfo);
                drawerComponent.AddDrawer(gridDrawer);
            }
        }
        
        public void UpdateTileData(Volume<Tile.Data> data)
        {
            if (tileDrawer != null)
            {
                tileDrawer.Get<TileMesher2D>().Init(data, 0, Info.FillValue, Info.TileTheme, CellSize, Info.TileSettings);
                tileDrawer.StartBuilder();

                if (latticeModifier != null)
                {
                    InitLatticeChange(data.XLength, data.YLength, data.ZLength);
                }
            }
            if (gridDrawer != null)
            {
                var mesher = gridDrawer.Get<GridMesher>();
                mesher.Init(data, Info.FillValue, CellSize, Info.GridResolution, Info.GridUvMode, Info.GridOffset);
                if (Info.GridHeightMap != null)
                {
                    mesher.InitHeightMapScaleFromHeightAvgOffset(Info.GridHeightMap, Info.MaxHeight, Info.ApplyHeightmapEdgeLerp);
                }
                gridDrawer.StartBuilder();
            }
        }

        private void InitLatticeChange(int dataX, int dataY, int dataZ)
        {
            Vector3 chunkCenter = new Vector3(dataX * CellSize.x * 0.5f, 0, dataZ * CellSize.z * 0.5f);
            Matrix4x4 originalGridWorldToLocal = Matrix4x4.Translate(-chunkCenter);
            Matrix4x4 targetLocalToWorld = Matrix4x4.Translate(chunkCenter);
            Matrix4x4 meshLocalToWorld = Matrix4x4.identity;
            // +2 on the horizontal plane (data contains cells, the lattice expect vertices nums, it's also a bit larger because there can be vertices outside of the cells)
            // +3 on the vertical plane (just a random value, the data in this case is just a 2d map, but the generated mesh has more verticality)
            var extents = new Unity.Mathematics.int3(dataX+ 2, dataY + 3, dataZ + 2);
            latticeModifier.InitRandomLatticeChange(originalGridWorldToLocal, extents, CellSize, targetLocalToWorld, meshLocalToWorld, Info.RandomLatticeOffsetRangeMin, Info.RandomLatticeOffsetRangeMax);
        }

        public bool IsGenerating
        {
            get => (tileDrawer != null && tileDrawer.IsBuilderGenerating) ||
                (gridDrawer != null && gridDrawer.IsBuilderGenerating) ||
                (alignNormals != null && alignNormals.IsGenerating) ||
                (latticeModifier != null && latticeModifier.IsGenerating) 
                ;
        }

        public void EndFrame()
        {
            if (alignNormals != null && alignNormals.IsGenerating)
            {
                alignNormals.Complete();
            }

            if (tileDrawer != null && tileDrawer.IsBuilderGenerating)
            {
                tileDrawer.CompleteBuilder();

                if (alignNormals != null)
                {
                    alignNormals.Start(tileDrawer.Mesh);
                }

                if (latticeModifier != null)
                {
                    latticeModifier.Start(tileDrawer.Mesh);
                    latticeModifier.Complete();
                }
            }

            if (gridDrawer != null && gridDrawer.IsBuilderGenerating)
            {
                gridDrawer.CompleteBuilder();
            }
        }

        public void Dispose()
        {
            if (tileDrawer != null)
            {
                tileDrawer.Dispose();
                tileDrawer = null;
            }
            if (alignNormals != null)
            {
                alignNormals.Dispose();
                alignNormals = null;
            }
            if (gridDrawer != null)
            {
                gridDrawer.Dispose();
                gridDrawer = null;
            }
            if (latticeModifier != null)
            {
                latticeModifier.Dispose();
                latticeModifier = null;
            }
        }
    }
}
