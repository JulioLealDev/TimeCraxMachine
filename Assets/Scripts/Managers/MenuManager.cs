using UnityEngine;

namespace TimeCrax.Managers
{
    public class MenuManager : MonoBehaviour
    {
        public void EnablingMenuOptions()
        {
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child.CompareTag("Selectable"))
                {
                    var col = child.GetComponent<MeshCollider>();
                    if (col != null) col.enabled = true;
                }
            }
        }

        public void DesablingMenuOptions()
        {
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child.CompareTag("Selectable"))
                {
                    var col = child.GetComponent<MeshCollider>();
                    if (col != null) col.enabled = false;
                }
            }
        }
    }
}
