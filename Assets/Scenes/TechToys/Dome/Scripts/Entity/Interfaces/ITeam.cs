using System.ComponentModel;
using Dome.Model;
using UnityEngine;

namespace Dome.Entity
{
    public enum ETeam
    {
        None = 0,
        Blue = 1,
        Red = 2,
        Neutral = 3,
    }

    public interface ITeam
    {
        public ETeam team { get; set; }
    }
    
    public static class ITeam_Extension
    {
        public static Color GetTeamColor(this ITeam _team)
        {
            switch (_team.team)
            {
                default: throw new InvalidEnumArgumentException();
                case ETeam.Blue: return Color.cyan;
                case ETeam.Red: return KColor.kOrange;
            }
        }
    
        public static void OnInitialize(this ITeam _this,EntityInitializeParameters _parameters)
        {
            _this.team = _parameters.team;
        }
        
        private static readonly int kEmissionColor = Shader.PropertyToID("_EmissionColor");
        
        public static void OnModelSet(this ITeam _team, IModel _model)
        {
            var emissionColor = _team.GetTeamColor();
            foreach (var renderer in _model.meshRenderers)
                renderer.material.SetColor(kEmissionColor,emissionColor * 1.5f);   
        }


    }
}