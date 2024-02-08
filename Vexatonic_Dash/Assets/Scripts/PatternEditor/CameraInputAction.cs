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
        if (em.editorState != EditorState.EditorMain) return;
        float input = value.Get<float>();
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + input, 1, 10);
        em.ReloadMeasureCountLine();
    }

    public void OnCameraMove(InputValue value) {
        if (em.editorState != EditorState.EditorMain) return;
        Vector2 input = value.Get<Vector2>();
        Camera.main.transform.position += new Vector3(2 * input.x, 2 * input.y, 0f);
        em.ReloadMeasureCountLine();
    }

    public void OnMusicOffset(InputValue value) {
        if (em.editorState != EditorState.EditorMain) return;
        em.ChangeMusicOffset(value.Get<float>());
    }

    public void OnDirectionToggle(InputValue value) {
        if (em.editorState != EditorState.EditorMain) return;
        em.ChangeDirection(value.Get<float>());
    }
    private void Update()
    {
        if (em.editorState != EditorState.EditorMain) return;
        if (Input.GetKey(KeyCode.Mouse1))
        {
            currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.Translate(previousMousePosition - currentMousePosition);
            em.ReloadMeasureCountLine();
        }
        previousMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
