using System.Collections.Generic;
using Runtime.Geometry;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

public interface IBVHVolume<Bounds,Element,Dimension> where Bounds:struct where Element:struct,IShapeDimension<Dimension> where Dimension:struct
{
    public IList<Element> elements { get; set; }
    public Bounds bounds { get; set; }
    public int iteration { get; set; }
    public void SortElements(int _median,IList<Element> _elements);
    public Bounds OutputBounds(IList<Element> _elements);
}

public class BVH<Volume,Bounds,Element,Dimension>   //Bounding volume hierarchy
    where Volume:struct,IBVHVolume<Bounds,Element,Dimension> 
    where Bounds:struct
    where Element:struct,IShapeDimension<Dimension>
    where Dimension:struct
{
    private Volume kHelper = default;
    public List<Volume> m_Volumes { get; private set; } = new List<Volume>();

    void Divide(Volume _volume,out Volume _node1,out Volume _node2)
    {
        var last = _volume.elements.Count;
        var median = _volume.elements.Count / 2;
        _volume.SortElements(median,_volume.elements);

        var nextIteration = _volume.iteration + 1;
        _node1 = default;
        _node1.iteration = nextIteration;
        _node1.elements = new List<Element>(_volume.elements.Iterate(0,median));
        _node1.bounds = kHelper.OutputBounds(_node1.elements);

        _node2 = default;
        _node2.iteration = nextIteration;
        _node2.elements = new List<Element>(_volume.elements.Iterate(median,last));
        _node2.bounds = kHelper.OutputBounds(_node2.elements);
    }
    
    public void Construct(IList<Element> _elements, int _maxIteration, int _volumeCapacity)
    { 
        m_Volumes.Clear();
        
        Volume initial = default;
        initial.iteration = 0;
        initial.bounds = kHelper.OutputBounds(_elements);
        initial.elements = _elements;
        
        m_Volumes.Add( initial);
        
        bool doBreak = true;
        while (doBreak)
        {
            bool split = false;
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                var node = m_Volumes[i];
                if (node.iteration >= _maxIteration)
                    continue;
                
                if (node.elements.Count <= _volumeCapacity)
                    continue;

                Divide(node,out var node1,out var node2);
                m_Volumes.Add(node1);
                m_Volumes.Add(node2);
                
                m_Volumes.RemoveAt(i);
                split = true;
                break;
            }
            
            doBreak = split;
        }
    }

    public void DrawGizmos()
    {
        int index = 0;
        var matrix = Gizmos.matrix;
        foreach (var node in m_Volumes)
        {
            Gizmos.color = UColor.IndexToColor(index++ % 6);
            Gizmos.matrix = matrix;
            if (node.bounds is IShapeDimension<Dimension> boundsShape)
                boundsShape.DrawGizmos();
            foreach (var element in node.elements)
                if(element is IShapeDimension<Dimension> gizmos)
                    gizmos.DrawGizmos();
        }

        Gizmos.matrix = matrix;
    }
}
