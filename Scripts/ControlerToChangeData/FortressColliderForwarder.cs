
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FortressColliderForwarder : UdonSharpBehaviour
{
    [SerializeField] FortressElementController linkedElementController;

    public FortressElementController LinkedElementController
    {
        get { return linkedElementController; }
    }
}
