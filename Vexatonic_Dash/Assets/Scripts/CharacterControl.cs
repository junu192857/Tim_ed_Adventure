using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    private IEnumerator characterCoroutine;

    private float g => GameManager.g;
    private int gravityAngle;

    private delegate float TimeFunc(float time, float playerMovingTime);
    TimeFunc func;

    [SerializeField] private GameObject afterimage;
    [SerializeField] private ParticleSystem particleSystem;

    public void MoveCharacter(Note note, double gameTime) {
        Instantiate(afterimage, transform.position, transform.rotation);
        transform.localScale = new Vector3((int)note.direction, 1, 1);
        
        if (characterCoroutine != null) StopCoroutine(characterCoroutine);
        switch (note.noteType) {
            case NoteType.Normal:
            case NoteType.Dash:
                if (note is PlatformNote)
                {
                    characterCoroutine = MoveCharacterCoroutine(note, gameTime);
                    StartCoroutine(characterCoroutine);
                }
                else Debug.LogError("note type wrong");
                break;
            case NoteType.Jump:
                if (note is JumpNote)
                {
                    characterCoroutine = JumpCharacterCoroutine(note, gameTime);
                    StartCoroutine(characterCoroutine);
                }
                else Debug.LogError("note type wrong");
                break;
            default:
                break;
        }
    }

    private IEnumerator MoveCharacterCoroutine(Note note, double gameTime) {
        PlatformNote platformNote = note as PlatformNote;
        gameObject.transform.position = platformNote.startPos;

        func = note.noteType switch
        {
            NoteType.Normal or NoteType.Jump => CalculateTimeForNormal,
            NoteType.Dash => CalculateTimeForDash,
            _ => throw new ArgumentException()
        };

        float playerMovingTime = (float)(platformNote.noteEndTime - gameTime);
        float time = 0;
        float adjustedTime = 0;

        if (note.noteSubType == NoteSubType.End) {
            Destroy(gameObject);
            yield break;
        }
        // 경사면 전용
        float forwardMovingTime;
        Vector3 stopoverPos;

        if (note.angle == 0) {
            gameObject.transform.localEulerAngles = Vector3.zero;
            while (adjustedTime < playerMovingTime + 0.166f)
            {
                Vector3 targetPosition = platformNote.startPos * (playerMovingTime - adjustedTime) / playerMovingTime + platformNote.endPos * adjustedTime / playerMovingTime;
                gameObject.transform.position = targetPosition;
                time += Time.deltaTime;
                adjustedTime = func(time, playerMovingTime);
                yield return null;
            }
        }
        else {
            forwardMovingTime = playerMovingTime * 0.16f / (platformNote.endPos.x - platformNote.startPos.x);
            stopoverPos = platformNote.startPos + new Vector3(0.16f, 0);
            gameObject.transform.localEulerAngles = Vector3.zero;
            while (adjustedTime < forwardMovingTime) {
                Vector3 targetPosition = platformNote.startPos * (forwardMovingTime - adjustedTime) / forwardMovingTime + stopoverPos * adjustedTime / forwardMovingTime;
                gameObject.transform.position = targetPosition;
                time += Time.deltaTime;
                adjustedTime = func(time, playerMovingTime);
                yield return null;
            }
            gameObject.transform.localEulerAngles = new Vector3(0, 0, (int)note.direction * note.actualAngle);
            while (adjustedTime < playerMovingTime + 0.166f) {
                Vector3 targetPosition = stopoverPos * (playerMovingTime - adjustedTime) / (playerMovingTime - forwardMovingTime)
                                       + platformNote.endPos * (adjustedTime - forwardMovingTime) / (playerMovingTime - forwardMovingTime);
                gameObject.transform.position = targetPosition;
                time += Time.deltaTime;
                adjustedTime = func(time, playerMovingTime);
                yield return null;
            }
        }
        characterCoroutine = null;
    }

    private IEnumerator JumpCharacterCoroutine(Note note, double gameTime) {
        JumpNote jumpNote = note as JumpNote;


        gameObject.transform.position = jumpNote.startPos;
        

        float playerMovingTime = (float)(jumpNote.noteEndTime - gameTime);
        float time = 0;

        float v_x = (note.endPos.x - note.startPos.x) / playerMovingTime;
        float v_y = (note.endPos.y - note.startPos.y) / playerMovingTime - 0.5f * g * playerMovingTime;
        
        // Vector3 vDelta = (note.endPos - note.startPos) / playerMovingTime;
        // Vector3 vVertical = -0.5f * g * playerMovingTime;
        // Vector3 v = vDelta + vVertical

        while (time < playerMovingTime + 0.166f) {
            Vector3 targetPosition = CalculateJumpPosition(new Vector2(v_x, v_y), time, note.startPos);
            if (time > playerMovingTime) targetPosition.y = note.endPos.y;
            // Vector3 targetPosition = CalculateJumpPosition(v, time, note.startPos);
            // if (time > playerMovingTime) targetPosition = note.endPos;
            gameObject.transform.position = targetPosition;
            time += Time.deltaTime;
            yield return null;
        }
        characterCoroutine = null;
    }

    [Obsolete("Please provide vector directly instead its x and y coordination")]
    private Vector3 CalculateJumpPosition(float v_x, float v_y, float time, Vector3 startPos)
    {
        return CalculateJumpPosition(new Vector2(v_x, v_y), time, startPos);
    }

    private Vector3 CalculateJumpPosition(Vector3 v, float time, Vector3 startPos)
    {
        return startPos + (v * time) + (0.5f * time * time * GameManager.myManager.GravityAsVector);
    }

    public void HurtPlayer(float health) {
        SpriteRenderer sr = gameObject.GetComponentInChildren<SpriteRenderer>();
        Color c = new Color(1, health / 100, health / 100);
        sr.color = c;
    }

    private void Start()
    {
        // Comment: temporarily disabled this code for testing character rotation over angled platforms.
        //UpdateGravity();
    }

    private void Update()
    {
        //UpdateGravity();
    }
    
    private void UpdateGravity()
    {
        gravityAngle = GameManager.myManager.gravity;
        transform.rotation = Quaternion.Euler(0f, 0f, gravityAngle);
    }

    private float CalculateTimeForNormal(float time, float playerMovingTime) => time;

    private float CalculateTimeForDash(float time, float playerMovingTime) {
        if (time <= playerMovingTime) return Mathf.Sqrt(time * playerMovingTime);
        else return time;
    }
}
