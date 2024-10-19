namespace Runtime.Geometry
{
    public interface IGizmos
    {
        public void DrawGizmos();
    }

    public interface IHandles
    {
#if UNITY_EDITOR
        public void DrawHandles();
#endif

    }
}