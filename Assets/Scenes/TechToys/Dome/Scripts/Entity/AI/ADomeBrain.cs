namespace Dome.Entity.AI
{
    public abstract class ADomeBrain
    {
        public static FDomeGrid kGrid;
        
        public static bool GetBrain(ADomeEntity _entity,out ADomeBrain _brain)
        {
            _brain = null;
            switch (_entity)
            {
                case ADomeARC arc:
                    _brain= new FARCBrain(arc);
                    return true;
                case FDomeTurret turret:
                    _brain = new FTurretBrain(turret);
                    return true;
                case FDomeCommander commander:
                    _brain = new FCommanderBrain(commander);
                    return true;
                default:
                    return false;
            }
        }
        
        public abstract void Tick(bool _working,float _deltaTime);
        public virtual void DrawGizmos(){}
    }

    public abstract class ADomeBrain<T>:ADomeBrain where T : ADomeEntity
    {
        protected T m_Entity { get; set; }
        public ADomeBrain(T _entity)
        {
            m_Entity = _entity;
        }

    }
}