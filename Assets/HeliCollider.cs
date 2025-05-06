using UnityEngine;

public class HeliCollider : MonoBehaviour
{
    public bool touching;
    public GameObject touchingObj;
    public string touchingObjName;
    public bool landed;
    public bool crashed;
    private GameObject collisionEnteredGameOBJ;
    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.name != "Citizen (Clone)")
        {
            touching = true;
            landed = true;
        }
        if(collision.gameObject == collisionEnteredGameOBJ)
        {
            crashed = false;
        }

        touchingObj = collision.gameObject;
        touchingObjName = collision.gameObject.name;
    }
    public void OnCollisionEnter(Collision collision)
    {
        collisionEnteredGameOBJ = collision.gameObject;
        crashed = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        collisionEnteredGameOBJ = null;
        crashed = false;
        landed = false;
        touching = false;
        touchingObj = null;
        touchingObjName=null;
    }
}
