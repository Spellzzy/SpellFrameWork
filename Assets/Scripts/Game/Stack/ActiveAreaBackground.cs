using Shapes;
using UnityEngine;

public class ActiveAreaBackground : MonoBehaviour
{
    public SpriteRenderer Sprite;

    public Rectangle RectDashed;

    public Color BgColor;

    private float _dashSpeed = -0.05f;

    private float Frac(float x)
	{
		return x - Mathf.Floor(x);
	}

    public void SetSize(Vector2 size)
    {
        Sprite.size = size;
        Sprite.color = BgColor;
        RectDashed.Width = size.x;
        RectDashed.Height = size.y;
    }

    public void UpdateTime(float deltaTime)
    {
        RectDashed.DashOffset = Frac(RectDashed.DashOffset + deltaTime * _dashSpeed);
    }
}
