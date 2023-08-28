using Dome.Entity;

namespace Dome.LocalPlayer
{
    public abstract class ADomePlayerControl
    {
        public virtual void Attach(FDomeLocalPlayer _player,IPlayerControl _entity,IPlayerControl _lastEntity) { }
        public abstract void Tick(FDomeLocalPlayer _player,float _deltaTime,IPlayerControl _entity);
        public abstract void Detach();
        public virtual void OnDrawGizmos(FDomeLocalPlayer _player){}
    }

    public abstract class ADomePlayerControl<T> : ADomePlayerControl where T :  class,IEntity
    {
        protected T m_Entity { get; set; }
        
        public override void Attach(FDomeLocalPlayer _player, IPlayerControl _entity, IPlayerControl _lastEntity)
        {
            base.Attach(_player, _entity, _lastEntity);
            m_Entity = _entity as T;
        }

        public override void Tick(FDomeLocalPlayer _player, float _deltaTime, IPlayerControl _entity)
        {
            m_Entity = _entity as T;
        }
    }
}