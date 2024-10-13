
using iffnsStuff.iffnsVRCStuff.FortressBuilder;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public class RemovalGun : UdonSharpBehaviour
{
    [SerializeField] Transform rayOriginTowardsZ;
    [SerializeField] FortressBuilderController linkedBuilderControler;
    [SerializeField] FortressViewPlacingModels linkedViewPlacingModels;
    [SerializeField] FortressModel linkedModel;
    [SerializeField] LineRenderer linkedLineRenderer1;
    [SerializeField] LineRenderer linkedLineRenderer2;
    [SerializeField] VRCPickup linkedPickup;
    [SerializeField] Transform pickupMoverSinceScalingBrokenOnDesktop;
    [SerializeField] Vector3 pickupOffsetSinceScalingBrokenOnDesktop;

    void Start()
    {
        
    }

    private void Update()
    {
        if (linkedPickup.IsHeld)
        {
            Vector3 rayDirection = rayOriginTowardsZ.rotation * Vector3.forward;

            Ray ray = new Ray(rayOriginTowardsZ.position, rayDirection);

            Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);

            float distance = 1000;

            if (hit.transform != null)
            {
                distance = (hit.point - rayOriginTowardsZ.position).magnitude;
            }

            Vector3 endPointLocal = distance * Vector3.right;

            linkedLineRenderer1.SetPosition(1, endPointLocal);
            linkedLineRenderer2.SetPosition(1, endPointLocal);
        }
    }

    public override void OnPickupUseDown()
    {
        base.OnPickupUseDown();

        //Remove
        Vector3 rayDirection = rayOriginTowardsZ.rotation * Vector3.right;

        Ray ray = new Ray(rayOriginTowardsZ.position, rayDirection);

        Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);

        if (hit.transform == null)
        {
            Debug.Log("No nit");
            return;
        }

        FortressElementController element = hit.transform.GetComponent<FortressElementController>(); //TryGetComponent is not exposed in U# (...)

        if (element == null)
        {
            Debug.Log("Not an element, instead " + hit.transform.name);
            return;
        } 

        int indexCanBeNegativeOne = linkedViewPlacingModels.GetElementIndexCanBeNegativeOne(element);

        if (indexCanBeNegativeOne == -1) return;

        linkedModel.RemoveElement(indexCanBeNegativeOne);

        linkedViewPlacingModels.RefreshEverything(linkedModel);
    }

    public override void OnPickup()
    {
        base.OnPickup();

        //Start beam
        linkedLineRenderer1.enabled = true;
        linkedLineRenderer2.enabled = true;
        pickupMoverSinceScalingBrokenOnDesktop.localPosition = pickupOffsetSinceScalingBrokenOnDesktop;
    }

    public override void OnDrop()
    {
        base.OnDrop();

        //Stop beam
        pickupMoverSinceScalingBrokenOnDesktop.localPosition = Vector3.zero;
        linkedLineRenderer1.enabled = false;
        linkedLineRenderer2.enabled = false;
    }
}
