using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace TTouchTracker
{
    public struct TrackData
    {
        public bool valid;
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
        public TrackData(Touch _touch, Vector2 _screenSize, bool _valid)
        {
            index = _touch.fingerId;
            origin = _touch.position;
            valid = _valid;
            
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
        private static IEnumerable<Touch> GetTouches()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            TouchPhase GetPhase(int id, KeyCode _keyCode, Vector2 position2)
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

            var position = (Vector2)Input.mousePosition;
            //LRM Mouse Button 0,1,2
            for (var index = 0; index < 3; index++)
            {
                var phase = GetPhase(index, KeyCode.Mouse0 + index, position);
                if(phase== TouchPhase.Canceled)
                    continue;
                yield return new Touch(){fingerId = index,phase=phase,position = position};
            }

            // LeftCtrl Touches 3
            {
                var phase = GetPhase(3, KeyCode.LeftControl, position);
                if (phase == TouchPhase.Canceled) yield break;
                 
                yield return (new Touch(){fingerId = 3,phase=phase,position =position});
                var center = new Vector2(Screen.width,Screen.height)/2f;
                var invertPosition = center + (center - position);
                yield return  (new Touch(){fingerId = 4,phase=phase,position =invertPosition});
            }
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
            foreach (var trackIndex in m_TrackData.Keys.Collect(p => m_TrackData[p].phase == TouchPhase.Ended||m_TrackData[p].phase== TouchPhase.Canceled).FillList(kTracksIndex))     //We don't remove touch the frame its cancelled, state will be used across applications
                m_TrackData.Remove(trackIndex);
            
            foreach (var touch in GetTouches())
            {
                var id = touch.fingerId;
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        m_TrackData.Add(touch.fingerId,new TrackData(touch, screenSize,_filter ==null || _filter(touch)));
                        break;
                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        m_TrackData[id]=m_TrackData[id].Record(touch,screenSize,_unscaledDeltaTime);
                        break;
                }
            }
            return m_TrackData.Values.Collect(p=>p.valid).FillList(kTracks);
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

        public static float2 CombinedDrag(this List<TrackData> _tracks)=> _tracks.Average(p => p.delta);
        public static float CombinedPinch(this List<TrackData> _tracks)
        {
            var pinch = 0f;
            
            #if UNITY_STANDALONE || UNITY_EDITOR
                pinch += Input.GetAxis("Mouse ScrollWheel") * Time.unscaledDeltaTime * -10000f ;      // pixels / sec while scrolling
            #endif
            
            if (_tracks.Count >= 2)
            {
                var center = _tracks.Average(p => p.current);
                var beginDelta = _tracks[0].delta;
            
                pinch += _tracks.Average(p =>
                {
                    var sign= Mathf.Sign( Vector2.Dot( p.delta,center-p.previous));
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

        public static float2 Input_ScreenMove(this List<TrackData> _tracks, RangeFloat _xActive)=> _tracks.Collect(p=>_xActive.Contains( p.originNormalized.x)).Average(p=>p.delta);
        public static float2 Input_ScreenMove_Normalized(this List<TrackData> _tracks,RangeFloat _xActive) => Input_ScreenMove(_tracks,_xActive) / new float2(Screen.width,Screen.height);
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

            var joystickDelta = trackData.current - trackData.origin;
            var direction = joystickDelta.normalized;
            var magnitude=Mathf.Clamp01(joystickDelta.magnitude/_joystickRadius);
            _normalizedTrackDelta?.Invoke(direction*magnitude);
            if (_removeTracker)
                _trackData.Remove(trackData);
        }
        #endregion
        
        #region Single Drag
        private static int m_DragID=-1;
        private static Vector2 m_LastDrag = Vector2.zero;
        public static void Input_SingleDrag(this List<TrackData> _tracks,Action<float2,bool> _onDragStatus,Action<float2> _onDrag,float _senseTime=.1f,bool _removeTacker=true)
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
