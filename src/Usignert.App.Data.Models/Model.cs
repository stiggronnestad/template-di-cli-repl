using YamlDotNet.Serialization;

namespace Usignert.App.Models
{
    [YamlSerializable]
    public abstract class Model
    {
        [YamlMember(Alias = "id", ApplyNamingConventions = false)]
        public string Id { get; set; } = string.Empty;
    }
}
