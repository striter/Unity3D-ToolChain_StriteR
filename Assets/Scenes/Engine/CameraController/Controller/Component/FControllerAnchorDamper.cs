using System;
using Runtime.CameraController.Inputs;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.CameraController.Component
{
    public enum EAnchorMode
    {
        Normal,
        ShockAbsorber,
        ShockAbsorber_VerticalSeperated,
    }
    [Serializable]
    public class FAnchorDamper
    {
        public EAnchorMode m_AnchorMode = EAnchorMode.Normal;
        public Damper m_OriginDamper = Damper.kDefault;
        [MFoldout(nameof(m_AnchorMode),EAnchorMode.ShockAbsorber,EAnchorMode.ShockAbsorber_VerticalSeperated)] public Damper m_OriginExtraDamper = Damper.kDefault;
        [MFoldout(nameof(m_AnchorMode),EAnchorMode.ShockAbsorber_VerticalSeperated)] public Damper m_OriginExtraDamper2 = Damper.kDefault;

        public void Initialize(AControllerInput _input,AnchoredControllerParameters parameters)
        {
            var origin = (float3)_input.Anchor.transform.position;
            var targetAnchor = parameters.anchor;
            switch (m_AnchorMode)
            {
                case EAnchorMode.Normal:
                {
                    m_OriginDamper.Initialize(targetAnchor);
                } 
                    break;
                case EAnchorMode.ShockAbsorber:
                {
                    m_OriginDamper.Initialize(origin);
                    m_OriginExtraDamper.Initialize(targetAnchor - origin);
                }
                    break;
                case EAnchorMode.ShockAbsorber_VerticalSeperated:
                {
                    m_OriginDamper.Initialize(origin.xz);
                    m_OriginExtraDamper.Initialize(targetAnchor - origin);
                    m_OriginExtraDamper2.Initialize(origin.y);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float3 Tick(float _deltaTime, AControllerInput _input, AnchoredControllerParameters parameters)
        {
            var origin = (float3)_input.Anchor.transform.position;
            var targetAnchor = parameters.anchor;
            float3 dampedAnchor;
            switch (m_AnchorMode)
            {
                case EAnchorMode.Normal:
                {
                    dampedAnchor = m_OriginDamper.Tick(_deltaTime, targetAnchor);
                }
                    break;
                case EAnchorMode.ShockAbsorber:
                {
                    dampedAnchor = m_OriginDamper.Tick(_deltaTime, origin);
                    dampedAnchor += m_OriginExtraDamper.Tick(_deltaTime, targetAnchor - origin);
                }
                    break;
                case EAnchorMode.ShockAbsorber_VerticalSeperated:
                {
                    dampedAnchor = m_OriginDamper.Tick(_deltaTime, origin.xz).to3xz();
                    dampedAnchor.y = m_OriginExtraDamper2.Tick(_deltaTime,origin.y);
                    dampedAnchor += m_OriginExtraDamper.Tick(_deltaTime, targetAnchor - origin);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return dampedAnchor;
        }

        public float3 DrawGizmos(AControllerInput _input,AnchoredControllerParameters parameters)
        {
            var origin = (float3)_input.Anchor.transform.position;
            var anchor = float3.zero;
            Gizmos.color = Color.green;
            switch (m_AnchorMode)
            {
                case EAnchorMode.Normal:
                {
                    anchor = parameters.anchor;
                }
                    break;
                case EAnchorMode.ShockAbsorber:
                case EAnchorMode.ShockAbsorber_VerticalSeperated:
                {
                    anchor = origin;
                    Gizmos.DrawWireSphere(anchor,.05f);
                    anchor += parameters.anchor - origin;
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return anchor;
        }
    }

}