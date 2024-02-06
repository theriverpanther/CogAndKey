using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public enum DirectionType
{
    Clockwise,
    CounterClockwise,
    Stop
}

public enum CopperType
{
    Shiny,
    Worn,
    Random
}

public class Gear : MonoBehaviour
{
    [SerializeField]
    float speed;

    [SerializeField]
    public DirectionType direction;
    int dir;

    GameObject attachedTo;

    public CopperType copperType;
    GameObject gearChosen;

    [SerializeField]
    List<Material> copperVariations;
    Material selectedMaterial;

    // Start is called before the first frame update
    void Start()
    {
        attachedTo = transform.root.gameObject;
        Debug.Log("Attached to: " + attachedTo.name);

        ChangeDirection();
        SetMetalMaterial();

        // Grab the first active gear in the GameObject
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            if (gameObject.transform.GetChild(i).gameObject.activeSelf == true)
            {
                gearChosen = gameObject.transform.GetChild(i).gameObject;
                break;
            }
        }

        gearChosen.GetComponent<MeshRenderer>().material = selectedMaterial;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(Vector3.forward * (speed * dir) * Time.deltaTime, Space.);
        Debug.Log("Rotating.. " + speed);
    }

    private void ChangeDirection()
    {
        switch(direction)
        {
            case DirectionType.Clockwise:
                dir = -1;
                break;
            case DirectionType.CounterClockwise:
                dir = 1;
                break;
            case DirectionType.Stop:
                dir = 0;
                break;
        }
    }

    private void SetMetalMaterial()
    {
        switch (copperType)
        {
            case CopperType.Shiny:
                selectedMaterial = copperVariations[0];
                break;
            case CopperType.Worn:
                selectedMaterial = copperVariations[1];
                break;
            case CopperType.Random:
                selectedMaterial = copperVariations[Random.Range(0, copperVariations.Count)];
                Debug.Log("Random copper indicated");
                break;
        }
    }
}
