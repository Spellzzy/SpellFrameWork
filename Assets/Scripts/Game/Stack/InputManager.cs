using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
	public class InputHandleRecord
	{
		public readonly IInputHandler Handler;

		public readonly int Priority;

		public InputHandleRecord(IInputHandler handler, int priority)
		{
			Handler = handler;
			Priority = priority;
		}
	}

	public Camera WorldCamera;

	public Camera HUDCamera;

	private static InputManager _instance;

	private List<InputHandleRecord> _inputHandlers = new List<InputHandleRecord>();

	private List<InputHandleRecord> _tempInputHandlers;

	private bool _inputEnabled = true;

	private bool _inUpdateIteration;

	private bool _touchesActive;

	public static InputManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindObjectOfType<InputManager>();
			}
			return _instance;
		}
	}

	public void RegisterInputHandler(IInputHandler handler, int priority)
	{
		if (HasInputHandler(handler))
		{
			return;
		}
		if (!_inUpdateIteration)
		{
			_inputHandlers.Add(new InputHandleRecord(handler, priority));
			_inputHandlers.Sort((InputHandleRecord b, InputHandleRecord a) => a.Priority.CompareTo(b.Priority));
			return;
		}
		if (_tempInputHandlers == null)
		{
			_tempInputHandlers = new List<InputHandleRecord>(_inputHandlers);
		}
		_tempInputHandlers.Add(new InputHandleRecord(handler, priority));
		_tempInputHandlers.Sort((InputHandleRecord b, InputHandleRecord a) => a.Priority.CompareTo(b.Priority));
	}

	public void SetInputEnabled(bool inputEnabled)
	{
		_inputEnabled = inputEnabled;
	}

	private bool HasInputHandler(IInputHandler handler)
	{
		foreach (InputHandleRecord inputHandler in _inputHandlers)
		{
			if (inputHandler.Handler == handler)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveInputHandler(IInputHandler handler)
	{
		if (handler == null || !HasInputHandler(handler))
		{
			return;
		}
		if (!_inUpdateIteration)
		{
			_inputHandlers.RemoveAll((InputHandleRecord record) => record.Handler == handler);
			return;
		}
		if (_tempInputHandlers == null)
		{
			_tempInputHandlers = new List<InputHandleRecord>(_inputHandlers);
		}
		_tempInputHandlers.RemoveAll((InputHandleRecord record) => record.Handler == handler);
	}

	private void Update()
	{
		if (!_inputEnabled)
		{
			return;
		}
		Vector2 vector = ClampMousePos(Input.mousePosition);
		bool mouseButtonDown = Input.GetMouseButtonDown(0);
		bool mouseButton = Input.GetMouseButton(0);
		bool mouseButtonUp = Input.GetMouseButtonUp(0);
		_inUpdateIteration = true;
		foreach (InputHandleRecord inputHandler in _inputHandlers)
		{
			if (inputHandler.Handler == null)
			{
				Debug.LogError("A registered input handler was destroyed");
			}
			else if (inputHandler.Handler.HandleInput(vector, GetInputHud(vector), GetInputWorld(vector), mouseButtonDown, mouseButton, mouseButtonUp))
			{
				break;
			}
		}
		if (mouseButtonUp)
		{
			foreach (InputHandleRecord inputHandler2 in _inputHandlers)
			{
				if (inputHandler2.Handler == null)
				{
					Debug.LogError("A registered input handler was destroyed");
				}
				else
				{
					inputHandler2.Handler.EndInput();
				}
			}
		}
		_inUpdateIteration = false;
		if (_tempInputHandlers != null)
		{
			_inputHandlers = _tempInputHandlers;
			_tempInputHandlers = null;
		}
	}

	private Vector2 ClampMousePos(Vector2 input)
	{
		Vector2 result = input;
		result.x = Mathf.Clamp(input.x, 0f, Screen.width);
		result.y = Mathf.Clamp(input.y, 0f, Screen.height);
		return result;
	}

	private Vector3 GetInputWorld(Vector2 input)
	{
		Ray ray = WorldCamera.ScreenPointToRay(input);
		if (new Plane(Vector3.forward, Vector3.zero).Raycast(ray, out var enter))
		{
			Vector3 point = ray.GetPoint(enter);
			point.z = 0f;
			return point;
		}
		return Vector3.zero;
	}

	private Vector3 GetInputHud(Vector2 input)
	{
		Vector3 position = new Vector3(input.x, input.y, Utility.HudZOffset);
		Vector3 result = HUDCamera.ScreenToWorldPoint(position);
		result.z = Utility.HudZOffset;
		return result;
	}
}
