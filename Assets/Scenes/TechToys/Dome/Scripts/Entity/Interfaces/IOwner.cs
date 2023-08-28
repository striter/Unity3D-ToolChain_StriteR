namespace Dome.Entity
{
    public interface IOwner : IEntity
    {
        public int ownerId { get; set; }
    }
    
    public static class IOwner_Extension
    {
        public static void OnInitialize(this IOwner _owner,EntityInitializeParameters _parameters)
        {
            _owner.ownerId = _parameters.ownerId;
        }
        
        public static bool HasOwner(this IOwner _owner) => _owner.ownerId != IEntity.kInvalidID;
        
        public static ADomeEntity GetOwner(this IOwner _owner)
        {
            return IEntity.kEntities.Get(_owner.ownerId);
        }
        
    }
    
    
}