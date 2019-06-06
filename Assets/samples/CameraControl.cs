using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    private float mouseMoveScale = 0.1f;
    [SerializeField]
    private float movementSpeed = 10f;

    private Vector3 lastMouse;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMouse = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            var delta = (lastMouse - Input.mousePosition) * mouseMoveScale;
            delta.z = delta.y;
            delta.y = 0;
            transform.position += delta;
            lastMouse = Input.mousePosition;
        }
        else
        {
            Vector3 movement = Vector3.zero;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) { movement.x -= movementSpeed; }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) { movement.x += movementSpeed; }
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) { movement.z += movementSpeed; }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) { movement.z -= movementSpeed; }
            if (Input.GetKey(KeyCode.KeypadPlus)) { movement = transform.forward * movementSpeed; }
            if (Input.GetKey(KeyCode.KeypadMinus)) { movement = -transform.forward * movementSpeed; }

            transform.position += movement * Time.deltaTime;
        }
    }
}
