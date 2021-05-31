// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;

using GihanSoft.Navigation;

using LiteDB;

using MangaReader.Data;

using Microsoft.Extensions.DependencyInjection;

namespace MangaReader.Bootstrap.Startup
{
    public class ServiceConfigurer
    {
        public const string CompanyNameKey = "CompanyName";
        public const string ProductNameKey = "ProductName";

        public void ConfigureServices(IServiceCollection services)
        {
            AddVersion(services);
            services.AddScoped<PageNavigator>();
            AddConnectionString(services);
            services.AddSingleton<LiteDatabase>();
            services.AddSingleton<DataDb>();
            services.AddScoped<SettingsManager>();
        }

        private void AddConnectionString(IServiceCollection services)
        {
            ConnectionString connectionString = new()
            {
                Upgrade = true,
                Connection = ConnectionType.Shared,
                Filename = Path.Combine(
                    @$"%appdata%\%{CompanyNameKey}%\%{ProductNameKey}%",
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
