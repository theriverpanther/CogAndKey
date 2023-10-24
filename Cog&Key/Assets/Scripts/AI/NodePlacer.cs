using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class NodePlacer : EditorWindow
{
    [MenuItem ("Window/Nodes")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<NodePlacer>();
    }

    public GameObject node;
    private void OnGUI()
    {

        if(Input.GetKeyDown(KeyCode.N))
        {
            Vector2 mousePos = Input.mousePosition;
            Instantiate(node, mousePos, Quaternion.identity);
        }
    }

    private void CheckInput()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            
        }
    }
}
