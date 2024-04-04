using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Users;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.UserManagement
{
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly SignInManager<Database.Schema.User> signInManager;
        private readonly UserManager<Database.Schema.User> userManager;
        private readonly IUserRepository userRepository;
        private readonly HttpClient httpClient;

        public UserAuthenticationService(SignInManager<Database.Schema.User> signInManager, UserManager<Database.Schema.User> userManager, IUserRepository userRepository, HttpClient httpClient)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.userRepository = userRepository;
            this.httpClient = httpClient;
        }

        public async Task<User> GetCurrentlySignedInUserAsync(ClaimsPrincipal principal)
        {
            if (!principal?.Identity?.IsAuthenticated ?? false) throw new InfrastructureException("Unauthenticated");

            var user = await userManager.GetUserAsync(principal);
            if (user == null) throw new InfrastructureException("Unauthenticated");

            return await userRepository.GetById(user.Id);
        }

        public async Task<Guid> SignInAsync(string email, string password)
        {
            var result = await signInManager.PasswordSignInAsync(email, password, true, false);

            if (result.Succeeded)
            {
                var successUser = await signInManager.UserManager.FindByEmailAsync(email);

                return successUser.Id;
            }

            var user = await signInManager.UserManager.FindByEmailAsync(email);

            // if email is invalid
            if (user == null) throw new InfrastructureException("Invalid credentials");
            else
            {
                // if email is valid but not confirmed
                if (!user.EmailConfirmed)
                {
                    throw new InfrastructureException("Account not Actived");
                }
                // if email is valid but password id wrong
                else
                {
                    throw new InfrastructureException("Invalid credentials");
                }
            }
        }

        public async Task<Guid> SignInByEmailAsync(string email)
        {
            var domainUser = await userRepository.GetByEmail(email);
            var infraUser = Infrastructure.Database.Schema.User.FromDomain(domainUser);
            await signInManager.SignInAsync(infraUser, true);
            var user = await signInManager.UserManager.FindByEmailAsync(email);
            if (user == null) throw new InfrastructureException("Invalid credentials");
            //else if (!user.EmailConfirmed)
            //{
            //    throw new InfrastructureException("Account not Actived");
            //}
            return user.Id;
        }


        public async Task<Guid> SignOutAsync(ClaimsPrincipal principal)
        {
            var isAuthenticated = principal?.Identity?.IsAuthenticated;
            if (isAuthenticated == null || isAuthenticated == false)
            {
                throw new InfrastructureException("Unauthenticated");
            }

            var user = await userManager.GetUserAsync(principal);
            if (user == null) throw new InfrastructureException("Unauthenticated");

            await signInManager.SignOutAsync();

            return user.Id;
        }

        public async Task<Tuple<bool, string, string>> AuthenticateGoogleAsync(string idToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            bool userStatus = await userRepository.CheckIfUserByEmail(payload.Email);
            return new Tuple<bool, string, string>(userStatus, payload.Name, payload.Email);
            //return new Tuple<bool, string, string>(false, "Hamza Muhammad Farooqi", idToken);
        }

        public async Task<Tuple<bool, string, string>> AuthenticateFacebookAsync(string accessToken)
        {
            var requestUri = $"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}";

            var response = await this.httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to retrieve Facebook user profile. Status code: {response.StatusCode}");
            }
            var content = await response.Content.ReadAsStringAsync();
            var userProfile = JsonConvert.DeserializeObject<Infrastructure.DTOs.FacebookUserProfile>(content);
            bool userStatus = await userRepository.CheckIfUserByEmail(userProfile.Email);
            return new Tuple<bool, string, string>(userStatus, userProfile.Name, userProfile.Email);
        }
        public async Task<Tuple<bool, bool, string>> AuthenticateAppleAsync(string token)
        {
            const string ApplePublicKeysUrl = "https://appleid.apple.com/auth/keys";
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(ApplePublicKeysUrl);
            var appleKeys = JsonConvert.DeserializeObject<ApplePublicKeys>(response);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Extract the kid from the JWT header
            var kid = jwtToken.Header.Kid;
            var emailClaim = jwtToken.Payload.Claims.FirstOrDefault(c => c.Type == "email");
            var email = emailClaim?.Value;
            if (string.IsNullOrEmpty(kid))
            {
                Console.WriteLine("JWT does not contain 'kid' in header.");
                return new Tuple<bool, bool, string>(false, false, "");
            }
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("JWT does not contain 'email' in payload.");
                return new Tuple<bool, bool, string>(false, false, "");
            }

            // Find the matching key
            var matchingKey = appleKeys.Keys.FirstOrDefault(k => k.Kid == kid);
            if (matchingKey == null)
            {
                Console.WriteLine($"No matching public key found for kid: {kid}");
                return new Tuple<bool, bool, string>(false, false, "");
            }

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(matchingKey.ToRSAParameters()),
                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",
                ValidateAudience = true,
                ValidAudience = "com.switchceroo.ios",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var claimsPrincipal = handler.ValidateToken(token, validationParameters, out var validatedToken);

                // Token is valid, and you can work with the claims
                foreach (var claim in claimsPrincipal.Claims)
                {
                    Console.WriteLine($"{claim.Type}: {claim.Value}");
                }
                bool userStatus = await userRepository.CheckIfUserByEmail(email);
                return new Tuple<bool, bool, string>(userStatus, true, email);
            }
            catch (SecurityTokenException e)
            {
                Console.WriteLine($"Token validation failed: {e.Message}");
                return new Tuple<bool, bool, string>(false, false, "");
            }
        }

    }
    public class ApplePublicKey
    {
        public string Kty { get; set; }
        public string Kid { get; set; }
        public string Use { get; set; }
        public string Alg { get; set; }
        public string N { get; set; }
        public string E { get; set; }
        public RSAParameters ToRSAParameters()
        {
            // Convert Base64URL to Base64
            string base64N = Base64UrlEncoderToBase64(N);
            string base64E = Base64UrlEncoderToBase64(E);

            // Convert Base64 to byte array
            byte[] nBytes = Convert.FromBase64String(base64N);
            byte[] eBytes = Convert.FromBase64String(base64E);

            return new RSAParameters
            {
                Modulus = nBytes,
                Exponent = eBytes
            };
        }
        private static string Base64UrlEncoderToBase64(string base64Url)
        {
            // Replace URL-safe characters and add padding if needed
            string paddedBase64 = base64Url.Replace('-', '+').Replace('_', '/');
            switch (paddedBase64.Length % 4)
            {
                case 2: paddedBase64 += "=="; break;
                case 3: paddedBase64 += "="; break;
            }
            return paddedBase64;
        }

    }

    public class ApplePublicKeys
    {
        public List<ApplePublicKey> Keys { get; set; }
    }
}
