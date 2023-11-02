using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;


#if UNITY_EDITOR
[ExecuteInEditMode]

//https://docs.unity3d.com/ScriptReference/MenuItem.html
public class NodePlacer : MonoBehaviour
{
    static string path = "AgentNode";
    [MenuItem("GameObject/Nodes/AgentNode", false, 10)]
    static void CreateNodeObject(MenuCommand cmd)
    {
        // Find location
        SceneView lastView = SceneView.lastActiveSceneView;
        // Create the object
        GameObject obj = PrefabUtility.InstantiatePrefab(Resources.Load(path)) as GameObject;


        // https://forum.unity.com/threads/mouse-position-in-a-menuitem-script.44311/
        FieldInfo field = typeof(Event).GetField("s_Current", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        Vector3 mousePos = Vector3.negativeInfinity;
        if (field != null)
        {
            Event current = field.GetValue(null) as Event;
            if(current != null)
            {
                mousePos = Camera.main.ScreenToViewportPoint(new Vector3(current.mousePosition.x, current.mousePosition.y * -1, 0f));
                //Vector3 offset = ;
                //mousePos += offset;

                Debug.DrawLine(current.mousePosition, mousePos, Color.cyan, 2f);
                //mousePos += (Vector3)SceneView.currentDrawingSceneView.position.center;
                //mousePos = current.mousePosition;
                //DebugDisplay.Instance.DrawDot(mousePos);
            }
        }
        if(mousePos != Vector3.negativeInfinity) obj.transform.position = mousePos;
        else obj.transform.position = lastView ? lastView.pivot : Vector3.zero;

        // Set name in proper scene
        StageUtility.PlaceGameObjectInCurrentStage(obj);
        GameObjectUtility.EnsureUniqueNameForSibling(obj);

        // Reparent if context clicked
        GameObjectUtility.SetParentAndAlign(obj, cmd.context as GameObject);
        
        // Register in the undo system
        Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
        Selection.activeGameObject = obj;

        // Mark Scene Dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
    
    //[MenuItem ("Nodes/Click")]
    //public static void Clicked()
    //{
        
    //}

    //[MenuItem("Nodes/Selected Transform Name")]
    //static void LogSelectedTransformName()
    //{
    //    Debug.Log("Selected Transform is on " + Selection.activeTransform.gameObject.name + ".");
    //}

    //// Validate
    //[MenuItem("Nodes/Log Selected Transform Name", true)]
    //static bool ValidateLogSelectedTransformName()
    //{
    //    return Selection.activeTransform != null;
    //}
}
#endif
