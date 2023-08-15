using HotChocolate;

namespace API.GraphQL
{
    public class InfrastructureErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            if (!(error.Exception is Infrastructure.InfrastructureException infraException))
            {
                if (string.IsNullOrWhiteSpace(error.Message))
                {
                    return error.WithMessage("Whoopsie, a bit of a buggy wuggy :(");
                }

                return error;
            }

            return ErrorBuilder.New()
                .SetMessage(infraException.Message ?? "")
                .Build();
        }
    }
}