using UnityEngine;

using MeshBuilder;
using Unity.Collections;
using System.IO;
using System;

using static MeshBuilder.MarchingSquaresMesher;

public class MarchingSquareTest3 : MonoBehaviour
{
    private const string AdditiveLabel = "Additive";
    private const string SubtractiveLabel = "Subtractive";

    private const string SimpleLabel = "Simple";
    private const string OptimizedGreedyLabel = "Optimized (greedy rect)";
    private const string OptimizedLargestLabel = "Optimized (next largest)";

    private string[] ModeButtonLabels = { SimpleLabel, OptimizedGreedyLabel, OptimizedLargestLabel };

    private const float CellSize = 0.2f;

    [SerializeField] private Camera cam = null; 

    [Header("brush")]
    [SerializeField] private MeshFilter meshFilter = null;
    [SerializeField] private float radius = 0.5f;

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

    private DistanceData data;

    private MarchingSquaresMesher march;
    private Mesh mesh;

    void Start()
    {
        march = new MarchingSquaresMesher();

        InitMesher();
        
        mesh = new Mesh();
        meshFilter.sharedMesh = mesh;

        Load();

        brushButtonLabel.text = additive ? AdditiveLabel : SubtractiveLabel;
        modeButtonLabel.text = ModeButtonLabels[meshModeOptimized];
    }

    private void InitMesher()
    {
        switch (meshModeOptimized)
        {
            case 0: march.Init(50, 50, CellSize, 0.1f); break;
            case 1: march.InitForOptimized(50, 50, CellSize, 0.1f, 1, OptimizationMode.GreedyRect); break;
            case 2: march.InitForOptimized(50, 50, CellSize, 0.1f, 1, OptimizationMode.NextLargestRect); break;
        }
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
        march.DistanceData.ApplyCircle(pos.x, pos.z, radius, CellSize);
        march.DistanceData.RemoveBorder();
        march.Start();
    }

    private void EraseAt(Vector3 pos)
    {
        march.DistanceData.RemoveCircle(pos.x, pos.z, radius, CellSize);
        march.DistanceData.RemoveBorder();
        march.Start();
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

        data.FromData(march.DistanceData);
        InitMesher();
        data.ToData(march.DistanceData);
        march.Start();
    }

    public void Clear()
    {
        march.DistanceData.Clear();
        march.Start();
    }

    private void Save()
    {
        data.FromData(march.DistanceData);
        DistanceData.WriteToFile(levelPath, data);
    }

    private void Load()
    {
        data = DistanceData.ReadFromFile(levelPath);
        if (data == null)
        {
            data = new DistanceData();
        }
        else
        {
            data.ToData(march.DistanceData);
            march.Start();
        }
    }

    private void LateUpdate()
    {
        if (march.IsGenerating)
        {
            march.Complete(mesh);
            UpdateMeshInfo();
        }
    }

    private void UpdateMeshInfo()
    {
        meshInfoLabel.text = $"mesh | vertex:{mesh.vertexCount} tri:{mesh.triangles.Length / 3}";
    }

    private Vector3 GetHitPosition(Vector3 pos)
    {
        Plane plane = new Plane(Vector3.up, 0);
        var ray = cam.ScreenPointToRay(pos);
        float enter;
        plane.Raycast(ray, out enter);
        return ray.GetPoint(enter);
    }

    private void OnDestroy()
    {
        march?.Dispose();
    }

    [System.Serializable]
    private class DistanceData
    {
        public int colNum;
        public int rowNum;
        public float[] distances;

        public void FromData(MarchingSquaresMesher.Data data)
        {
            colNum = data.ColNum;
            rowNum = data.RowNum;
            distances = new float[colNum * rowNum];
            NativeArray<float>.Copy(data.RawData, distances);
        }

        public void ToData(MarchingSquaresMesher.Data data)
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
