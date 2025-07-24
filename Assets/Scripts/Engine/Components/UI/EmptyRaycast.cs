
namespace UnityEngine.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class EmptyRaycast : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }

}