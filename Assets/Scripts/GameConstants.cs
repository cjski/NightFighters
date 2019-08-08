using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameConstants
{
    public static int NUM_MONSTER_CLASSES = 2;
    public static int NUM_HUMAN_CLASSES = 2;

    public static int NUM_PLAYERS = 4;
    public static int NUM_TYPES_OF_CLASSES = 2;
    public static int HUMAN_CLASS_TYPE_INDEX = 0;
    public static int MONSTER_CLASS_TYPE_INDEX = 1;

    public static int MAP_ROWS = 4;
    public static int MAP_COLUMNS = 6;
    public static float MAP_TILE_SIZE = 3.5f;
    public static Vector2 MAP_OFFSET = new Vector2(-4, -2);

    public static Vector3[] CHARACTER_INFO_PANEL_POSITIONS = 
    {
        new Vector3(0, 8.6f, 0),
        new Vector3(6.2f, 8.6f, 0),
        new Vector3(12.4f, 8.6f, 0),
        new Vector3(18.6f, 8.6f, 0)
    };

}
