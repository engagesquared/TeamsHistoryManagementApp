// <copyright file="Startup.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.WebApp
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.BotFramework;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Queue;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.Reports;
    using Microsoft.Teams.Apps.TeamHistoryManagement.MSGraphProvider;
    using Microsoft.Teams.Apps.TeamHistoryManagement.ReportsGenerators.Services;
    using Microsoft.Teams.Apps.TeamHistoryManagement.WebApp.Background;
    using Wkhtmltopdf.NetCore;

    public class Startup
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">configuration</param>
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.AddOptions();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddWkhtmltopdf();

            // config as singleton
            services.AddSingleton(configuration);

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Create the Bot Framework Adapter.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            // Graph API implementation for working this chat/teams channel messages and another requests to Graph API
            services.AddSingleton<IMSGraphClient, MSGraphClient>();

            // Report body generator provider which implements report generations.
            services.AddSingleton<IReportGenerator, ReportGeneratorsProvider>();

            // Background services which runs tasks from queque and uses parallelism for executions
            services.AddHostedService<BackgroundQueuedService>();
            services.AddSingleton<IBackgroundQueue, BackgroundTaskQueue>();

            // Run TeamsBot project init
            new TeamsBot.Startup(configuration).ConfigureServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
