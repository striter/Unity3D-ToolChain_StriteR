using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.TouchTracker
{
    
    public static class UTouchTracker
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
        private static IEnumerable<Touch> GetTouches()
        {
#if UNITY_STANDALONE || UNITY_EDITOR

            var position = (Vector2)Input.mousePosition;
            //LRM Mouse Button 0,1,2
            for (var index = 0; index < 3; index++)
            {
                var phase = GetStandalonePhase(index, KeyCode.Mouse0 + index, position);
                if(phase== TouchPhase.Canceled)
                    continue;
                yield return new Touch(){fingerId = index,phase=phase,position = position};
            }

            //LeftCtrl Touches 3
            // {
            //     var phase = GetPhase(3, KeyCode.LeftControl, position);
            //     if (phase == TouchPhase.Canceled) yield break;
            //     
            //     yield return (new Touch(){fingerId = 3,phase=phase,position =position});
            //     var center = new Vector2(Screen.width,Screen.height)/2f;
            //     var invertPosition = center + (center - position);
            //     yield return  (new Touch(){fingerId = 4,phase=phase,position =invertPosition});
            // }
#else
            foreach (var touch in Input.touches)
                yield return touch;
#endif
        }
        
        private static readonly List<int> kTracksIndex = new List<int>();
        private static readonly List<TrackData> kTracks = new List<TrackData>();
        public static List<TrackData> Execute(float _unscaledDeltaTime,Predicate<Touch> _filter = null)
        {
            var screenSize = new Vector2(Screen.width, Screen.height);
            kTracksIndex.Clear();
            foreach (var pair in m_TrackData)
            {
                if (pair.Value.phase is TouchPhase.Ended or TouchPhase.Canceled)
                    kTracksIndex.Add(pair.Key);
            }

            foreach (var trackIndex in kTracksIndex)
                m_TrackData.Remove(trackIndex);
            
            foreach (var touch in GetTouches())
            {
                var id = touch.fingerId;
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if(!m_TrackData.ContainsKey(touch.fingerId))
                            m_TrackData.Add(touch.fingerId,new TrackData(touch, screenSize,_filter ==null || _filter(touch)));
                        break;
                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        if(m_TrackData.ContainsKey(id))
                            m_TrackData[id]=m_TrackData[id].Record(touch,screenSize,_unscaledDeltaTime);
                        break;
                }
            }

            kTracks.Clear();
            foreach (var value in m_TrackData.Values)
            {
                if(value.valid)
                    kTracks.Add(value);
            }
            return kTracks;
        }
        
        
        #if UNITY_EDITOR
        private static readonly Texture m_GUITexture =  UnityEditor.EditorGUIUtility.IconContent("TouchInputModule Icon").image;
        public static void DrawDebugGUI()
        {
            foreach (var trackPosition in m_TrackData.Values)
            {
                var screenRect = new Rect(trackPosition.current, Vector2.one * 60f);
                screenRect.y = Screen.height - screenRect.y;
                GUI.DrawTexture(screenRect,m_GUITexture );   
            }
        }
        #endif
    }


}
