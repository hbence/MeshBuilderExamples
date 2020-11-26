using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MeshBuilder;

public class SampleEditorControl : MonoBehaviour
{
    private const float CursorHeight = 0.55f;
    private const float PlacementTimeLimit = 0.15f;

    private const byte WaterIndex = 0;
    private const byte GroundIndex = 1;
    private const byte WallIndex = 2;

    private const float ObjectPlacementOffset = -0.5f;

    private enum EditMode
    {
        Tiles,
        Objects
    }

    private EditMode editMode;

    [Serializable]
    public class ObjectItem
    {
        public string name;
        public GameObject prototype;
    }
    
    [SerializeField] private Camera editorCamera = null;
    [SerializeField] private ChunkDrawerInfo chunkInfo = null;
    [SerializeField] private string levelPath = "test_file.txt";
    [SerializeField] private TextAsset testLevel = null;

    [SerializeField] private ObjectItem[] objectPrototypes = null;

    [Header("ui")]
    [SerializeField] private Transform cursor = null;
    [SerializeField] private GameObject itemButton = null;
    [SerializeField] private Transform itemRoot = null;
    [SerializeField] private Button tileBtn = null;
    [SerializeField] private Button objectBtn = null;

    private readonly Item[] Tiles = new Item[]
    {
        new Item("water", WaterIndex),
        new Item("ground", GroundIndex),
        new Item("wall", WallIndex),
    };

    private Item[] Objects;

    public bool printMeshData = false;
    public bool getMergedMesh = false;
    public string meshDataPath;
    public Mesh testMesh;

    public float CellSize => chunkInfo.CellSize.x;
    private Vector2 StartPos;
    private Vector2Int cursorCoord;

    private ChunkDrawer chunkDrawer = null;
    private Volume<Tile.Data> data;

    private Transform objectRoot = null;
    private List<PlacedObject> placedObjects;

    private Plane editorPlane;
    private byte selectedTile = 0;

    private int selectedObjectIndex = 0;

    private ObjectMeshCombination mergedObjects;

    void Awake()
    {
        mergedObjects = new ObjectMeshCombination(transform);

        Objects = new Item[objectPrototypes.Length + 1];
        Objects[0] = new Item("Remove", -1);
        for (int i = 1; i < Objects.Length; ++i)
        {
            int protoIndex = i - 1;
            Objects[i] = new Item(objectPrototypes[protoIndex].name, protoIndex);
        }

        editorPlane = new Plane(Vector3.down, 0);

        GameObject go = new GameObject("editor_chunk");
        go.transform.SetParent(transform);

        chunkDrawer = go.AddComponent<ChunkDrawer>();
        chunkDrawer.Info = chunkInfo;

        data = new Volume<Tile.Data>(10, 1, 10);
        InitData();
        chunkDrawer.TileData = data;

        objectRoot = new GameObject("object_root").transform;
        objectRoot.SetParent(transform);
        objectRoot.localPosition = new Vector3(0, ObjectPlacementOffset, 0);

        placedObjects = new List<PlacedObject>();

        int colNum = chunkDrawer.TileData.XLength;
        int rowNum = chunkDrawer.TileData.ZLength;
        float sizeX = colNum * CellSize;
        float sizeZ = rowNum * CellSize;
        StartPos = new Vector2(-sizeX * 0.5f, -sizeZ * 0.5f);
        Place(chunkDrawer);
    }

    private void Start()
    {
        OnTileButtonClicked();
    }

    void Update()
    {
        HandleMouse();

        if (printMeshData)
        {
            printMeshData = false;
            PrintMeshData(testMesh, meshDataPath);
        }

        if (getMergedMesh)
        {
            getMergedMesh = false;
            testMesh = mergedObjects.GetMesh();
        }
    }

    private float mouseDownTimer;

