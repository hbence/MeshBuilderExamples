using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SampleSwitcher : MonoBehaviour
{
    private const int SceneListBtnGap = 5;

    [SerializeField] private Camera cam = null;
    [SerializeField] private SceneInfo[] scenes = null;
    [Header("ui")]
    [SerializeField] private Text infoText = null;
    [SerializeField] private Text titleText = null;
    [SerializeField] private Transform infoBoard = null;
    [SerializeField] private GameObject sceneSelect = null;
    [SerializeField] private GameObject btnPrototype = null;
    [SerializeField] private Transform sceneSelectRoot = null;

    private int currentLoaded = -1;

    void Start()
    {
        Load(0);

        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => 
        { 
            cam.gameObject.SetActive(false);
            
            foreach (var go in scene.GetRootGameObjects())
            {
                var localEventSystem = go.GetComponentInChildren<EventSystem>();
                if (localEventSystem != null)
                {
                    localEventSystem.gameObject.SetActive(false);
                    break;
                }
            }
        };
        SceneManager.sceneUnloaded += (Scene) => { cam.gameObject.SetActive(true); };

        InitSceneSelectPanel();
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
        sceneSelect.gameObject.SetActive(false);
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

    public void SwitchSelectScene()
    {
        sceneSelect.SetActive(!sceneSelect.activeSelf);
    }

    private void InitSceneSelectPanel()
    {
        for (int i = 0; i < scenes.Length; ++i)
        {
            GameObject go = Instantiate(btnPrototype);
            Button btn = go.GetComponent<Button>();
            int index = i;
            btn.onClick.AddListener(() => { OnSceneSelected(index); });
            go.transform.SetParent(sceneSelectRoot);

            Text text = go.GetComponentInChildren<Text>();
            text.text = scenes[i].SceneTitle;
        }

        var btnRect = btnPrototype.GetComponent<RectTransform>();
        var contentRect = sceneSelectRoot.GetComponent<RectTransform>();
        var size = contentRect.sizeDelta;
        size.y = (btnRect.sizeDelta.y + SceneListBtnGap) * scenes.Length;
        contentRect.sizeDelta = size;
    }

    private void OnSceneSelected(int index)
    {
        Load(index);
    }

    private SceneInfo CurrentInfo { get => currentLoaded >= 0 ? scenes[currentLoaded] : null; }
}
