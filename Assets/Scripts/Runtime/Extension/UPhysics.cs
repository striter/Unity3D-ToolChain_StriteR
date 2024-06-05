using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class UPhysics
{
    #region Physics Cast
    public static RaycastHit[] BoxCastAll(Vector3 position, Vector3 forward, Vector3 up, Vector3 boxBounds, int layerMask = -1)
    {
        float castBoxLength = .1f;
        return Physics.BoxCastAll(position + forward * castBoxLength / 2f, new Vector3(boxBounds.x / 2, boxBounds.y / 2, castBoxLength / 2), forward, Quaternion.LookRotation(forward, up), boxBounds.z - castBoxLength, layerMask);
    }

    public static RaycastHit[] TrapeziumCastAll(Vector3 position, Vector3 forward, Vector3 up, Vector4 trapeziumInfo, int layerMask = -1, int castCount = 8)
    {
        List<RaycastHit> hitsList = new List<RaycastHit>();
        float castLength = trapeziumInfo.z / castCount;
        for (int i = 0; i < castCount; i++)
        {
            Vector3 boxPos = position + forward * castLength * i;
            Vector3 boxInfo = new Vector3(trapeziumInfo.x + (trapeziumInfo.w - trapeziumInfo.x) * i / castCount, trapeziumInfo.y, castLength);
            RaycastHit[] hits = BoxCastAll(boxPos, forward, up, boxInfo, layerMask);
            for (int j = 0; j < hits.Length; j++)
            {
                bool b_continue = false;

                for (int k = 0; k < hitsList.Count; k++)
                    if (hitsList[k].collider == hits[j].collider)
                        b_continue = true;

                if (b_continue)
                    continue;

                hitsList.Add(hits[j]);
            }
        }
        return hitsList.ToArray();
    }
    #endregion
}
