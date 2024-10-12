
using iffnsStuff.iffnsVRCStuff.FortressBuilder;
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class FortressBuilderController : UdonSharpBehaviour
{
    [SerializeField] FortressModel linkedModel;
    [SerializeField] FortressViewPlacingModels linkedViewPlacingModels;
    [SerializeField] GameObject BuilderOnlyObjectHolder;
    [SerializeField] GameObject VisitorOnlyObjectHolder;

    [SerializeField] float builderPlayerHeight = 10;

    //Setup variables
    VRCPlayerApi localPlayer;
    bool inVR;

    //Static variables
    readonly Vector3 defaultOffset = new Vector3(-1.5f, 0, -1.5f);

    readonly Vector3Int largeOffset = new Vector3Int(300, 0, 300);

    //Runtime variables
    bool inBuildMode = false;
    float normalUserHeight = 1;
    public FortressElementController selectedElement;
    Vector3 initialElementPosition;

    float walkSpeed;
    float runSpeed;
    float straveSpeed;

    public Vector3Int currentGridPosition;

    //Internal functions
    void MakePlayerTheBuilder()
    {
        normalUserHeight = localPlayer.GetAvatarEyeHeightAsMeters();
        localPlayer.SetAvatarEyeHeightByMeters(builderPlayerHeight);

        ActivateBuilderObjects(true);

        inBuildMode = true;

        //Somehow gets reset:
        /*
        localPlayer.SetRunSpeed(runSpeed * builderPlayerHeight);
        localPlayer.SetWalkSpeed(walkSpeed * builderPlayerHeight);
        localPlayer.SetStrafeSpeed(straveSpeed * builderPlayerHeight);
        */

    }

    void ExitBuildMode()
    {
        localPlayer.SetAvatarEyeHeightByMeters(normalUserHeight);

        ActivateBuilderObjects(false);

        inBuildMode = false;

        /*
        localPlayer.SetRunSpeed(runSpeed);
        localPlayer.SetWalkSpeed(walkSpeed);
        localPlayer.SetStrafeSpeed(straveSpeed);
        */
    }

    void ActivateBuilderObjects(bool builder)
    {
        VisitorOnlyObjectHolder.SetActive(!builder);
        BuilderOnlyObjectHolder.SetActive(builder);
    }

    void HandleGetFortressElement(bool rightHandOrDesktop)
    {
        if (inVR)
        {
            //ToDo
            return;
        }
        else
        {
            VRCPlayerApi.TrackingData head = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

            Ray ray = new Ray(head.position, head.rotation * Vector3.forward);

            Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);

            if (hit.transform == null) return;

            FortressElementController element = hit.transform.GetComponent<FortressElementController>(); //TryGetComponent is not exposed in U# (...)

            if (element == null) return;

            selectedElement = element;
            initialElementPosition = selectedElement.transform.position;
        }
    }

    public Vector3 headPosition;
    public Vector3 rayDirection;
    public float targetHeight;
    public float heightDifference;
    public float heightMultiplier;
    public Vector3 rayOffset;
    public Vector3 hitPosition;
    public Vector3 localHitPosition;
    public Vector3 placePosition;

    void HandlePositionFortressElement()
    {
        if (inVR)
        {
            //ToDo
            return;
        }
        else
        {
            VRCPlayerApi.TrackingData head = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

            headPosition = head.position;
            rayDirection = head.rotation * Vector3.forward;
            targetHeight = linkedModel.transform.position.y;
            heightDifference = targetHeight - head.position.y;
            heightMultiplier = heightDifference / rayDirection.y;
            rayOffset = rayDirection * heightMultiplier;
            hitPosition = head.position + rayOffset;
            localHitPosition = linkedModel.transform.InverseTransformPoint(hitPosition);
            
            /*
            Ray ray = new Ray(head.position, rayDirection);

            Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);

            if (hit.transform == null) return;

            while(hit.transform == selectedElement.transform || hit.transform.IsChildOf(selectedElement.transform))
            {
                ray = new Ray(hit.point + rayDirection * 0.05f, rayDirection);

                Physics.Raycast(ray, out RaycastHit newHit, Mathf.Infinity);

                if(newHit.transform == null) break;

                hit = newHit;
            }

            Vector3 localHitPosition = linkedModel.transform.InverseTransformPoint(hit.point);
            */

            localHitPosition += largeOffset * 3; //Making sure the values are positive and rounded in the same direction

            currentGridPosition = new Vector3Int(
                (int)(localHitPosition.x / 3),
                (int)(localHitPosition.y / 3),
                (int)(localHitPosition.z / 3)
                );

            currentGridPosition -= largeOffset;

            placePosition = linkedModel.transform.TransformPoint(currentGridPosition * 3);
            selectedElement.transform.position = placePosition;

        }
    }

    void PlaceElement()
    {
        linkedModel.AddElement(selectedElement.ReferenceIndex, currentGridPosition.x, currentGridPosition.y, currentGridPosition.z);

        linkedViewPlacingModels.RefreshEverything(linkedModel);
    }

    void DropElement()
    {
        selectedElement.transform.position = initialElementPosition;

        selectedElement = null;
    }

    //Unity functions
    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        inVR = localPlayer.IsUserInVR();

        walkSpeed = localPlayer.GetWalkSpeed();
        runSpeed = localPlayer.GetRunSpeed();
        straveSpeed = localPlayer.GetStrafeSpeed();

        if (Networking.LocalPlayer.playerId == 1)
        {
            MakePlayerTheBuilder();
        }
    }

    private void Update()
    {
        //Doing it once somehow gets reset
        if (inBuildMode)
        {
            localPlayer.SetRunSpeed(runSpeed * builderPlayerHeight);
            localPlayer.SetWalkSpeed(walkSpeed * builderPlayerHeight);
            localPlayer.SetStrafeSpeed(straveSpeed * builderPlayerHeight);
        }
        else
        {
            localPlayer.SetRunSpeed(runSpeed);
            localPlayer.SetWalkSpeed(walkSpeed);
            localPlayer.SetStrafeSpeed(straveSpeed);
        }

        if (selectedElement != null) HandlePositionFortressElement();

        if (inVR)
        {

        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                VRCPlayerApi.TrackingData head = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

                Vector3 rayDirection = head.rotation * Vector3.forward;

                Ray ray = new Ray(head.position, rayDirection);

                Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity);

                if (hit.transform == null) return;

                FortressElementController element = hit.transform.GetComponent<FortressElementController>(); //TryGetComponent is not exposed in U# (...)

                if (element == null) return;

                int indexCanBeNegativeOne = linkedViewPlacingModels.GetElementIndexCanBeNegativeOne(element);

                if (indexCanBeNegativeOne == -1) return;

                Debug.Log($"Removing element {indexCanBeNegativeOne}");

                linkedModel.RemoveElement(indexCanBeNegativeOne);

                linkedViewPlacingModels.RefreshEverything(linkedModel);
            }
        }
    }

    //VRC functions
    public override void OnAvatarChanged(VRCPlayerApi player)
    {
        base.OnAvatarChanged(player);

        if (!player.isLocal) return;

        normalUserHeight = localPlayer.GetAvatarEyeHeightAsMeters();
        localPlayer.SetAvatarEyeHeightByMeters(builderPlayerHeight);
    }

    public override void InputUse(bool value, UdonInputEventArgs args)
    {
        base.InputUse(value, args);

        if (!inBuildMode) return;

        if (inVR)
        {

        }
        else
        {
            if (value)
            {
                if (selectedElement == null) HandleGetFortressElement(true);
                else PlaceElement();
            }
        }
    }

    public override void InputDrop(bool value, UdonInputEventArgs args)
    {
        base.InputDrop(value, args);

        if (!inBuildMode) return;

        if (inVR)
        {

        }
        else
        {
            if (value)
            {
                if (selectedElement != null) DropElement();
            }
        }
    }
}
