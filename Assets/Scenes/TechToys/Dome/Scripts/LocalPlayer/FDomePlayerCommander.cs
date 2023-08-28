using System;
using System.Collections.Generic;
using Dome.Entity;
using Dome.Model;
using Geometry;
using Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;

namespace Dome.LocalPlayer
{

    [Serializable]
    public class FDomePlayerCommander : ADomePlayerControl<FDomeCommander> ,IModel      //Using IModel to create ghost model
    {
        [Header("Output")]
        [Readonly] public float3 m_RootPosition;
        [Readonly] public float2 m_ViewRotation;
        [Readonly] public float m_Zoom;
        [ScriptableObjectEdit] public FDomeCameraData_Commander m_Constrains;

        private ISelection m_Hovering,m_Selecting;
        
        [Header("Placement")]
        [Readonly] public string m_Placement = null;
        public Material m_PlacementMaterial;

        public override void Attach(FDomeLocalPlayer _player, IPlayerControl _entity, IPlayerControl _lastEntity)
        {
            base.Attach(_player, _entity, _lastEntity);
            var eulerAngles = umath.closestPitchYaw(m_Entity.rotation);
            if (_lastEntity == null) //Initialize
            {
                m_RootPosition = m_Entity.position;
                m_ViewRotation = eulerAngles;
                m_ViewRotation += m_Constrains.initialRotation;
                m_Zoom = m_Constrains.initialZoom;
            }
            else
            {
                m_RootPosition = _lastEntity.position;
            }

            m_Constrains.m_PositionDamper.Initialize(m_RootPosition + math.mul(quaternion.Euler( m_ViewRotation.to3xy()*kmath.kDeg2Rad) , kfloat3.back) * m_Zoom);
            m_Constrains.m_RotationDamper.Initialize(m_ViewRotation.to3xy());
            _player.Refer<FDomeCamera>().SetPositionRotation(m_Entity.position,quaternion.Euler(eulerAngles.to3xy() *kmath.kDeg2Rad) );
            
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
            var move = input.entityInput.move;

            if (move.sum() != 0f)
            {
                var lookDirection = quaternion.Euler(0, m_ViewRotation.y * kmath.kDeg2Rad, 0);
                var forward = math.mul(lookDirection, kfloat3.forward);
                var right = math.mul(lookDirection, kfloat3.right);

                move = move.normalize();
            
                m_RootPosition += (move.x * right + move.y * forward) * m_Constrains.movementSensitive;
                m_RootPosition = _player.Refer<FDomeGrid>().ConstrainPosition(m_RootPosition);
            }

            if(input.zoom!=0)
                m_Zoom = m_Constrains.zoomClamp.Clamp(m_Zoom + input.zoom * m_Constrains.zoomSensitive);

            if (input.leftAlt.Press())
            {
                m_ViewRotation += new float2(input.rotate.y, input.rotate.x) * m_Constrains.rotationSensitive;
                m_ViewRotation.x = m_Constrains.rotationClamp.Clamp(m_ViewRotation.x);
            }
            
            var euler = m_Constrains.m_RotationDamper.Tick(_deltaTime, m_ViewRotation.to3xy());
            var rotation = quaternion.Euler( euler*kmath.kDeg2Rad);
            var position = m_Constrains.m_PositionDamper.Tick(_deltaTime,m_RootPosition + math.mul(rotation , kfloat3.back) * m_Zoom);
            var camera = _player.Refer<FDomeCamera>();
            
            camera.SetPositionRotation(position,rotation);

            if (TickPlacement(_player,camera,input)) return;
            
            TickSelection(_player,camera,input);
            if (m_Selecting != null) m_RootPosition = m_Selecting.position;
            
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
            var position = ray.GetPoint(UGeometry.Distance.Eval(ray, GPlane.kUp));

            var primary = _input.entityInput.primary;
            if (primary.Down())
                placementStartPosition = position;

            var yaw = m_ViewRotation.y;
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