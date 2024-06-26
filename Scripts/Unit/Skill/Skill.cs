using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    public float attackRange = 2f;
    public float skillCoefficient;

    public abstract void Activate(BaseUnit user, BaseUnit target);

    public abstract void ApplyEffect(BaseUnit target);

    public virtual void ActiveEffect()
    {

    }

    public virtual void DeActiveEffect()
    {

    }
}