using System;
using Unity.Mathematics;

namespace CameraController.Component
{
    public enum ERotationMode
    {
        Euler,
        Quaternion,
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
                case ERotationMode.Quaternion:
                {
                    m_RotationDamper.Initialize(quaternion.Euler((_input.euler + baseParameters.euler) * kmath.kDeg2Rad));
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

        public quaternion Tick(float _deltaTime, AnchoredControllerParameters _input, AnchoredControllerParameters baseParameters)
        {
            return m_RotationMode switch
            {
                ERotationMode.Euler => quaternion.Euler(m_RotationDamper.Tick(_deltaTime, _input.euler + baseParameters.euler) * kmath.kDeg2Rad),
                ERotationMode.Quaternion => m_RotationDamper.Tick(_deltaTime, quaternion.Euler((_input.euler + baseParameters.euler) * kmath.kDeg2Rad)),
                ERotationMode.EulerInputSeperated => quaternion.Euler(m_RotationDamper.Tick(_deltaTime, baseParameters.euler) * kmath.kDeg2Rad +
                                                                       m_PlayerInputDamper.Tick(_deltaTime, _input.euler) * kmath.kDeg2Rad),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}