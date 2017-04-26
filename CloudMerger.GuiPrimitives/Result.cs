namespace CloudMerger.GuiPrimitives
{
    public class Result<T>
    {
        public T Value { get; set; }
        public bool HasBeenCanceled { get; set; }

        public Result(T value = default(T))
        {
            Value = value;
        }

        public static implicit operator T(Result<T> result)
        {
            return result.Value;
        }

        public static implicit operator Result<T>(T value)
        {
            return new Result<T>(value);
        }
    }
}