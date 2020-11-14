using UnityEngine;

[CreateAssetMenu(fileName = "sample_scene", menuName = "Sample/SampleScene", order = 1)]
public class SampleScene : ScriptableObject
{
    [SerializeField] private string sceneTitle = "";
    public string SceneTitle => sceneTitle;

    [SerializeField] private string info = "";
    public string Info => info;

    [SerializeField] private string scenePath;
}
