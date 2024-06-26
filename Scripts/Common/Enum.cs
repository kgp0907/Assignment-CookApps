using System;

public enum UNIT_STATE
{
    IDLE = 0,
    MOVE,
    VICTORY,
    ATTACK,
    DEAD,
    PATROL,
    SKILL,
    STUNNED,

    NONE   
}

public enum UNIT_TYPE
{
    CHARACTER = 0,
    ENEMY,

    NONE
}

public enum MONSTER_TYPE
{
    NORMAL,
    BOSS,

    NONE
}

public enum ClassType
{
    Knight,
    Thief,
    Archer,
    Priest,
    NONE
}

public enum ATTACK_TYPE
{
    Normal,
    Skill
}

public enum FONT_TYPE
{
    HIT,
    HEAL
}
