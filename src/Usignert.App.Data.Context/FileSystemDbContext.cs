using Usignert.App.Models;
using YamlDotNet.Serialization;

namespace Usignert.App.Data.Context
{
    public class FileSystemDbContext<TEntity>
        where TEntity : Model
    {
        private readonly string _basePath;

        public FileSystemDbContext(string basePath)
        {
            _basePath = basePath;
        }

        public bool TryWrite(TEntity model)
        {
            var filePath = Path.Combine(_basePath, $"{model.Id}.yml");
            var serializer = new SerializerBuilder().Build();

            try
            {
                var yaml = serializer.Serialize(model);
                File.WriteAllText(filePath, yaml);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool TryRead(string id, out TEntity entity)
        {
            var filePath = Path.Combine(_basePath, $"{id}.yml");

            if (!File.Exists(filePath))
            {
                entity = default!;
                return false;
            }

            try
            {
                var deserializer = new DeserializerBuilder().Build();
                var yaml = File.ReadAllText(filePath);
                entity = deserializer.Deserialize<TEntity>(yaml);
            }
            catch (Exception)
            {
                entity = default!;
                return false;
            }

            return true;
        }

        public bool TryDelete(string id)
        {
            var filePath = Path.Combine(_basePath, $"{id}.yml");

            try
            {
                File.Delete(filePath);
            }

            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public IQueryable<TEntity> ReadAll()
        {
            var yamlPaths = GetYamlFiles(_basePath);
            var models = new List<TEntity>();

            foreach (var path in yamlPaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(path);

                if (TryRead(fileName, out var model))
                {
                    models.Add(model);
                }
            }

            return models.AsQueryable();
        }

        private string[] GetYamlFiles(string path)
        {
            if (!Directory.Exists(path))
            {
                return [];
            }

            var files = Directory.GetFiles(path, "*.yml");
            return [.. files];
        }
    }
}
