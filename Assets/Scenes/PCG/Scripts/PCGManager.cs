using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PCG.Module;
using PCG.Simplex;
using Procedural.Hexagon;
using TTouchTracker;
using UnityEngine;
using TDataPersistent;
using UnityEngine.UI;

namespace PCG
{
    using static PCGDefines<int>;
    interface IPolyGridControl
    {
        void Init();
        void Tick(float _deltaTIme);
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
        private IPolyGridControl[] m_Controls;

        //Executions
        [Header("Constant")]
        public float m_Width = 1f;
        public float m_Height = 1f;

        private Text m_Spawning;
        private string[] m_ModuleKeys;
        private string m_ModuleSpawning;
        private bool m_SimplexForward;
        public ModulePersistent m_Persistent = new ModulePersistent();
        private void Awake()
        {
            KPCG.Setup(m_Width,m_Height);
            m_Grid = transform.Find("Grid").GetComponent<GridManager>();
            m_Module = transform.Find("Module").GetComponent<ModuleManager>();
            m_Simplex = transform.Find("Simplex").GetComponent<SimplexManager>();
            m_Camera = transform.Find("Camera").GetComponent<PCGCamera>();
            m_Controls = new IPolyGridControl[]{ m_Grid,m_Module,m_Simplex,m_Camera };
            m_Controls.Traversal(p=>p.Init());

            m_Grid.Setup(m_GridData);
            m_Module.Setup(m_ModuleData,m_Grid);
            m_Simplex.Setup(m_SimplexData,m_Grid);
            //Extras
            m_Spawning = transform.Find("Canvas/Spawning").GetComponent<Text>();
            TouchConsole.InitDefaultCommands();
            TouchConsole.Command("Reset",KeyCode.R).Button(Clear);
            TouchConsole.Command("Next Module", KeyCode.Alpha1).Button(NextModule);
            TouchConsole.Command("Marker",KeyCode.Alpha2).Button(() => { 
                if(!m_SimplexForward)
                    m_Simplex.Construct(("Grid",m_Persistent.m_Modules.Select(p=>p.origin).ToList()));
                else
                    m_Simplex.Fade();
                m_SimplexForward = !m_SimplexForward;
            });
            NextModule();
            this.StartCoroutine(LoadPersistent());
        }
        
        IEnumerator LoadPersistent()
        {
            m_Persistent.ReadPersistentData();
            if (m_Persistent.m_Modules == null)
                yield break;
            
            m_Persistent.m_Modules.Sort((a,b)=>(a.origin.height-b.origin.height));
            foreach (var cornerData in m_Persistent.m_Modules)
            {
                m_Module.Construct(cornerData.type,cornerData.origin);
                yield return new WaitForSeconds(.1f);
            }
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
            float deltaTime = Time.unscaledDeltaTime;
            m_Controls.Traversal(p=>p.Tick(Time.deltaTime));
            InputTick(deltaTime);
        }
        
        void InputTick(float _deltaTime)
        {
            var touch=TouchTracker.Execute(_deltaTime);
            foreach (var clickPos in touch.ResolveClicks(.2f))
            {
                m_Module.Input( m_ModuleSpawning,m_Camera.m_Camera.ScreenPointToRay(clickPos),touch.Count==1);
                SavePersistent();
            }

            touch.Input_SingleDrag(m_Camera.SetDrag, m_Camera.Drag, .0f, true);
            var drag = touch.CombinedDrag() * _deltaTime * 5f;
            var pinch = touch.CombinedPinch() * _deltaTime * 5f;
            m_Camera.Rotate(drag.y, drag.x);
            m_Camera.Pinch(pinch);
        }
#if UNITY_EDITOR
        
        #region Gizmos
        private void OnGUI()
        {
            TouchTracker.DrawDebugGUI();
        }
        #endregion
#endif
    }
    
}
