using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TTouchTracker
{
    public struct TrackData
    {
        public int index;
        public Vector2 origin;
        public float lifeTime;
        public Vector2 current;
        public Vector2 previous;
        public Vector2 delta;
        public TouchPhase phase;
        public float phaseDuration;
        public Vector2 originNormalized;
        public Vector2 previousNormalized;
        public Vector2 currentNormalized;
        public Vector2 deltaNormalized;
        public TrackData(Touch _touch,Vector2 _screenSize)
        {
            index = _touch.fingerId;
            origin = _touch.position;
            
            current = origin;
            previous = current;
            delta = Vector2.zero;
            lifeTime = 0f;
            phase = _touch.phase;
            phaseDuration = 0f;
            
            originNormalized = origin / _screenSize;
            currentNormalized = originNormalized;
            previousNormalized = originNormalized;
            deltaNormalized = Vector2.zero;
        }

        public TrackData Record(Touch _touch,Vector2 _screenSize, float _deltaTime)
        {
            lifeTime += _deltaTime;
            previous = current;
            current = _touch.position;
            delta = current - previous;
                
            if (phase == _touch.phase)
                phaseDuration += _deltaTime;
            else
                phaseDuration = 0f;
            phase = _touch.phase;

            previousNormalized = currentNormalized;
            currentNormalized = current /_screenSize;
            deltaNormalized = delta / _screenSize;
            return this;
        }
    }
    
    public static class TouchTracker
    {
        static readonly Dictionary<int,TrackData> m_TrackData=new Dictionary<int, TrackData>();
        private static Touch[] GetTouches()
        {
#if UNITY_EDITOR
            List<Touch> simulateTouches = new List<Touch>();

            TouchPhase GETPhase(int id, bool pressing, Vector2 position2)
            {
                if (pressing)
                    if (m_TrackData.ContainsKey(id))
                        return m_TrackData[id].current == position2 ? TouchPhase.Stationary : TouchPhase.Moved;
                    else
                        return TouchPhase.Began;

                if (m_TrackData.ContainsKey(id)) return TouchPhase.Ended;

                return TouchPhase.Canceled;
            }

            Vector2 position = Input.mousePosition;
            //LRM Mouse Button 0,1,2
            for (int index = 0; index < 3; index++)
            {
                TouchPhase phase = GETPhase(index, Input.GetMouseButton(index), position);
                if(phase== TouchPhase.Canceled)
                    continue;
                simulateTouches.Add(new Touch(){fingerId = index,phase=phase,position = position});
            }

            //LeftCtrl Touches 3
            {
                TouchPhase phase = GETPhase(3, Input.GetKey(KeyCode.LeftControl), position);
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
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            foreach (var trackIndex in m_TrackData.Keys.Collect(p => m_TrackData[p].phase == TouchPhase.Ended||m_TrackData[p].phase== TouchPhase.Canceled).ToArray())
                m_TrackData.Remove(trackIndex);
            
            foreach (Touch touch in GetTouches())
            {
                var id = touch.fingerId;
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        m_TrackData.Add(id,new TrackData(touch,screenSize));
                        break;
                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        m_TrackData[id]=m_TrackData[id].Record(touch,screenSize,_unscaledDeltaTime);
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
                Rect screenRect = new Rect(trackPosition.current, Vector2.one * 60f);
                screenRect.y = Screen.height - screenRect.y;
                GUI.DrawTexture(screenRect,m_GUITexture );   
            }
        }
        #endif
    }
    
    
    public static class TouchTracker_Extension
    {
        public static IEnumerable<Vector2> ResolveClicks(this List<TrackData> _tracks,float _clickSenseTime=.1f)=>_tracks.Collect(p => p.phase == TouchPhase.Ended && p.lifeTime < _clickSenseTime).Select(p=>p.origin);
        public static IEnumerable<TrackData> ResolvePress(this List<TrackData> _tracks)=>_tracks.Collect(p => p.phase == TouchPhase.Stationary);
        public static IEnumerable<Vector2> ResolveTouch(this List<TrackData> _tracks, float _senseTime=.3f, TouchPhase tp = TouchPhase.Stationary)=>_tracks.Collect(p => p.phase == tp && p.lifeTime > _senseTime).Select(p=>p.origin);

        public static Vector2 CombinedDrag(this List<TrackData> _tracks)=> _tracks.Average(p => p.delta);
        public static float CombinedPinch(this List<TrackData> _tracks)
        {
            float pinch = 0;
            
            #if UNITY_EDITOR
                pinch += Input.GetAxis("Mouse ScrollWheel")*500f;
            #endif
            
            if (_tracks.Count >= 2)
            {
                Vector2 center = _tracks.Average(p => p.current);
                var beginDelta = _tracks[0].delta;
            
                pinch += _tracks.Average(p =>
                {
                    float sign=Mathf.Sign( Vector2.Dot( p.delta,center-p.previous));
                    return (beginDelta-p.delta).magnitude*sign;
                });
            
                return pinch;
            }
            return pinch;
        }


    }

    public static class TouchTracker_Extension_Advanced
    {
        #region Joystick

        public static Vector2 Input_ScreenMove(this List<TrackData> _tracks, RangeFloat _xActive)=> _tracks.Collect(p=>_xActive.Contains( p.originNormalized.x)).Average(p=>p.delta);
        public static Vector2 Input_ScreenMove_Normalized(this List<TrackData> _tracks,RangeFloat _xActive) => Input_ScreenMove(_tracks,_xActive).div(Screen.width,Screen.height);
        private static int m_JoystickID = -1;
        private static Vector2 m_JoystickStationaryPos = Vector2.zero;
        public static void Joystick_Stationary(this List<TrackData> _trackData,Action<Vector2,bool> _onJoyStickSet,Action<Vector2> _normalizedTrackDelta,RangeFloat _activeXRange,float _joystickRadius,bool _removeTracker=true)
        {
            if (m_JoystickID == -1)
            {
                foreach (var track in _trackData)
                {
                    if (track.phase != TouchPhase.Began || !_activeXRange.Contains(track.originNormalized.x))
                        continue;

                    m_JoystickID = track.index;
                    m_JoystickStationaryPos = track.origin;
                    _onJoyStickSet?.Invoke(m_JoystickStationaryPos,true);
                    break;
                }
            }

            if (m_JoystickID == -1)
                return;

            if (!_trackData.TryFind(p => p.index == m_JoystickID,out var trackData))
            {
                _onJoyStickSet?.Invoke(m_JoystickStationaryPos,false);
                m_JoystickID = -1;
                m_JoystickStationaryPos = Vector2.zero;
                return;
            }

            Vector2 joystickDelta = trackData.current - trackData.origin;
            Vector2 direction = joystickDelta.normalized;
            float magnitude=Mathf.Clamp01(joystickDelta.magnitude/_joystickRadius);
            _normalizedTrackDelta?.Invoke(direction*magnitude);
            if (_removeTracker)
                _trackData.Remove(trackData);
        }
        #endregion
        
        #region Single Drag
        private static int m_DragID=-1;
        private static Vector2 m_LastDrag = Vector2.zero;
        public static void Input_SingleDrag(this List<TrackData> _tracks,Action<Vector2,bool> _onDragStatus,Action<Vector2> _onDrag,float _senseTime=.1f,bool _removeTacker=true)
        {
            if (m_DragID == -1)
            {
                if (_tracks.Count != 1)
                    return;
                var dragTrack = _tracks[0];
                if (dragTrack.phase != TouchPhase.Stationary&&dragTrack.phase!=TouchPhase.Moved)
                    return;
                if (dragTrack.phaseDuration < _senseTime)
                    return;
                    
                m_DragID = dragTrack.index;
                m_LastDrag = dragTrack.current;
                _onDragStatus(dragTrack.current, true);
                return;
            }

            if (!_tracks.TryFind(p => p.index == m_DragID, out var dragging) || _tracks[0].phase == TouchPhase.Ended)
            {
                m_DragID = -1;
                m_LastDrag = Vector2.zero;
                _onDragStatus?.Invoke(m_LastDrag, false);
                return;
            }

            if (_removeTacker)
                _tracks.Remove(dragging);
            m_LastDrag = dragging.current;
            _onDrag?.Invoke(m_LastDrag);
        }
        #endregion
    }

}
