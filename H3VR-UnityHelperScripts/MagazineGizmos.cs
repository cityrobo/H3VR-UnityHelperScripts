using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagazineGizmos : MonoBehaviour {

    public float gizmoSize;

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(this.transform.position, gizmoSize);
    }
#else
    public void OnAwake()
    {
        Destroy(this);
    }
#endif
}
