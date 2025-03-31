using UnityEngine;

public class HeliCollider : MonoBehaviour
{
    public bool touching;
    public GameObject touchingObj;
    private void OnCollisionStay(Collision collision)
    {
        touching = true;
        touchingObj = collision.gameObject;
    }
    private void OnCollisionExit(Collision collision)
    {
        touching = false;
        touchingObj = null;
    }
}
