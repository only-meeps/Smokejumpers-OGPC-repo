using UnityEngine;

public class HeliCollider : MonoBehaviour
{
    public bool touching;
    private void OnCollisionStay(Collision collision)
    {
        touching = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        touching = false;
    }
}
