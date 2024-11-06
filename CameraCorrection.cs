using UnityEngine;

namespace _Development.Scripts.SkinSelectionMenu
{
    public class CameraCorrection : MonoBehaviour
    {
        private const float OffsetCamera = 0.001375f;
        
        public UnityEngine.Camera CameraView;

        private void Update() => 
            CameraView.orthographicSize = OffsetCamera * Screen.height;
    }
}