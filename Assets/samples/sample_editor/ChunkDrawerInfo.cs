using UnityEngine;

using MeshBuilder;

[CreateAssetMenu(fileName = "chunk_info", menuName = "EditorSample/ChunkInfo")]
public class ChunkDrawerInfo : ScriptableObject
{
    [SerializeField]
    private Vector3 cellSize = new Vector3(1, 1, 1);
    public Vector3 CellSize { get => cellSize; }

    [SerializeField]
    private TileInfo[] tileInfos = null;
    public TileInfo[] TileInfos { get { return tileInfos; } }

    [System.Serializable]
    public class TileInfo
    {
        [SerializeField]
        private string name = "tile_name";
        public string Name { get { return name; } }

        [SerializeField]
        private int fillValue = -1;
        public int FillValue { get { return fillValue; } }

        [Header("tile")]
        [SerializeField]
        private bool hasTileMesher = false;
        public bool HasTileMesher { get { return hasTileMesher; } }
        [SerializeField]
        private TileTheme tileTheme = null;
        public TileTheme TileTheme { get { return tileTheme; } }
        [SerializeField]
        private TileMesher2D.Settings tileSettings = null;
        public TileMesher2D.Settings TileSettings { get { return tileSettings; } }
        [SerializeField]
        private bool alignNormals = false;
        public bool AlignNormals { get { return alignNormals; } }
        [SerializeField]
        private bool allowRandomLatticeMod = false;
        public bool AllowRandomLatticeMod { get { return allowRandomLatticeMod; } }
        [SerializeField]
        private Vector3 randomLatticeOffsetRangeMin = Vector3.zero;
        public Vector3 RandomLatticeOffsetRangeMin => randomLatticeOffsetRangeMin;
        [SerializeField]
        private Vector3 randomLatticeOffsetRangeMax = Vector3.zero;
        public Vector3 RandomLatticeOffsetRangeMax => randomLatticeOffsetRangeMax;
        [SerializeField]
        private MeshBuilderDrawer.RenderInfo tileRenderInfo = null;
        public MeshBuilderDrawer.RenderInfo TileRenderInfo { get { return tileRenderInfo; } }

        [Header("grid")]
        [SerializeField]
        private bool hasGridMesher = false;
        public bool HasGridMesher { get { return hasGridMesher; } }
        [SerializeField]
        private int gridResolution = 2;
        public int GridResolution { get { return gridResolution; } }
        [SerializeField]
        private GridMesher.UVMode gridUvMode = GridMesher.UVMode.Normalized;
        public GridMesher.UVMode GridUvMode { get { return gridUvMode; } }
        [SerializeField]
        private Vector3 gridOffset = Vector3.zero;
        public Vector3 GridOffset { get { return gridOffset; } }
        [SerializeField]
        private Texture2D gridHeightMap = null;
        public Texture2D GridHeightMap { get { return gridHeightMap; } }
        [SerializeField]
        private float maxHeight = 0;
        public float MaxHeight { get { return maxHeight; } }
        [SerializeField]
        private bool applyHeightmapEdgeLerp = false;
        public bool ApplyHeightmapEdgeLerp { get { return applyHeightmapEdgeLerp; } }
        [SerializeField]
        private MeshBuilderDrawer.RenderInfo gridRenderInfo = null;
        public MeshBuilderDrawer.RenderInfo GridRenderInfo { get { return gridRenderInfo; } }
    }
}
