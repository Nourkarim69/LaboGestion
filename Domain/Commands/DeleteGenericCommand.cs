using MediatR;

namespace Domain.Commands
{
    public class DeleteGenericCommand<T> : IRequest<bool> where T : class
    {
        public int Id { get; set; }

        public DeleteGenericCommand(int id)
        {
            Id = id;
        }
    }
}