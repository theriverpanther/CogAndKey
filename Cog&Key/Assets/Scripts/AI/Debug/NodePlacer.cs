using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor.SceneManagement;

[ExecuteInEditMode]

public class NodePlacer : MonoBehaviour
{
    static string path = "AgentNode";

    Vector3 mousePos = Vector3.zero;

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
            if (current != null)
            {
                //https://forum.unity.com/threads/how-to-get-mouseposition-in-scene-view.208911/
                mousePos = Event.current.mousePosition;
                try
                {
                    Camera sceneCam = lastView.camera;
                    //mousePos.y += sceneCam.scaledPixelHeight / 2f;

                    Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
                    mousePos = ray.origin;

                    //mousePos.y = sceneCam.scaledPixelHeight - mousePos.y;
                    //mousePos = sceneCam.ScreenToWorldPoint(mousePos);
                    mousePos.z = 0;
                    // Magic value that has to do with the height of the default resolution scene window (7.5)
                    mousePos.y += sceneCam.orthographicSize / 7.5f;
                    //Debug.DrawLine(ray.origin, mousePos, Color.red, 2f);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }
        if (mousePos != Vector3.negativeInfinity) obj.transform.position = mousePos;
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

    public void OnGUI()
    {
        string mouseOver = EditorWindow.mouseOverWindow ? EditorWindow.mouseOverWindow.ToString() : "Nada";
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
