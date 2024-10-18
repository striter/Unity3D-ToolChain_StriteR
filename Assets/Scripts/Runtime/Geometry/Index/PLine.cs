using System;
using System.Collections;
using System.Collections.Generic;

namespace Runtime.Geometry
{
    [Serializable]
    public struct PLine:IEquatable<PLine> , IEnumerable<int>
    {
        public int start;
        public int end;

        public PLine(int _start,int _end)
        {
            start = _start;
            end = _end;
        }

        
        #region Implements
        public bool Equals(PLine other) => start == other.start && end == other.end;
        public bool EqualsNonVector(PLine other) => (start == other.start && end == other.end) 
                                                    || (end == other.start && start == other.end);

        public IEnumerator<int> GetEnumerator()
        {
            yield return start;
            yield return end;
        }

        public override bool Equals(object obj) => obj is PLine other && Equals(other);
        public override int GetHashCode()=> HashCode.Combine(start, end);
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}