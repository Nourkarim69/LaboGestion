using MediatR;

namespace Domain.Queries
{
    public class GetListGenericQuery<T> : IRequest<List<T>> where T : class
    {
        public GetListGenericQuery()
        {
        }
    }
}