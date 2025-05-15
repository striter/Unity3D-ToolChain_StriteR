using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Runtime.TouchTracker
{
   
    
    public static class TouchTracker
    {
        static readonly List<TrackData> m_TrackData = new List<TrackData>();
        static bool GetTrackData(int trackID, out int index,out TrackData _trackData)
        {
            index = -1;
            _trackData = default;
            for (var i = m_TrackData.Count - 1; i >= 0; i--)
            {
                var trackData = m_TrackData[i];
                if (trackData.id == trackID)
                {
                    index = i;
                    _trackData = trackData;
                    return true;
                }
            }
            return false;
        }
        
        static TouchPhase GetStandalonePhase(int _id, KeyCode _keyCode, Vector2 _position2)
        {
            var pressing = Input.GetKey(_keyCode);
            
            if(GetTrackData(_id,out var index,out var trackData))
            {
                if(!pressing) return TouchPhase.Ended;
                return trackData.current == _position2 ? TouchPhase.Stationary : TouchPhase.Moved;
            }
            
            if(pressing) return TouchPhase.Began;
                
            return TouchPhase.Canceled;
        }
        
        private static IList<Touch> GetTouches()
        {
            #if  UNITY_STANDALONE || UNITY_EDITOR
                var touches = UList.Empty<Touch>();
                var position = (Vector2)Input.mousePosition;
                for (var index = 0; index < 3; index++)
                {
                    var phase = GetStandalonePhase(index, KeyCode.Mouse0 + index, position);
                    if (phase == TouchPhase.Canceled)
                        continue;
                    touches.Add(new Touch() { fingerId = index, phase = phase, position = position });
                }
                return touches;
            #else
                return Input.touches;
            #endif
        }
        
        private static List<RaycastResult> kResults = new List<RaycastResult>();
        private static PointerEventData kPointerEventData = new PointerEventData(null);
        public static bool FilterUITrack(Touch _data,EventSystem _eventSystem,out List<RaycastResult> _results)
        {
            _results = kResults;
            if (_eventSystem == null)
                return true;
            kPointerEventData.position = _data.position;
            _eventSystem.RaycastAll(kPointerEventData,kResults);
            return kResults.Count == 0;
        }

        private static List<TrackData> kTrackResults = new();
        public static List<TrackData> Execute(float _unscaledDeltaTime,bool _filterUI = false,EventSystem _eventSystem = null)
        {
            for (var i = m_TrackData.Count - 1; i >= 0; i--)
            {
                var trackData = m_TrackData[i];
                if (trackData.phase is TouchPhase.Ended or TouchPhase.Canceled)
                {
                    m_TrackData.RemoveAt(i);
                    continue;
                }
                trackData.phase = TouchPhase.Canceled;
                m_TrackData[i] = trackData;
            }
            
            var touches = GetTouches();
            var screenSize = new Vector2(Screen.width, Screen.height);
            for(var i = touches.Count - 1;i>=0;i--)
            {
                var touch = touches[i];
                if(GetTrackData(touch.fingerId,out var index,out var trackData))
                {
                    m_TrackData[index] = trackData.Record(touch,screenSize,_unscaledDeltaTime);
                    continue;
                }
                
                var initialValid = !_filterUI || FilterUITrack(touch,_eventSystem,out kResults);
                m_TrackData.Add(new TrackData(touch, screenSize, initialValid));
            }
            
            kTrackResults.Clear();
            for (var i = m_TrackData.Count - 1; i >= 0; i--)
            {
                var trackData = m_TrackData[i];
                if(trackData.valid) 
                    kTrackResults.Add(trackData);
            }
            
            return kTrackResults;
        }
    }
}