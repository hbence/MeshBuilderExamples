using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MeshBuilder;

public class NineScaleSample01 : MonoBehaviour
{
    private const string InstancedRendering = "Rendering: Instanced";
    private const string NonInstancedRendering = "Rendering: Not Instanced";

    private bool instancedRendering = false;

    [SerializeField] private NineScaleDrawer building = null;
    [SerializeField] private NineScaleDrawer platform = null;

    [SerializeField] private UnityEngine.UI.Text switchBtnText;

    [SerializeField] private float yOffset = 0.6f;

    [SerializeField] private float minX = 0.5f;
    [SerializeField] private float maxX = 5.0f;

    [SerializeField] private float minY = 0.5f;
    [SerializeField] private float maxY = 5.0f;

    [SerializeField] private float minZ = 0.5f;
    [SerializeField] private float maxZ = 5.0f;

    private enum Axis { X, Y, Z }

    private void Start()
    {
        building.transform.localScale = new Vector3(minX, minY, minZ);
        platform.transform.localScale = new Vector3(minX, minY, minZ);

        OffsetY(building.transform);
        OffsetY(platform.transform);

        switchBtnText.text = NonInstancedRendering;
    }

    public void SwitchInstancedRendering()
    {
        instancedRendering = !instancedRendering;
        building.InstancedRendering = instancedRendering;
        platform.InstancedRendering = instancedRendering;
        switchBtnText.text = instancedRendering ? InstancedRendering : NonInstancedRendering;
    }

    public void ScaleX(float value)
    {
        Scale(building.transform, value, Axis.X);
        Scale(platform.transform, value, Axis.X);
    }

    public void ScaleY(float value)
    {
        Scale(building.transform, value, Axis.Y);
        Scale(platform.transform, value, Axis.Y);

        OffsetY(building.transform);
        OffsetY(platform.transform);
    }

    public void ScaleZ(float value)
    {
        Scale(building.transform, value, Axis.Z);
        Scale(platform.transform, value, Axis.Z);
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
}
