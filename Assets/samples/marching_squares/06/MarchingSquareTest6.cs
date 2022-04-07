using UnityEngine;

using MeshBuilder;

using Data = MeshBuilder.MarchingSquaresMesherData;

public class MarchingSquareTest6 : MonoBehaviour
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

    private const float HeightChangeValue = 0.01f;

    [SerializeField] private Camera cam = null; 

    [SerializeField] private MarchingSquaresComponent marchingMesher = null;
    [SerializeField] private float radius = 0.5f;

    private float CellSize => marchingMesher.CellSize;

    private int modeIndex = 0;
    private int brushIndex = 0;
    private float maxHeight = MaxHeight * 0.5f;

    [SerializeField] private UnityEngine.UI.Text buttonLabel = null;
    [SerializeField] private GameObject brushRoot = null;
    [SerializeField] private UnityEngine.UI.Text shapeLabel = null;

    private Data data;

    void Start()
    {
        data = new Data(50, 50);
        data.InitHeights();

        marchingMesher.InitWithData(data);
        
        DrawAt(new Vector3(5, 0, 5), data);

        marchingMesher.Regenerate();

        buttonLabel.text = ModeLabels[modeIndex];
        brushRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 pos = GetHitPosition(Input.mousePosition);
            DrawAt(pos, data);

            marchingMesher.Regenerate();
        }
    }

    private void DrawAt(Vector3 pos, Data data)
    {
        switch ((Mode)modeIndex)
        {
            case Mode.Subtract:
                {
                    data.RemoveCircle(pos.x, pos.z, radius, CellSize);
                    break;
                }
            case Mode.Add:
                {
                    data.ApplyCircle(pos.x, pos.z, radius, CellSize);
                    data.RemoveBorder();
                    break;
                }
            case Mode.IncreaseHeight:
                {
                    if (brushIndex == 0)
                    {
                        data.ChangeHeightCircleFlat(pos.x, pos.z, radius, HeightChangeValue, CellSize, 0, maxHeight);
                    }
                    else
                    {
                        data.ChangeHeightCircleSmooth(pos.x, pos.z, radius, HeightChangeValue, CellSize, 0, maxHeight);
                    }
                    break;
                }
            case Mode.DecreaseHeight:
                {
                    if (brushIndex == 0)
                    {
                        data.ChangeHeightCircleFlat(pos.x, pos.z, radius, -HeightChangeValue, CellSize, 0, maxHeight);
                    }
                    else
                    {
                        data.ChangeHeightCircleSmooth(pos.x, pos.z, radius, -HeightChangeValue, CellSize, 0, maxHeight);
                    }
                    break;
                }
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
        data?.Dispose();
    }
}
