using System.Collections.Generic;
using System.Linq.Extensions;
using Dome.LocalPlayer;

namespace Dome.Entity.AI
{
    public class FDomeAI : ADomeController
    {
        private Dictionary<int, ADomeBrain> m_Actors = new Dictionary<int, ADomeBrain>();

        public override void OnInitialized()
        {
            ADomeBrain.kGrid = Refer<FDomeGrid>();
        }

        public override void Dispose()
        {
            m_Actors.Clear();
        }

        public override void Tick(float _deltaTime)
        {
            var playerControlling = Refer<FDomeLocalPlayer>().m_ControllingID;
            m_Actors.Traversal(_pair=>_pair.Value.Tick(_pair.Key!=playerControlling,_deltaTime));
        }

        protected void OnEntitySpawn(ADomeEntity _entity)
        {
            if(ADomeBrain.GetBrain(_entity,out var brain))
                m_Actors.Add(_entity.id,brain);
        }

        protected void OnEntityRecycle(ADomeEntity _entity)
        {
            m_Actors.TryRemove(_entity.id);
        }

        private void OnDrawGizmos()
        {
            foreach (var actor in m_Actors.Values)
                actor.DrawGizmos();
        }
    }
}
