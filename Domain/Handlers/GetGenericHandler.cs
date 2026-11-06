using Domain.Interface;
using Domain.Queries;
using MediatR;

namespace Domain.Handlers
{
    public class GetGenericHandler<T> : IRequestHandler<GetGenericQuery<T>, T> where T : class
    {
        private readonly IGenericRepository<T> _repository;

        public GetGenericHandler(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<T> Handle(GetGenericQuery<T> request, CancellationToken cancellationToken)
        {
            return await _repository.GetById(request.Id);
        }
    }
}