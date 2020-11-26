using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditorUtils
{
    public static bool IsPointerOverGameObject
    {
        get
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            //check mouse
            if (EventSystem.current.IsPointerOverGameObject())
                return true;

            //check touch
            if (Input.touchCount > 0 && Input.touches != null && Input.touches[0].phase == TouchPhase.Began)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
                    return true;
            }

            if (IsPointerOverUIObject)
            {
                return true;
            }

            return false;
        }
    }

    private static bool IsPointerOverUIObject
    {
        get
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
    }
}
