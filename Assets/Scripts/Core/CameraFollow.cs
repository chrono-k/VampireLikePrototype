using UnityEngine;

/// <summary>
/// Keeps this transform aligned with a target in X/Y; Z stays fixed for 2D.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    private float fixedZ;

    void Awake()
    {
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 p = target.position;
        transform.position = new Vector3(p.x, p.y, fixedZ);
    }
}
