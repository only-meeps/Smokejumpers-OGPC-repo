using UnityEngine;

public class HeliCollider : MonoBehaviour
{
    public bool touching;
    public GameObject touchingObj;
    public string touchingObjName;
    private void OnCollisionStay(Collision collision)
    {
        touching = true;
        touchingObj = collision.gameObject;
        touchingObjName = collision.gameObject.name;
    }
    private void OnCollisionExit(Collision collision)
    {
        touching = false;
        touchingObj = null;
        touchingObjName=null;
    }
}
