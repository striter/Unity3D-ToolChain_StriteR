using System.Linq.Extensions;
using Runtime.CameraController.Inputs;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.CameraController
{
    [CreateAssetMenu(fileName = "AnchoredController", menuName = "Camera/Controller/Simple")]
    public class FAnchoredControllerPinch : AAnchoredController
    {
        public AnchoredControllerInput[] m_PinchParameters = new AnchoredControllerInput[] { AnchoredControllerInput.kDefault };
        protected override AnchoredControllerParameters EvaluateBaseParameters(AControllerInput _input)
        {
            var gradient = m_PinchParameters.Gradient(_input.InputPinch * (m_PinchParameters.Length - 1));
            return AnchoredControllerParameters.Lerp(gradient.start.Evaluate(_input.Anchor), gradient.end.Evaluate(_input.Anchor), gradient.value) ;
        }

    }
}