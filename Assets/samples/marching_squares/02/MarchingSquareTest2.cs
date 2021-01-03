using UnityEngine;

using MeshBuilder;

public class MarchingSquareTest2 : MonoBehaviour
{
    private string AdditiveLabel = "Additive";
    private string SubtractiveLabel = "Subtractive";

    private const float CellSize = 0.2f;

    [SerializeField] private Camera cam = null; 

    [SerializeField] private MeshFilter meshFilter1;
    [SerializeField] private float radius1;
    [SerializeField] private MeshFilter meshFilter2;
    [SerializeField] private float radius2;

    [SerializeField] private bool additive = true;
    [SerializeField] private UnityEngine.UI.Text buttonLabel = null;

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
        mesh2 = new Mesh();
        meshFilter2.sharedMesh = mesh2;

        DrawAt(new Vector3(5, 0, 5));

        buttonLabel.text = additive ? AdditiveLabel : SubtractiveLabel;
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
    }

    private void DrawAt(Vector3 pos)
    {
        march1.DistanceData.ApplyCircle(pos.x, pos.z, radius1, CellSize * 2);
        march1.DistanceData.RemoveBorder();
        march1.Start();

        march2.DistanceData.ApplyCircle(pos.x, pos.z, radius2, CellSize);
        march2.DistanceData.RemoveBorder();
        march2.Start();
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
        additive = !additive;
        buttonLabel.text = additive ? AdditiveLabel : SubtractiveLabel;
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
