using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBoundScript : MonoBehaviour
{
    public enum Type
    {
        Right,
        Left,
        Up,
        Down,
        Lock
    }

    [SerializeField] private Type type;
    public Type AreaType { get { return type; } }
    public Rect Area { get; set; }
}
