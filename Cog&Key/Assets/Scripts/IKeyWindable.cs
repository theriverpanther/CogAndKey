using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyState
{
    Normal, Reverse, Lock, Fast
}

public interface IKeyWindable
{
    abstract public void InsertKey(KeyState key);
}
