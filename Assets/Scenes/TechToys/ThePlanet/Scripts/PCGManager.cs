using System;
using System.Collections;
using System.Linq;
using System.Linq.Extensions;
using Runtime.TouchTracker;
using UnityEngine;
using TDataPersistent;
using TechToys.ThePlanet.Module;
using TechToys.ThePlanet.Simplex;
using UnityEngine.UI;

namespace TechToys.ThePlanet
{
    interface IPCGControl
    {
        void Init();
        void Tick(float _deltaTime);
        void Clear();
        void Dispose();
    }

    public class PCGManager : MonoBehaviour
    {
        public GridCollection m_GridData;
        public ModuleCollection m_ModuleData;
        public SimplexCollection m_SimplexData;

        private GridManager m_Grid;
        private ModuleManager m_Module;
        private SimplexManager m_Simplex;
        private PCGCamera m_Camera;
        private PCGEnvironment m_Environment;
        private PCGAudios m_Audio;
        private IPCGControl[] m_Controls;

        //Executions
        private Text m_Spawning;
        private string[] m_ModuleKeys;
        private string m_ModuleSpawning;
        private bool m_SimplexForward;

        public bool m_ReadPersistent = true;
        public ModulePersistent m_Persistent = new ModulePersistent();

        [InspectorButton]
        void CopyToClipboard()=> GUIUtility.systemCopyBuffer = TDataConvert.Convert(m_Persistent);
        [InspectorButton]
        void ClipboardToData()=>m_Persistent = TDataConvert.Convert<ModulePersistent>(GUIUtility.systemCopyBuffer);
        
        private void Awake()
        {
            m_Grid = transform.Find("Grid").GetComponent<GridManager>();
            m_Module = transform.Find("Module").GetComponent<ModuleManager>();
            m_Simplex = transform.Find("Simplex").GetComponent<SimplexManager>();
            m_Camera = transform.Find("Camera").GetComponent<PCGCamera>();
            m_Environment = transform.Find("Environment").GetComponent<PCGEnvironment>();
            m_Audio = transform.Find("Audios").GetComponent<PCGAudios>();
            m_Controls = new IPCGControl[]{ m_Grid,m_Module,m_Simplex,m_Camera,m_Environment,m_Audio };
            m_Controls.Traversal(p=>p.Init());

            m_Grid.Setup(m_GridData);
            m_Module.Setup(m_ModuleData,m_Grid);
            m_Simplex.Setup(m_SimplexData,m_Grid);
            //Extras
            m_Spawning = transform.Find("Canvas/Spawning").GetComponent<Text>();
            TouchConsole.InitDefaultCommands();
            TouchConsole.Command("Reset",KeyCode.R).Button(Clear);
            TouchConsole.Command("Next Module", KeyCode.Alpha1).Button(NextModule);
            TouchConsole.Command("Marker",KeyCode.Alpha2).Button( SwitchGrid);
            TouchConsole.SwitchVisible();
            NextModule();
            ReadPersistent();
            SwitchGrid();
        }

        void SwitchGrid()
        {
            if(!m_SimplexForward)
                m_Simplex.Construct(("Grid",m_Grid.m_Vertices.Select(p=>new PCGID(p.Key,1)).ToList()));
            else
                m_Simplex.Fade();
            m_SimplexForward = !m_SimplexForward;
        }

        void ReadPersistent()
        {
            if(m_ReadPersistent)
                m_Persistent.ReadPersistentData();
            foreach (var cornerData in m_Persistent.m_Modules)
                m_Module.Construct(cornerData.type,cornerData.origin);
        }
        

        void SavePersistent()
        {
            m_Persistent.m_Modules = m_Module.m_GridManager.CollectPersistentData().ToList();
            m_Persistent.SavePersistentData();
        }
        
        void NextModule()
        {
            m_ModuleKeys = m_ModuleData.m_ModuleLibrary.Select(p => p.name).ToArray();
            int index = m_ModuleKeys.FindIndex(p => p == m_ModuleSpawning);
            m_ModuleSpawning = m_ModuleKeys[(index + 1) % m_ModuleKeys.Length];
            m_Spawning.text = m_ModuleSpawning;
        }
        

        void Clear()
        {
            m_Controls.Traversal(p=>p.Clear());
        }

        private void Update()
        {
            InputTick(Time.unscaledDeltaTime);

            float deltaTime = Time.deltaTime;
            m_Controls.Traversal(p=>p.Tick(deltaTime));
            m_Module.TickEnvironment(deltaTime,m_Environment.Output());
        }
        
        void InputTick(float _deltaTime)
        {
            var touch= UTouchTracker.Execute(_deltaTime);
            foreach (var clickPos in touch.ResolveClicks(.2f))
            {
                m_Module.Input( m_ModuleSpawning,m_Camera.m_Camera.ScreenPointToRay(clickPos),touch.Count==1);
                SavePersistent();
            }

            var drag = touch.CombinedDrag() * _deltaTime * 5f;
            m_Camera.Drag(drag.y,drag.x);
            if (touch.Count > 1)
                m_Environment.Rotate(drag.y, drag.x);
            var pinch = touch.CombinedPinch() * _deltaTime * 5f;
            float normalizedPitch = m_Camera.Pinch(pinch);

            PCGAudios.SetBGVolume(normalizedPitch);
        }

        private void OnDestroy()
        {
            m_Controls.Traversal(p=>p.Dispose());
        }
// #if UNITY_EDITOR
//         
//         #region Gizmos
//         private void OnGUI()
//         {
//             TouchTracker.DrawDebugGUI();
//         }
//         #endregion
// #endif
    }
    
}
