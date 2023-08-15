using System;
using System.Collections.Generic;

namespace API.GraphQL
{
    public class ApiException : ApplicationException
    {
        public string[] Errors { get; private set; }

        public ApiException(params string[] errors) : base(string.Join(";", errors))
        {
            Errors = errors;
        }

        public static string[] CombineErrors(string error, string[] additionalErrors)
        {
            var retVal = new List<string>();
            retVal.AddRange(additionalErrors);
            retVal.Add(error);

            return retVal.ToArray();
        }
    }
}
