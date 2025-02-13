using System;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things.Attributes;

namespace Waher.Things.Script.Parameters
{
    /// <summary>
    /// Represents a media-valued script parameter.
    /// </summary>
    public class ScriptMediaParameterNode : ScriptParameterNode
    {
        private string url;
        private string contentType;

        /// <summary>
        /// Represents a media-valued script parameter.
        /// </summary>
        public ScriptMediaParameterNode()
            : base()
        {
        }

        /// <summary>
        /// Media URL
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(77, "URL:")]
        [ToolTip(78, "URL to media.")]
        [Required]
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
        [Page(2, "Script", 100)]
        [Header(63, "Content-Type:")]
        [ToolTip(79, "Content-Type of media.")]
        [Required]
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
        [Page(2, "Script", 100)]
        [Header(80, "Width:")]
        [ToolTip(81, "Width of media.")]
        public ushort? Width { get; set; }

        /// <summary>
        /// Height of media.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(82, "Height:")]
        [ToolTip(83, "Height of media.")]
        public ushort? Height { get; set; }

        /// <summary>
        /// Gets the type name of the node.
        /// </summary>
        /// <param name="Language">Language to use.</param>
        /// <returns>Localized type node.</returns>
        public override Task<string> GetTypeNameAsync(Language Language)
        {
            return Language.GetStringAsync(typeof(ScriptNode), 84, "Media parameter");
        }

        /// <summary>
        /// Populates a data form with parameters for the object.
        /// </summary>
        /// <param name="Parameters">Data form to host all editable parameters.</param>
        /// <param name="Language">Current language.</param>
        /// <param name="Value">Value for parameter.</param>
        public override Task PopulateForm(DataForm Parameters, Language Language, object Value)
        {
            Media Media = new Media(this.url, this.contentType, this.Width, this.Height);
            MediaField Field = new MediaField(this.ParameterName, this.Label, Media);

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