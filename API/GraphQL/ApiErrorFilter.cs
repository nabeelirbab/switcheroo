using HotChocolate;

namespace API.GraphQL
{
    public class ApiErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            if (error.Exception is ApiException apiException)
            {
                return ErrorBuilder.New()
                    .SetMessage(apiException.Message ?? "An error occurred.")
                    .Build();
            }

            if (error.Exception != null && !string.IsNullOrWhiteSpace(error.Exception.Message))
            {
                return ErrorBuilder.New()
                    .SetMessage(error.Exception.Message)
                    .Build();
            }
            if (error.Message != null)
            {
                return ErrorBuilder.New()
                    .SetMessage(error.Message)
                    .Build();
            }

            return ErrorBuilder.New()
                .SetMessage("Whoopsie, a bit of a buggy wuggy :(")
                .Build();
        }
    }
}
