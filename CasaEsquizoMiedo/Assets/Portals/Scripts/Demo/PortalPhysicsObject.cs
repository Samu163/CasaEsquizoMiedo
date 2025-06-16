using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PortalPhysicsObject : PortalTraveller {

    public float force = 10;
    new Rigidbody rigidbody;
    public Color[] colors;
    static int i;

    void Awake () {
        rigidbody = GetComponent<Rigidbody> ();
        graphicsObject.GetComponent<MeshRenderer> ().material.color = colors[i];
        i++;
        if (i > colors.Length - 1) {
            i = 0;
        }
    }

    public override void Teleport (Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot) {
        base.Teleport (fromPortal, toPortal, pos, rot);
        rigidbody.linearVelocity = toPortal.TransformVector (fromPortal.InverseTransformVector (rigidbody.linearVelocity));
        rigidbody.angularVelocity = toPortal.TransformVector (fromPortal.InverseTransformVector (rigidbody.angularVelocity));
    }
}