namespace Usignert.CommandLine
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ArgumentAttribute : Attribute
    {
        public string Description { get; }

        public ArgumentAttribute(string description)
        {
            Description = description;
        }
    }
}
