using System.Collections;
using UnityEngine;

using MeshBuilder;

using Data = MeshBuilder.MarchingSquaresMesherData;

public class MarchingSquareTest2 : MonoBehaviour
{
    private string AdditiveLabel = "Additive";
    private string SubtractiveLabel = "Subtractive";

    [SerializeField] private Camera cam = null; 

    [SerializeField] private MarchingSquaresComponent march1 = null;
    [SerializeField] private float radius1 = 0.5f;
    [SerializeField] private MarchingSquaresComponent march2 = null;
    [SerializeField] private float radius2 = 0.35f;

    [SerializeField] private bool additive = true;
    [SerializeField] private UnityEngine.UI.Text buttonLabel = null;

    private float cellSize1 => march1.CellSize;
    private float cellSize2 => march2.CellSize;

    private Data data1 => march1.Data;
    private Data data2 => march2.Data;
    
    void Start()
    {
        DrawAt(new Vector3(5, 0, 5));
        march1.Regenerate();
        march2.Regenerate();

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

            march1.Regenerate();
            march2.Regenerate();
        }
    }

    private void DrawAt(Vector3 pos)
    {
        data1.ApplyCircle(pos.x, pos.z, radius1, cellSize1);
        data1.RemoveBorder();
        data2.ApplyCircle(pos.x, pos.z, radius2, cellSize2);
        data2.RemoveBorder();
    }

    private void EraseAt(Vector3 pos)
    {
        data1.RemoveCircle(pos.x, pos.z, radius2, cellSize1);
        data2.RemoveCircle(pos.x, pos.z, radius1, cellSize2);
    }

    public void ChangeBrushMode()
    {
        additive = !additive;
        buttonLabel.text = additive ? AdditiveLabel : SubtractiveLabel;
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
