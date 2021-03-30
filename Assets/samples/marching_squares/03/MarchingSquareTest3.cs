using UnityEngine;

using MeshBuilder;
using Unity.Collections;
using System.IO;
using System;

using static MeshBuilder.MarchingSquaresMesher;
using MesherType = MeshBuilder.MarchingSquaresComponent.InitializationInfo.Type;

public class MarchingSquareTest3 : MonoBehaviour
{
    private const string AdditiveLabel = "Additive";
    private const string SubtractiveLabel = "Subtractive";

    private const string SimpleLabel = "Simple";
    private const string OptimizedGreedyLabel = "Optimized (greedy rect)";
    private const string OptimizedLargestLabel = "Optimized (next largest)";

    private string[] ModeButtonLabels = { SimpleLabel, OptimizedGreedyLabel, OptimizedLargestLabel };

    [SerializeField] private Camera cam = null; 

    [Header("brush")]
    [SerializeField] private MarchingSquaresComponent march = null;
    [SerializeField] private float radius = 0.5f;

    private float CellSize => march.CellSize;


    [Header("ui")]
    private bool additive = true;
    [SerializeField] private UnityEngine.UI.Text brushButtonLabel = null;
    private int meshModeOptimized = 0;
    [SerializeField] private UnityEngine.UI.Text modeButtonLabel = null;

    [SerializeField] private UnityEngine.UI.Text meshInfoLabel = null;

    [Header("file")]
    [SerializeField] private string levelPath = "ms03_dist_data.bin";
    [SerializeField] private bool save = false;
    [SerializeField] private bool load = false;

    private Data data => march.Data;
    private DistanceData dataHandler;

    void Start()
    {
        InitMesher();

        Load();

        march.OnMeshChanged += UpdateMeshInfo;
        march.Regenerate();

        brushButtonLabel.text = additive ? AdditiveLabel : SubtractiveLabel;
        modeButtonLabel.text = ModeButtonLabels[meshModeOptimized];
    }

    private void InitMesher()
    {
        switch (meshModeOptimized)
        {
            case 0: march.InitInfo.type = MesherType.TopOnly; break;
            case 1: march.InitInfo.type = MesherType.TopOptimizedGreedy; break;
            case 2: march.InitInfo.type = MesherType.TopOptimizedLargestRect; break;
        }
        march.Init();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 pos = GetHitPosition(Input.mousePosition);
            if (additive)
            {
                DrawAt(pos);
            }
            else
            {
                EraseAt(pos);
            }

            march.Regenerate();
        }

        if (save)
        {
            save = false;
            Save();
        }
        if (load)
        {
            load = false;
            Load();
        }
    }

    private void DrawAt(Vector3 pos)
    {
        data.ApplyCircle(pos.x, pos.z, radius, CellSize);
    }

    private void EraseAt(Vector3 pos)
    {
        data.RemoveCircle(pos.x, pos.z, radius, CellSize);
    }

    public void ChangeBrushMode()
    {
        additive = !additive;
        brushButtonLabel.text = additive ? AdditiveLabel : SubtractiveLabel;
    }

    public void ChangeMeshMode()
    {
        meshModeOptimized = (meshModeOptimized + 1) % ModeButtonLabels.Length;
        modeButtonLabel.text = ModeButtonLabels[meshModeOptimized];

        dataHandler.FromData(data);
        InitMesher();
        dataHandler.ToData(data);
        march.Regenerate();
    }

    public void Clear()
    {
        data.Clear();
        march.Regenerate();
    }

    private void Save()
    {
        dataHandler.FromData(data);
        DistanceData.WriteToFile(levelPath, dataHandler);
    }

    private void Load()
    {
        dataHandler = DistanceData.ReadFromFile(levelPath);
        if (dataHandler == null)
        {
            dataHandler = new DistanceData();
        }
        else
        {
            dataHandler.ToData(data);
        }
    }

    private void UpdateMeshInfo(Mesh mesh)
    {
        meshInfoLabel.text = $"mesh | vertex:{mesh.vertexCount} tri:{mesh.triangles.Length / 3}";
    }

    private Vector3 GetHitPosition(Vector3 pos)
    {
        Plane plane = new Plane(Vector3.up, march.transform.position.y);
        var ray = cam.ScreenPointToRay(pos);
        float enter;
        plane.Raycast(ray, out enter);
        return ray.GetPoint(enter);
    }

    [Serializable]
    private class DistanceData
    {
        public int colNum;
        public int rowNum;
        public float[] distances;

        public void FromData(Data data)
        {
            colNum = data.ColNum;
            rowNum = data.RowNum;
            distances = new float[colNum * rowNum];
            NativeArray<float>.Copy(data.RawData, distances);
        }

        public void ToData(Data data)
        {
            if (distances != null && distances.Length == data.RawData.Length)
            {
                NativeArray<float>.Copy(distances, data.RawData);
            }
        }

        public static void WriteToFile(string path, DistanceData data)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
                {
                    writer.Write(data.colNum);
                    writer.Write(data.rowNum);
                    foreach(var v in data.distances)
                    {
                        writer.Write(v);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static DistanceData ReadFromFile(string path)
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                {
                    DistanceData data = new DistanceData();
                    data.colNum = reader.ReadInt32();
                    data.rowNum = reader.ReadInt32();
                    int length = data.colNum * data.rowNum;
                    data.distances = new float[length];
                    for (int i = 0; i < length; ++i)
                    {
                        data.distances[i] = reader.ReadSingle();
                    }
                    return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return null;
        }
    }
}
