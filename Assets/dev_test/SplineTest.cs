using UnityEngine;

using Unity.Mathematics;

using MeshBuilder;

public class SplineTest : MonoBehaviour
{
    [SerializeField] private Transform[] points = null;

    [SerializeField] private Spline.CatmullRom catmullRom;

    [SerializeField] private Spline.Bezier bezier = null;

    [SerializeField] private bool isClosed = true;

    [SerializeField] MeshFilter meshFilter = null;

    private Spline spline;
    private SplineMeshBuilder.CrossSectionData crossSectionData;
    private Vector3[] controlPoints;

    private SplineMeshBuilder mesh;

    private float3[] slicePoints = new float3[] { new float3(-0.25f, 0, 0), new float3(-0.15f, 0.1f, 0), new float3(0.15f, 0.1f, 0), new float3(0.25f, 0, 0) };

    void Start()
    {
        controlPoints = new Vector3[points.Length];
        for (int i = 0; i < controlPoints.Length; ++i)
        {
            controlPoints[i] = points[i].position;
        }

        spline = new Spline(bezier, controlPoints, true);
        spline.Recalculate();

        crossSectionData = new SplineMeshBuilder.CrossSectionData(slicePoints);

        mesh = new SplineMeshBuilder(spline, crossSectionData, points);
    }

    void Update()
    {
        if (controlPoints != null && spline != null)
        {
            spline.IsClosed = isClosed;
            for (int i = 0; i < controlPoints.Length; ++i)
            {
                controlPoints[i] = points[i].position;
            }
            spline.Recalculate();
            mesh?.Rebuild();

            meshFilter.sharedMesh = mesh.Mesh;
        }
    }

    private void OnDrawGizmos()
    {
     //   if (spline != null && mesh != null && mesh.Vertices != null)
        {
            /*
            Gizmos.color = Color.red;
            int pc = spline.PointCount;
            for (int i = 0; i < pc; ++i)
            {
                Vector3 p = spline.GetPoint(i);
                Gizmos.DrawSphere(p, 0.1f + i*0.001f);
            }
            //*/
            /*
            Gizmos.color = Color.cyan;
            for(float dist = 0; dist <= spline.Length; dist += 0.2f)
            {
                Vector3 p = spline.CalcAtDistance(dist);
                Gizmos.DrawSphere(p, 0.05f);
            }
            //*/
            /*
            Gizmos.color = Color.cyan;
            for (int i = 0; i < mesh.Vertices.Length; ++i)
            {
                Vector3 p = mesh.Vertices[i];
                Gizmos.DrawSphere(p, 0.05f);
            }
            */
        }
    }
}
