namespace Usignert.CommandLine
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SubCommandAttribute : CommandAttribute
    {
        public SubCommandAttribute(string name, string description) : base(name, description)
        {
        }
    }
}
