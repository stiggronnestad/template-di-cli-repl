namespace Usignert.Di
{
    public sealed class DiServiceAttribute : Attribute
    {
        public DiServiceType Type { get; }

        public enum DiServiceType
        {
            Singleton,
            Scoped,
            Transient
        }

        public DiServiceAttribute(DiServiceType type)
        {
            Type = type;
        }
    }
}
