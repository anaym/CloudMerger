namespace CloudMerger.GuiPrimitives
{
    public class Mutable<T>
    {
        public T Value { get; set; }

        public Mutable(T value = default(T))
        {
            Value = value;
        }

        public static implicit operator T(Mutable<T> mutable)
        {
            return mutable.Value;
        }

        public static implicit operator Mutable<T>(T value)
        {
            return new Mutable<T>(value);
        }
    }
}