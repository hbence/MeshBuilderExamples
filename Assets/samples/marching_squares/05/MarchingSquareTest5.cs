using UnityEngine;

using MeshBuilder;

using Unity.Mathematics;
using System;
using Unity.Profiling;

public class MarchingSquareTest5 : MonoBehaviour
{
    private const string AdditiveLabel = "Add";
    private const string SubtractiveLabel = "Subtract";
    private const string IncreaseLabel = "Increase Height";
    private const string DecreaseLabel = "Decrease Height";
    private const string FlatLabel = "Flat";
    private const string SmoothLabel = "Smooth";

    private string[] ModeLabels = new string[] { AdditiveLabel, SubtractiveLabel, IncreaseLabel, DecreaseLabel };
    private string[] ShapeLabels = new string[] { FlatLabel, SmoothLabel };

    private const float MaxHeight = 1f;
    private const float MinCullingX = 2f;
    private const float MaxCullingX = 18f;
    private const float MinCullingZ = 2f;
    private const float MaxCullingZ = 18f;

    private const int ColNum = 41;
    private const int RowNum = 41;

    enum Mode
    {
        Add, Subtract, IncreaseHeight, DecreaseHeight
    }

    private const float CellSize = 0.25f;
    private const float HeightChangeValue = 0.01f;

    [SerializeField] private Camera cam = null;

    [SerializeField] private float brushRadius = 1f;
    [SerializeField] private MarchingSquaresComponent[] meshers = null;
    [SerializeField] private Transform cullingBox = null;

    private Vector3 cullingMovement;

    private int modeIndex = 0;
    private int brushIndex = 0;
    private float maxHeight = MaxHeight * 0.5f;

    [SerializeField] private UnityEngine.UI.Text buttonLabel = null;
    [SerializeField] private GameObject brushRoot = null;
    [SerializeField] private UnityEngine.UI.Text shapeLabel = null;

    private MarchingSquaresMesher.Data[] data = null;

    private bool[] defCulling;
    
    void Start()
    {
        cullingMovement = new Vector3(1, 0, 1) * 3.2f;

        defCulling = new bool[ColNum * RowNum];
        
        for (int i = 0; i < ColNum; ++i)
        {
            defCulling[i] = true;
        }
        for (int i = 0; i < RowNum; ++i)
        {
            defCulling[i * ColNum] = true;
        }

        data = new MarchingSquaresMesher.Data[meshers.Length];
        for(int i = 0; i < data.Length; ++i)
        {
            data[i] = meshers[i].Data;
        }

        /*
        for (int i = 0; i < meshFilters.Length; ++i)
        {
            meshers[i] = new MarchingSquaresMesher();
            meshers[i].InitForFullCellTapered(ColNum, RowNum, CellSize, 0.2f, 0.5f);
            //meshers[i].InitForFullCell(ColNum, RowNum, CellSize, 0.2f);
            //meshers[i].InitForOptimized(41, 41, CellSize, 0.2f, 0.5f);
            meshers[i].DistanceData.InitHeights();
            meshers[i].DistanceData.InitCullingData(defCulling);

            meshFilters[i].sharedMesh = new Mesh();

            UpdateCullingData(i, cullingBox);
        }
        */

        float rad = brushRadius;
        brushRadius = 5;
        DrawAt(new Vector3(10, 0, 10));
        brushRadius = rad;

        buttonLabel.text = ModeLabels[modeIndex];
        brushRoot.SetActive(false);
    }

    void Update()
    {
        if (!EditorUtils.IsPointerOverGameObject && Input.GetMouseButton(0))
        {
            Vector3 pos = GetHitPosition(Input.mousePosition);
            if (modeIndex == (int)Mode.Subtract)
            {
                EraseAt(pos);
            }
            else
            {
                DrawAt(pos);
            }
        }

        MoveCullingBox();
        UpdateCulling();

        foreach(var mesher in meshers)
        {
            mesher.Regenerate();
        }
    }

