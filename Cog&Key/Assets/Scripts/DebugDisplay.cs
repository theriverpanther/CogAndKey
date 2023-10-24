using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDisplay : MonoBehaviour
{
    [SerializeField] private GameObject DebugDotPrefab;
    public static DebugDisplay Instance {  get; private set; }
    private Dictionary<string, GameObject> savedDots = new Dictionary<string, GameObject>();

    private void Awake() {
        Instance = this;
    }

    public void DrawDot(Vector3 position) {
        Instantiate(DebugDotPrefab).transform.position = position;
    }

    public void PlaceDot(string identifier, Vector3 position) {
        if(!savedDots.ContainsKey(identifier)) {
            savedDots[identifier] = Instantiate(DebugDotPrefab);
        }

        savedDots[identifier].transform.position = position;
    }
}
