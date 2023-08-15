using System.Collections.Generic;
using HotChocolate;

namespace API.GraphQL
{

    public class Paginated<T>
    {
        public Paginated(List<T> data, string cursor, int totalCount, bool hasNextPage)
        {
            Data = data;
            Cursor = cursor;
            TotalCount = totalCount;
            HasNextPage = hasNextPage;
        }

        [GraphQLNonNullType]
        public List<T> Data { get; set; }

        public string Cursor { get; set; }

        public int TotalCount { get; set; }

        public bool HasNextPage { get; set; }
 
    }
}
