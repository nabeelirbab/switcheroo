using System;

namespace Infrastructure
{
    public class InfrastructureException : ApplicationException
    {
        public string[] Errors { get; private set; }
        public InfrastructureException(params string[] errors) : base(string.Join(";", errors))
        {
            Errors = errors;
        }
    }
}