    private void HandleMouse()
    {
        if (EditorUtils.IsPointerOverGameObject)
        {
            return;
        }

        PlaceCursor();

        if (Input.GetMouseButton(0))
        {
            mouseDownTimer += Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0))
        {
            mouseDownTimer = 0;
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (mouseDownTimer < PlacementTimeLimit)
            {
                var mousePos = GetMousePosition();
                var coord = ToCoord(mousePos);

                if (editMode == EditMode.Tiles)
                {
                    ChangeTileData(coord.x, coord.y, selectedTile);
                }
                else
                {
                    ChangeObjectData(coord.x, coord.y, selectedObjectIndex);
                }
            }
        }
    }

    private void OnDestroy()
    {
        data.Dispose();
    }

    private void InitData()
    {
        for(int i = 0; i < data.Length; ++i)
        {
            data[i] = ToData(WaterIndex);
        }

        for (int z = 1; z < data.ZLength - 1; ++z)
        {
            for (int x = 1; x < data.XLength - 1; ++x)
            {
                data[x, 0, z] = ToData(GroundIndex);

                if ((x == 4 || x == 7) || (z == 4 || z == 7) && x > 4 && z > 4)
                {
                    data[x, 0, z] = ToData(WallIndex);
                }
            }
        }
    }

    private void ChangeTileData(int col, int row, byte tile)
    {
        if (IsInBounds(col, row))
        {
            data[col, 0, row] = ToData(tile);
            chunkDrawer.TileData = data;
        }
    }

    private void ChangeObjectData(int col, int row, int index)
    {
        if (IsInBounds(col, row))
        {
            int found = placedObjects.FindIndex((PlacedObject obj) => { return obj.Coord.x == col && obj.Coord.y == row; } );
            if (found >= 0)
            {
                DestroyImmediate(placedObjects[found].GO);
                placedObjects.RemoveAt(found);
            }

            if (index >= 0)
            {
                CreateObject(col, row, index);
            }

            mergedObjects.CreateMerged(placedObjects);
        }
    }

    private void CreateObject(int col, int row, int index)
    {
        var item = objectPrototypes[index];
        GameObject go = Instantiate(item.prototype);
        go.transform.SetParent(objectRoot);
        go.transform.localPosition = ToPos(col, row);

        placedObjects.Add(new PlacedObject(item.name, go, new Vector2Int(col, row)));
    }

    private Vector3 GetMousePosition()
    {
        var ray = editorCamera.ScreenPointToRay(Input.mousePosition);
        float enter;
        editorPlane.Raycast(ray, out enter);
        return ray.GetPoint(enter);
    }

    private void PlaceCursor()
    {
        var mousePos = GetMousePosition();
        var coord = ToCoord(mousePos);
        cursor.transform.localPosition = ToPos(coord.x, coord.y);
        cursor.gameObject.SetActive(IsInBounds(coord.x, coord.y));

        if (IsInBounds(coord.x, coord.y))
        {
            cursorCoord = coord;
        }
    }
    
    private Vector2Int ToCoord(Vector3 pos)
    {
        float x = pos.x - StartPos.x;
        float z = pos.z - StartPos.y;
        int col = Mathf.FloorToInt(x);
        int row = Mathf.FloorToInt(z);
        return new Vector2Int(col, row);
    }
    
    private Vector3 ToPos(int col, int row)
    {
        return new Vector3(StartPos.x + (col + 0.5f) * CellSize, CursorHeight,
                           StartPos.y + (row + 0.5f) * CellSize);
    }

    private bool IsInBounds(int col, int row)
    {
        return col >= 0 && row >= 0 && col < chunkDrawer.TileData.XLength && row < chunkDrawer.TileData.ZLength;
    }

    private void Place(ChunkDrawer chunk)
    {
        chunk.transform.localPosition = new Vector3(StartPos.x, 0, StartPos.y);
    }

    public void Save()
    {
        try
        {
            LevelData levelData = new LevelData();
            levelData.FromDataVolume(data);
            levelData.FromPlacedObjects(placedObjects);
            LevelData.WriteToFile(levelPath, levelData);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void Load()
    {
        try
        {
            LevelData levelData = null;
            if (File.Exists(levelPath))
            {
                levelData = LevelData.ReadFromFile(levelPath);
            }
            else
            {
                levelData = LevelData.FromText(testLevel.text);
            }
            levelData.ToDataVolume(data);
            chunkDrawer.TileData = data;

            foreach(var obj in placedObjects)
            {
                DestroyImmediate(obj.GO);
            }
            placedObjects.Clear();

            if (levelData.Objects != null)
            {
                for (int i = 0; i < levelData.Objects.Length; ++i)
                {
                    var objData = levelData.Objects[i];
                    int found = Array.FindIndex(objectPrototypes, (ObjectItem item) => { return item.name == objData.Name; });
                    if (found >= 0)
                    {
                        CreateObject(objData.Col, objData.Row, found);
                    }
                }
            }

            mergedObjects.CreateMerged(placedObjects);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void OnTileButtonClicked()
    {
        editMode = EditMode.Tiles;
        SetButtons(Tiles);
        SetColor(tileBtn, true);
        SetColor(objectBtn, false);
    }

    public void OnObjectButtonClicked()
    {
        editMode = EditMode.Objects;
        SetButtons(Objects);
        SetColor(objectBtn, true);
        SetColor(tileBtn, false);
    }

    private void SetButtons(Item[] items)
    {
        while(itemRoot.childCount > 0)
        {
            DestroyImmediate(itemRoot.GetChild(0).gameObject);
        }

        foreach(var item in items)
        {
            CreateButton(item);
        }
    }

    private void CreateButton(Item item)
    {
        GameObject go = Instantiate(itemButton);
        Button button = go.GetComponent<Button>();
        Text label = go.GetComponentInChildren<Text>();
        label.text = item.Label;
        button.onClick.AddListener(() => { OnItemClicked(button, item.Value); });
        go.transform.SetParent(itemRoot);
    }

    private void OnItemClicked(Button button, int value)
    {
        SetActiveListButton(button);

        if (editMode == EditMode.Tiles)
        {
            selectedTile = (byte)value;
        }
        else
        {
            selectedObjectIndex = value;
        }
    }

    private void SetActiveListButton(Button button)
    {
        for (int i = 0; i < itemRoot.childCount; ++i)
        {
            Button child = itemRoot.GetChild(i).GetComponent<Button>();
            if (child != null)
            {
                bool active = child == button;
                SetColor(child, active);
            }
        }
    }

    static private void SetColor(Button button, bool active)
    {
        var colors = button.colors;
        colors.normalColor = active ? Color.yellow : Color.white;
        colors.highlightedColor = colors.normalColor;
        colors.pressedColor = colors.normalColor;
        colors.selectedColor= colors.normalColor;
        button.colors = colors;
    }

    static private Tile.Data ToData(byte themeIndex) => new Tile.Data() { themeIndex = themeIndex };

    private class Item
    {
        public string Label { get; }
        public int Value { get; }
        public Item(string label, int value) { Label = label; Value = value; }
    }

    private class PlacedObject
    {
        public string Name { get; }
        public GameObject GO { get; }
        public Vector2Int Coord { get; }
        public PlacedObject(string name, GameObject go, Vector2Int coord) { Name = name; GO = go; Coord = coord; }
    }

    private class ObjectMeshCombination
    {
        private Transform parent;
        private GameObject merged;

        public ObjectMeshCombination(Transform parent)
        {
            this.parent = parent;
        }
        
        public void CreateMerged(List<PlacedObject> placed)
        {
            GameObject[] gos = new GameObject[placed.Count];
            for(int i = 0; i < gos.Length; ++i)
            {
                gos[i] = placed[i].GO;
            }

            if(merged != null)
            {
                DestroyImmediate(merged);
            }

            merged = MeshCombinationUtils.CreateMergedMesh(gos, true);
            merged.transform.SetParent(parent);
        }
        
        /*
        public void CreateMerged(List<PlacedObject> placed)
        {
            if (placed.Count <= 0)
            {
                return;
            }

            Mesh[] meshes = new Mesh[placed.Count];
            Matrix4x4[] matrices = new Matrix4x4[meshes.Length];

            for (int i = 0; i < meshes.Length; ++i)
            {
                MeshFilter filter = placed[i].GO.GetComponent<MeshFilter>();
                meshes[i] = filter.sharedMesh;
                matrices[i] = Matrix4x4.identity;
            }

            merged = new GameObject("merged");
            merged.transform.SetParent(parent);

            var renderer = merged.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = placed[0].GO.GetComponent<MeshRenderer>().sharedMaterials;
            renderer.receiveShadows = true;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            Mesh mesh = new Mesh();

            var combinationBuilder = new MeshCombinationBuilder();
            combinationBuilder.Init(meshes, matrices);
            combinationBuilder.Start();
            combinationBuilder.Complete(mesh);
            combinationBuilder.Dispose();

            var meshFilter = merged.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
        }*/

        public Mesh GetMesh()
        {
            var meshFilter = merged.GetComponentInChildren<MeshFilter>();
            return meshFilter.sharedMesh;
        }
    }

    private void PrintMeshData(Mesh mesh, string path)
    {
        string data = "";
        for (int i = 0; i < mesh.vertices.Length; ++i)
        {
            data += $"\n {i}: {mesh.vertices[i]} | {mesh.normals[i]}";
        }
        File.WriteAllText(path, data);
    }

    [Serializable]
    private class LevelData
    {
        [Serializable]
        public class ObjectData
        {
            [SerializeField] private int col;
            public int Col => col;
            [SerializeField] private int row;
            public int Row => row;
            [SerializeField] private string name;
            public string Name => name;
            public ObjectData(int x, int y, string n) { col = x; row = y; name = n; }
        }

        [SerializeField] private int colNum;
        public int ColNum => colNum;
        [SerializeField] private int rowNum;
        public int RowNum => rowNum;
        [SerializeField] private byte[] tileData;
        public byte[] TileData => tileData;
        [SerializeField] private ObjectData[] objects;
        public ObjectData[] Objects => objects;

        public void FromPlacedObjects(List<PlacedObject> placed)
        {
            if (objects == null || objects.Length != placed.Count)
            {
                objects = new ObjectData[placed.Count];
            }

            for (int i = 0; i < placed.Count; ++i)
            {
                var obj = placed[i];
                objects[i] = new ObjectData(obj.Coord.x, obj.Coord.y, obj.Name);
            }
        }

        public void FromDataVolume(Volume<Tile.Data> volume)
        {
            colNum = volume.XLength;
            rowNum = volume.ZLength;

            if (tileData == null || tileData.Length != colNum * rowNum)
            {
                tileData = new byte[colNum * rowNum];
            }

            for (int i = 0; i < tileData.Length; ++i)
            {
                tileData[i] = volume[i].themeIndex;
            }
        }

        public void ToDataVolume(Volume<Tile.Data> volume)
        {
            if (volume.IsDisposed || volume.XLength != colNum || volume.ZLength != rowNum)
            {
                if (!volume.IsDisposed)
                {
                    volume.Dispose();
                }

                volume = new Volume<Tile.Data>(colNum, 1, rowNum);
            }

            for (int i = 0; i < tileData.Length; ++i)
            {
                volume[i] = new Tile.Data() { themeIndex = tileData[i] };
            }
        }

        public static void WriteToFile(string path, LevelData levelData)
        {
            try
            {
                string data = JsonUtility.ToJson(levelData);
                File.WriteAllText(path, data);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static LevelData ReadFromFile(string path)
        {
            try
            {
                string text = File.ReadAllText(path);
                return JsonUtility.FromJson<LevelData>(text);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return null;
        }

        public static LevelData FromText(string text)
        {
            try
            {
                return JsonUtility.FromJson<LevelData>(text);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return null;
        }
    }
}
