// <copyright file="IntentStrings.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Common
{
    using System.Collections.Generic;

    /// <summary>
    /// Describes list of intents for teams bot messages recognizer.
    /// </summary>
    internal static class IntentStrings
    {
        public static List<string> HelpCommands { get; } = new List<string>
            {
                "I need a help",
                "Help me",
                "Help",
                "\\?",
            };

        public static List<string> CancelCommands { get; } = new List<string>
            {
                "Cancel",
                "Abort",
                "Stop",
            };

        public static List<string> LogoutCommands { get; } = new List<string>
            {
                "I want to logout",
                "Logout",
            };

        public static List<string> All
        {
            get
            {
                var commands = new List<string>();
                commands.AddRange(HelpCommands);
                commands.AddRange(LogoutCommands);
                commands.AddRange(CancelCommands);
                return commands;
            }
        }
    }
}
