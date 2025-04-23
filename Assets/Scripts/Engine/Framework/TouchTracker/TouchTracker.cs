using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Runtime.TouchTracker
{
    public static class TouchTracker
    {
        static readonly Dictionary<int,TrackData> m_TrackData=new Dictionary<int, TrackData>();
        static TouchPhase GetStandalonePhase(int id, KeyCode _keyCode, Vector2 position2)
        {
            var pressing = Input.GetKey(_keyCode);
            if (m_TrackData.TryGetValue(id, out var trackData))
            {
                if(!pressing) return TouchPhase.Ended;
                return trackData.current == position2 ? TouchPhase.Stationary : TouchPhase.Moved;
            }

            if(pressing) return TouchPhase.Began;
                
            return TouchPhase.Canceled;
        }
        private static IList<Touch> GetTouches()
        {
            var touches = UList.Empty<Touch>();

    #if  UNITY_STANDALONE || UNITY_EDITOR
            var position = (Vector2)Input.mousePosition;
            //LRM Mouse Button 0,1,2
            for (var index = 0; index < 3; index++)
            {
                var normalPhase = GetStandalonePhase(index, KeyCode.Mouse0 + index, position);
                if (normalPhase == TouchPhase.Canceled)
                    continue;

                touches.Add(new Touch() { fingerId = index, phase = normalPhase, position = position });
            }

            //LeftCtrl Pinch Touches 3
            // var pinchPhase = GetStandalonePhase(3, KeyCode.LeftControl, position);
            // if (pinchPhase != TouchPhase.Canceled)
            // {
            //     kStandaloneTouches.Add(new Touch(){fingerId = 3,phase=pinchPhase,position =position});
            //     var center = new Vector2(Screen.width,Screen.height)/2f;
            //     var invertPosition = center + (center - position);
            //     kStandaloneTouches.Add(new Touch(){fingerId = 4,phase=pinchPhase,position =invertPosition});
            // }
    #else
            
            var touchCount = Input.touchCount;
            for(var i = 0;i<touchCount;i++)
                touches.Add(Input.GetTouch(i));
     
       #endif
            return touches;
        }
        
        private static readonly List<int> kTracksIndex = new List<int>();
        private static readonly List<TrackData> kTracks = new List<TrackData>();
        
        private static List<RaycastResult> kResults = new List<RaycastResult>();
        public static bool FilterUITrack(Touch _data,EventSystem _eventSystem,out List<RaycastResult> _results)
        {
            _results = kResults;
            if (_eventSystem == null)
                return true;
            _eventSystem.RaycastAll(new PointerEventData(_eventSystem){position = _data.position},kResults);
            return kResults.Count == 0;
        }
        public static List<TrackData> Execute(float _unscaledDeltaTime,bool _filterUI = false,EventSystem _eventSystem = null,Predicate<Touch> _extraFilter = null)
        {
            kTracksIndex.Clear();
            foreach (var pair in m_TrackData)
            {
                if (pair.Value.phase is TouchPhase.Ended or TouchPhase.Canceled)
                    kTracksIndex.Add(pair.Key);
            }
            foreach (var trackIndex in kTracksIndex)
                m_TrackData.Remove(trackIndex);

            
            m_TrackData.Keys.FillList(kTracksIndex);
            foreach (var trackKey in kTracksIndex)
            { 
                var trackData = m_TrackData[trackKey];
                trackData.phase = TouchPhase.Canceled;
                m_TrackData[trackKey] = trackData;
            }
            
            var touches = GetTouches();
            var screenSize = new Vector2(Screen.width, Screen.height);
            for(var i =0;i<touches.Count;i++)
            {
                var touch = touches[i];
                var id = touch.fingerId;
                if (!m_TrackData.ContainsKey(touch.fingerId))
                {
                    var valid = !_filterUI || FilterUITrack(touch,_eventSystem,out kResults);
                    valid &= _extraFilter == null || _extraFilter(touch);
                    m_TrackData.Add(touch.fingerId,new TrackData(touch, screenSize,valid));
                }
                m_TrackData[id]=m_TrackData[id].Record(touch,screenSize,_unscaledDeltaTime);
            }
            
            kTracks.Clear();
            foreach (var pair in m_TrackData)
            {
                if(pair.Value.valid)
                    kTracks.Add(pair.Value);
            }
            return kTracks;
        }
        
        #if UNITY_EDITOR
        private static readonly Texture m_GUITexture =  UnityEditor.EditorGUIUtility.IconContent("TouchInputModule Icon").image;
        public static void DrawDebugGUI()
        {
            foreach (TrackData trackPosition in m_TrackData.Values)
            {
                Rect screenRect = new Rect(trackPosition.current, Vector2.one * 60f);
                screenRect.y = Screen.height - screenRect.y;
                GUI.DrawTexture(screenRect,m_GUITexture );   
            }
        }
        #endif
    }
}