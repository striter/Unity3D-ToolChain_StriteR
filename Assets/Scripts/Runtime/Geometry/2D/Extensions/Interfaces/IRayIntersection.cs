namespace Runtime.Geometry.Extension
{
    public interface IRayIntersection2
    {
        public bool Intersect(G2Ray _ray,out float distance);
    }
}