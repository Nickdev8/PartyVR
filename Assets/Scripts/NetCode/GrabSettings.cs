using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabSettings : MonoBehaviour
{
     public NetcodeSendTransform netcodeSendTransform;
     public bool sendPosition = true;
     public bool sendRotation = true;
     public bool sendScale = false;
     public bool useGravity = true;
     public int damageOnHit = 0;
     public ulong lastOwnerId;

     public void Awake()
     {
          netcodeSendTransform.sendPosition = sendPosition;
          netcodeSendTransform.sendRotation = sendRotation;
          netcodeSendTransform.sendScale = sendScale;
          netcodeSendTransform.useGravity = useGravity;
     }
}
