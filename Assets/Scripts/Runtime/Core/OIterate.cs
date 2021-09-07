
    public interface IIterate<T>
    {
        T GetElement(int index);
        int Length { get; }
    }