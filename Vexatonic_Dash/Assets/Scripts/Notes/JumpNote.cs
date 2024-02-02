using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JumpNote : Note
{
    public Vector3 GetInformationForPlayer(float inputWidth, float jumpHeight, Vector3 startPos)
    {
        this.startPos = startPos;
        Vector3 endPos = startPos + new Vector3(inputWidth, jumpHeight);
        this.endPos = endPos;
        return endPos;
    }
}
