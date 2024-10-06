
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FortressElementController : UdonSharpBehaviour
{
    int referenceIndex = -1;

    public int ReferenceIndex
    {
        get { return referenceIndex; }
    }

    public void Setup(int referenceIndex)
    {
        this.referenceIndex = referenceIndex;
    }

    void Start()
    {
        
    }
}
