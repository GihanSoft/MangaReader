using GihanSoft.Navigation;

using LiteDB;

using MangaReader.Data;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.IO;
using System.Reflection;

namespace MangaReader.Startup
{
    internal class ServiceConfigurer
    {
        private const string ConnectionStringKey = "ConnectionString";

        private readonly IConfiguration configuration;

        public ServiceConfigurer(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            AddVersion(services);
            services.AddScoped<PageNavigator>();
            AddConnectionString(services);
            services.AddScoped<DataDb>();
            services.AddScoped<SettingsManager>();
        }

        private void AddConnectionString(IServiceCollection services)
        {
            var connectionString = configuration
                .GetSection(ConnectionStringKey)
                .Get<ConnectionString>();
            connectionString ??= new ConnectionString
            {
                Upgrade = true,
                Connection = ConnectionType.Shared,
                Filename = Path.Combine(
                        /*Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),*/
                        "%appdata%",
                        @"GihanSoft\MangaReader",
                        "data.litedb"),
            };
            connectionString.Filename = Environment.ExpandEnvironmentVariables(connectionString.Filename);
            var dirPath = Path.GetDirectoryName(connectionString.Filename);
            if (dirPath is not null)
            {
                Directory.CreateDirectory(dirPath);
            }
            services.AddSingleton(connectionString);
        }

        private static void AddVersion(IServiceCollection services)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version is not null)
            {
                services.AddSingleton(version);
            }
        }
    }
}
