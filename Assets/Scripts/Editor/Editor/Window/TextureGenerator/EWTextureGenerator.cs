using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityEditor.Extensions
{
    public enum EResolution
    {
        _1 = 1,
        _2 = 2,
        _4 = 4,
        _8 = 8,
        _16 = 16,
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
    }

    public enum ETextureGenerateMode
    {
        Gradient,
        Noise,
        Material,
    }

    [Serializable]
    public struct TextureGeneratorData
    {
        public EResolution resolutionX;
        public EResolution resolutionY;
        public FilterMode filterMode;
        public static readonly TextureGeneratorData kDefault = new() {
            resolutionX = EResolution._256, resolutionY = EResolution._256, filterMode = FilterMode.Point
        };
        public Color[] Colors(Color[] _reference)
        {
            var length = (int)resolutionX * (int)resolutionY;
            if (_reference == null || _reference.Length != length)
                _reference = new Color[length];
            return _reference;
        }
        public RenderTexture RenderTexture(RenderTexture _texture)
        {
            if (_texture != null && (_texture.width != (int)resolutionX || _texture.height != (int)resolutionY))
            {
                UnityEngine.RenderTexture.ReleaseTemporary(_texture);
                _texture = null;
            }

            if (_texture == null)
                _texture = UnityEngine.RenderTexture.GetTemporary((int)resolutionX, (int)resolutionY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            _texture.filterMode = filterMode;
            return _texture;
        }
        public Texture2D Texture2D(Texture2D _texture)
        {
            if (_texture != null && (_texture.width != (int)resolutionX || _texture.height != (int)resolutionY))
            {
                GameObject.DestroyImmediate(_texture);
                _texture = null;
            }

            if (_texture == null)
                _texture = new Texture2D((int)resolutionX,(int)resolutionY,TextureFormat.ARGB32,false);

            _texture.filterMode = filterMode;
            return _texture;
        }
    }
    
    public interface ITextureGenerator
    {
        bool Valid { get; }
        void Setup(TextureGeneratorData _data);
        void Preview(Rect _rect);
        void Output();
        void Dispose();
    }

    
    [Serializable]
    public struct FTextureGenerateData
    {
        public TextureGeneratorData config;
        public ETextureGenerateMode mode;

        [Foldout(nameof(mode), ETextureGenerateMode.Gradient)] public FGradientTextureGenerator gradientGenerator;
        [Foldout(nameof(mode),ETextureGenerateMode.Material)] public FTextureGeneratorMaterial materialGenerator;
        [Foldout(nameof(mode), ETextureGenerateMode.Noise)] public FNoiseTextureGenerator noiseGenerator;
        public ITextureGenerator Generator => mode switch {
            ETextureGenerateMode.Material => materialGenerator,
            ETextureGenerateMode.Noise => noiseGenerator,
            ETextureGenerateMode.Gradient => gradientGenerator,
            _ => null
        };

        public static readonly FTextureGenerateData kDefault = new() {
            config = TextureGeneratorData.kDefault,
            materialGenerator = FTextureGeneratorMaterial.kDefault,
            noiseGenerator = FNoiseTextureGenerator.kDefault
        };
    }
    
    public class EWTextureGenerator : EditorWindow
    {
        [SerializeField] private FTextureGenerateData m_Data = FTextureGenerateData.kDefault;
        private SerializedProperty m_DataProperty;
        private SerializedObject m_SerializedWindow;
        private const float kTextureCollapse = 6f;
        private const float kTexturePadding = 10f;
        private ITextureGenerator m_Generator;
        private void OnEnable()
        {
            m_SerializedWindow = new SerializedObject(this);
            m_DataProperty = m_SerializedWindow.FindProperty(nameof(m_Data));
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            m_SerializedWindow.Dispose();
            m_SerializedWindow = null;
            if(m_Generator != null)
                m_Generator.Dispose();
            m_Generator = null;
        }

        private void OnUndoRedo()
        {
            if (m_SerializedWindow != null)
            {
                m_SerializedWindow.Update();
                Repaint();
            }
        }
        
        private void OnGUI()
        {
            EditorGUILayout.PropertyField(m_DataProperty);
            var propertyHeight = EditorGUI.GetPropertyHeight(m_DataProperty, true);
            HorizontalScope.Begin(5,5,propertyHeight,Screen.width);
            if (EditorGUI.EndChangeCheck())
            {
                m_SerializedWindow.ApplyModifiedProperties();
                Undo.RecordObject(this, "Texture Generator");                
            }

            var generator = m_Data.Generator;
            if (m_Generator != generator)
            {
                m_Generator?.Dispose();
                m_Generator = generator;
            }
            if (generator is not { Valid: true })
                return;
            
            generator.Setup(m_Data.config);
            var aspect = (float)m_Data.config.resolutionY / (float)m_Data.config.resolutionX;
            var width = math.min(position.width,(position.height - propertyHeight - 20 - 20) / aspect) - kTexturePadding;
            HorizontalScope.NextLine(0f,20);
            var previewRect = HorizontalScope.NextRect(0,width);
            GUI.Label(previewRect,"Texture Preview",UEGUIStyle_Window.m_TitleLabel);
            HorizontalScope.NextLine(0f,aspect * width);
            
            var textureRect = HorizontalScope.NextRect((position.width - width - kTexturePadding) / 2,0);
            textureRect = HorizontalScope.NextRect(0, width);
            GUI.DrawTexture(textureRect,Texture2D.whiteTexture);
            textureRect = textureRect.Collapse(Vector2.one * kTextureCollapse,Vector2.one * .5f);
            generator.Preview(textureRect);
            HorizontalScope.NextLine(2, 20);
            if (GUI.Button(HorizontalScope.NextRect(0, 80), "Export"))
                generator.Output();
        }
    }
}