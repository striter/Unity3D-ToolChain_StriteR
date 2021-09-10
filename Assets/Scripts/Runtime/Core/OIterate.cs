
    public interface IIterate<T>
    {
        T GetElement(int _index);
        int Length { get; }
    }