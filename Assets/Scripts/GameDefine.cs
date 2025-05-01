using UnityEngine;

public enum FOOD_TYPE
{
    SHRIMP = 0,
    ENOKI = 1,
    MEAT = 2,
    BOKCHOY = 3,
    FISHBALL = 4,
}

public enum MISTAKE_TYPE
{
    UNDERCOOKED,
    OVERCOOKED,
    DROPPED,
    STOLEN,
    SERVED_WRONG_FOOD,
    TIMEOUT,
    DIRTY_FOOD
}

public enum GAME_STATE
{
    PLAYING,
    WON,
    LOST
}

public enum FOOD_COOKING_STATUS
{
    RAW,
    UNDERCOOKED,
    COOKED,
    OVERCOOKED,
    DIRTY
}

public enum FOOD_STATUS
{
    INITIAL,
    GRABBED,
    DROPPED,
    SERVED,
    COOKING,
    STOLEN,
    ON_SKEWER
}

public enum AI_STATUS
{
    IDLE,
    STEALING
}

public enum RELATIVE_MAT_STATUS
{
    IDLE,
    IMPATIENT,
    HAPPY,
    SAD
}

public enum COUSIN_MAT_STATUS
{
    IDLE,
    STEALING,
    FAILED,
    STOLEN,
    LOSE,
    WIN
}