using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeSceneControl : MonoBehaviour
{
    [SerializeField] private RandomPlacement randomElems = null;
    [SerializeField] private Transform originalRoot = null;
    [SerializeField] private UnityEngine.UI.Text text = null;

    private GameObject merged = null;

    private void Start()
    {
        RandomizeElements();
        UpdateMergedGameObject();
    }

    public void RandomizeElements()
    {
        randomElems.UpdatePlacement();
    }

    public void UpdateMergedGameObject()
    {
        if (merged != null)
        {
            DestroyImmediate(merged);
            merged = null;
        }

        List<GameObject> gos = new List<GameObject>();
        for (int i = 0; i < originalRoot.childCount; ++i)
        {
            var child = originalRoot.GetChild(i); 
            if (child.gameObject.activeSelf)
            {
                gos.Add(child.gameObject);
            }
        }

        var goArray = gos.ToArray();
        merged = MeshBuilder.MeshCombinationUtils.CreateMergedMesh(goArray, false);
        merged.transform.SetParent(transform);
        merged.transform.localPosition = Vector3.zero;

        string res = "";
        for (int i = 0; i < merged.transform.childCount; ++i)
        {
            res += GetInfo(merged.transform.GetChild(i));
            res += "\n------------\n";
        }
        text.text = res;
    }

    static private string GetInfo(Transform mergedChild)
    {
        string res = "obj: " + mergedChild.name;

        MeshFilter mesh = mergedChild.GetComponent<MeshFilter>();
        if (mesh != null)
        {
            res += "\nvert: " + mesh.mesh.vertexCount;
        }

        return res;
    }
}
