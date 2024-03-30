using UnityEngine;

namespace TPSgameAssets.Scripts.Player
{
    public class SpellCaster : MonoBehaviour
    {
        public Transform targetPoint;
        public Camera cameraLink;
        
        void Update()
        {
            var ray = cameraLink.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 700))
            {
                targetPoint.position = hit.point;
            }
            transform.LookAt(targetPoint);
        }
    }
}
