using System;
using System.Collections.Generic;
using Dome.Entity;
using Dome.Model;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.LocalPlayer
{

    [Serializable]
    public class FDomePlayerCommander : ADomePlayerControl<FDomeCommander> ,IModel      //Using IModel to create ghost model
    {
        private ISelection m_Hovering,m_Selecting;
        
        [Header("Placement")]
        [Readonly] public string m_Placement = null;
        public Material m_PlacementMaterial;

        public override void Attach(FDomeLocalPlayer _player, IPlayerControl _entity, IPlayerControl _lastEntity)
        {
            base.Attach(_player, _entity, _lastEntity);
            m_Selecting?.SetSelectionFlag(ESelections.Select,true);
            m_Placement = null;
        }

        public override void Detach()
        {
            m_Hovering?.ClearSelectionFlag();  m_Hovering = null;
            m_Selecting?.ClearSelectionFlag();
            SetPlacement(null);
        }

        public override void Tick(FDomeLocalPlayer _player,float _deltaTime,IPlayerControl _entity)
        {
            base.Tick(_player,_deltaTime,_entity);
            var input = _player.Refer<FDomeInput>().playerInputs;
            var camera = _player.Refer<FDomeCamera>();
            
            if (TickPlacement(_player,camera,input)) return;
            
            TickSelection(_player,camera,input);
        }

        void TickSelection(FDomeLocalPlayer _player,FDomeCamera _camera,FDomePlayerInputs _input)
        {
            ISelection possibleSelection = null;
            var ray = _camera.ScreenPointToRay(_input.hoverPosition);
            var entities = _player.Refer<FDomeEntities>().
                GetEntityWithMixin<ISelection>(FDomeEntityFilters.FilterTeams[_player.m_ControlTeam]);
            foreach (var selection in entities) {
                if(!selection.RayIntersect(ray))
                    continue;
                possibleSelection = selection;
                break;
            }
            Hover(possibleSelection);

            if (_input.entityInput.primary.Down())
            {
                if (m_Selecting == possibleSelection)
                {
                    TakeControl(_player,possibleSelection);
                    return;
                }
                
                if(m_Selecting!=null) m_Selecting.SetSelectionFlag(ESelections.Select,false);
                m_Selecting = possibleSelection;
                m_Selecting?.SetSelectionFlag(ESelections.Select,true);
            }
        }

        void TakeControl(FDomeLocalPlayer _player,ISelection _entity)
        {
            if(!(_entity is IPlayerControl control)) return;
            _player.Refer<FDomeLocalPlayer>().TakeControl(control.id);
        }

        ISelection Hover(ISelection _entity)
        {
            if (m_Hovering == _entity) return m_Hovering;
            
            if(m_Hovering!=null) m_Hovering.SetSelectionFlag(ESelections.Hover,false);
            m_Hovering = _entity;
            m_Hovering?.SetSelectionFlag(ESelections.Hover,true);
            return m_Hovering;
        }

        private float3 placementStartPosition;
        public void SetPlacement(string _placement)
        {
            if (_placement == null)
            {
                m_Placement = null;
                this.ClearModel();
                return;
            }
            
            m_Placement = _placement;
            this.SetModel(ADomeEntity.GetDefines(m_Placement).modelPath);
            this.SetOverrideMaterial(m_PlacementMaterial);
        }

        public bool TickPlacement(FDomeLocalPlayer _player,FDomeCamera _camera,FDomePlayerInputs _input)
        {
            if (m_Placement == null) return false;

            if (_input.secondary.Down()) {
                SetPlacement(null);
                return true;
            }
            
            var ray = _camera.ScreenPointToRay(_input.hoverPosition);
            var position = ray.GetPoint(ray.IntersectDistance(GPlane.kUp));

            var primary = _input.entityInput.primary;
            if (primary.Down())
                placementStartPosition = position;

            var yaw = _player.Refer<FDomeCamera>().m_Input.euler.y;
            if (primary.Press() || primary.Up())
            {
                yaw = umath.closestYaw(position - placementStartPosition);
                position = placementStartPosition;
            }
                
;           var rotation = quaternion.Euler(0,yaw * kmath.kDeg2Rad,0);
            this.SetModelPositionRotation(position,rotation);
            
            if (primary.Up())
            {
                _player.Refer<FDomeEntities>().Spawn(m_Placement, new TR(position,rotation),m_Entity.team, m_Entity);
                SetPlacement(null);
            }
            
            return true;
        }
        
        public override void OnDrawGizmos(FDomeLocalPlayer _player)
        {
            base.OnDrawGizmos(_player);
            var entities = _player.Refer<FDomeEntities>()
                .GetEntityWithMixin<ISelection>(FDomeEntityFilters.FilterTeams[_player.m_ControlTeam]);
            foreach (var entity in entities) {
                entity.DrawGizmos();
            }
        }

        public string modelPath { get; set; }
        public GameObject modelRoot { get; set; }
        public MeshRenderer[] meshRenderers { get; set; }
        public Material[] restoreMaterials { get; set; }
        public Dictionary<string, Transform> modelNodes { get; set; }
    }
}