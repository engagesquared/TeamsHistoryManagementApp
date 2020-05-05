// <copyright file="CardSubmitResult.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.TeamsBot.Helpers
{
    using Microsoft.Bot.Builder.Dialogs;
    using Newtonsoft.Json;

    public class CardSubmitResult<T>
    {
        public T Result { get; set; }

        public static CardSubmitResult<T> Get(DialogContext context)
        {
            CardSubmitResult<T> result = null;
            try
            {
                result = JsonConvert.DeserializeObject<CardSubmitResult<T>>(context.Context.Activity.Value.ToString());
            }
            catch
            {
                return null;
            }

            return result;
        }
    }
}
