using System;
using System.Threading.Tasks;
using API.GraphQL;
using Domain.Users;
using Microsoft.AspNetCore.Mvc;

//TODO remove this class. Not required
namespace API.Http
{
    public class UserRegistration : Controller
    {
        private readonly IUserRegistrationService userRegistrationService;

        public UserRegistration(IUserRegistrationService userRegistrationService)
        {
            this.userRegistrationService = userRegistrationService;
        }
    }
}
