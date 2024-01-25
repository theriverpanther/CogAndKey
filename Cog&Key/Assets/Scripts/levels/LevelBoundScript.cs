using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CamerBoundType {
    Right,
    Left,
    Up,
    Down,
    Lock
}

public class LevelBoundScript : MonoBehaviour {
    [SerializeField] private CamerBoundType type;
    public CamerBoundType AreaType { get { return type; } }
    public Rect Area { get; set; }
}
