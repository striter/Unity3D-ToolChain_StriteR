using System;
using Unity.Mathematics;

namespace CameraController.Component
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
        [MFoldout(nameof(m_RotationMode),ERotationMode.EulerInputSeperated)]public Damper m_PlayerInputDamper = new Damper();
        public Damper m_RotationDamper = new Damper();
        
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
                    m_RotationDamper.Initialize(baseParameters.euler);
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
                ERotationMode.EulerInputSeperated => m_RotationDamper.Tick(_deltaTime, baseParameters.euler)  + m_PlayerInputDamper.Tick(_deltaTime, _input.euler),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}