using UnityEngine;

namespace ObjectPlacementVFX
{
    [ExecuteAlways]
    public class EnableDepthTexture : MonoBehaviour
    {
        //Should be placed onto the main Camera so that a Depth Texture is generated.
        //We will use this depth texture inside of our shader.

        private void OnEnable()
        {
            var cam = GetComponent<Camera>();

            cam.depthTextureMode = DepthTextureMode.Depth;
        }
    }
}

