using UnityEngine;

namespace FynnsToolkit
{
    public class Texture2NormalmapPreset : ScriptableObject
    {
        public float heightScale = 1.0f;
        public int selectedChannel = 0;
        public bool invertX = false;
        public bool invertY = false;
        public int conversionAlgorithm = 0;
        public float sobelInnerWeight = 2f;
        public float sobelOuterWeight = 1f;
        public float prewittWeight = 1f;
        public float scharrLowWeight = 3f;
        public float scharrHighWeight = 10f;
        public float laplacianCenterWeight = 4f;
        public float laplacianNeighborWeight = 1f;
        public float robertsScale = 1f;
        public float kirschScale = 1f;
        public float freiChenScale = 1f;
        public float multiScaleWeight1 = 1.0f;
        public float multiScaleWeight2 = 0.5f;
        public float contrastEnhancementFactor = 1.0f;
        public float cannyLowThreshold = 0.1f;
        public float cannyHighThreshold = 0.3f;
        public float bilateralSigmaSpatial = 2.0f;
        public float bilateralSigmaRange = 0.1f;
        public float nlmH = 0.1f;
        public bool applyPreFilter = false;
        public int preFilterKernelSize = 5;
        public float preFilterSigma = 1.0f;
    }
}
