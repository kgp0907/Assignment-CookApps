using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefAttackFunc : MonoBehaviour
{
    public MonsterUnit BaseUnit;

    public void Attack()
    {
        BaseUnit.Attack();
    }
}
