

using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.DAL.Context;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.OData;
using Siffrum.Ecom.Foundation.Extensions;
using Siffrum.Ecom.BAL.Base.Seeder;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace Siffrum.Ecom.Foundation
{
    public partial class Startup
    {
        public IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            /*var builder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory()) // Ensure correct path
                .AddJsonFile("/etc/secrets/appSettings.Production.json", optional: true, reloadOnChange: true) // Load from Render Secret Files
                .AddEnvironmentVariables(); // Load env variables

            Configuration = builder.Build();*/
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var configObject = new APIConfiguration();
            var mailSettings = new SmtpMailSettings();
            var appleAuth = new AppleAuth();
            var razorPaySettings = new RazorpaySettings();
            var googleCloudLocation = new GoogleCloudLocation();
            var oneSignal = new OneSignalSettings();
            var smsSettings = new SmsSettings();
            //var externalIntegrations = new ExternalIntegrations();

            // Bind from appsettings.json or Environment Variables
            Configuration.GetRequiredSection("APIConfiguration").Bind(configObject);
            //Configuration.GetRequiredSection("ExternalIntegrations").Bind(externalIntegrations);
            Configuration.GetRequiredSection("SmtpMailSettings").Bind(mailSettings);
            Configuration.GetRequiredSection("AppleAuth").Bind(appleAuth);
            Configuration.GetRequiredSection("RazorpaySettings").Bind(razorPaySettings);
            Configuration.GetRequiredSection("GoogleCloudLocation").Bind(googleCloudLocation);
            Configuration.GetRequiredSection("OneSignalSettings").Bind(oneSignal);
            Configuration.GetRequiredSection("SmsSettings").Bind(smsSettings);

            configObject.SmtpMailSettings = mailSettings;
            configObject.AppleAuth = appleAuth;
            configObject.RazorpaySettings = razorPaySettings;
            configObject.GoogleCloudLocation = googleCloudLocation;
            configObject.OneSignalSettings = oneSignal;
            configObject.SmsSettings = smsSettings;
            //configObject.ExternalIntegrations = externalIntegrations;

            // Override values with environment variables from Render
            configObject.ApiDbConnectionString = Environment.GetEnvironmentVariable("ApiDbConnectionString") ?? configObject.ApiDbConnectionString;
            configObject.JwtTokenSigningKey = Environment.GetEnvironmentVariable("JwtTokenSigningKey") ?? configObject.JwtTokenSigningKey;

            services.AddSingleton<APIConfiguration>(configObject);
            services.ConfigureCommonApplicationDependencies(Configuration, configObject);
            InitializeFirebase();
            RegisterAllThirdParties(services);

            var mvcBuilder = services.AddControllers(x =>
            {
                x.Filters.Add<APIExceptionFilter>();
                x.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                opt.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.None;
                opt.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            });

            if (configObject.IsOdataEnabled)
            {
                mvcBuilder.AddOData((opt, x) =>
                {
                    opt.AddRouteComponents("v1", x.GetEdmModel())
                    .Filter().Select().Expand().OrderBy().SetMaxTop(100).SkipToken().Count();
                });
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, APIConfiguration configObject)
        {
            // Auto-apply pending EF Core migrations on startup
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                dbContext.Database.Migrate();

                // Sync SuperAdmin from appsettings (creates if missing, updates if changed)
                var seeder = scope.ServiceProvider.GetRequiredService<SeederProcess>();
                seeder.SeedData().GetAwaiter().GetResult();
            }

            app.ConfigureCommonInPipeline(configObject);
            EnsureDirectoriesExist(env);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, "content")),
            });

            app.Use(async (context, next) =>
            {
                context.Request.GetOrAddTracingId();
                await next.Invoke();
            });

            app.UseRouting();
            if (configObject.IsCorsEnabled)
            {
                app.UseCors("AllowAllPolicy");
            }
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void RegisterAllThirdParties(IServiceCollection services)
        {
            services.AddDbContextPool<ApiDbContext>((provider, options) =>
            {
                var configuration = provider.GetService<APIConfiguration>();
                var connectionString = Environment.GetEnvironmentVariable("ApiDbConnectionString") ?? configuration.ApiDbConnectionString;

                //options.UseSqlServer(connectionString);
                options.UseNpgsql(connectionString);

                options.EnableSensitiveDataLogging();
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
            });
        }

        private void EnsureDirectoriesExist(IWebHostEnvironment env)
        {
            string[] directories = new string[]
            {
                Path.Combine(env.WebRootPath, "website"),
                Path.Combine(env.WebRootPath, "website/superadmin"),
                Path.Combine(env.WebRootPath, "website/superadmin/browser"),
                Path.Combine(env.WebRootPath, "website/end-user"),
                Path.Combine(env.WebRootPath, "website/end-user/browser")
            };

            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }
        private void InitializeFirebase()
        {
            if (FirebaseApp.DefaultInstance != null)
                return;

            GoogleCredential credential;

            var credJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");
            if (!string.IsNullOrWhiteSpace(credJson))
            {
                credential = GoogleCredential.FromJson(credJson);
            }
            else
            {
                var fireBasePath = @"wwwroot/content/firebase/firebase.json";
                var path = Path.Combine(Directory.GetCurrentDirectory(), fireBasePath);
                credential = GoogleCredential.FromFile(path);
            }

            FirebaseApp.Create(new AppOptions { Credential = credential });
        }
    }
}
