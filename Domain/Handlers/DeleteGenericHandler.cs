using Domain.Commands;
using Domain.Interface;
using MediatR;

namespace Domain.Handlers
{
    public class DeleteGenericHandler<T> : IRequestHandler<DeleteGenericCommand<T>, bool> where T : class
    {
        private readonly IGenericRepository<T> _repository;

        public DeleteGenericHandler(IGenericRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteGenericCommand<T> request, CancellationToken cancellationToken)
        {
            await _repository.Delete(request.Id);
            return true;
        }
    }
}