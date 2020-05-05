// <copyright file="Startup.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.TeamHistoryManagement.Contracts;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Bots;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Dialogs;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.MessagingExtensions;
    using Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Services;

    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">configuration</param>
        public Startup(IConfiguration configuration)
        {
            AppSettings = new AppSettings(configuration);
        }

        private IAppSettings AppSettings { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // app settings as singleton
            services.AddSingleton<IAppSettings>(AppSettings);
            services.AddSingleton(new TenantInfo(AppSettings.TenantId));

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            // services.AddSingleton<IStorage, MemoryStorage>();
            services.AddSingleton<IStorage, AzureBlobStorage>(s => new AzureBlobStorage(AppSettings.BlobStorageConnectionString, AppSettings.BlobStorageContainerName));

            // Add Application Credentials
            services.AddSingleton(new MicrosoftAppCredentials(AppSettings.MicrosoftAppId, AppSettings.MicrosoftAppPassword));

            services.AddSingleton<PrivateChat>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Service which provides main methods for working with reports generation like get messages, parse messages, upload reports to OneDrive
            services.AddSingleton<HistoryReportService>();

            // Messaging extension which fetch tasks module for preparing messages history report without added bot in teams
            services.AddSingleton<ExtractHistoryMessagingExtension>();

            if (AppSettings.UseCardUpdating)
            {
                services.AddSingleton<IHistoryDialog, HistoryUpdateDialog>();
            }
            else
            {
                services.AddSingleton<IHistoryDialog, HistoryDialog>();
            }

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, HistoryLoaderTeamsBot>();
        }
    }
}
