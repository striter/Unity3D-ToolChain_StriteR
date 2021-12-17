using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TTouchTracker
{
    public class TrackData
    {
        public int m_Index { get; private set; }
        public float m_Lifetime { get; private set; }
        public Vector2 m_Start { get; private set; }
        public Vector2 m_Current { get;private set; }
        public Vector2 m_Previous { get; private set; }
        public Vector2 m_Delta { get; private set; }
        public TouchPhase m_Phase { get; private set; }
        public float m_PhaseTime { get; private set; }
        public TrackData(Touch _touch)
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
            m_Lifetime += _deltaTime;
            m_Previous = m_Current;
            m_Current = _touch.position;
            m_Delta = m_Current - m_Previous;
            
            if (m_Phase == _touch.phase)
                m_PhaseTime += _deltaTime;
            else
                m_PhaseTime = 0f;
            m_Phase = _touch.phase;
        }
    }
    
    public static class TouchTracker_Helper
    {
        public static Vector2 CombinedDrag(this List<TrackData> _tracks)=> _tracks.Average(p => p.m_Delta);
        public static float CombinedPinch(this List<TrackData> _tracks)
        {
            if (_tracks.Count<2)
                return 0f;

            Vector2 center = _tracks.Average(p => p.m_Current);
            var beginDelta = _tracks[0].m_Delta;
            return _tracks.Average(p =>
            {
                float sign=Mathf.Sign( Vector2.Dot( p.m_Delta,center-p.m_Previous));
                return (beginDelta-p.m_Delta).magnitude*sign;
            });
        }

        public static IEnumerable<Vector2> ResolveClicks(this List<TrackData> _tracks,float _clickSenseTime=.1f)
        {
            return _tracks.Collect(p => p.m_Phase == TouchPhase.Ended && p.m_Lifetime < _clickSenseTime).Select(p=>p.m_Start);
        }
        public static IEnumerable<TrackData> ResolvePress(this List<TrackData> _tracks)
        {
            return _tracks.Collect(p => p.m_Phase == TouchPhase.Stationary);
        }
        
        public static IEnumerable<Vector2> ResolveTouch(this List<TrackData> _tracks, float _senseTime=.3f, TouchPhase tp = TouchPhase.Stationary)
        {
            return _tracks.Collect(p => p.m_Phase == tp && p.m_Lifetime > _senseTime).Select(p=>p.m_Start);
        }

        private static int hDragID=-1;
        private static Vector2 hLastDrag = Vector2.zero;
        public static void ResolveSingleDrag(this List<TrackData> _tracks,Action<Vector2,bool> _onDragStatus,Action<Vector2> _onDrag,float _senseTime=.1f,bool _removeTacker=true)
        {
            if (hDragID == -1)
            {
                if (_tracks.Count != 1)
                    return;
                var dragTrack = _tracks[0];
                if (dragTrack.m_Phase != TouchPhase.Stationary&&dragTrack.m_Phase!=TouchPhase.Moved)
                    return;
                if (dragTrack.m_PhaseTime < _senseTime)
                    return;
                    
                hDragID = dragTrack.m_Index;
                hLastDrag = dragTrack.m_Current;
                _onDragStatus(dragTrack.m_Current, true);
                return;
            }

            if (!_tracks.TryFind(p => p.m_Index == hDragID, out var dragging))
            {
                hDragID = -1;
                hLastDrag = Vector2.zero;
                _onDragStatus?.Invoke(hLastDrag, false);
                return;
            }

            if (_removeTacker)
                _tracks.Remove(dragging);
            hLastDrag = dragging.m_Current;
            _onDrag?.Invoke(hLastDrag);
        }
    }

    public static class TouchTracker
    {
        static readonly Dictionary<int,TrackData> m_TrackData=new Dictionary<int, TrackData>();
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
        private static readonly List<TrackData> kTracks = new List<TrackData>();
        public static List<TrackData> Execute(float _unscaledDeltaTime)
        {
            foreach (var trackIndex in m_TrackData.Keys.Collect(p => m_TrackData[p].m_Phase == TouchPhase.Ended||m_TrackData[p].m_Phase== TouchPhase.Canceled).ToArray())
                m_TrackData.Remove(trackIndex);
            
            foreach (Touch touch in GetTouches())
            {
                var id = touch.fingerId;
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        m_TrackData.Add(id,new TrackData(touch));
                        break;
                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        m_TrackData[id].Record(touch,_unscaledDeltaTime);
                        break;
                }
            }
            m_TrackData.Values.FillList(kTracks);
            return kTracks;
        }
        
        #if UNITY_EDITOR
        private static readonly Texture m_GUITexture =  UnityEditor.EditorGUIUtility.IconContent("TouchInputModule Icon").image;
        public static void DrawDebugGUI()
        {
            foreach (TrackData trackPosition in m_TrackData.Values)
            {
                Rect screenRect = new Rect(trackPosition.m_Current, Vector2.one * 60f);
                screenRect.y = Screen.height - screenRect.y;
                GUI.DrawTexture(screenRect,m_GUITexture );   
            }
        }
        #endif
    }
}
