using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "scene_info", menuName = "Sample/SceneInfo", order = 1)]
public class SceneInfo : ScriptableObject
{
    [SerializeField] private int sceneIndex = 1;
    public int SceneIndex => sceneIndex;

    [SerializeField] private string sceneTitle = "";
    public string SceneTitle => sceneTitle;

    [TextArea]
    [SerializeField] private string info = "";
    public string Info => info;
}
