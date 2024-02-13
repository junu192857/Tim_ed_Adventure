using System;
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

    private bool isBeingControlled;
    private Vector2 cameraVelocity;
    private Note currentPlayingNote;
    private int currentAngle;

    private Queue<CameraControlInfo> cameraInfoQueue => GameManager.myManager.rm.cameraInfoQueue;

    private double gameTime => GameManager.myManager.rm.gameTime;
    private double lastNoteTime;

    private IEnumerator zoomCoroutine;
    private IEnumerator rotateCoroutine;
    private IEnumerator moveCoroutine;

    private void Start()
    {
        isBeingControlled = false;
        StartCoroutine(LateStart());
        cameraVelocity = new Vector2();

        lastNoteTime = 0;
        currentAngle = 0;
    }

    private IEnumerator LateStart()
    {
        yield return null;
        character = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        if (character == null) return;
        ChangeCurrentPlayingNote();

        while (cameraInfoQueue.TryPeek(out CameraControlInfo info) && (info.time <= gameTime))
        {
            ProcessCameraInfo(info);
            cameraInfoQueue.Dequeue();
        }

        if (isBeingControlled) ControlledMove();
        else AutoMove();
    }

    private void ControlledMove()
    {
        if (moveCoroutine != null || !isBeingControlled) return;
        
        Vector3 currentCamPos = _camera.transform.position;
        Vector3 newCamPos = currentCamPos + (Vector3) (Time.deltaTime * cameraVelocity);
        _camera.transform.position = newCamPos;
    }

    private void AutoMove()
    {
        // _2DViewPointPos = Camera.main.WorldToViewportPoint(character.transform.position);
        //
        // switch (CheckPlayerPosition(_2DViewPointPos)) { 
        //     case PlayerPositionState.InsideBorder:
        //         break;
        //     case PlayerPositionState.OutsideBorder:
        //         InstantlyMoveCamera(_2DViewPointPos);
        //         break;
        //     case PlayerPositionState.CriticallyOutsideBorder:
        //         ContinuouslyMoveCamera(_2DViewPointPos);
        //         break;
        //     default:
        //         break;
        // }

        if (currentPlayingNote == null || cameraCoroutine != null || isBeingControlled) return;

        cameraCoroutine = AutoMoveCoroutine();
        StartCoroutine(cameraCoroutine);
    }

    private IEnumerator AutoMoveCoroutine()
    {
        Vector3 currentCamPos = _camera.transform.position;
        Vector3 currentEndPos = ConvertToCameraPos(currentPlayingNote.endPos);

        double endTime = currentPlayingNote.noteEndTime;
        
        Vector3 cameraVelocity = (currentEndPos - currentCamPos) / (float) (endTime - gameTime);

        while (!isBeingControlled)
        {
            Vector3 tempCamPos = _camera.transform.position + Time.deltaTime * cameraVelocity;
            _camera.transform.position = tempCamPos;
            yield return null;
        }

        StopCameraCoroutine();
    }

    private void ProcessCameraInfo(CameraControlInfo info)
    {
        switch (info.type)
        {
            case CameraControlType.Zoom:
                ZoomCamera((info as CameraZoomInfo).scale, info.term);
                break;
            case CameraControlType.Rotate:
                RotateCamera((info as CameraRotateInfo).angle, info.term);
                break;
            case CameraControlType.Velocity:
                ChangeCameraVelocity((info as CameraVelocityInfo).cameraVelocity);
                break;
            case CameraControlType.Fix:
                FixCamera((info as CameraFixInfo).fixPivot, info.term);
                break;
            case CameraControlType.Return:
                FollowPlayer(info.term);
                break;
            default:
                throw new ArgumentException("Camera control type not supported");
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

    /// <summary>
    /// Moves camera to given position and fixes camera.
    /// The usage of this method will have the camera enter the controlled mode.
    /// To exit it, see this: <see cref="FollowPlayer"/>
    /// </summary>
    /// <param name="fixPivot">The position you want to fix camera.</param>
    /// <param name="term">The time you want to let this operation take.</param>
    public void FixCamera(Vector2 fixPivot, double term)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        moveCoroutine = FixCameraCoroutine(fixPivot, term);
        StartCoroutine(moveCoroutine);
    }
    
    private IEnumerator FixCameraCoroutine(Vector2 fixPivot, double term)
    {
        isBeingControlled = true;

        Vector3 currentPosition = _camera.transform.position;
        Vector3 fixPivot3D = ConvertToCameraPos(fixPivot);
        float localTime = 0f;

        while (localTime < term)
        {
            Vector3 tempPos = GetSineEaseValue(currentPosition, fixPivot3D, (float)(localTime / term));
            _camera.transform.position = tempPos;
            yield return null;
            localTime += Time.deltaTime;
        }

        _camera.transform.position = fixPivot3D;
        moveCoroutine = null;
    }

    /// <summary>
    /// Restores camera to position of player, and exit controlled mode.
    /// </summary>
    /// <param name="term">The time you want to let this operation take.</param>
    public void FollowPlayer(double term)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        moveCoroutine = FollowPlayerCoroutine(term);
        StartCoroutine(moveCoroutine);
    }
    
    private IEnumerator FollowPlayerCoroutine(double term)
    {
        cameraVelocity = new Vector2();
        
        Vector3 currentPosition = _camera.transform.position;
        float localTime = 0f;
            
        while (localTime < term)
        {
            Vector3 currentCharacterPos = ConvertToCameraPos(character.transform.position);
            Vector3 tempPos = GetSineOutValue(currentPosition, currentCharacterPos, (float)(localTime / term));
            _camera.transform.position = tempPos;
            
            yield return null;
            localTime += Time.deltaTime;
        }

        _camera.transform.position = ConvertToCameraPos(character.transform.position);
        
        isBeingControlled = false;
        moveCoroutine = null;
    }

    /// <summary>
    /// Sets the angle of camera to given angle.
    /// </summary>
    /// <param name="angle">The angle of camera(in degrees) you want to set</param>
    /// <param name="term">The time you want to let this operation take</param>
    public void RotateCamera(int angle, double term)
    {
        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);

        rotateCoroutine = RotateCameraCoroutine(angle, term);
        StartCoroutine(rotateCoroutine);
    }
    
    private IEnumerator RotateCameraCoroutine(int angle, double term)
    {
        int startAngle = currentAngle;
        float localTime = 0f;

        while (localTime < term)
        {
            float tempAngle = GetSineEaseValue(startAngle, angle, (float)(localTime / term));
            _camera.transform.rotation = Quaternion.AngleAxis(tempAngle, Vector3.forward);
            currentAngle = (int)tempAngle;
            yield return null;
            localTime += Time.deltaTime;
        }

        _camera.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        currentAngle = angle;
        rotateCoroutine = null;
    }

    /// <summary>
    /// Sets the magnification scale into given scale.
    /// </summary>
    /// <param name="scale">The scale of camera, compared by original size.</param>
    /// <param name="term">The time you want to let this operation take.</param>
    private void ZoomCamera(double scale, double term)
    {
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);

        zoomCoroutine = ZoomCameraCoroutine(scale, term);
        StartCoroutine(zoomCoroutine);
    }
    
    private IEnumerator ZoomCameraCoroutine(double scale, double term)
    {
        float currentScale = _camera.orthographicSize;
        float destScale = 3f / (float)scale;
        float localTime = 0f;

        while (localTime < term)
        {
            float tempScale = GetSineEaseValue(currentScale, destScale, (float) (localTime / term));
            _camera.orthographicSize = tempScale;
            yield return null;
            localTime += Time.deltaTime;
        }

        _camera.orthographicSize = destScale;
        zoomCoroutine = null;
    }

    /// <summary>
    /// Sets the velocity of camera.
    /// </summary>
    /// <param name="velocity">The velocity of camera you want to set.</param>
    private void ChangeCameraVelocity(Vector2 velocity)
    {
        this.cameraVelocity = velocity;
    }

    private float GetSineEaseValue(float startValue, float endValue, float ratio)
    {
        float x = Mathf.PI * ratio;
        float scale = (endValue - startValue) / 2.0f;
        float pivot = (endValue + startValue) / 2.0f;

        return pivot - scale * Mathf.Cos(x);
    }

    private Vector3 GetSineEaseValue(Vector3 startValue, Vector3 endValue, float ratio)
    {
        return new Vector3(
            GetSineEaseValue(startValue.x, endValue.x, ratio),
            GetSineEaseValue(startValue.y, endValue.y, ratio),
            GetSineEaseValue(startValue.z, endValue.z, ratio)
        );
    }

    private float GetSineOutValue(float startValue, float endValue, float ratio)
    {
        float x = Mathf.PI / 2.0f * ratio;
        float scale = endValue - startValue;

        return startValue + scale * Mathf.Sin(x);
    }

    private Vector3 GetSineOutValue(Vector3 startValue, Vector3 endValue, float ratio)
    {
        return new Vector3(
            GetSineOutValue(startValue.x, endValue.x, ratio),
            GetSineOutValue(startValue.y, endValue.y, ratio),
            GetSineOutValue(startValue.z, endValue.z, ratio)
        );
    }

    private void ChangeCurrentPlayingNote()
    {
        Note note = GameManager.myManager.rm.CurrentPlayingNote;
        if (note == null) return;
        
        StopCameraCoroutine();

        lastNoteTime = gameTime;
        currentPlayingNote = note;
    }

    private void StopCameraCoroutine()
    {
        if (cameraCoroutine != null) StopCoroutine(cameraCoroutine);
        cameraCoroutine = null;
    }

    private Vector3 ConvertToCameraPos(Vector3 position)
    {
        return new Vector3(
            position.x,
            position.y,
            -5.0f
            );
    }
}
