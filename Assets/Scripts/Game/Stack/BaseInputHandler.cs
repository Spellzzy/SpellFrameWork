using UnityEngine;

public class BaseInputHandler : MonoBehaviour, IInputHandler
{
    public int Priority { get; set; }

    public void OnEnable()
    {
        if (InputManager.Instance == null)
        {
            InputManager.Instance.RegisterInputHandler(this, Priority);
        }
    }

    public void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.RemoveInputHandler(this);
        }
    }

    public virtual bool HandleInput(Vector2 inputPos, Vector3 inputHUD, Vector3 inputWorld, bool begun, bool moved, bool ended)
    {
        return false;
    }

    public virtual void EndInput()
    {
        
    }

    public virtual void ResetTouchCount()
    {
        
    }
}
