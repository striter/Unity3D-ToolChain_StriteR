using System;
using UnityEngine;

namespace CameraController.Inputs.Touch
{
    public enum EConstrainMode
    {
        None,
        Clamp,
    }

    [Serializable]
    public struct TouchInputProcessorCore : IControllerInputProcessor
    {
        public bool Controllable => true;
        
        [Serializable]
        public struct FInputInitializer
        {
            public bool setOnEnter;
            public bool setOnReset;
            public float defaultValue;

            public bool Initialize(bool _isReset,out float _value)
            {
                _value = defaultValue;
                return _isReset ? setOnReset : setOnEnter;
            }

            public static readonly FInputInitializer kDefault = new FInputInitializer()
                { defaultValue = 0, setOnEnter = true, setOnReset = true };
        }
        
        [Header("Input")] 
        public FPlayerInputMultiplier inputMultiplier ;
        
        [Header("Initialize")]
        public EConstrainMode pitchMode;
        [Foldout(nameof(pitchMode),EConstrainMode.Clamp)] [MinMaxRange(-90,90)] public RangeFloat pitchClamp;
        public FInputInitializer pitchInitializer;
        public EConstrainMode yawMode;
        [Foldout(nameof(yawMode),EConstrainMode.Clamp)] [MinMaxRange(-360,360)] public RangeFloat yawClamp;
        public FInputInitializer yawInitializer;
        public bool initialYawWithAnchor;
        public EConstrainMode pinchMode;
        public FInputInitializer pinchInitializer;
        
        [Header("Constrains")]
        public static readonly TouchInputProcessorCore kZero = new TouchInputProcessorCore();
        public static readonly TouchInputProcessorCore kDefault = new TouchInputProcessorCore()
        {
            inputMultiplier = FPlayerInputMultiplier.kOne,
            pitchMode = EConstrainMode.Clamp,   pitchInitializer = FInputInitializer.kDefault,
            pitchClamp = RangeFloat.Minmax(-90f, 90f),yawInitializer = FInputInitializer.kDefault, initialYawWithAnchor = true,
            pinchMode = EConstrainMode.Clamp,   pinchInitializer = FInputInitializer.kDefault,
        };

        public float Evaluate(EConstrainMode _mode, float _val,RangeFloat _clamp)=> _mode switch {
            EConstrainMode.None => _val,
            EConstrainMode.Clamp => _clamp.Clamp(_val),
            _ => throw new ArgumentOutOfRangeException(nameof(_mode), _mode, null)
        };

        private float lastActiveYaw;
        void Initialize<T>(bool _isReset,ref T _input) where T : AControllerInput
        {
            if (_input is not IControllerPlayerTouchInput playerInput)
                return;

            var initialYaw = _input.Anchor.transform.eulerAngles.y;
            if (pitchInitializer.Initialize(_isReset, out var pitch))
                playerInput.Pitch = pitch;
            if (yawInitializer.Initialize(_isReset, out var yaw))
                playerInput.Yaw = initialYawWithAnchor ? initialYaw + yaw : yaw;
            else if (initialYawWithAnchor)  //it takes to keep yawing the same
            {
                var lastYawDelta = umath.deltaAngle(lastActiveYaw,_input.InputEuler.y );
                playerInput.Yaw = initialYaw + lastYawDelta;
            }
            if (pinchInitializer.Initialize(_isReset, out var pinch))
                playerInput.Pinch = pinch;
        }

        public void OnEnter<T>(ref T _input) where T : AControllerInput
        {
            lastActiveYaw = _input.Anchor.transform.eulerAngles.y;
            Initialize(false,ref _input);
        }
        public void OnReset<T>(ref T _input) where T : AControllerInput => Initialize(true,ref _input);
        public void OnTick<T>(float _deltaTime,ref T _input) where T : AControllerInput     //here handles the input
        {
            if (_input is not IControllerPlayerTouchInput touchInput)
                return;
            lastActiveYaw = _input.Anchor.transform.eulerAngles.y;
            
            var drag = touchInput.PlayerDrag;
            var pinch = touchInput.PlayerPinch;
            touchInput.PlayerDrag = 0;
            touchInput.PlayerPinch = 0;

            var multiplier = touchInput.Sensitive * inputMultiplier;
            touchInput.Pitch += drag.y * multiplier.kPitchMultiplier;
            touchInput.Yaw += drag.x * multiplier.kYawMultiplier;
            touchInput.Pinch += pinch * multiplier.kPinchMultiplier;
             
            touchInput.Pitch = Evaluate(pitchMode, touchInput.Pitch, pitchClamp);
            touchInput.Yaw = Evaluate(yawMode, touchInput.Yaw, yawClamp);
            touchInput.Pinch = Evaluate(pinchMode, touchInput.Pinch, RangeFloat.k01);
        }

        public void OnExit()
        {
            lastActiveYaw = 0f;
        }
    }

    [CreateAssetMenu(fileName = "InputProcessor", menuName = "Camera/InputProcessor/PlayerInputConstrains", order = 0)]
    public class FTouchInputProcessor : AControllerInputProcessor
    {
        public TouchInputProcessorCore data = TouchInputProcessorCore.kDefault;
        public override bool Controllable => data.Controllable;
        public override void OnEnter<T>(ref T _input)=> data.OnEnter(ref _input);
        public override void OnTick<T>(float _deltaTime,ref T _input)=> data.OnTick(_deltaTime,ref _input);
        public override void OnReset<T>(ref T _input)=> data.OnReset(ref _input);
        public override void OnExit()=> data.OnExit();
    }
}