using System.Collections.Generic;
using Geometry;
using TechToys.ThePlanet.Module;
using TechToys.ThePlanet.Module.Prop;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR

namespace TechToys.ThePlanet.Baking
{
    public class ModulePropPossibilitySetBaker : MonoBehaviour
    {
        public Qube<bool> m_Relation;
        public void Import(Mesh[] _importMeshes,byte _qubeByte)
        {
            m_Relation = default;
            m_Relation.SetByteElement(_qubeByte);
            var decorationName = DBaking.GetDecorationName(_qubeByte);
            gameObject.name = decorationName;
            var mesh = _importMeshes?.Find(p => p.name.Equals(decorationName));
            if (mesh == null)
            {
                GameObject template = new GameObject("Default");
                template.transform.SetParent(transform);
                template.transform.localPosition = Vector3.zero;
                template.transform.localRotation=Quaternion.identity;
                template.transform.localScale = Vector3.one;
                var qubeBytes = UModuleByte.kByteQubeIndexer[m_Relation.ToByte()];
                for (int i = 0; i < qubeBytes.Length; i++)
                {
                    Qube<bool> byteQube = default;
                    byteQube.SetByteElement(qubeBytes[i]);
                    var matrix = Matrix4x4.TRS(KQube.kUnitQubeBottomed[i] * .5f, quaternion.identity, Vector3.one * .5f);
                    for (int j = 0; j < 8; j++)
                    {
                        if (!byteQube[j])
                            continue;

                        var subCube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                        subCube.SetParent(template.transform);
                        subCube.localScale = Vector3.one * .25f;
                        subCube.localPosition = matrix.MultiplyPoint(KQube.kHalfUnitQubeBottomed[j] + kfloat3.up * .25f);
                    }
                }

                return;
            }

            var subMesh = new GameObject("Model").transform;
            subMesh.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            subMesh.gameObject.AddComponent<MeshRenderer>().sharedMaterials =new Material[mesh.subMeshCount];
            subMesh.transform.SetParent(transform);
            subMesh.transform.localPosition = Vector3.zero;
        }
        public ModulePossibilitySet Export(List<Mesh> _meshLibrary,List<Material> _materialLibrary)
        {
            int childCount = transform.childCount;
            ModulePropSet[] allPossibilities = new ModulePropSet[childCount];
            for (int i = 0; i < childCount; i++)
            {
                var propSetTransform = transform.GetChild(i);
                var worldToLocalMatrix = propSetTransform.worldToLocalMatrix;
                var renderers = propSetTransform.GetComponentsInChildren<MeshRenderer>();
                ModulePropData[] props = new ModulePropData[renderers.Length];
                for(int j=0;j<renderers.Length;j++)
                {
                    var meshRenderer = renderers[j];
                    
                    var mesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh;
                    var materials = meshRenderer.sharedMaterials;

                    int embedMesh = _meshLibrary.FindIndex(p => p == mesh);
                    if (embedMesh == -1)
                    {
                        _meshLibrary.Add(mesh);
                        embedMesh = _meshLibrary.Count - 1;
                    }
                    
                    int[] embedMaterials = new int[materials.Length];
                    for (int k = 0; k < materials.Length; k++)
                    {
                        var material = materials[k];
                        var index = _materialLibrary.FindIndex(p => p == material);
                        if (index == -1)
                        {
                            _materialLibrary.Add(material);
                            index = _materialLibrary.Count - 1;
                        }
                        embedMaterials[k] = index;
                    }

                    var renderTransform = meshRenderer.transform;
                    var localPosition = DModuleProp.ObjectToOrientedVertex( worldToLocalMatrix.MultiplyPoint(renderTransform.position));
                    var localRotation = worldToLocalMatrix.rotation * renderTransform.rotation;
                    var localScale = renderTransform.lossyScale.mul(worldToLocalMatrix.lossyScale);
                    props[j] = new ModulePropData {type=DBaking.GetPropType(renderTransform.name), meshIndex = embedMesh,embedMaterialIndex = embedMaterials,position = localPosition,rotation = localRotation,scale = localScale};
                }

                allPossibilities[i] = new ModulePropSet { props = props };
            }
            return new ModulePossibilitySet { possibilities = allPossibilities };
        }
        public void OnDrawGizmos()
        {
            int childCount = transform.childCount;
            if (childCount == 0)
            {
                DrawQubeCorners(transform,Color.red.SetAlpha(.5f));
                return;
            }
            for (int i = 0; i < childCount; i++)
            {
                var setTransform = transform.GetChild(i);
                setTransform.localPosition = Vector3.up * i;
                setTransform.localRotation = Quaternion.identity;
                setTransform.localScale = Vector3.one;
                DrawQubeCorners(setTransform,Color.green.SetAlpha(.5f));
            }
        }

        void DrawQubeCorners(Transform qubeTransform,Color color)
        {
            Gizmos.matrix = qubeTransform.localToWorldMatrix;
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.up*.5f,Vector3.one);
            Gizmos.DrawLine(KQuad.k3SquareCentered[0]+kfloat3.up*.5f,KQuad.k3SquareCentered[2]+kfloat3.up*.5f);
            var qubeBytes = UModuleByte.kByteQubeIndexer[m_Relation.ToByte()];
            for (int j = 0; j < qubeBytes.Length; j++)
            {
                Qube<bool> byteQube = default;
                byteQube.SetByteElement(qubeBytes[j]);
                Gizmos.matrix = qubeTransform.localToWorldMatrix*Matrix4x4.TRS(KQube.kUnitQubeBottomed[j]*.5f,quaternion.identity,Vector3.one*.5f);
                for (int k = 0; k < 8; k++)
                {
                    if(!byteQube[k])
                        continue;
                    Gizmos.color = color;
                    Gizmos.DrawWireSphere(KQube.kUnitQubeBottomed[k]*.8f,.1f);
                }
            }
        }
    }

}
#endif