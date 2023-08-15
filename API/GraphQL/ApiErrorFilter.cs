using HotChocolate;

namespace API.GraphQL
{
    public class ApiErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            if (!(error.Exception is ApiException apiException))
            {
                if (string.IsNullOrWhiteSpace(error.Message)) {
                    return error.WithMessage("Whoopsie, a bit of a buggy wuggy :(");
                }

                return error;
            }

            return ErrorBuilder.New()
                .SetMessage(apiException.Message ?? "")
                .Build();
        }
    }
}
