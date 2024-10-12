
using iffnsStuff.iffnsVRCStuff.FortressBuilder;
using Newtonsoft.Json.Linq;
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
    [SerializeField] Transform buildingLibrary;
    [SerializeField] Transform buildingGrid;

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
    FortressElementController selectedElement;
    Vector3 initialElementPosition;


    float floorHeight = 0;
    int selectedFloorDontWrite = 0;
    int SelectedFloor
    {
        set
        {
            selectedFloorDontWrite = value;
            floorHeight = selectedFloorDontWrite * linkedViewPlacingModels.GridSize.y;
            Vector3 gridPosition = buildingGrid.localPosition;
            gridPosition.y = floorHeight;
            buildingGrid.localPosition = gridPosition;
        }
        get
        {
            return selectedFloorDontWrite;
        }
    }

    float walkSpeed;
    float runSpeed;
    float straveSpeed;

    public Vector3Int currentGridPosition;

    //Internal functions
    void MakePlayerTheBuilder()
    {
        normalUserHeight = localPlayer.GetAvatarEyeHeightAsMeters();
        localPlayer.SetAvatarEyeHeightByMeters(builderPlayerHeight);

        Networking.SetOwner(localPlayer, linkedModel.gameObject);

        ActivateBuilderObjects(true);

        inBuildMode = true;

        localPlayer.Respawn();

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

        localPlayer.Respawn();

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

            if (element.transform.parent != buildingLibrary) return;

            selectedElement = element;
            initialElementPosition = selectedElement.transform.position;
        }
    }

    /*
    public Vector3 headPosition;
    public Vector3 rayDirection;
    public float targetHeight;
    public float heightDifference;
    public float heightMultiplier;
    public Vector3 rayOffset;
    public Vector3 hitPosition;
    public Vector3 localHitPosition;
    public Vector3 placePosition;
    */

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

            Vector3 rayDirection = head.rotation * Vector3.forward;
            float targetHeight = buildingGrid.position.y + linkedViewPlacingModels.GridSize.y * 0.5f;
            float heightDifference = targetHeight - head.position.y;
            Debug.Log($"{nameof(targetHeight)} {targetHeight}");
            Debug.Log($"{nameof(heightDifference)} {heightDifference}");
            float heightMultiplier = heightDifference / rayDirection.y;
            Vector3 rayOffset = rayDirection * heightMultiplier;
            Vector3 hitPosition = head.position + rayOffset;
            Vector3 localHitPosition = linkedModel.transform.InverseTransformPoint(hitPosition);

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

            Vector3 placePosition = linkedModel.transform.TransformPoint(currentGridPosition * 3);
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

        SelectedFloor = 0;
        buildingGrid.SetPositionAndRotation(linkedViewPlacingModels.transform.position, linkedViewPlacingModels.transform.rotation);

        if (localPlayer.isInstanceOwner)
        {
            Debug.Log("Making player instance owner");
            MakePlayerTheBuilder();
        }
        else
        {
            ActivateBuilderObjects(false);
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

        if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetAxis("Mouse ScrollWheel") > 0f) SelectedFloor++;
        if (Input.GetKeyDown(KeyCode.PageDown) || Input.GetAxis("Mouse ScrollWheel") < -0f) SelectedFloor--;

        if (Input.GetKeyDown(KeyCode.F1)) MakePlayerTheBuilder(); 
        if (Input.GetKeyDown(KeyCode.F2)) ExitBuildMode(); 

        if (selectedElement != null) HandlePositionFortressElement();

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (selectedElement == null) HandleGetFortressElement(true);
            else PlaceElement();
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (selectedElement != null) DropElement();
        }

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
            // VRChat event called multiple times for 2 years now (...): https://vrchat.canny.io/udon/p/1275-inputuse-is-called-twice-per-mouse-click
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
            // VRChat event called multiple times for 2 years now (...): https://vrchat.canny.io/udon/p/1275-inputuse-is-called-twice-per-mouse-click
        }
    }
}
