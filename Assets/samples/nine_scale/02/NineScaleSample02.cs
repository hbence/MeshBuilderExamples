using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MeshBuilder;

public class NineScaleSample02 : MonoBehaviour
{
    [SerializeField] private NineScale building = null;

    [SerializeField] private float yOffset = 0.6f;

    [SerializeField] private float minX = 0.5f;
    [SerializeField] private float maxX = 5.0f;

    [SerializeField] private float minY = 0.5f;
    [SerializeField] private float maxY = 5.0f;

    [SerializeField] private float minZ = 0.5f;
    [SerializeField] private float maxZ = 5.0f;

    [SerializeField] private Transform marker = null;

    [SerializeField] private MeshFilter buildingMeshFilter = null;

    private NineScaleMeshBuilder buildingBuilder;
    private Mesh buildingMesh = null;

    private enum Axis { X, Y, Z }

    private void Start()
    {
        buildingBuilder = new NineScaleMeshBuilder();
        buildingMesh = new Mesh();

        ScaleX(0);
        ScaleY(0);
        ScaleZ(0);
        OffsetY(marker);
    }

    public void Build()
    {
        buildingBuilder.Init(building, marker.localScale);
        buildingBuilder.Start();
    }

    public void LateUpdate()
    {
        if (buildingBuilder.IsGenerating)
        {
            buildingBuilder.Complete(buildingMesh);
            buildingMeshFilter.sharedMesh = buildingMesh;

            SetY(buildingMeshFilter.transform, marker);
        }
    }

    public void OnDestroy()
    {
        buildingBuilder.Dispose();
        buildingBuilder = null;
    }

    public void ScaleX(float value)
    {
        Scale(marker, value, Axis.X);
    }

    public void ScaleY(float value)
    {
        Scale(marker, value, Axis.Y);
        OffsetY(marker);
    }

    public void ScaleZ(float value)
    {
        Scale(marker, value, Axis.Z);
    }

    private void Scale(Transform transform, float value, Axis axis)
    {
        Vector3 scale = transform.localScale;

        switch(axis)
        {
            case Axis.X: scale.x = Mathf.Lerp(minX, maxX, value); break;
            case Axis.Y: scale.y = Mathf.Lerp(minY, maxY, value); break;
            case Axis.Z: scale.z = Mathf.Lerp(minZ, maxZ, value); break;
        }

        transform.localScale = scale;
    }

    private void OffsetY(Transform transform)
    {
        Vector3 pos = transform.localPosition;
        pos.y = transform.localScale.y * 0.5f + yOffset;
        transform.localPosition = pos;
    }

    private void SetY(Transform target, Transform source)
    {
        Vector3 pos = target.transform.localPosition;
        pos.y = source.transform.localPosition.y;
        target.transform.localPosition = pos;
    }
}
