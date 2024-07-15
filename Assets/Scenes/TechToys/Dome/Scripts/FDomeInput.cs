using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.TouchTracker;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dome
{
    public enum EInputState
    {
        Empty,
        Down,
        Press,
        Up,
    }
    [Serializable]
    public struct FDomeEntityInput
    {
        public float2 move;
        public EInputState primary;
        public static readonly FDomeEntityInput kDefault = new FDomeEntityInput();
    }
    
    [Serializable]
    public struct FDomePlayerInputs
    {
        public FDomeEntityInput entityInput;
        public float2 rotate;
        public float zoom;

        public EInputState secondary;
        public EInputState leftAlt;
        public float2 hoverPosition;
        public bool changeClicked;
    }
    
    public class FDomeInput : ADomeController
    {
        [Readonly] public FDomePlayerInputs playerInputs;
        public override void OnInitialized()
        {
        }

        EInputState GetState(KeyCode _code)
        {
            if (Input.GetKeyDown(_code))
                return EInputState.Down;
            if (Input.GetKeyUp(_code))
                return EInputState.Up;
            if (Input.GetKey(_code))
                return EInputState.Press;
            return EInputState.Empty;
        }
        
        public override void Tick(float _deltaTime)
        {
            playerInputs = new FDomePlayerInputs()  //Resolve mobile commands in the future,if theres one lel
            {
                entityInput = new FDomeEntityInput()
                {
                    move= new float2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical")),
                    primary = GetState(KeyCode.Mouse0),
                },
                rotate = new float2(Input.GetAxis("Mouse X"),-Input.GetAxis("Mouse Y")),
                zoom = Input.GetAxis("Mouse ScrollWheel"),
                hoverPosition = Input.mousePosition.XY(),
                
                leftAlt = GetState(KeyCode.LeftAlt),
                secondary = GetState(KeyCode.Mouse1),
                changeClicked = Input.GetKeyDown(KeyCode.Tab),
            };
        }
        
        public override void Dispose()
        {
        }
    }

    public static class FDomeInput_Extension
    {
        public static bool Down(this EInputState _state) => _state == EInputState.Down;
        public static bool Press(this EInputState _state) => _state == EInputState.Press;
        public static bool Up(this EInputState _state) => _state == EInputState.Up;
    }

}