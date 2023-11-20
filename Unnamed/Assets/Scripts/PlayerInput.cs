using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput
{
    public NoteType inputType;
    public double inputLifeTime;

    public PlayerInput(NoteType inputType) {
        this.inputType = inputType;
        inputLifeTime = 0.167;
    }
}
