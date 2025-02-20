using System;
using Unity.Mathematics;

namespace Runtime.CameraController.Component
{
    public enum ERotationMode
    {
        Euler,
        EulerRepeated,
        EulerInputSeperated,
    }

    [Serializable]
    public class FRotationDamper
    {
        public ERotationMode m_RotationMode = ERotationMode.Euler;
        [Foldout(nameof(m_RotationMode),ERotationMode.EulerInputSeperated)]public Damper m_PlayerInputDamper = Damper.kDefault;
        public Damper m_RotationDamper = Damper.kDefault;
        
        public void Initialize(AnchoredControllerParameters _input,AnchoredControllerParameters baseParameters)
        {
            switch (m_RotationMode)
            {
                case ERotationMode.Euler:
                {
                    m_RotationDamper.Initialize(_input.euler + baseParameters.euler);
                }
                    break;
                case ERotationMode.EulerRepeated:
                {
                    m_RotationDamper.InitializeAngle(_input.euler + baseParameters.euler);
                }
                    break;
                case ERotationMode.EulerInputSeperated:
                {
                    m_RotationDamper.InitializeAngle(baseParameters.euler);
                    m_PlayerInputDamper.Initialize(_input.euler);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float3 Tick(float _deltaTime, AnchoredControllerParameters _input, AnchoredControllerParameters baseParameters)
        {
            
            return m_RotationMode switch
            {
                ERotationMode.Euler => m_RotationDamper.Tick(_deltaTime, _input.euler + baseParameters.euler),
                ERotationMode.EulerRepeated => m_RotationDamper.TickAngle(_deltaTime,_input.euler + baseParameters.euler),
                ERotationMode.EulerInputSeperated => m_RotationDamper.TickAngle(_deltaTime, baseParameters.euler)  + m_PlayerInputDamper.Tick(_deltaTime, _input.euler),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}