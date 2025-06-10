using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Runtime.TouchTracker
{
    public static class TouchTracker_Extension
    {
        public static IEnumerable<Vector2> ResolveClicks(this List<TrackData> _tracks, float _distanceSQR = 100, float _clickSenseTime = .3f,bool _removeTracker = true)
        {
            for (var i = _tracks.Count - 1; i >= 0; i--)
            {
                var p = _tracks[i];
                if (p.phase != TouchPhase.Ended 
                    || !((p.origin - p.current).SqrMagnitude() < _distanceSQR) 
                    || !(p.lifeTime < _clickSenseTime)
                    ) 
                    continue;
                if(_removeTracker)
                    _tracks.RemoveAt(i);
                yield return p.origin;
            }
        }
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
        public static float CombinedPinch(this List<TrackData> _tracks,EventSystem _eventSystem = null)
        {
            var pinch = 0f;
            
        #if UNITY_STANDALONE || UNITY_EDITOR
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (math.abs(scroll) > float.Epsilon && ExecuteEvents.GetEventHandler<IScrollHandler>(!TouchTracker.FilterUITrack(new Touch(){position = Input.mousePosition},_eventSystem,out var results) ?results[0].gameObject : null) == null)
                pinch += scroll * Time.unscaledDeltaTime * -10000f ;      // pixels / sec while scrolling
        #endif

            if (_tracks.Count < 2) 
                return pinch;
            var center = _tracks.Average(p => p.current);
            var beginDelta = _tracks[0].delta;

            var count = _tracks.Count;
            for (var i = 0; i < count; i++)
            {
                var p = _tracks[i];
                var sign=Mathf.Sign( Vector2.Dot( p.delta,center-p.previous));
                pinch += (beginDelta-p.delta).magnitude*sign;
            }
            pinch /= count;
            // pinch += _tracks.Average(p =>
            // {
            // var sign=Mathf.Sign( Vector2.Dot( p.delta,center-p.previous));
            // return (beginDelta-p.delta).magnitude*sign;
            // });
            
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

                    m_JoystickID = track.id;
                    m_JoystickStationaryPos = track.origin;
                    _onJoyStickSet?.Invoke(m_JoystickStationaryPos,true);
                    break;
                }
            }

            if (m_JoystickID == -1)
                return;

            if (!_trackData.TryFind(p => p.id == m_JoystickID,out var trackData))
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

        public static int kDragId = -1;
        public static bool Input_SingleDrag(this List<TrackData> _tracks,out TrackData _output,bool _removeTacker=true)
        {
            _output = default;
            if (kDragId == -1)
            {
                if (_tracks.Count == 0)
                    return false;
                _output = _tracks[0];
                kDragId = _output.id;
            }

            if (kDragId == -1 || !_tracks.TryFind(p => p.id == kDragId, out  _output) || _output.phase is TouchPhase.Ended or TouchPhase.Canceled)
            {
                kDragId = -1;
                return false;
            }

            if (_removeTacker)
                _tracks.Remove(_output);
            return true;
        }
        #endregion
    }

}