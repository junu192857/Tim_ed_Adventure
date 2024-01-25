using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PlayerPositionState { 
    InsideBorder,
    OutsideBorder,
    CriticallyOutsideBorder
}
public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private GameObject character;

    private Vector2 _2DViewPointPos;

    private IEnumerator cameraCoroutine;

    private void Start()
    {
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart() {
        yield return null;
        character = GameObject.FindGameObjectWithTag("Player");
    }
    private void Update()
    {
        if (character == null) return;
        _2DViewPointPos = Camera.main.WorldToViewportPoint(character.transform.position);

        switch (CheckPlayerPosition(_2DViewPointPos)) { 
            case PlayerPositionState.InsideBorder:
                break;
            case PlayerPositionState.OutsideBorder:
                InstantlyMoveCamera(_2DViewPointPos);
                break;
            case PlayerPositionState.CriticallyOutsideBorder:
                ContinuouslyMoveCamera(_2DViewPointPos);
                break;
            default:
                break;
        }
    }

    private PlayerPositionState CheckPlayerPosition(Vector2 viewpointPos) {
        Vector2 centeredPos = viewpointPos - new Vector2(0.5f, 0.5f);

        //TODO: 나중에 더 플랫포머같게 값 재설정하기.
        if (Mathf.Abs(centeredPos.x) < 0.35f && Mathf.Abs(centeredPos.y) < 0.4f)
        {
            return PlayerPositionState.InsideBorder;
        }
        else if (Mathf.Abs(centeredPos.x) < 0.36f && Mathf.Abs(centeredPos.y) < 0.41f)
        {
            return PlayerPositionState.OutsideBorder;
        }
        else return PlayerPositionState.CriticallyOutsideBorder;
    }

    private void InstantlyMoveCamera(Vector2 viewpointPos)
    {
        Vector2 newCameraPos = CalcalateNewCameraPos(viewpointPos);

        _camera.transform.position = newCameraPos;
    }
    private void ContinuouslyMoveCamera(Vector2 viewpointPos) {
        if (cameraCoroutine != null) { 
            StopCoroutine(cameraCoroutine);
            cameraCoroutine = null;
        }
        cameraCoroutine = CameraMoveCoroutine(viewpointPos);
        StartCoroutine(cameraCoroutine);
    }

    private IEnumerator CameraMoveCoroutine(Vector2 viewpointPos) {
        Vector2 currentCameraPos = _camera.transform.position;
        Vector2 newCameraPos = CalcalateNewCameraPos(viewpointPos);
        float time = 0f;
        //0.2초간 카메라 움직이기.
        while (time < 0.2f) {
            _camera.transform.position = 5f * (currentCameraPos * (0.2f - time) + newCameraPos * time);
            time += Time.deltaTime;
            yield return null;
        }
    }

    private Vector2 CalcalateNewCameraPos(Vector2 viewpointPos) {
        Vector2 centeredPos = viewpointPos - new Vector2(0.5f, 0.5f);
        Vector2 newCameraPos = _camera.transform.position;
        //x좌표는 해상도에 따라 다르게 조정해야 하지만 우선 1920*1080 (16:9) 비율 가정하고 작성함
        if (centeredPos.x < -0.35f) newCameraPos.x -= _camera.orthographicSize * 16 / 9 * 2 * (-0.35f - centeredPos.x);
        else if (centeredPos.x > 0.35f) newCameraPos.x += _camera.orthographicSize * 16 / 9 * 2 * (centeredPos.x - (-0.35f));
        if (centeredPos.y < -0.4f) newCameraPos.y -= _camera.orthographicSize * 2 * (-0.4f - centeredPos.y);
        else if (centeredPos.y > 0.4f) newCameraPos.y += _camera.orthographicSize * 2 * (centeredPos.y - 0.4f);

        return newCameraPos;
    }

}
