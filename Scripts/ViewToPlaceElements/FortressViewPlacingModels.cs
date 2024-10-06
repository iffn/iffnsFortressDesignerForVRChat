
using iffnsStuff.iffnsVRCStuff.FortressBuilder;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FortressViewPlacingModels : UdonSharpBehaviour
{
    [SerializeField] FortressElementController[] prefabElements;
    [SerializeField] Transform elementHolder;
    Vector3 gridSize = new Vector3(3f, 3f, 3f);

    private void Start()
    {
        Setup();
    }

    void Setup()
    {
        for (int i = 0; i < prefabElements.Length; i++)
        {
            prefabElements[i].Setup(i);
        }
    }

    public void RefreshEverything(FortressModel model)
    {
        RemoveAllElements();
        BuildAllElements(model.ElementTypes, model.XPos, model.YPos, model.ZPos);
    }

    public void RefreshEverything(int[] elementTypes, int[] xPos, int[] yPos, int[] zPos)
    {
        RemoveAllElements();
        BuildAllElements(elementTypes, xPos, yPos, zPos);
    }

    public void BuildAllElements(int[] elementTypes, int[] xPos, int[] yPos, int[] zPos)
    {
        for(int i = 0; i < elementTypes.Length; i++)
        {
            Transform newElement = GameObject.Instantiate(prefabElements[elementTypes[i]].transform.gameObject).transform;
            newElement.parent = elementHolder;
            newElement.localPosition = new Vector3(
                gridSize.x * xPos[i],
                gridSize.y * yPos[i],
                gridSize.z * zPos[i]);
        }
    }

    public int GetElementIndexCanBeNegativeOne(FortressElementController element)
    {
        if (element.transform.parent != transform) return -1;

        return element.transform.GetSiblingIndex();
    }

    public void RemoveAllElements()
    {
        foreach (Transform child in elementHolder)
        {
            Destroy(child.gameObject);
        }
    }
}
