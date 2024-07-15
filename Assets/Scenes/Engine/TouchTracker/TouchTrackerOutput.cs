using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.TouchTracker
{
        
    public static class TouchTracker_Extension
    {
        public static IEnumerable<Vector2> ResolveClicks(this List<TrackData> _tracks,float _distanceSQR = 100,float _clickSenseTime=.3f)=>_tracks.Collect(p => p.phase == TouchPhase.Ended && (p.origin - p.current).SqrMagnitude()< _distanceSQR  && p.lifeTime < _clickSenseTime).Select(p=>p.origin);
        public static IEnumerable<TrackData> ResolvePress(this List<TrackData> _tracks)=>_tracks.Collect(p => p.phase == TouchPhase.Stationary);
        public static IEnumerable<Vector2> ResolveTouch(this List<TrackData> _tracks, float _senseTime=.3f, TouchPhase tp = TouchPhase.Stationary)=>_tracks.Collect(p => p.phase == tp && p.lifeTime > _senseTime).Select(p=>p.origin);

        public static float2 CombinedDrag(this List<TrackData> _tracks)
        {
            var sum = float2.zero;
            var count = 0;
            foreach (var track in _tracks)
            {
                sum += (float2)track.delta;
                count++;
            }
            return sum / math.max(count,1);
        }
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
                    var sign=Mathf.Sign( Vector2.Dot( p.delta,center-p.previous));
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