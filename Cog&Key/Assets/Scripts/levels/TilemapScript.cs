using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapScript : MonoBehaviour
{
    public static TilemapScript Instance { get; private set; }
    public Tilemap WallGrid { get; private set; }

    void Awake()
    {
        Instance = this;
        WallGrid = GetComponent<Tilemap>();
    }


}
