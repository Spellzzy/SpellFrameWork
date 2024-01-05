using SpellFramework.Tools;
using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    private Vector3 position;
    public float knockForce;
    public int attackDamage = 1;
    void Start()
    {
        position = transform.localPosition;
    }

    void IsFaceRight(bool isFaceRight)
    {
        transform.localPosition = isFaceRight ? position : new Vector3(-position.x , position.y, position.z);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        var targetDamage = col.GetComponent<IDamageable>();
        if (targetDamage != null)
        {
            Vector2 dir = col.transform.position - transform.parent.position;
            int damage = ZMath.Random(1, attackDamage);
            bool isCritical = ZMath.Random(0, 10) > 5;
            if (isCritical)
            {
                damage *= 2;
            }
            targetDamage.OnHit(damage, dir * knockForce);
            DamagePopup.Create(col.transform.position, damage, isCritical);
        }
    }
}
