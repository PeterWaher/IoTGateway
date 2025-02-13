using System;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Runtime.Language;
using Waher.Script;

namespace Waher.Reports.Files.Model.Parameters
{
    /// <summary>
    /// Represents a media-valued parameter.
    /// </summary>
    public class MediaParameter : ReportParameter
	{
        private string url;
        private string contentType;

		/// <summary>
		/// Represents a media-valued parameter.
		/// </summary>
		/// <param name="Page">Parameter Page</param>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Label">Parameter label.</param>
		/// <param name="Description">Parameter description.</param>
		/// <param name="Required">If parameter is required.</param>
		/// <param name="Url">Media URL</param>
		/// <param name="ContentType">Content-Type of media.</param>
		/// <param name="Width">Width of media.</param>
		/// <param name="Height">Height of media.</param>
		public MediaParameter(string Page, string Name, string Label, string Description,
			bool Required, string Url, string ContentType, ushort? Width, ushort? Height)
			: base(Page, Name, Label, Description, Required)
        {
            this.Url = Url;
			this.ContentType = ContentType;
			this.Width = Width;
			this.Height = Height;
		}

        /// <summary>
        /// Media URL
        /// </summary>
        public string Url 
        {
            get => this.url;
            set
            {
                if (!Uri.TryCreate(value, UriKind.Absolute, out Uri _))
                    throw new NotSupportedException("Invalid URL.");

                this.url = value;
            }
        }

        /// <summary>
        /// Content-Type of media.
        /// </summary>
        public string ContentType 
        {
            get => this.contentType;
            set
            {
                if (!string.IsNullOrEmpty(value) && (
                    !InternetContent.IsAccepted(value, InternetContent.CanDecodeContentTypes) ||
                    !InternetContent.IsAccepted(value, InternetContent.CanEncodeContentTypes)))
                {
                    throw new NotSupportedException("Content-Type not supported.");
                }

                this.contentType = value;
            }
        }

        /// <summary>
        /// Width of media.
        /// </summary>
        public ushort? Width { get; }

        /// <summary>
        /// Height of media.
        /// </summary>
        public ushort? Height { get; }

        /// <summary>
        /// Populates a data form with parameters for the object.
        /// </summary>
        /// <param name="Parameters">Data form to host all editable parameters.</param>
        /// <param name="Language">Current language.</param>
        /// <param name="Value">Value for parameter.</param>
        public override Task PopulateForm(DataForm Parameters, Language Language, object Value)
        {
            Media Media = new Media(this.url, this.contentType, this.Width, this.Height);
            MediaField Field = new MediaField(this.Name, this.Label, Media);

            Parameters.Add(Field);

            Page Page = Parameters.GetPage(this.Page);
            Page.Add(Field);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the parameters of the object, based on contents in the data form.
        /// </summary>
        /// <param name="Parameters">Data form with parameter values.</param>
        /// <param name="Language">Current language.</param>
        /// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
        /// <param name="Values">Collection of parameter values.</param>
        /// <param name="Result">Result set to return to caller.</param>
        /// <returns>Any errors encountered, or null if parameters was set properly.</returns>
        public override Task SetParameter(DataForm Parameters, Language Language, bool OnlySetChanged, Variables Values,
            SetEditableFormResult Result)
        {
            return Task.CompletedTask;
        }

    }
}