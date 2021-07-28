using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace  TTouchTracker
{
    public class TrackPosition
    {
        public int m_Index { get; private set; }
        public float m_Lifetime { get; private set; }
        public Vector2 m_Start { get; private set; }
        public Vector2 m_Current { get;private set; }
        public Vector2 m_Previous { get; private set; }
        public Vector2 m_Delta { get; private set; }
        public TouchPhase m_Phase { get; private set; }
        public TrackPosition(Touch _touch)
        {
            m_Index = _touch.fingerId;
            m_Start = _touch.position;
            m_Current = m_Start;
            m_Previous = m_Current;
            m_Delta = Vector2.zero;
            m_Lifetime = 0f;
            m_Phase = _touch.phase;
        }

        public void Record(Touch _touch,float _deltaTime)
        {
            m_Phase = _touch.phase;
            m_Lifetime += _deltaTime;
            m_Previous = m_Current;
            m_Current = _touch.position;
            m_Delta = m_Current - m_Previous;
        }
    }
    public static class TouchTracker_Helper
    {
        public static IEnumerable<Vector2> GetPositions(this List<TrackPosition> _tracks) =>
            _tracks.Select(p => p.m_Current);
        public static Vector2 RawDrag(this List<TrackPosition> _tracks)=> _tracks.Average(p => p.m_Delta);
        public static float RawPinch(this List<TrackPosition> _tracks)
        {
            if (_tracks.Count<2)
                return 0f;

            Vector2 center = _tracks.Average(p => p.m_Current);
            return _tracks.Average(p =>
            {
                float sign=Mathf.Sign( Vector2.Dot( p.m_Delta,center-p.m_Previous));
                return p.m_Delta.magnitude*sign;
            });
        }
    }

    public static class TouchTracker
    {
        static readonly Dictionary<int,TrackPosition> m_TrackData=new Dictionary<int, TrackPosition>();
        private static Touch[] GetTouches()
        {
#if UNITY_EDITOR
            List<Touch> simulateTouches = new List<Touch>();

            Func<int, bool, Vector2, TouchPhase> getPhase = (id, pressing, position2) =>
            {
                if (pressing)
                    if (m_TrackData.ContainsKey(id))
                        return m_TrackData[id].m_Current == position2 ? TouchPhase.Stationary : TouchPhase.Moved;
                    else
                        return TouchPhase.Began;

                if (m_TrackData.ContainsKey(id))
                    return TouchPhase.Ended;

                return TouchPhase.Canceled;
            };
            Vector2 position = Input.mousePosition;
            //LRM Mouse Button 0,1,2
            for (int index = 0; index < 3; index++)
            {
                TouchPhase phase = getPhase(index, Input.GetMouseButton(index), position);
                if(phase== TouchPhase.Canceled)
                    continue;
               simulateTouches.Add(new Touch(){fingerId = index,phase=phase,position = position});
            }

            //LeftCtrl Touches 3
            {
                TouchPhase phase = getPhase(3, Input.GetKey(KeyCode.LeftControl), position);
                if (phase != TouchPhase.Canceled)
                {
                    simulateTouches.Add(new Touch(){fingerId = 3,phase=phase,position =position});
                    var center = new Vector2(Screen.width,Screen.height)/2f;
                    var invertPosition = center + (center - position);
                    simulateTouches.Add(new Touch(){fingerId = 4,phase=phase,position =invertPosition});
                }
            }

            return simulateTouches.ToArray();
#else
      return Input.touches;
#endif
        }

        public static void Init() => m_TrackData.Clear();
        public static List<TrackPosition> Execute(float _unscaledDeltaTime)
        {
            foreach (var trackIndex in m_TrackData.Keys.FindAll(p => m_TrackData[p].m_Phase == TouchPhase.Ended||m_TrackData[p].m_Phase== TouchPhase.Canceled))
                m_TrackData.Remove(trackIndex);
            
            foreach (Touch touch in GetTouches())
            {
                var id = touch.fingerId;
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        m_TrackData.Add(id,new TrackPosition(touch));
                        break;
                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        m_TrackData[id].Record(touch,_unscaledDeltaTime);
                        break;
                }
            }
            return  m_TrackData.Values.ToList();
        }
        
        #if UNITY_EDITOR
        private static readonly Texture m_GUITexture =  UnityEditor.EditorGUIUtility.IconContent("TouchInputModule Icon").image;
        public static void DrawDebugGUI()
        {
            foreach (TrackPosition trackPosition in m_TrackData.Values)
            {
                Rect screenRect = new Rect(trackPosition.m_Current, Vector2.one * 60f);
                screenRect.y = Screen.height - screenRect.y;
                GUI.DrawTexture(screenRect,m_GUITexture );   
            }
        }
        #endif
    }
}