    private void DrawAt(Vector3 pos)
    {
        switch ((Mode)modeIndex)
        {
            case Mode.Add:
                {
                    ApplyOperation(pos,
                        (MarchingSquaresMesher.Data data, float x, float y) =>
                        {
                            data.ApplyCircle(x, y, brushRadius, CellSize);
                        }
                    );
                    break;
                }
            case Mode.IncreaseHeight:
                {
                    ApplyOperation(pos,
                        (MarchingSquaresMesher.Data data, float x, float y) =>
                        {
                            if (brushIndex == 0)
                            {
                                data.ChangeHeightCircleFlat(x, y, brushRadius, HeightChangeValue, CellSize, 0, maxHeight);
                            }
                            else
                            {
                                data.ChangeHeightCircleSmooth(x, y, brushRadius, HeightChangeValue, CellSize, 0, maxHeight);
                            }
                        }
                    );
                    break;
                }
            case Mode.DecreaseHeight:
                {
                    ApplyOperation(pos,
                        (MarchingSquaresMesher.Data data, float x, float y) =>
                        {
                            if (brushIndex == 0)
                            {
                                data.ChangeHeightCircleFlat(x, y, brushRadius, -HeightChangeValue, CellSize, 0, maxHeight);
                            }
                            else
                            {
                                data.ChangeHeightCircleSmooth(x, y, brushRadius, -HeightChangeValue, CellSize, 0, maxHeight);
                            }
                        }
                    );
                    break;
                }
        }
    }

    private void EraseAt(Vector3 pos)
    {
        ApplyOperation(pos, 
            (MarchingSquaresMesher.Data data, float x, float y) =>
            {
                data.RemoveCircle(x, y, brushRadius, CellSize);
            }
        );
    }

    private void ApplyOperation(Vector3 pos, Action<MarchingSquaresMesher.Data, float, float> op)
    {
        for (int i = 0; i < meshers.Length; ++i)
        {
            var mesher = meshers[i];
            Vector3 localPos = pos - mesher.transform.position;
            op(data[i], localPos.x, localPos.z);
            UpdateCullingData(i, cullingBox);
        }
    }

    private void UpdateCullingData(int index, Transform cullingBox)
    {
        Transform owner = meshers[index].transform;

        var boxPos = cullingBox.transform.position - owner.transform.position;
        float left = boxPos.x - cullingBox.localScale.x * 0.5f;
        float right = boxPos.x + cullingBox.localScale.x * 0.5f;
        float bottom = boxPos.z - cullingBox.localScale.z * 0.5f;
        float top = boxPos.z + cullingBox.localScale.z * 0.5f;

        var cullingData = data[index].CullingDataRawData;
        for (int i = 0; i < cullingData.Length; ++i)
        {
            Vector3 pos = GetCellPosition(owner, i);
            bool inCullingBox = pos.x > left && pos.x < right && pos.z > bottom && pos.z < top;  
            cullingData[i] = defCulling[i] || inCullingBox;
        }
    }

    public void ChangeBrushMode()
    {
        modeIndex = (modeIndex + 1) % ModeLabels.Length;
        buttonLabel.text = ModeLabels[modeIndex];

        brushRoot.SetActive(modeIndex == (int)Mode.DecreaseHeight || modeIndex == (int)Mode.IncreaseHeight);
    }

    public void ChangeBrushShape()
    {
        brushIndex = (brushIndex + 1) % ShapeLabels.Length;
        shapeLabel.text = ShapeLabels[brushIndex];
    }

    public void SetMaxHeightIncrease(float value)
    {
        maxHeight = value * MaxHeight; 
    }

    private void MoveCullingBox()
    {
        Vector3 pos = cullingBox.position;

        pos += cullingMovement * Time.deltaTime;

        if (pos.x < MinCullingX) { pos.x = MinCullingX; cullingMovement.x *= -1; }
        if (pos.x > MaxCullingX) { pos.x = MaxCullingX; cullingMovement.x *= -1; }
        if (pos.z < MinCullingZ) { pos.z = MinCullingZ; cullingMovement.z *= -1; }
        if (pos.z > MaxCullingZ) { pos.z = MaxCullingZ; cullingMovement.z *= -1; }

        cullingBox.position = pos;
    }

    private void UpdateCulling()
    {
        for (int i = 0; i < meshers.Length; ++i)
        {
            UpdateCullingData(i, cullingBox);
        }
    }

    private Vector3 GetHitPosition(Vector3 pos)
    {
        Plane plane = new Plane(Vector3.up, 0);
        var ray = cam.ScreenPointToRay(pos);
        float enter;
        plane.Raycast(ray, out enter);
        return ray.GetPoint(enter);
    }

    private Vector3 GetCellPosition(Transform t, int i)
    {
        return transform.position + new Vector3(i % ColNum, 0, i / ColNum) * CellSize;
    }
}
