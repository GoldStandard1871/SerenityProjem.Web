using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serenity.Extensions.DependencyInjection;
using Serenity.Localization;
using Serenity.Navigation;
using System.Data.Common;
using System.IO;
using Hangfire;
using Hangfire.MemoryStorage;

using Hangfire.SqlServer;
using Hangfire.Dashboard;
using Hangfire.Annotations;
using Serenity.Services;
using Serenity;





namespace SerenityProjem;

public partial class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
    {
        Configuration = configuration;
        HostEnvironment = hostEnvironment;
        RegisterDataProviders();
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment HostEnvironment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationPartsTypeSource();
        services.ConfigureSections(Configuration);

        services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SupportedUICultures = AppServices.UserCultureProvider.SupportedCultures;
            options.SupportedCultures = AppServices.UserCultureProvider.SupportedCultures;
            options.RequestCultureProviders.Insert(Math.Max(options.RequestCultureProviders.Count - 1, 0),
                new AppServices.UserCultureProvider()); // insert it before AcceptLanguage header provider
        });

        var dataProtectionKeysFolder = Configuration?["DataProtectionKeysFolder"];
        if (!string.IsNullOrEmpty(dataProtectionKeysFolder))
        {
            dataProtectionKeysFolder = Path.Combine(HostEnvironment.ContentRootPath, dataProtectionKeysFolder);
            if (Directory.Exists(dataProtectionKeysFolder))
                services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysFolder));
        }

        services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
        services.Configure<KestrelServerOptions>(options => options.AllowSynchronousIO = true);
        services.Configure<IISServerOptions>(options => options.AllowSynchronousIO = true);

        var builder = services.AddControllersWithViews(options =>
        {
            options.Filters.Add(typeof(AutoValidateAntiforgeryIgnoreBearerAttribute));
            options.Filters.Add(typeof(AntiforgeryCookieResultFilterAttribute));
            options.Conventions.Add(new ServiceEndpointActionModelConvention());
            options.ModelMetadataDetailsProviders.Add(new ServiceEndpointBindingMetadataProvider());
        });

        services.Configure<JsonOptions>(options => JSON.Defaults.Populate(options.JsonSerializerOptions));

        services.AddAuthentication(o =>
        {
            o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            o.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }).AddCookie(o =>
        {
            o.Cookie.Name = ".AspNetAuth";
            o.LoginPath = new PathString("/Account/Login/");
            o.AccessDeniedPath = new PathString("/Account/AccessDenied");
            o.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            o.SlidingExpiration = true;
        });

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
        });

        services.AddSingleton<IDataMigrations, AppServices.DataMigrations>();
        services.AddSingleton<IElevationHandler, DefaultElevationHandler>();
        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IHttpContextItemsAccessor, HttpContextItemsAccessor>();
        services.AddSingleton<INavigationModelFactory, AppServices.NavigationModelFactory>();
        services.AddSingleton<IPermissionService, AppServices.PermissionService>();
        services.AddSingleton<IPermissionKeyLister, AppServices.PermissionKeyLister>();
        services.AddSingleton<IRolePermissionService, AppServices.RolePermissionService>();
        services.AddSingleton<IUploadAVScanner, ClamAVUploadScanner>();
        services.AddSingleton<IUserPasswordValidator, AppServices.UserPasswordValidator>();
        services.AddUserProvider<AppServices.UserAccessor, AppServices.UserRetrieveService>();
        services.AddSingleton<Administration.IUserActivityService, Administration.UserActivityService>();
        services.AddScoped<Administration.UserActivityBackgroundJobs>();
        services.AddScoped<Administration.MovieSystem.MovieSystemBackgroundJobs>();
        services.AddSignalR(options =>
        {
            // Optimize timeout settings for faster disconnect detection
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(15); // Default: 30 seconds
            options.KeepAliveInterval = TimeSpan.FromSeconds(7); // Default: 15 seconds
            options.HandshakeTimeout = TimeSpan.FromSeconds(5); // Default: 15 seconds
        });
        services.AddServiceHandlers();
        services.AddDynamicScripts();
        services.AddCssBundling();
        services.AddScriptBundling();
        services.AddUploadStorage();
        services.AddReporting();
        // Hangfire Configuration - SQL Server Storage with optimized settings
        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(Configuration.GetValue<string>("Data:Default:ConnectionString"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        // Hangfire Server with multiple queues for job prioritization
        services.AddHangfireServer(options =>
        {
            options.Queues = new[] { "security", "maintenance", "reports", "monitoring", "default" };
            options.WorkerCount = Math.Max(Environment.ProcessorCount, 2);
        });

    }

    public static void InitializeLocalTexts(IServiceProvider services)
    {
        var env = services.GetRequiredService<IWebHostEnvironment>();
        services.AddBaseTexts(env.WebRootFileProvider)
            .AddJsonTexts(env.WebRootFileProvider, "Scripts/site/texts")
            .AddJsonTexts(env.ContentRootFileProvider, "App_Data/texts");
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        RowFieldsProvider.SetDefaultFrom(app.ApplicationServices);

        var startNodeScripts = Configuration["StartNodeScripts"];
        if (!string.IsNullOrEmpty(startNodeScripts))
        {
            foreach (var script in startNodeScripts.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                app.StartNodeScript(script);
            }
        }

        InitializeLocalTexts(app.ApplicationServices);

        app.UseRequestLocalization();

        if (Configuration["UseForwardedHeaders"] == "True")
            app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        if (!string.IsNullOrEmpty(Configuration["UsePathBase"]))
            app.UsePathBase(Configuration["UsePathBase"]);

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();

        // Service Provider'ı global olarak erişilebilir yap
        Administration.Hangfire.ServiceProviderAccessor.Current = app.ApplicationServices;

        // Custom Hangfire Dashboard configuration
        Administration.Hangfire.HangfireCustomDashboard.ConfigureHangfireDashboard(app, app.ApplicationServices);
        Administration.Hangfire.HangfireCustomDashboard.AddCustomHangfireMetrics();

        // User Activity Background Jobs
        RegisterUserActivityJobs();

        ConfigureTestPipeline?.Invoke(app);

        app.UseDynamicScripts();

        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
            endpoints.MapHub<Administration.UserActivityHub>("/userActivityHub");
            endpoints.MapHub<Administration.SystemMonitor.SystemMonitorHub>("/systemMonitorHub");
        });

        app.ApplicationServices.GetRequiredService<IDataMigrations>().Initialize();

    }



    public static Action<IApplicationBuilder> ConfigureTestPipeline { get; set; }

    /// <summary>
    /// User Activity için Hangfire job'larını kaydet
    /// </summary>
    private static void RegisterUserActivityJobs()
    {
        // Günlük eski kayıtları temizle (her gece 02:00'da)
        RecurringJob.AddOrUpdate<Administration.UserActivityBackgroundJobs>(
            "cleanup-old-activity-records",
            job => job.CleanupOldActivityRecords(),
            "0 2 * * *"); // Her gece saat 02:00

        // Günlük istatistikleri hesapla (her gün 01:00'da)
        RecurringJob.AddOrUpdate<Administration.UserActivityBackgroundJobs>(
            "generate-daily-activity-stats",
            job => job.GenerateDailyActivityStats(),
            "0 1 * * *"); // Her gece saat 01:00

        // Şüpheli aktiviteleri kontrol et (her 15 dakikada)
        RecurringJob.AddOrUpdate<Administration.UserActivityBackgroundJobs>(
            "detect-suspicious-activity",
            job => job.DetectSuspiciousActivity(),
            "*/15 * * * *"); // Her 15 dakikada

        // Haftalık rapor oluştur (her Pazartesi 09:00'da)
        RecurringJob.AddOrUpdate<Administration.UserActivityBackgroundJobs>(
            "generate-weekly-report",
            job => job.GenerateWeeklyReport(),
            "0 9 * * 1"); // Her Pazartesi saat 09:00

        // Online kullanıcı metriklerini logla (her 5 dakikada)
        RecurringJob.AddOrUpdate<Administration.UserActivityBackgroundJobs>(
            "log-online-user-metrics",
            job => job.LogOnlineUserMetrics(),
            "*/5 * * * *"); // Her 5 dakikada

        // MOVIE SYSTEM BACKGROUND JOBS
        
        // Film popülerlik skorlarını güncelle (günde 2 kez - 06:00 ve 18:00)
        RecurringJob.AddOrUpdate<Administration.MovieSystem.MovieSystemBackgroundJobs>(
            "update-movie-popularity-scores",
            job => job.UpdateMoviePopularityScores(),
            "0 6,18 * * *");

        // Film istatistiklerini güncelle (her gece 02:00'da)
        RecurringJob.AddOrUpdate<Administration.MovieSystem.MovieSystemBackgroundJobs>(
            "update-movie-statistics", 
            job => job.UpdateMovieStatistics(),
            "0 2 * * *");

        // Popüler filmler raporu (her Cuma 10:00'da)
        RecurringJob.AddOrUpdate<Administration.MovieSystem.MovieSystemBackgroundJobs>(
            "generate-popular-movies-report",
            job => job.GeneratePopularMoviesReport(),
            "0 10 * * 5");

        // Film veritabanı sağlık kontrolü (her gün 04:00'da)
        RecurringJob.AddOrUpdate<Administration.MovieSystem.MovieSystemBackgroundJobs>(
            "movie-data-health-check",
            job => job.PerformMovieDataHealthCheck(),
            "0 4 * * *");

        // Eski verileri temizle (her Pazar 01:00'da)
        RecurringJob.AddOrUpdate<Administration.MovieSystem.MovieSystemBackgroundJobs>(
            "cleanup-orphaned-movie-data",
            job => job.CleanupOrphanedMovieData(),
            "0 1 * * 0");
    }

    public static void RegisterDataProviders()
    {
        DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);
        DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", Microsoft.Data.Sqlite.SqliteFactory.Instance);

        // to enable FIREBIRD: add FirebirdSql.Data.FirebirdClient reference, set connections, and uncomment line below
        // DbProviderFactories.RegisterFactory("FirebirdSql.Data.FirebirdClient", FirebirdSql.Data.FirebirdClient.FirebirdClientFactory.Instance);

        // to enable MYSQL: add MySql.Data reference, set connections, and uncomment line below
        // DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySql.Data.MySqlClient.MySqlClientFactory.Instance);

        // to enable POSTGRES: add Npgsql reference, set connections, and uncomment line below
        // DbProviderFactories.RegisterFactory("Npgsql", Npgsql.NpgsqlFactory.Instance);
    }
}
