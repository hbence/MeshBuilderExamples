using UnityEngine;

using MeshBuilder;

using MesherType = MeshBuilder.MarchingSquaresComponent.InitializationInfo.Type;

public class MarchingSquareTest : MonoBehaviour
{
    [System.Serializable]
    private class BrushInfo
    {
        public bool circle = true;
        public Transform transform;
        public float speedX;
        public float speedY;
    }

    [SerializeField] private bool showValues = false;
    [SerializeField] private BrushInfo[] brushes = null;
    [SerializeField] private float areaWidth = 10f;
    [SerializeField] private float areaHeight = 10f;
    [SerializeField] private float border = 0.2f;
    [SerializeField] private UnityEngine.UI.Text btnLabel = null;

    [SerializeField] private MarchingSquaresComponent marchingSquares = null;

    private MarchingSquaresMesher.Data Data => marchingSquares.Data;
    private float CellSize => marchingSquares.CellSize;

    void Start()
    {
        //marchingSquares.Mesher.InitForFullCellTapered(data, CellSize, 0.2f, 0.4f);
        //marchingSquares.Mesher.InitForFullCell(data, CellSize, 0.2f, false);
        //marchingSquares.Mesher.InitForFullCellSimpleMesh(data, CellSize, 0.2f);
        //marchingSquares.Mesher.InitForTest(data, CellSize, 0.2f, 1);
        //marchingSquares.Mesher.Init(data, CellSize);
        //marchingSquares.Mesher.InitForOptimized(data, CellSize, 0.5f, 1, MarchingSquaresMesher.OptimizationMode.NextLargestRect);

        UpdateButtonLabel();
    }

    public void SwitchMesherType()
    {
        int count = System.Enum.GetNames(typeof(MesherType)).Length;
        int type = ((int)marchingSquares.InitInfo.type + 1) % count;
        marchingSquares.InitInfo.type = (MesherType) type;
        marchingSquares.Init();

        UpdateButtonLabel();
    }

    private void UpdateButtonLabel()
    {
        btnLabel.text = marchingSquares.InitInfo.type.ToString();
    }

    void Update()
    {
        Data.Clear();
        foreach(var brush in brushes)
        {
            MoveBrush(brush);
            ApplyBrush(brush);
        }
        Data.RemoveBorder();
        marchingSquares.Regenerate();
    }

    private void ApplyBrush(BrushInfo brush)
    {
        Vector3 p = brush.transform.position;
        float rad = brush.transform.localScale.x * 0.5f;
        if (brush.circle)
        {
            Data.ApplyCircle(p.x, p.z, rad, CellSize);
        }
        else
        {
            Data.ApplyRectangle(p.x, p.z, rad, rad, CellSize);
        }
    }

    private void MoveBrush(BrushInfo brush)
    {
        float speedX = brush.speedX;
        float speedY = brush.speedY;

        Vector3 p = brush.transform.position;
        float rad = brush.transform.localScale.x * 0.5f;
        
        if (p.x - rad < border) { speedX = Mathf.Abs(speedX); }
        if (p.z - rad < border) { speedY = Mathf.Abs(speedY); }
        if (p.x + rad > areaWidth - border) { speedX = -Mathf.Abs(speedX); }
        if (p.z + rad > areaHeight - border) { speedY = -Mathf.Abs(speedY); }

        brush.speedX = speedX;
        brush.speedY = speedY;

        p.x += speedX * Time.deltaTime;
        p.z += speedY * Time.deltaTime;
        
        brush.transform.position = p;
    }

    private void OnDrawGizmos()
    {
        if (showValues)
        {
            if (Data != null)
            {
                Vector3 origin = transform.position;
                for (int y = 0; y < Data.RowNum; ++y)
                {
                    for (int x = 0; x < Data.ColNum; ++x)
                    {
                        float dist = Data.DistanceAt(x, y);
                        Vector3 offset = new Vector3(x * CellSize, 0.3f, y * CellSize);
                        DrawString(dist.ToString("0.0"), origin + offset, 0, 0);
                    }
                }
            }
        }
    }

    static public void DrawString(string text, Vector3 worldPos, float oX = 0, float oY = 0, Color? colour = null)
    {
#if UNITY_EDITOR
        UnityEditor.Handles.BeginGUI();

        var restoreColor = GUI.color;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }

        UnityEditor.Handles.Label(TransformByPixel(worldPos, oX, oY), text);

        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();
#endif
    }
#if UNITY_EDITOR
    static Vector3 TransformByPixel(Vector3 position, float x, float y)
    {
        return TransformByPixel(position, new Vector3(x, y));
    }

    static Vector3 TransformByPixel(Vector3 position, Vector3 translateBy)
    {
        Camera cam = UnityEditor.SceneView.currentDrawingSceneView.camera;
        if (cam)
            return cam.ScreenToWorldPoint(cam.WorldToScreenPoint(position) + translateBy);
        else
            return position;
    }
#endif

}
