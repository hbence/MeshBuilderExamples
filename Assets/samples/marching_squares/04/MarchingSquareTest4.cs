using UnityEngine;

using MeshBuilder;

using Unity.Mathematics;

public class MarchingSquareTest4 : MonoBehaviour
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

    enum Mode
    {
        Add, Subtract, IncreaseHeight, DecreaseHeight
    }

    private const float CellSize = 0.2f;
    private const float HeightChangeValue = 0.01f;

    [SerializeField] private Camera cam = null; 

    [SerializeField] private MeshFilter meshFilter1 = null;
    [SerializeField] private float radius1 = 0.5f;
    [SerializeField] private MeshFilter meshFilter2 = null;
    [SerializeField] private float radius2 = 0.35f;

    private int modeIndex = 0;
    private int brushIndex = 0;
    private float maxHeight = MaxHeight * 0.5f;

    [SerializeField] private UnityEngine.UI.Text buttonLabel = null;
    [SerializeField] private GameObject brushRoot = null;
    [SerializeField] private UnityEngine.UI.Text shapeLabel = null;

    private MarchingSquaresMesher march1;
    private MarchingSquaresMesher march2;
    private Mesh mesh1;
    private Mesh mesh2;

    void Start()
    {
        march1 = new MarchingSquaresMesher();
        march1.InitForFullCellTapered(25, 25, CellSize*2, 0.2f, 0.5f);
        mesh1 = new Mesh();
        meshFilter1.sharedMesh = mesh1;

        march2 = new MarchingSquaresMesher();
        march2.InitForFullCell(50, 50, CellSize, 0.3f);
        march2.DistanceData.InitHeights();
        mesh2 = new Mesh();
        meshFilter2.sharedMesh = mesh2;

        DrawAt(new Vector3(5, 0, 5));
        //DrawAt(new Vector3(6, 0, 6));

        buttonLabel.text = ModeLabels[modeIndex];
        brushRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
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
    }

    private void DrawAt(Vector3 pos)
    {
        switch ((Mode)modeIndex)
        {
            case Mode.Add:
                {
                    march1.DistanceData.ApplyCircle(pos.x, pos.z, radius1, CellSize * 2);
                    march1.DistanceData.RemoveBorder();
                    march1.Start();

                    march2.DistanceData.ApplyCircle(pos.x, pos.z, radius2, CellSize);
                    march2.DistanceData.RemoveBorder();
                    march2.Start();
                    break;
                }
            case Mode.IncreaseHeight:
                {
                    if (brushIndex == 0)
                    {
                        march2.DistanceData.ChangeHeightCircleFlat(pos.x, pos.z, radius2, HeightChangeValue, CellSize, 0, maxHeight);
                    }
                    else
                    {
                        march2.DistanceData.ChangeHeightCircleSmooth(pos.x, pos.z, radius2, HeightChangeValue, CellSize, 0, maxHeight);
                    }
                    march2.Start();
                    break;
                }
            case Mode.DecreaseHeight:
                {
                    if (brushIndex == 0)
                    {
                        march2.DistanceData.ChangeHeightCircleFlat(pos.x, pos.z, radius2, -HeightChangeValue, CellSize, 0, maxHeight);
                    }
                    else
                    {
                        march2.DistanceData.ChangeHeightCircleSmooth(pos.x, pos.z, radius2, -HeightChangeValue, CellSize, 0, maxHeight);
                    }
                    march2.Start();
                    break;
                }
        }
    }

    private void EraseAt(Vector3 pos)
    {
        march1.DistanceData.RemoveCircle(pos.x, pos.z, radius2, CellSize * 2);
        march1.DistanceData.RemoveBorder();
        march1.Start();

        march2.DistanceData.RemoveCircle(pos.x, pos.z, radius1, CellSize);
        march2.DistanceData.RemoveBorder();
        march2.Start();
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

    private void LateUpdate()
    {
        if (march1.IsGenerating)
        {
            march1.Complete(mesh1);
        }
        if (march2.IsGenerating)
        {
            march2.Complete(mesh2);
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

    private void OnDestroy()
    {
        march1?.Dispose();
        march2?.Dispose();
    }
}
