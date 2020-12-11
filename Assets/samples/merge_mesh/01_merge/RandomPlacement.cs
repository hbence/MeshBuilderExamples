using UnityEngine;

public class RandomPlacement : MonoBehaviour
{
    [SerializeField] private float width = 10;
    [SerializeField] private float height = 10;
    [SerializeField] private float turnOffPossibility = 0;
    [SerializeField] private Transform[] elems = null;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UpdatePlacement();
        }
    }

    public void UpdatePlacement()
    {
        foreach(var elem in elems)
        {
            Vector3 pos = elem.localPosition;
            pos.x = Random.Range(-1f, 1f) * width / 2f;
            pos.z = Random.Range(-1f, 1f) * height / 2f;
            elem.localPosition = pos;
            elem.gameObject.SetActive(Random.value > turnOffPossibility);
        }
    }
}
