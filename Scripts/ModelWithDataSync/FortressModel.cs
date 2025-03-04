﻿using System.Xml.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace iffnsStuff.iffnsVRCStuff.FortressBuilder
{
    public class FortressModel : UdonSharpBehaviour
    {
        [SerializeField] FortressViewPlacingModels linkedViewPlacingModels;

        [UdonSynced] public int[] elementTypes = new int[0];
        [UdonSynced] public int[] xPos = new int[0];
        [UdonSynced] public int[] yPos = new int[0];
        [UdonSynced] public int[] zPos = new int[0];

        public int[] ElementTypes { get { return elementTypes; } }
        public int[] XPos { get { return xPos; } }
        public int[] YPos { get { return yPos; } }
        public int[] ZPos { get { return zPos; } }

        int[] AddElementToArray(int[] baseArray, int number)
        {
            int[] returnArray = new int[baseArray.Length + 1];

            for(int i = 0; i < baseArray.Length; i++)
            {
                returnArray[i] = baseArray[i];
            }

            returnArray[returnArray.Length - 1] = number;

            return returnArray;
        }

        int[] RemoveElementFromArray(int[] baseArray, int indexToBeRemoved)
        {
            int[] returnArray = new int[baseArray.Length - 1];

            int grabIndex = 0;

            for (int i = 0; i < baseArray.Length; i++)
            {
                if (i == indexToBeRemoved) continue;

                returnArray[grabIndex++] = baseArray[i];
            }

            return returnArray;
        }

        public void AddElement(int element, int x, int y, int z)
        {
            elementTypes = AddElementToArray(elementTypes, element);
            xPos = AddElementToArray(xPos, x);
            yPos = AddElementToArray(yPos, y);
            zPos = AddElementToArray(zPos, z);
        }

        public void RemoveElement(int index)
        {
            elementTypes = RemoveElementFromArray(elementTypes, index);
            xPos = RemoveElementFromArray(xPos, index);
            yPos = RemoveElementFromArray(yPos, index);
            zPos = RemoveElementFromArray(zPos, index);
        }

        public void Sync()
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
            RequestSerialization();
            //Debug.Log("Request serialization");
        }

        public override void OnPreSerialization()
        {
            base.OnPreSerialization();
            //Debug.Log($"Serializing with {elementTypes.Length}");
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            base.OnPostSerialization(result);
            //Debug.Log($"Done serializing with {result.success}");
        }

        public override void OnDeserialization()
        {
            base.OnDeserialization();

            //Debug.Log($"Update model with length {elementTypes.Length}");

            linkedViewPlacingModels.RefreshEverything(this);
        }
    }
}