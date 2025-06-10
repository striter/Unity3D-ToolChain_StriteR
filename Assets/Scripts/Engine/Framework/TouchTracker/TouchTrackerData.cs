using UnityEngine;

namespace Runtime.TouchTracker
{

   
    public struct TrackData
    {
        public bool valid;
        public int id;
        public Vector2 origin;
        public float lifeTime;
        public Vector2 current;
        public Vector2 previous;
        public Vector2 delta;
        public TouchPhase phase;
        public Vector2 originNormalized;
        public Vector2 previousNormalized;
        public Vector2 currentNormalized;
        public Vector2 deltaNormalized;
        public TrackData(Touch _touch, Vector2 _screenSize, bool _valid)
        {
            id = _touch.fingerId;
            origin = _touch.position;
            valid = _valid;
            
            current = origin;
            previous = current;
            delta = Vector2.zero;
            lifeTime = 0f;
            phase = _touch.phase;
            
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
            
            phase = _touch.phase;
            previousNormalized = currentNormalized;
            currentNormalized = current /_screenSize;
            deltaNormalized = delta / _screenSize;
            return this;
        }
        public static readonly TrackData kInvalid = new(){id = -1};
    }
}