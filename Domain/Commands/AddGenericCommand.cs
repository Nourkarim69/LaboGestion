using MediatR;

namespace Domain.Commands
{
    public class AddGenericCommand<T> : IRequest<T> where T : class
    {
        public T Entity { get; set; }

        public AddGenericCommand(T entity)
        {
            Entity = entity;
        }
    }
}