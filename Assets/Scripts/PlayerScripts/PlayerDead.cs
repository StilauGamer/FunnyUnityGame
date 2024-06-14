using Mirror;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerDead : NetworkBehaviour
    {
        public ParticleSystem deathParticles;
        
        [SyncVar(hook = nameof(OnColorChanged))]
        internal Color Color;
        
        [ClientRpc]
        internal void RpcDie()
        {
            deathParticles.Play();
        }
        
        private void OnColorChanged(Color oldColor, Color _2)
        {
            if (oldColor == Color)
            {
                Debug.Log("The color is the same, not updating");
                return;
            }
            
            var playerRenderer = GetComponentInChildren<Renderer>();
            foreach (var material in playerRenderer.sharedMaterials)
            {
                Debug.Log("Material name: " + material.name);
                if (material.name != "Body")
                {
                    continue;
                }

                var newMaterialColor = Color;
                newMaterialColor.a = 1;
                
                material.color = newMaterialColor;
                Debug.Log("Material color changed to " + newMaterialColor);
                break;
            }
        }
    }
}
