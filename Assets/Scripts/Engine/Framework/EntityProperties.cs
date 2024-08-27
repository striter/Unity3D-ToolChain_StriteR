using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TObjectPool;
namespace OEntityProperty
{
    public class EntityExpRank
    {
        public int m_Rank { get; private set; }
        public int m_TotalExpOwned { get; private set; }
        public int m_ExpCurRankOwned { get; private set; }
        public int m_ExpCurRankRequired { get; private set; }
        public int m_ExpLeftToNextRank => m_ExpCurRankRequired - m_ExpCurRankOwned;
        public float m_ExpCurRankScale => m_ExpCurRankOwned / (float)m_ExpCurRankRequired;
        readonly Func<int, int> GetExpToNextLevel;
        public EntityExpRank(Func<int, int> GetExpToNextLevel)
        {
            this.GetExpToNextLevel = GetExpToNextLevel;
            m_TotalExpOwned = 0;
            m_Rank = 0;
            m_ExpCurRankOwned = 0;
        }
        public void OnExpSet(int totalExp)
        {
            m_TotalExpOwned = 0;
            m_Rank = 0;
            m_ExpCurRankOwned = 0;
            OnExpGainCheckLevelOffset(totalExp);
        }

        public int OnExpGainCheckLevelOffset(int exp)
        {
            int startRank = m_Rank;
            m_TotalExpOwned += exp;
            m_ExpCurRankOwned += exp;
            for (; ; )
            {
                m_ExpCurRankRequired = GetExpToNextLevel(m_Rank);
                if (m_ExpCurRankOwned < m_ExpCurRankRequired)
                    break;
                m_ExpCurRankOwned -= m_ExpCurRankRequired;
                m_Rank++;
            }
            return m_Rank - startRank;
        }
    }
    public class EntityProperty<T>
    {
        public T m_Identity { get; private set; }
        public float m_StartAmount { get; private set; }
        public float m_CurAmount { get; protected set; }
        public float m_AmountDelta_Start => m_CurAmount - m_StartAmount;
        public EntityProperty(T _identity, float _start)
        {
            m_Identity = _identity;
            m_StartAmount = _start;
            m_CurAmount = m_StartAmount;
        }

        public virtual void AddCurDelta(float _delta) => m_CurAmount += _delta;
        public void ResetAmount() => m_CurAmount = m_StartAmount; 
        public virtual string ToString_Detailed() => string.Format("{0}:C|{1:F1},D|{2:F1}", m_Identity, m_CurAmount, m_AmountDelta_Start);
  
    }
    public class EntityProperty_0Max<T> : EntityProperty<T>
    {
        public float m_MaxStart { get; private set; }
        public float m_MaxModify { get; private set; }
        public float m_MaxAmount { get; private set; }
        public float m_MaxScale => m_CurAmount / m_MaxAmount;
        public float m_AmountDelta_Max => m_MaxAmount - m_CurAmount;
        public EntityProperty_0Max(T _identity, float _startValue, float _maxStart) : base(_identity, _startValue)
        {
            m_MaxStart = _maxStart;
            m_MaxModify = 0f;
            m_MaxAmount = m_MaxStart + m_MaxModify;
        }
        public override void AddCurDelta(float _delta)
        {
            m_CurAmount = (Mathf.Clamp(m_CurAmount + _delta, 0, m_MaxAmount));
        }

        public void AddModifyDelta(float _delta)
        {
            m_MaxModify += _delta;
            m_MaxAmount = m_MaxStart + m_MaxModify;
            m_CurAmount = (Mathf.Clamp(m_CurAmount + _delta, 0, m_MaxAmount));
        }
        public void ResetMaxModify()
        {
            m_MaxModify = 0;
            m_MaxAmount = m_MaxStart + m_MaxModify;
        }
        public override string ToString_Detailed() => string.Format("{0}:C|{1:F1},M|{2:F1},MD|{3:F1}", m_Identity, m_CurAmount, m_MaxAmount, m_MaxModify);
    }
    public class EntitySheildItem : IObjectPool
    {
        public int m_ID { get; private set; }
        public float m_Amount { get; private set; }
        public EntitySheildItem Set(int _ID, float _amount)
        {
            m_ID = _ID;
            m_Amount = _amount;
            return this;
        }
        public void AddDelta(float _delta) => m_Amount += _delta;

        public void OnPoolCreate() { }
        public void OnPoolInitialize() { }
        public void OnPoolRecycle() { }
    }
    public class EntityShieldCombine
    {
        public Dictionary<int, EntitySheildItem> m_Shields = new Dictionary<int, EntitySheildItem>();
        public float m_TotalAmount { get; private set; }
        void CheckTotalAmount()
        {
            m_TotalAmount = 0;
            m_TotalAmount = m_Shields.Values.Sum(p => p.m_Amount);
        }
        public void SpawnShield(int _id, float _amount)
        {
            EntitySheildItem shield = ObjectPool<EntitySheildItem>.Spawn().Set(_id, _amount);
            m_Shields.Add(_id, shield);
            CheckTotalAmount();
        }
        public void RecycleShield(int _shieldID)
        {
            if (!m_Shields.ContainsKey(_shieldID))
                throw new Exception("Invalid Shield Found Of:" + _shieldID);
            m_Shields[_shieldID].Recycle();
            m_Shields.Remove(_shieldID);
            CheckTotalAmount();
        }

        public float DoShieldDamageReduction(float _amount, Comparison<EntitySheildItem> _sort = null)
        {
            List<EntitySheildItem> _shields = m_Shields.Values.ToList();
            if (_sort != null)
                _shields.Sort(_sort);

            foreach (var shield in m_Shields.Values)
            {
                float delta = _amount - shield.m_Amount;
                if (delta >= 0)
                    delta = shield.m_Amount;
                else
                    delta = _amount;
                shield.AddDelta(-delta);
                _amount -= delta;
                if (_amount == 0)
                    break;
            }
            CheckTotalAmount();
            return _amount;
        }
    }
}