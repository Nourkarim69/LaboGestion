using Domain.Interface;
using Domain.Queries;
using MediatR;

namespace Domain.Handlers
{
    public class GetListGenericHandler<T> : IRequestHandler<GetListGenericQuery<T>, List<T>> where T : class
    {
        private readonly IGenericRepository<T> _repository;

        public GetListGenericHandler(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<List<T>> Handle(GetListGenericQuery<T> request, CancellationToken cancellationToken)
        {
            return await _repository.GetAll();
        }
    }
}