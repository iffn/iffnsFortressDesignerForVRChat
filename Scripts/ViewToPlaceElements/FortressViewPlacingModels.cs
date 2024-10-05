
using iffnsStuff.iffnsVRCStuff.FortressBuilder;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FortressViewPlacingModels : UdonSharpBehaviour
{
    [SerializeField] GameObject[] prefabElements;
    [SerializeField] Transform elementHolder;
    Vector3 gridSize = new Vector3(3f, 3f, 3f);

    public void BuildAllElements(int[] elementTypes, int[] xPos, int[] yPos, int[] zPos)
    {
        for(int i = 0; i < elementTypes.Length; i++)
        {
            Transform newElement = GameObject.Instantiate(prefabElements[elementTypes[i]]).transform;
            newElement.parent = elementHolder;
            newElement.localPosition = new Vector3(
                gridSize.x * xPos[i],
                gridSize.y * yPos[i],
                gridSize.z * zPos[i]);
        }
    }

    public void RemoveAllElements()
    {
        foreach (Transform child in elementHolder)
        {
            Destroy(child.gameObject);
        }
    }
}
