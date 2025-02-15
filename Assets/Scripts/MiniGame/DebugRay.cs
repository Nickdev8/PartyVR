using UnityEngine;

[ExecuteInEditMode]
public class DebugRay : MonoBehaviour
{
    public bool showRay = true;
    public bool cornerPoint;
    public bool spawnPoint;
    // Color codes (in hex):
    // 0D47FF blue, FF0C00 red, 6BF71D green

    private void OnDrawGizmos()
    {
        if (showRay)
        {
            if (spawnPoint)
            {
                // Blue ray (0D47FF) in the object's forward direction
                Gizmos.color = new Color32(13, 71, 255, 255);
                Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.forward * 0.2f));

                // Red ray (FF0C00) in the object's right direction
                Gizmos.color = new Color32(255, 12, 0, 255);
                Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.right * 0.2f));

                // Green ray (6BF71D) in the object's up direction
                Gizmos.color = new Color32(107, 247, 29, 255);
                Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.up * 0.2f));
            }

            if (cornerPoint)
            {
                Gizmos.color = new Color32(255, 255, 255, 255);
                // For the offsets, also apply the rotation so that the corner positions are rotated.
                Gizmos.DrawRay(
                    transform.position - transform.TransformDirection(Vector3.left * 0.1f),
                    transform.TransformDirection(Vector3.left * 0.2f)
                );
                Gizmos.DrawRay(
                    transform.position - transform.TransformDirection(Vector3.up * 0.1f),
                    transform.TransformDirection(Vector3.up * 0.2f)
                );
                Gizmos.DrawRay(
                    transform.position - transform.TransformDirection(Vector3.forward * 0.1f),
                    transform.TransformDirection(Vector3.forward * 0.2f)
                );
            }
        }
    }
}