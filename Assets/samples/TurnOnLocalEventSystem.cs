using UnityEngine;
using UnityEngine.EventSystems;

public class TurnOnLocalEventSystem : MonoBehaviour
{
    private void Awake()
    {
        if (EventSystem.current == null)
        {
            foreach (var go in gameObject.scene.GetRootGameObjects())
            {
                var localEventSystem = go.GetComponentInChildren<EventSystem>();
                if (localEventSystem != null)
                {
                    localEventSystem.gameObject.SetActive(true);
                    break;
                }
            }
        }
    }
}
