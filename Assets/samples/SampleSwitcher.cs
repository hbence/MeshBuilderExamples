using UnityEngine;
using UnityEngine.SceneManagement;

public class SampleSwitcher : MonoBehaviour
{
    [SerializeField] private Camera cam = null;
    [SerializeField] private SceneInfo[] scenes = null;
    [SerializeField] private UnityEngine.UI.Text infoText = null;
    [SerializeField] private UnityEngine.UI.Text titleText = null;
    [SerializeField] private Transform infoBoard = null;

    private int currentLoaded = -1;

    void Start()
    {
        Load(0);

        SceneManager.sceneLoaded += (Scene, LoadSceneMode) => { cam.gameObject.SetActive(false); };
        SceneManager.sceneUnloaded += (Scene) => { cam.gameObject.SetActive(true); };
    }

    private void Load(int index)
    {
        if (CurrentInfo != null)
        {
            SceneManager.UnloadSceneAsync(CurrentInfo.SceneIndex);
        }

        currentLoaded = index;
        SceneManager.LoadSceneAsync(CurrentInfo.SceneIndex, LoadSceneMode.Additive);

        infoText.text = CurrentInfo.Info;
        titleText.text = CurrentInfo.SceneTitle;
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
}
