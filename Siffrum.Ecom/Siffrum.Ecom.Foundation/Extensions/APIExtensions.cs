using AutoMapper;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using Siffrum.Ecom.BAL.ExceptionHandler;
using Siffrum.Ecom.BAL.Foundation;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.BAL.Foundation.Config;
using Siffrum.Ecom.BAL.Foundation.Web;
using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.Foundation.AutoMapperBindings;
using Siffrum.Ecom.Foundation.Foundation.Web.Security;
using Siffrum.Ecom.Foundation.Security;
using Siffrum.Ecom.ServiceModels.AppUser;
using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base.Interfaces;

namespace Siffrum.Ecom.Foundation.Extensions
{
    public static class APIExtensions
    {
        public static void ConfigureCommonApplicationDependencies(this IServiceCollection services, IConfiguration baseConfiguration, APIConfiguration configObject)
        {
            #region Register Application Identification

            services.AddSingleton<ApplicationIdentificationRoot>((x) =>
            {
                var appIdentification = new ApplicationIdentificationRoot();
                baseConfiguration.GetRequiredSection("ApplicationIdentification").Bind(appIdentification);
                return appIdentification;
            });

            #endregion Application Identification

            #region Register Mapper

            services.AddSingleton<AutoMapper.IConfigurationProvider>(x =>
            {
                var config = new MapperConfiguration(
                    cfg =>
                    {
                        cfg.ConstructServicesUsing(t => x.GetService(t));
                        cfg.AddProfile(new AutoMapperDefaultProfile(x));
                    });
                return config;
            });
            services.AddSingleton(x =>
            {
                var config = x.GetRequiredService<AutoMapper.IConfigurationProvider>();
                return config.CreateMapper();
            });

            #endregion Register Mapper

            #region Register Logger

            #endregion Register Logger

            #region Register Context Accessor

            services.AddHttpContextAccessor();
            //services.AddScoped<APIExceptionFilter>();
            #endregion Register Context Accessor

            #region Stripe
            //Stripe.StripeConfiguration.ApiKey = configObject.StripeSettings.PrivateKey;
            #endregion Stripe

            #region Register Base Configuration

            #endregion Register Base Configuration

            #region API Authentication

            //Register Auth
            services.Configure<SiffrumAuthenticationSchemeOptions>(x => x.JwtTokenSigningKey = configObject.JwtTokenSigningKey);

            //Auth // can use issuer constructor if we want seperate issuer for qa,reg and prod etc
            services.AddSingleton(x => new JwtHandler(configObject.JwtIssuerName));

            services.AddAuthentication(o =>
            {
                o.DefaultScheme = SiffrumBearerTokenAuthHandlerRoot.DefaultSchema;
            })
                .AddScheme<SiffrumAuthenticationSchemeOptions, APIBearerTokenAuthHandler>(SiffrumBearerTokenAuthHandlerRoot.DefaultSchema, o => { })
                // Uncomment for Cookie Authentication , see CookieController for more info
                //.AddCookie((x) =>
                //{
                //    x.LoginPath = "/Cookie/ClientLogin";
                //    x.TicketDataFormat = new CustomSecureDateFormatter(JwtHandler, objAuthDecryptionConfiguration);
                //})
                ;

            services.AddSingleton<IPasswordEncryptHelper>((x) => new PasswordEncryptHelper(configObject.AuthTokenEncryptionKey, configObject.AuthTokenDecryptionKey));


            #endregion

            #region AutoRegister All Process

            services.AutoRegisterAllBALAsSelfFromBaseTypes<SiffrumBalBase>(ServiceLifetime.Scoped);
            /*services.AddSingleton<TextAnalyticsClient>(sp =>
            {
                string endpoint = configObject.ExternalIntegrations.AzureConfiguration.TextAnalyticsConfiguration.EndPoint;
                string key = configObject.ExternalIntegrations.AzureConfiguration.TextAnalyticsConfiguration.ApiKey;

                var credential = new AzureKeyCredential(key);
                return new TextAnalyticsClient(new Uri(endpoint), credential);
            });*/
            services.AddHttpClient();


            #endregion AutoRegister All Process

            #region Register Swagger
            if (configObject.IsSwaggerEnabled)
            {
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(option =>
                {
                    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Siffrum Ecom API", Version = "v1" });
                    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Enter Token Only (Without 'Bearer')",
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        BearerFormat = "JWT",
                        Scheme = "Bearer"
                    });
                    option.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type=ReferenceType.SecurityScheme,
                                    Id="Bearer"
                                }
                            },
                            new string[]{}
                        }
                    });
                });
            }
            #endregion Register Swagger

            #region  To Enable Cors

            if (configObject.IsCorsEnabled)
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("AllowAllPolicy",
                        builder =>
                        {
                            builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            //.AllowCredentials()
                            ;
                        });
                });
            }

            #endregion  To Enable Cors

            #region LoggedInUser

            services.AddScoped<ILoginUserDetail>(x =>
            {
                var user = x.GetService<IHttpContextAccessor>().HttpContext.User;
                if (user != null && user.Identity.IsAuthenticated)
                {
                    if (user.IsInRole(RoleTypeSM.SuperAdmin.ToString()) || user.IsInRole(RoleTypeSM.SystemAdmin.ToString()) || user.IsInRole(RoleTypeSM.User.ToString()))
                    {
                        var u = new LoginUserDetail();
                        u.DbRecordId = user.GetUserRecordIdFromCurrentUserClaims();
                        u.LoginId = user.Identity.Name;
                        u.UserType = Enum.Parse<RoleTypeSM>(user.GetUserRoleTypeFromCurrentUserClaims());
                        return u;
                    }
                    else if (user.IsInRole(RoleTypeSM.Seller.ToString()) || user.IsInRole(RoleTypeSM.DeliveryBoy.ToString()))
                    {
                        var u = new LoginUserDetailWithAdmin();
                        u.DbRecordId = user.GetUserRecordIdFromCurrentUserClaims();
                        u.LoginId = user.Identity.Name;
                        u.UserType = Enum.Parse<RoleTypeSM>(user.GetUserRoleTypeFromCurrentUserClaims());
                        u.AdminId = user.GetUserAdminIdFromCurrentUserClaims();
                        //u.CompanyRecordId = user.GetCompanyRecordIdFromCurrentUserClaims();
                        //u.CompanyCode = user.GetCompanyCodeFromCurrentUserClaims();
                        return u;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                return new LoginUserDetail() { DbRecordId = 0, LoginId = "nullUser", UserType = RoleTypeSM.Unknown };
            });

            #endregion LoggedInUser

            #region Register Error Handler

            services.AddSingleton<ErrorLogHandlerRoot>(x =>
            {
                var appId = x.GetService<ApplicationIdentificationRoot>();                
                var errorBal = new ErrorLogProcessRoot(configObject.ApiDbConnectionString, appId);
                return new ErrorLogHandlerRoot(configObject, errorBal, appId);
            });

            #endregion Register Error Handler
        }


        public static void ConfigureCommonInPipeline(this IApplicationBuilder app, APIConfiguration configObject)
        {
            //To Enable Cors
            if (configObject.IsCorsEnabled)
            {
                app.UseCors("AllowAllPolicy");
            }

            if (configObject.IsSwaggerEnabled)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
        }

        #region Register Edmx Model
        public static IEdmModel GetEdmModel(this IServiceProvider serviceProvider)
        {
            ODataConventionModelBuilder builder = new();
            builder.EntitySet<ClientUserSM>(nameof(ClientUserSM));
            return builder.GetEdmModel();
        }

        #endregion Register Edmx Model
    }
}
