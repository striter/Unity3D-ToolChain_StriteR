namespace Runtime.Geometry.Extension
{
    public interface IRayIntersection2
    {
        public bool RayIntersection(G2Ray _ray,out float distance);
    }
}