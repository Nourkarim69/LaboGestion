using Domain.Commands;
using Domain.Interface;
using MediatR;

namespace Domain.Handlers
{
    public class UpdateGenericHandler<T> : IRequestHandler<UpdateGenericCommand<T>, T> where T : class
    {
        private readonly IGenericRepository<T> _repository;

        public UpdateGenericHandler(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<T> Handle(UpdateGenericCommand<T> request, CancellationToken cancellationToken)
        {
            await _repository.Update(request.Entity);
            return request.Entity;
        }
    }
}