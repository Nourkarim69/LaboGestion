using MediatR;

namespace Domain.Queries
{
    public class GetGenericQuery<T> : IRequest<T> where T : class
    {
        public int Id { get; set; }

        public GetGenericQuery(int id)
        {
            Id = id;
        }
    }
}