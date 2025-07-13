using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Extensions;
using Rendering;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Pool;
using Runtime.TouchTracker;
using TMPro;
using TPool;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.TheBook
{
    public class TheBook : MonoBehaviour
    {
        public bool m_PictureBook;
        public int m_DesireFlip;
        public Damper m_FlipDamper = Damper.kDefault;
        protected GameObjectPool<APageContainer> m_Pages { get; private set; }
        protected SkinnedMeshRenderer m_Renderer { get; private set; }
        private static readonly int kProgressID = Shader.PropertyToID("_Progress");
        private static string kPictureKeyword = "_PICTURE";
        private static readonly int[] kPagesId = new[]
        {
            Shader.PropertyToID("_Page1Tex"), 
            Shader.PropertyToID("_Page2Tex"), 
            Shader.PropertyToID("_Page3Tex"),
            Shader.PropertyToID("_Page4Tex")
        };

        private void Awake()
        {
            m_Renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            m_Pages = new GameObjectPool<APageContainer>(new (transform.Find("Pages/Element")));
            m_Pages.transform.SetPositionAndRotation(Vector3.zero,quaternion.identity);
            m_FlipDamper.Initialize(m_DesireFlip);
            ApplyPages(m_DesireFlip,0f);     //Lets start with page 1

            TouchConsole.InitDefaultCommands();
            TouchConsole.Command("Forward",KeyCode.D).Button(()=>m_DesireFlip +=1);
            TouchConsole.Command("Backward",KeyCode.A).Button(()=>m_DesireFlip -=1);
        }

        private void OnDestroy()
        {
            m_Pages.Dispose();
        }

        private void Update()
        {
            if (!enabled)
                return;

            m_DesireFlip = math.max(m_DesireFlip, 0);
            var currentFlip = m_FlipDamper.Tick(Time.deltaTime,m_DesireFlip);
            ApplyPages(currentFlip,m_FlipDamper.velocity.x);

            var tracks = TouchTracker.Execute(Time.unscaledDeltaTime);
            if (tracks.Input_SingleDrag(out var track) && math.abs(track.originOffset.x) > 200)
            {
                var sign = (int)math.sign(track.originOffset.x);
                m_DesireFlip -= sign;
            }
        }
        
        protected void ApplyPages(float _flip,float _velocity)
        {
            var fFlip = _flip;
            var iFlip = (int)fFlip;
            var pagesToShow = PoolList<int>.Empty("pagesToShow");
            var additionalPage = _velocity != 0;
            if (m_PictureBook)
            {
                var currentPage = iFlip;
                pagesToShow.Add(currentPage);
                if (additionalPage)
                    pagesToShow.Add(currentPage + 1);
            }
            else
            {
                var currentPage = iFlip * 2;
                pagesToShow.Add(currentPage);
                pagesToShow.Add(currentPage+1);
                if (additionalPage)
                {
                    pagesToShow.Add(currentPage+2);
                    pagesToShow.Add(currentPage+3);
                }
            }
            
            foreach (var page in m_Pages.m_Dic.Keys.FillList(PoolList<int>.Empty(nameof(ApplyPages))))
            {
                if (!pagesToShow.Contains(page))
                    m_Pages.Recycle(page);
            }
                
            var pixels = m_PictureBook ? new int2(1920,1080) : new int2(1920 / 2, 1080);
            foreach (var pageIndex in pagesToShow)
            {
                if (!m_Pages.Contains(pageIndex))
                    m_Pages.Spawn(pageIndex).Init(pixels,$"Pages/Page{pageIndex}");
            }

            for (var i = 0; i < pagesToShow.Count; i++)
            {
                m_Renderer.material.SetTexture(kPagesId[i],
                    i<pagesToShow.Count? m_Pages[pagesToShow[i]].m_Texture: null);
            }
            
            var progressAnimation = math.clamp( fFlip - iFlip , 0, 1);;     //Output progress animation
            m_Renderer.material.SetFloat(kProgressID, progressAnimation);
            m_Renderer.material.EnableKeyword(kPictureKeyword, m_PictureBook);
        }
    }

    public class APageContainer : APoolElement
    {
        public RenderTexture m_Texture { get; set; }
        private Canvas m_Canvas;
        private TMP_Text m_PageIndex;
        private Camera m_Camera;
        private GameObject m_PageElement;
        public APageContainer(Transform _transform) : base(_transform)
        {
        }
        
        public override void OnPoolCreate()
        {
            base.OnPoolCreate();
            m_Canvas = transform.GetComponentInChildren<Canvas>();
            m_PageIndex = m_Canvas.GetComponentInChildren<TMP_Text>();
            m_Camera = transform.GetComponentInChildren<Camera>();
            m_Canvas.worldCamera = m_Camera;
        }

        public APageContainer Init(int2 _pixelSize,string _resPath)
        {
            m_Texture = RenderTexture.GetTemporary(_pixelSize.x, _pixelSize.y, 0, RenderTextureFormat.RGB111110Float);
            m_Camera.targetTexture = m_Texture;
            m_Camera.enabled = true;
            m_PageIndex.text = $"#{identity}";
            transform.localPosition = Vector3.right * identity * 3;
            if (_resPath != null)
            {
                var prefab = (GameObject)Resources.Load(_resPath);
                if (!prefab) 
                    prefab = Resources.Load("Pages/Empty") as GameObject;
                m_PageElement = GameObject.Instantiate(prefab, m_Canvas.transform);
                m_PageElement.transform.SetAsFirstSibling();
            }
            return this;
        }
        
        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Camera.enabled = false;
            m_Camera.targetTexture = null;
            if (m_PageElement != null)
            {
                GameObject.Destroy(m_PageElement);
                m_PageElement = null;
            }
            RenderTexture.ReleaseTemporary(m_Texture);
        }
    }
}
