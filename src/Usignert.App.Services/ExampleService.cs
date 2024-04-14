using Usignert.App.Dtos;
using Usignert.App.Models;
using Usignert.App.Repositories;
using Usignert.Di;

namespace Usignert.App.Services
{
    [DiService(DiServiceAttribute.DiServiceType.Singleton)]
    public sealed class ExampleService
    {
        private readonly IRepository<PositionModel> _repository;

        public ExampleService(IRepository<PositionModel> repository)
        {
            _repository = repository;
        }

        public bool Add(PositionDto dto)
        {
            var model = ToModel(dto);
            return _repository.Add(model);
        }

        public IQueryable<PositionDto> GetAll() => _repository.GetAll().Select(x => FromModel(x));

        public bool Remove(PositionDto dto)
        {
            var model = ToModel(dto);
            return _repository.Delete(model.Id);
        }

        public IQueryable<PositionDto> QueryById(string id)
        {
            return _repository.GetAll()
                .Where(s => s.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => FromModel(x)
            );
        }

        public static PositionDto FromModel(PositionModel model)
        {
            var dto = new PositionDto
            {
                X = model.X,
                Y = model.Y
            };

            return dto;
        }

        public static PositionModel ToModel(PositionDto dto) => new PositionModel
        {
            X = dto.X,
            Y = dto.Y
        };
    }
}
