using Domain.Commands;
using Domain.Interface;
using MediatR;

namespace Domain.Handlers
{
    public class AddGenericHandler<T> : IRequestHandler<AddGenericCommand<T>, T> where T : class
    {
        private readonly IGenericRepository<T> _repository;

        public AddGenericHandler(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<T> Handle(AddGenericCommand<T> request, CancellationToken cancellationToken)
        {
            await _repository.Add(request.Entity);
            return request.Entity;
        }
    }
}