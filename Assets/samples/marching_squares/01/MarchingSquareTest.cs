using UnityEngine;

using MeshBuilder;

using Data = MeshBuilder.MarchingSquaresMesherData;

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

    [System.Serializable]
    private class InitInfo
    {
        public string name;
        public MarchingSquaresComponent.InitializationInfo info;
    }

    [SerializeField] private bool showValues = false;
    [SerializeField] private BrushInfo[] brushes = null;
    [SerializeField] private float areaWidth = 10f;
    [SerializeField] private float areaHeight = 10f;
    [SerializeField] private float border = 0.2f;
    [SerializeField] private UnityEngine.UI.Text btnLabel = null;

    [SerializeField] private InitInfo[] initInfos = null;
    [SerializeField] private MarchingSquaresComponent marchingSquares = null;

    private int currentInfoIndex = 0;

    private Data Data => marchingSquares.Data;
    private float CellSize => marchingSquares.CellSize;

    void Start()
    {
        SetInit(currentInfoIndex);
    }

    public void SwitchMesherType()
    {
        currentInfoIndex = (currentInfoIndex + 1) % initInfos.Length;
        SetInit(currentInfoIndex);
    }

    private void SetInit(int index)
    {
        marchingSquares.Init(initInfos[index].info);
        btnLabel.text = initInfos[index].name;
    }

    void Update()
    {
        if (!marchingSquares.IsGenerating)
        {
            Data.Clear();
            foreach (var brush in brushes)
            {
                MoveBrush(brush);
                ApplyBrush(brush);
            }
            Data.RemoveBorder();
            marchingSquares.Regenerate();
        }
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
