using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInputAction : MonoBehaviour
{
    [SerializeField] private EditorManager em;

    private Vector2 previousMousePosition;
    private Vector2 currentMousePosition;
    public void OnCameraZoom(InputValue value) {
        float input = value.Get<float>();
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + input, 1, 10);
        em.ReloadMeasureCountLine();
    }

    public void OnCameraMove(InputValue value) {
        Vector2 input = value.Get<Vector2>();
        Camera.main.transform.position += new Vector3(input.x, input.y, 0f);
        em.ReloadMeasureCountLine();
    }


    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse1)) {
            currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.Translate(previousMousePosition - currentMousePosition);
        }
        previousMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetKeyUp(KeyCode.Mouse1)) em.ReloadMeasureCountLine();
    }
}