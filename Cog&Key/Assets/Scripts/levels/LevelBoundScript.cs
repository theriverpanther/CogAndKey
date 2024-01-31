using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraBoundType {
    Right,
    Left,
    Up,
    Down,
    Lock
}

public class LevelBoundScript : MonoBehaviour {
    [SerializeField] private CameraBoundType type;
    public CameraBoundType AreaType { get { return type; } }
    public Rect Area { get; set; }
}
