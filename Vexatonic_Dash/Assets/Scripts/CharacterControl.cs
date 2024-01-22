using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    public void MoveCharacter(Note note, double gameTime) {
        switch (note.noteType) {
            case NoteType.Normal:
            case NoteType.Dash:
                if (note is PlatformNote)
                {
                    StartCoroutine(MoveCharacterCoroutine(note, gameTime));
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

        float playerMovingTime = (float)(platformNote.noteEndTime - gameTime);
        float time = 0;

        while (time < playerMovingTime + 0.166f) {
            Vector3 targetPosition = platformNote.startPos * (playerMovingTime - time) / playerMovingTime + platformNote.endPos * time / playerMovingTime;
            gameObject.transform.position = targetPosition;
            time += Time.deltaTime;
            yield return null;
        }
    }
}
