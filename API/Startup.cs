using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Voyager;
using Microsoft.Extensions.Configuration;
using Infrastructure.Database;
using Microsoft.AspNetCore.Identity.UI.Services;
using Infrastructure.Email;
using System;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using Infrastructure.UserManagement;
using Domain.Users;
using API.GraphQL;
using Domain.Categories;
using Infrastructure.Categories;
using Domain.Items;
using Domain.Offers;
using Infrastructure.Items;
using Infrastructure.Offers;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using tusdotnet;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Stores;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Domain.Locations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Infrastructure;

namespace API
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IHostEnvironment Environment { get; }

        public Startup(IHostEnvironment env, IConfiguration configuration)
        {
            this.Environment = env;
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // If you need dependency injection with your query object add your query type as a services.
            // services.AddSingleton<Query>();

            // enable InMemory messaging services for subscription support.
            // services.AddInMemorySubscriptionProvider();

            services.AddControllers();

            // Infastructure
            AddDatabase(services);
            AddEmail(services);
            AddGraphQL(services);
            AddUserManagement(services);

            AddRepositories(services);
        }

        private void AddRepositories(IServiceCollection services)
        {
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IOfferRepository, OfferRepository>();
            services.AddTransient<IItemRepository, ItemRepository>();
            services.AddTransient<IMessageRepository, MessageRepository>();
        }

        private void AddUserManagement(IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.User.RequireUniqueEmail = true;
            });

            services.AddIdentity<Infrastructure.Database.Schema.User, IdentityRole<Guid>>(config =>
            {
                config.SignIn.RequireConfirmedEmail = true;
            })
              .AddEntityFrameworkStores<SwitcherooContext>()
              .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "switcheroo.session";
                options.SlidingExpiration = true;
            });

            services.AddTransient<IUserAuthenticationService, UserAuthenticationService>();
            services.AddTransient<IUserRegistrationService, UserRegistrationService>();
        }

        private void AddEmail(IServiceCollection services)
        {
            services.Configure<SmtpOptions>(Configuration);

            // For now we are always using the dev email sender, which will send to localhost
            // and expect an SMTP server to be present
            services.AddTransient<IEmailSender, GmailEmailSender>();
            // services.AddTransient<IEmailSender, DevEmailSender>();
            // services.AddTransient<IEmailSender, SendGridEmailSender>();

            // if (Environment.IsDevelopment())
            // {
            //     services.AddTransient<IEmailSender, DevEmailSender>();
            // }
            // else
            // {
            //     services.AddTransient<IEmailSender, SendGridEmailSender>();
            // }
        }

        private void AddDatabase(IServiceCollection services)
        {
            services.Configure<PostgresOptions>(Configuration);

            services.AddSingleton<IDbContextConfigurator, PostgresDbConfigurator>();

            services.AddEntityFrameworkNpgsql()
                    .AddDbContext<SwitcherooContext>(ServiceLifetime.Transient)
                    .BuildServiceProvider();
        }

        private void AddGraphQL(IServiceCollection services)
        {
            // this enables you to use DataLoader in your resolvers.
            services.AddDataLoaderRegistry();
            services.AddErrorFilter<ApiErrorFilter>();
            services.AddErrorFilter<InfrastructureErrorFilter>();
            services.AddGraphQL(SchemaBuilder.New()
                .AddAuthorizeDirectiveType()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .ModifyOptions(o => o.RemoveUnreachableTypes = true));
        }

        private static void CreateImageVariant(string location, string quality, int outputWidth, int outputHeight)
        {
            using var image = Image.Load(location);
            image.Mutate(x => x.Resize(outputWidth, outputHeight));
            var path = $"{location}_{quality}";
            image.Save(path);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app
                .UseDeveloperExceptionPage()
                .UseAuthentication()
                .UseRouting()
                .UseWebSockets()
                .UseEndpoints(endpoints => endpoints.MapControllers())
                .UseGraphQL()
                .UsePlayground()
                .UseVoyager()
                .UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        System.IO.Path.Combine(Directory.GetCurrentDirectory(),
                            "Assets")),
                    RequestPath = "/Assets"
                });

            // For now we are always using tus, for prod otherwise we wont be doing this
            if (true || Environment.IsDevelopment())
            {
                var tusDir = System.Environment.GetEnvironmentVariable("TUS_DIR");
            
                var tusdConfig = new DefaultTusConfiguration
                {
                    Store = new TusDiskStore(tusDir),
                    UrlPath = "/files",
                    Events = new Events
                    {
                        OnFileCompleteAsync = async eventContext =>
                        {
                            var file = await eventContext.GetFileAsync();
                            var metaData = await file.GetMetadataAsync(CancellationToken.None);
                            CreateImageVariant($"{tusDir}{file.Id}", "blur", 100, 100);
                        }
                    }
                };

                app
                .UseTus(httpContext => tusdConfig)
                .Run(async (context) =>
                {
                    var requestPath = context.Request.Path.HasValue ? context.Request.Path.Value : null;
                    if (string.IsNullOrWhiteSpace(requestPath)) return;

                    var isTusdDownload = requestPath.StartsWith("/files/", StringComparison.Ordinal);
                    if (!isTusdDownload) return;

                    // Try to get a file id e.g. /files/<fileId>
                    var fileId = context.Request.Path.Value.Replace("/files/", "").Trim();
                    if (string.IsNullOrWhiteSpace(fileId)) return;

                    var store = new TusDiskStore(tusDir);
                    var file = await store.GetFileAsync(fileId, CancellationToken.None);

                    if (file == null)
                    {
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync($"File with id {fileId} was not found.", CancellationToken.None);
                        return;
                    }

                    await using var fileStream = await file.GetContentAsync(CancellationToken.None);
                    var metadata = await file.GetMetadataAsync(CancellationToken.None);

                    // The tus protocol does not specify any required metadata.
                    // "contentType" is metadata that is specific to this domain and is not required.
                    context.Response.ContentType = metadata.ContainsKey("contentType")
                        ? metadata["contentType"].GetString(Encoding.UTF8)
                        : "application/octet-stream";

                    if (metadata.ContainsKey("name"))
                    {
                        var name = metadata["name"].GetString(Encoding.UTF8);
                        context.Response.Headers.Add("Content-Disposition",
                            new[] { $"attachment; filename=\"{name}\"" });
                    }

                    await fileStream.CopyToAsync(context.Response.Body, 81920, CancellationToken.None);
                });
            }

        }
    }
}
