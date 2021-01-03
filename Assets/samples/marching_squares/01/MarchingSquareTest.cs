using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MeshBuilder;

public class MarchingSquareTest : MonoBehaviour
{
    private const float CellSize = 0.2f;

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

    [SerializeField] private MeshFilter meshFilter;
    private MarchingSquaresMesher march;
    private Mesh mesh;

    void Start()
    {
        march = new MarchingSquaresMesher();
        //march.Init(50, 50, CellSize, 0.2f);
       march.InitForFullCellTapered(50, 50, CellSize, 0.2f, 0.4f);

        mesh = new Mesh();
        meshFilter.sharedMesh = mesh;
    }

    void Update()
    {
        march.DistanceData.Clear();
        foreach(var brush in brushes)
        {
            MoveBrush(brush);
            ApplyCircle(brush);
        }
        march.DistanceData.RemoveBorder();
        march.Start();
    }

    private void LateUpdate()
    {
        if (march.IsGenerating)
        {
            march.Complete(mesh);
        }
    }

    private void ApplyCircle(BrushInfo brush)
    {
        Vector3 p = brush.transform.position;
        float rad = brush.transform.localScale.x * 0.5f;
        if (brush.circle)
        {
            march.DistanceData.ApplyCircle(p.x, p.z, rad, CellSize);
        }
        else
        {
            march.DistanceData.ApplyRectangle(p.x, p.z, rad, rad, CellSize);
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
            if (march != null && march.DistanceData != null)
            {
                Vector3 origin = transform.position;
                for (int y = 0; y < march.DistanceData.RowNum; ++y)
                {
                    for (int x = 0; x < march.DistanceData.ColNum; ++x)
                    {
                        float dist = march.DistanceData.DistanceAt(x, y);
                        Vector3 offset = new Vector3(x * CellSize, 0.3f, y * CellSize);
                        DrawString(dist.ToString("0.0"), origin + offset, 0, 0);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        march?.Dispose();
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
