using UnityEngine;
using UnityEngine.SceneManagement;

public class SampleSwitcher : MonoBehaviour
{
    [SerializeField]
    private SceneInfo[] scenes = null;
    [SerializeField]
    private UnityEngine.UI.Text infoText = null;
    [SerializeField]
    private Transform infoBoard = null;

    private int currentLoaded = -1;

    void Start()
    {
        Load(0);
    }

    private void Load(int index)
    {
        if (CurrentInfo != null)
        {
            SceneManager.UnloadSceneAsync(CurrentInfo.index);
        }

        currentLoaded = index;
        SceneManager.LoadSceneAsync(CurrentInfo.index, LoadSceneMode.Additive);

        infoText.text = CurrentInfo.info;
    }

    public void Next()
    {
        int next = (currentLoaded + 1) % scenes.Length;
        Load(next);
    }

    public void Prev()
    {
        int next = (currentLoaded + scenes.Length - 1) % scenes.Length;
        Load(next);
    }

    public void SwitchShowInfo()
    {
        infoBoard.gameObject.SetActive(!infoBoard.gameObject.activeSelf);
    }

    private SceneInfo CurrentInfo { get => currentLoaded >= 0 ? scenes[currentLoaded] : null; }

    [System.Serializable]
    public class SceneInfo
    {
        public int index;
        [TextArea]
        public string info;
    }
}
