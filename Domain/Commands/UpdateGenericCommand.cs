using MediatR;

namespace Domain.Commands
{
    public class UpdateGenericCommand<T> : IRequest<T> where T : class
    {
        public T Entity { get; set; }

        public UpdateGenericCommand(T entity)
        {
            Entity = entity;
        }
    }
}