using System;
using System.Collections;
using System.Collections.Generic;
using Rendering;
using UnityEngine;
using UnityEngine.UI;
    public class UIT_PostEffectAdjustConsole : MonoBehaviour
    {
        Transform m_Container;
        PostEffect_ColorGrading m_ColorGrading;
        PostEffect_Bloom m_Bloom;
        Toggle m_ColorGradingToggle;
        FormatValueSlider m_Weight;
        FormatValueSlider m_Brightness;
        FormatValueSlider m_Saturation;
        FormatValueSlider m_Contrast;
        ToggleGroupSingleSwitch m_ChannelGroup;
        FormatValueSlider m_MixRed;
        FormatValueSlider m_MixBlue;
        FormatValueSlider m_MixGreen;

        Toggle m_BloomOptimizedToggle;
        FormatValueSlider m_Threshold;
        FormatValueSlider m_Intensity;
        class FormatValueSlider
        {
            Text m_Text;
            Slider m_Slider;
            Func<float, float> NormalizeValue;
            Func<float, float> DenormalizeValue;
            public FormatValueSlider(Slider _slider, Func<float, float> _NomalizeValue = null, Func<float, float> _DenomalizeValue = null)
            {
                m_Slider = _slider;
                m_Text = m_Slider.transform.Find("Value").GetComponent<Text>();
                NormalizeValue = _NomalizeValue;
                DenormalizeValue = _DenomalizeValue;
            }

            public void SetValue(float srcValue)
            {
                m_Slider.value = NormalizeValue == null ? srcValue : NormalizeValue(srcValue);
                TickValue();
            }
            public float TickValue()
            {
                float targetValue = DenormalizeValue == null ? m_Slider.value : DenormalizeValue(m_Slider.value);
                m_Text.text = string.Format("{0:0.00}", targetValue);
                return targetValue;
            }
        }
        class ToggleGroupSingleSwitch
        {
            Dictionary<string, Toggle> m_Toggles = new Dictionary<string, Toggle>();
            public string m_ActiveTogName;
            Action<string> OnActiveChange;
            public ToggleGroupSingleSwitch(Transform transform, Action<string> OnActiveChange)
            {
                foreach (Toggle tog in transform.GetComponentsInChildren<Toggle>())
                {
                    m_Toggles.Add(tog.name, tog);
                    tog.onValueChanged.AddListener((bool value) => OnToggleValueChanged(tog.name, value));       //?
                }
                m_ActiveTogName = "";
                this.OnActiveChange = OnActiveChange;
            }

            public void SetToggleOn(string name)
            {
                if (m_ActiveTogName == name)
                    return;

                m_ActiveTogName = name;
                foreach (Toggle tog in m_Toggles.Values)
                    tog.isOn = tog.name == name;
                OnActiveChange(m_ActiveTogName);
            }

            void OnToggleValueChanged(string name, bool on)
            {
                if (!on)
                    return;

                SetToggleOn(name);
            }

        }
        public void Init(PostEffect_ColorGrading _colorGrading,PostEffect_Bloom _bloom)
        {
            m_ColorGrading = _colorGrading;
            m_Bloom = _bloom;

            m_Container = transform.Find("Container");
            m_Container.Find("Hide").GetComponent<Button>().onClick.AddListener(() => { m_Container.gameObject.SetActive(false); });
            m_Container.Find("Quit").GetComponent<Button>().onClick.AddListener(() => { Destroy(this.gameObject); });

            m_ColorGradingToggle = m_Container.Find("ColorGrading").GetComponent<Toggle>();

            Func<float, float> BSCValueNormalize = (float srcValue) => srcValue / 2f;
            Func<float, float> BSCValueDenormalize = (float normalizedValue) => normalizedValue * 2f;

            m_Weight = new FormatValueSlider(m_Container.Find("Weight").GetComponent<Slider>());
            m_Brightness = new FormatValueSlider(m_Container.Find("Brightness").GetComponent<Slider>(), BSCValueNormalize, BSCValueDenormalize);
            m_Saturation = new FormatValueSlider(m_Container.Find("Saturation").GetComponent<Slider>(), BSCValueNormalize, BSCValueDenormalize);
            m_Contrast = new FormatValueSlider(m_Container.Find("Contrast").GetComponent<Slider>(), BSCValueNormalize, BSCValueDenormalize);
            m_ChannelGroup = new ToggleGroupSingleSwitch(m_Container.Find("ChannelGroup"), OutputChannelChange);
            m_Container.Find("ResetChannels").GetComponent<Button>().onClick.AddListener(OnOutputChannelReset);
            Func<float, float> ChannelMixValueNormalize = (float srcValue) => (srcValue + 2f) / 4f;
            Func<float, float> ChannelMixValueDenormalize = (float normalizedValue) => normalizedValue * 4f - 2f;
            m_MixRed = new FormatValueSlider(m_Container.Find("MixRed").GetComponent<Slider>(), ChannelMixValueNormalize, ChannelMixValueDenormalize);
            m_MixBlue = new FormatValueSlider(m_Container.Find("MixBlue").GetComponent<Slider>(), ChannelMixValueNormalize, ChannelMixValueDenormalize);
            m_MixGreen = new FormatValueSlider(m_Container.Find("MixGreen").GetComponent<Slider>(), ChannelMixValueNormalize, ChannelMixValueDenormalize);


            m_BloomOptimizedToggle = m_Container.Find("Bloom").GetComponent<Toggle>();
            m_Threshold = new FormatValueSlider(m_Container.Find("Threshold").GetComponent<Slider>());
            m_Intensity = new FormatValueSlider(m_Container.Find("Intensity").GetComponent<Slider>(), (float srcValue) => srcValue / 2.5f, (float normalizedValue) => normalizedValue * 2.5f);

            m_ColorGradingToggle.isOn = m_ColorGrading.enabled;
            m_Weight.SetValue(m_ColorGrading.m_Params.m_Weight);
            m_Brightness.SetValue(m_ColorGrading.m_Params.m_brightness);
            m_Saturation.SetValue(m_ColorGrading.m_Params.m_saturation);
            m_Contrast.SetValue(m_ColorGrading.m_Params.m_contrast);
            m_ChannelGroup.SetToggleOn(ImageEffect_ColorGrading.enum_MixChannel.Red.ToString());

            m_BloomOptimizedToggle.isOn = m_Bloom.enabled;
            m_Threshold.SetValue(m_Bloom.m_BloomParams.threshold / 1.5f);
            m_Intensity.SetValue(m_Bloom.m_BloomParams.intensity / 2.5f);
        }

        private void Update()
        {
            if (!m_Container.gameObject.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.BackQuote) || Input.touches.Length >= 4)
                    m_Container.gameObject.SetActive(true);
                return;
            }

            m_ColorGrading.enabled = m_ColorGradingToggle.isOn;
            m_ColorGrading.m_Params.m_Weight = m_Weight.TickValue();
            m_ColorGrading.m_Params.m_brightness = m_Brightness.TickValue();
            m_ColorGrading.m_Params.m_saturation = m_Saturation.TickValue();
            m_ColorGrading.m_Params.m_contrast = m_Contrast.TickValue();
            Vector3 channelMixVector = new Vector3(m_MixRed.TickValue(), m_MixGreen.TickValue(), m_MixBlue.TickValue());
            switch ((ImageEffect_ColorGrading.enum_MixChannel)Enum.Parse(typeof(ImageEffect_ColorGrading.enum_MixChannel), m_ChannelGroup.m_ActiveTogName))
            {
                case ImageEffect_ColorGrading.enum_MixChannel.Red: m_ColorGrading.m_Params.m_MixRed = channelMixVector; break;
                case ImageEffect_ColorGrading.enum_MixChannel.Green: m_ColorGrading.m_Params.m_MixGreen = channelMixVector; break;
                case ImageEffect_ColorGrading.enum_MixChannel.Blue: m_ColorGrading.m_Params.m_MixBlue = channelMixVector; break;
            }

            m_Bloom.enabled = m_BloomOptimizedToggle.isOn;
            m_Bloom.m_BloomParams.threshold = m_Threshold.TickValue();
            m_Bloom.m_BloomParams.intensity = m_Intensity.TickValue();

            m_ColorGrading.OnValidate();
            m_Bloom.OnValidate();
        }

        void OutputChannelChange(string name)
        {
            Vector3 targetVector = Vector3.zero;
            switch ((ImageEffect_ColorGrading.enum_MixChannel)Enum.Parse(typeof(ImageEffect_ColorGrading.enum_MixChannel), m_ChannelGroup.m_ActiveTogName))
            {
                case ImageEffect_ColorGrading.enum_MixChannel.Red: targetVector = m_ColorGrading.m_Params.m_MixRed; break;
                case ImageEffect_ColorGrading.enum_MixChannel.Green: targetVector = m_ColorGrading.m_Params.m_MixGreen; break;
                case ImageEffect_ColorGrading.enum_MixChannel.Blue: targetVector = m_ColorGrading.m_Params.m_MixBlue; break;
            }
            m_MixRed.SetValue(targetVector.x);
            m_MixGreen.SetValue(targetVector.y);
            m_MixBlue.SetValue(targetVector.z);
        }

        void OnOutputChannelReset()
        {
            m_ColorGrading.m_Params.m_MixRed = Vector3.zero;
            m_ColorGrading.m_Params.m_MixGreen = Vector3.zero;
            m_ColorGrading.m_Params.m_MixBlue = Vector3.zero;
            OutputChannelChange(m_ChannelGroup.m_ActiveTogName);
        }
    }

