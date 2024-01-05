using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public static class Utility
{
	public static readonly string DefaultLayer = "Default";

	public static readonly string DraggableLayer = "Draggable";

	public static readonly float CardWidth = 322f;

	public static readonly float CardHeight = 512f;

	public static float GridPadding = 12f;

	public static int GridMaxSize = 150;

	public static float ExtraSectorPadSize = 45f;

	public static float SectorAreaPadSize = 20f;

	public static int maxZoneSize = 5;

	public static readonly int HungerEventID = 999;

	public static float HudZOffset;

	public const float PunchFactor = 1.5f;

	public static Color DefaultCardColor = new Color(0.9f, 0.9f, 0.9f);

	public static Color EventOpportunityColor = new Color(0.44f, 0.89f, 0.41f);

	public static Color EventDisasterColor = new Color(0.89f, 0.25f, 0.17f);

	public static IEnumerator WaitUntilEvent(UnityEvent unityEvent)
	{
		bool trigger = false;
		Action action = delegate
		{
			trigger = true;
		};
		unityEvent.AddListener(action.Invoke);
		yield return new WaitUntil(() => trigger);
		unityEvent.RemoveListener(action.Invoke);
	}

	public static float MapRange(float x, float a, float b, float c, float d)
	{
		if (b - a == 0f)
		{
			return 0f;
		}
		return (d - c) / (b - a) * (x - a) + c;
	}

	public static int[] GetRandomNumberArray(int length)
	{
		int[] array = new int[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = i;
		}
		int num = length;
		while (num > 1)
		{
			num--;
			int num2 = UnityEngine.Random.Range(0, num + 1);
			ref int reference = ref array[num2];
			ref int reference2 = ref array[num];
			int num3 = array[num];
			int num4 = array[num2];
			reference = num3;
			reference2 = num4;
		}
		return array;
	}
}
