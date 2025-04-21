using UnityEngine;

public enum FOOD_TYPE
{
    SHRIMP = 0,
    ENOKI = 1,
    MEAT = 2,
    BOKCHOY = 3,
}

public enum MISTAKE_TYPE
{
    UNDERCOOKED,
    OVERCOOKED,
    DROPPED,
    STOLEN,
    SERVED_WRONG_FOOD,
    TIMEOUT
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
}

public enum FOOD_STATUS
{
    INITIAL,
    GRABBED,
    DROPPED,
    SERVED,
    COOKING,
    STOLEN
}

public enum AI_STATUS
{
    IDLE,
    STEALING
}