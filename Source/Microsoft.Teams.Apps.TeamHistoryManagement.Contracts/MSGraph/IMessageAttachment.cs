// <copyright file="IMessageAttachment.cs" company="Engage Squared">
// Copyright (c) Engage Squared. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.TeamHistoryManagement.Contracts.MSGraph
{
    /// <summary>
    /// Represents an attachment to a chat message entity.
    /// </summary>
    public interface IMessageAttachment
    {
        /// <summary>
        /// Gets the identifier. Read-only. Unique id of the attachment.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        string Id { get; }

        /// <summary>
        /// Gets or sets The media type of the content attachment. It can have the following values:
        ///     reference: Attachment is a link to another file.Populate the contentURL with the link to the object.
        ///     file: Raw file attachment.Populate the contenturl field with the base64 encoding of the file in data: format.
        ///     image/: Image type with the type of the image specified ex: image/png, image/jpeg, image/gif.Populate the contentUrl field with the base64 encoding of the file in data: format.
        ///     video/: Video type with the format specified.Ex: video/mp4.Populate the contentUrl field with the base64 encoding of the file in data: format.
        ///     audio/: Audio type with the format specified. Ex: audio/wmw.Populate the contentUrl field with the base64 encoding of the file in data: format.
        ///     application/card type: Rich card attachment type with the card type specifying the exact card format to use.Set content with the json format of the card.Supported values for card type include:
        ///     application/vnd.microsoft.card.adaptive: A rich card that can contain any combination of text, speech, images,, buttons, and input fields.Set the content property to, an AdaptiveCard object.
        ///     application/vnd.microsoft.card.animation: A rich card that plays animation. Set the content property, to an AnimationCardobject.
        ///     application/vnd.microsoft.card.audio: A rich card that plays audio files.Set the content property, to an AudioCard object.
        ///     application/vnd.microsoft.card.video: A rich card that plays videos. Set the content property, to a VideoCard object.
        ///     application/vnd.microsoft.card.hero: A Hero card.Set the content property to a HeroCard object.
        ///     application/vnd.microsoft.card.thumbnail: A Thumbnail card.Set the content property to a ThumbnailCard object.
        ///     application/vnd.microsoft.com.card.receipt: A Receipt card.Set the content property to a ReceiptCard object.
        ///      application/vnd.microsoft.com.card.signin: A user Sign In card.Set the content property to a SignInCard object.
        /// </summary>
        /// <value>
        /// The type of the content.
        /// </value>
        string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the URL for the content of the attachment. Supported protocols: http, https, file and data.
        /// </summary>
        /// <value>
        /// The content URL.
        /// </value>
        string ContentUrl { get; set; }

        /// <summary>
        /// Gets or sets The content of the attachment. If the attachment is a rich card, set the property to the rich card object. This property and contentUrl are mutually exclusive.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        string Content { get; set; }

        /// <summary>
        /// Gets or sets the name of the attachment.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the URL to a thumbnail image that the channel can use if it supports using an alternative, smaller form of content or contentUrl..
        /// </summary>
        /// <value>
        /// The thumbnail URL.
        /// </value>
        string ThumbnailUrl { get; set; }
    }
}
