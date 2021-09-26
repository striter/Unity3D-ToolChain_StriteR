
    public interface IIterate<T>
    {
        T this[int _index] { get; }
        int Length { get; }
    }