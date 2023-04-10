using UnityEngine;

namespace MS.Player
{
    public class PlayerGFX : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        public void SetMaterial(Material material)
        {
            meshRenderer.sharedMaterial = material;
        }
    }
}
