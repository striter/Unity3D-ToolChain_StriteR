using System;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.CharacterControl
{
    [Serializable]
    public struct FCharacterControlInput
    {
        public float2 move;
        public bool sprint;
        public bool aim;
        public float2 cameraMove;
        public float cameraZoom;
    }
    
    public class CharacterControl_Input : SingletonMono<CharacterControl_Input> , ICharacterControlMgr
    {
        [field : SerializeField] public FCharacterControlInput m_Input { get; private set; }

        public void ClearInput()
        {
            m_Input = new FCharacterControlInput()
            {
                cameraMove = m_Input.cameraMove,
                cameraZoom = m_Input.cameraZoom,
            };
        }
        public void Initialize()
        {
            
        }

        public void Dispose()
        {
        }

        public void Tick(float _deltaTime)
        {
            m_Input = new FCharacterControlInput
            {
                move = new float2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).safeNormalize(),
                sprint = Input.GetKey(KeyCode.LeftShift),
                aim = Input.GetMouseButton(1),
                cameraMove = new float2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")),
                cameraZoom = Input.GetAxis("Mouse ScrollWheel"),
            };
        }

        public void LateTick(float _deltaTime)
        {
        }
    }
}