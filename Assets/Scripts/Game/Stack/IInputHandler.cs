using System;
using UnityEngine;

public interface IInputHandler
{
    bool HandleInput(Vector2 inputPos, Vector3 inputHUD, Vector3 inputWorld, 
        bool begun, bool moved, bool ended);

    void EndInput();

    void ResetTouchCount();
}