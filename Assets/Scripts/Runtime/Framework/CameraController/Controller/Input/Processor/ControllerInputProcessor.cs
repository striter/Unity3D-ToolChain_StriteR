using System;
using UnityEngine;


namespace CameraController.Inputs
{
    public enum EConstrainMode
    {
        None,
        Clamp,
    }

    [Serializable]
    public struct ControllerInputProcessorCore : IControllerInputProcessor
    {
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
        [MFoldout(nameof(pitchMode),EConstrainMode.Clamp)] [MinMaxRange(-90,90)] public RangeFloat pitchClamp;
        public FInputInitializer pitchInitializer;
        public EConstrainMode yawMode;
        [MFoldout(nameof(yawMode),EConstrainMode.Clamp)] [MinMaxRange(-360,360)] public RangeFloat yawClamp;
        public FInputInitializer yawInitializer;
        public bool initialYawWithAnchor;
        public EConstrainMode pinchMode;
        public FInputInitializer pinchInitializer;
        
        [Header("Constrains")]
        public static readonly ControllerInputProcessorCore kZero = new ControllerInputProcessorCore();
        public static readonly ControllerInputProcessorCore kDefault = new ControllerInputProcessorCore()
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

        void Initialize<T>(bool _isReset,ref T _input) where T : AControllerInput
        {
            if (pitchInitializer.Initialize(_isReset, out var pitch))
                _input.Pitch = pitch;
            if (yawInitializer.Initialize(_isReset, out var yaw))
            {
                if(initialYawWithAnchor && !_input.Target)
                    yaw += _input.Anchor.eulerAngles.y;
                _input.Yaw = yaw;
            }
            if (pinchInitializer.Initialize(_isReset, out var pinch))
                _input.Pinch = pinch;
        }

        public void OnEnter<T>(ref T _input) where T : AControllerInput
        {
            Initialize(false,ref _input);            
            _input.ClearOffset();
        }
        
        public void OnTick<T>(float _deltaTime,ref T _input) where T : AControllerInput
        {
            if (_input is not IControllerMobileInput playerInput)
                return;
            
            var drag = playerInput.PlayerDrag;
            var pinch = playerInput.PlayerPinch;
            playerInput.PlayerDrag = 0;
            playerInput.PlayerPinch = 0;

            var multiplier = playerInput.Sensitive * inputMultiplier;
            _input.Pitch += drag.y * multiplier.kPitchMultiplier;
            _input.Yaw += drag.x * multiplier.kYawMultiplier;
            _input.Pinch += pinch * multiplier.kPinchMultiplier;
             
            _input.Pitch = Evaluate(pitchMode, _input.Pitch, pitchClamp);
            _input.Yaw = Evaluate(yawMode, _input.Yaw, yawClamp);
            _input.Pinch = Evaluate(pinchMode, _input.Pinch, RangeFloat.k01);
        }

        public void OnReset<T>(ref T _input) where T : AControllerInput => Initialize(true,ref _input);       
        public void OnExit<T>(ref T _input) where T : AControllerInput => _input.ClearOffset();
    }

    [CreateAssetMenu(fileName = "InputProcessor", menuName = "Camera2/InputProcessor/InputProcessor", order = 0)]
    public class ControllerInputProcessor : AControllerInputProcessor
    {
        public ControllerInputProcessorCore data = ControllerInputProcessorCore.kDefault;
        public override void OnEnter<T>(ref T _input)=> data.OnEnter(ref _input);
        public override void OnTick<T>(float _deltaTime,ref T _input)=> data.OnTick(_deltaTime,ref _input);
        public override void OnReset<T>(ref T _input)=> data.OnReset(ref _input);
        public override void OnExit<T>(ref T _input)=> data.OnExit(ref _input);
    }
}