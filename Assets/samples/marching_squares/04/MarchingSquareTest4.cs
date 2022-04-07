using UnityEngine;

using MeshBuilder;

using Data = MeshBuilder.MarchingSquaresMesherData;

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

    private const float HeightChangeValue = 0.01f;

    [SerializeField] private Camera cam = null; 

    [SerializeField] private MarchingSquaresComponent march1 = null;
    [SerializeField] private float radius1 = 0.5f;
    [SerializeField] private MarchingSquaresComponent march2 = null;
    [SerializeField] private float radius2 = 0.35f;

    private int modeIndex = 0;
    private int brushIndex = 0;
    private float maxHeight = MaxHeight * 0.5f;

    [SerializeField] private UnityEngine.UI.Text buttonLabel = null;
    [SerializeField] private GameObject brushRoot = null;
    [SerializeField] private UnityEngine.UI.Text shapeLabel = null;

    private float CellSize1 => march1.CellSize;
    private float CellSize2 => march2.CellSize;

    private Data data1 => march1.Data;
    private Data data2 => march2.Data;

    void Start()
    {
        DrawAt(new Vector3(5, 0, 5));

        march1.Regenerate();
        march2.Regenerate();

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

            march1.Regenerate();
            march2.Regenerate();
        }
    }

    private void DrawAt(Vector3 pos)
    {
        switch ((Mode)modeIndex)
        {
            case Mode.Add:
                {
                    data1.ApplyCircle(pos.x, pos.z, radius1, CellSize1);
                    data1.RemoveBorder();

                    data2.ApplyCircle(pos.x, pos.z, radius2, CellSize2);
                    data2.RemoveBorder();
                    break;
                }
            case Mode.IncreaseHeight:
                {
                    if (brushIndex == 0)
                    {
                        data2.ChangeHeightCircleFlat(pos.x, pos.z, radius2, HeightChangeValue, CellSize2, 0, maxHeight);
                    }
                    else
                    {
                        data2.ChangeHeightCircleSmooth(pos.x, pos.z, radius2, HeightChangeValue, CellSize2, 0, maxHeight);
                    }
                    break;
                }
            case Mode.DecreaseHeight:
                {
                    if (brushIndex == 0)
                    {
                        data2.ChangeHeightCircleFlat(pos.x, pos.z, radius2, -HeightChangeValue, CellSize2, 0, maxHeight);
                    }
                    else
                    {
                        data2.ChangeHeightCircleSmooth(pos.x, pos.z, radius2, -HeightChangeValue, CellSize2, 0, maxHeight);
                    }
                    break;
                }
        }
    }

    private void EraseAt(Vector3 pos)
    {
        data1.RemoveCircle(pos.x, pos.z, radius2, CellSize1);
        data2.RemoveCircle(pos.x, pos.z, radius1, CellSize2);
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
}
