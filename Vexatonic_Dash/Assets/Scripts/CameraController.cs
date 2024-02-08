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

    private bool isFixed;

    private void Start()
    {
        isFixed = false;
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

    private IEnumerator FixCamera(Vector2 fixPivot, double term)
    {
        isFixed = true;

        Vector2 currentPosition = _camera.transform.position;
        float localTime = 0f;

        while (localTime < term)
        {
            Vector2 tempPos = GetSineEaseValue(currentPosition, fixPivot, (float)(localTime / term));
            _camera.transform.position = tempPos;
            yield return null;
            localTime += Time.deltaTime;
        }

        _camera.transform.position = fixPivot;
    }

    private IEnumerator UnfixCamera(double term)
    {
        Vector2 currentPosition = _camera.transform.position;
        float localTime = 0f;

        while (localTime < term)
        {
            yield return null;
            localTime += Time.deltaTime;
        }

        isFixed = false;
    }

    private IEnumerator RotateCamera(int angle, double term)
    {
        int currentAngle = (int) _camera.transform.rotation.eulerAngles.z;
        float localTime = 0f;

        while (localTime < term)
        {
            float tempAngle = GetSineEaseValue(currentAngle, angle, (float)(localTime / term));
            _camera.transform.rotation = Quaternion.AngleAxis(tempAngle, Vector3.forward);
            yield return null;
            localTime += Time.deltaTime;
        }

        _camera.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private IEnumerator ZoomCamera(double scale, double term)
    {
        float currentScale = _camera.orthographicSize;
        float destScale = 3f * (float)scale;
        float localTime = 0f;

        while (localTime < term)
        {
            float tempScale = GetSineEaseValue(currentScale, destScale, (float) (localTime / term));
            _camera.orthographicSize = tempScale;
            yield return null;
            localTime += Time.deltaTime;
        }

        _camera.orthographicSize = destScale;
    }

    private IEnumerator ChangeCameraVelocity(Vector2 velocity, double term)
    {
        // TODO: Velocity Logic
        yield return null;
    }

    private float GetSineEaseValue(float startValue, float endValue, float ratio)
    {
        float x = 2 * Mathf.PI * ratio;
        float scale = (endValue - startValue) / 2.0f;
        float pivot = (endValue + startValue) / 2.0f;

        return pivot - scale * Mathf.Cos(x);
    }

    private Vector2 GetSineEaseValue(Vector2 startValue, Vector2 endValue, float ratio)
    {
        return new Vector2(
            GetSineEaseValue(startValue.x, endValue.x, ratio),
            GetSineEaseValue(startValue.y, endValue.y, ratio)
        );
    }
}
