using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Selectable : BaseInputHandler
{
    [SerializeField]
    protected BoxCollider2D _collider;

    [SerializeField]
    protected SpriteRenderer _spriteRenderer;

    public UnityEvent OnSelect;

    public UnityEvent OnDeselect;
    
    [SerializeField]
    protected Color _defaultColor = Color.white;
    [SerializeField]
    protected Color _selectedColor = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField]
    protected Color _disabledColor = new Color(0.5f, 0.5f, 0.5f);

    protected bool _selected;

    protected bool _disabled;

    private bool _inputBeganHere;

    public void Enable ()
    {
        _disabled = false;
        _spriteRenderer.color = _defaultColor;
    }

    public void Disable()
    {
        _disabled = true;
        _spriteRenderer.color = _disabledColor;
    }

    public void ChangeColor(Color color)
    {
        _defaultColor = color;
        _spriteRenderer.color = color;
        _selectedColor = color + Color.gray * 0.5f;
    }


    public override bool HandleInput(Vector2 inputPos, Vector3 inputHUD, Vector3 inputWorld, bool begun, bool moved, bool ended)
    {
        if (_disabled)
        {
            return false;
        }
        // if (begun && _collider.OverlapPoint(inputWorld))
        // {
        //     _inputBeganHere = true;
        // }
        if (begun && _collider.bounds.Contains(inputHUD))
        {
            _inputBeganHere = true;
        }
        if (ended && _inputBeganHere && _collider.bounds.Contains(inputHUD))
        {
            _inputBeganHere = false;
            Select();
            return true;
        }
        return false;
    }

    public override void EndInput()
    {
        _inputBeganHere = false;
    }

    public virtual void Select()
    {
        if (_selected)
        {
            return;
        }
        _selected = true;
        _spriteRenderer.color = _selectedColor;
        OnSelect?.Invoke();
    }

    public virtual void Deselect()
    {
        if (!_selected)
        {
            return;
        }
        _selected = false;
        _spriteRenderer.color = _defaultColor;
        OnDeselect?.Invoke();
    }

    private void OnDestroy()
    {
        if (_selected)
        {
            Deselect();
        }
        OnSelect.RemoveAllListeners();
    }   


}
