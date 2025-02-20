using System;
using System.IO;
using Waher.Script;

namespace Waher.Content
{
	/// <summary>
	/// Contains the state of a content conversion process.
	/// </summary>
	public class ConversionState
	{
		/// <summary>
		/// Contains the state of a content conversion process.
		/// </summary>
		/// <param name="FromContentType">Content type of the content to convert from.</param>
		/// <param name="From">Stream pointing to binary representation of content.</param>
		/// <param name="FromFileName">If the content is coming from a file, this parameter contains the name of that file. 
		/// Otherwise, the parameter is the empty string.</param>
		/// <param name="LocalResourceName">Local resource name of file, if accessed from a web server.</param>
		/// <param name="URL">URL of resource, if accessed from a web server.</param>
		/// <param name="ToContentType">Content type of the content to convert to. This value might be changed, in case
		/// the converter finds a better option.</param>
		/// <param name="To">Stream pointing to where binary representation of content is to be sent.</param>
		/// <param name="Session">Session states.</param>
		/// <param name="Progress">Optional interface for reporting progress during conversion.</param>
		/// <param name="PossibleContentTypes">Possible content types the converter is allowed to convert to. 
		/// Can be null if there are no alternatives.</param>
		public ConversionState(string FromContentType, Stream From, string FromFileName, string LocalResourceName, string URL,
			string ToContentType, Stream To, Variables Session, ICodecProgress Progress, params string[] PossibleContentTypes)
		{
			this.FromContentType = FromContentType;
			this.From = From;
			this.FromFileName = FromFileName;
			this.LocalResourceName = LocalResourceName;
			this.URL = URL;
			this.ToContentType = ToContentType;
			this.To = To;
			this.Session = Session;
			this.Progress = Progress;
			this.PossibleContentTypes = PossibleContentTypes;
		}

		/// <summary>
		/// Content type of the content to convert from.
		/// </summary>
		public string FromContentType { get; }

		/// <summary>
		/// Stream pointing to binary representation of content.
		/// </summary>
		public Stream From { get; }

		/// <summary>
		/// If the content is coming from a file, this parameter contains the name of that file. 
		/// Otherwise, the parameter is the empty string.
		/// </summary>
		public string FromFileName { get; set; }

		/// <summary>
		/// Local resource name of file, if accessed from a web server.
		/// </summary>
		public string LocalResourceName { get; set; }

		/// <summary>
		/// URL of resource, if accessed from a web server.
		/// </summary>
		public string URL { get; }

		/// <summary>
		/// Content type of the content to convert to. This value might be changed, in case
		/// the converter finds a better option. May get updated during the conversion process.
		/// </summary>
		public string ToContentType { get; set; }

		/// <summary>
		/// Stream pointing to where binary representation of content is to be sent.
		/// </summary>
		public Stream To { get; }

		/// <summary>
		/// Session states.
		/// </summary>
		public Variables Session { get; }

		/// <summary>
		/// Optional interface for reporting progress during conversion.
		/// </summary>
		public ICodecProgress Progress { get; }

		/// <summary>
		/// Possible content types the converter is allowed to convert to. 
		/// Can be null if there are no alternatives.
		/// </summary>
		public string[] PossibleContentTypes { get; }

		/// <summary>
		/// Error response, in case conversion should return an error.
		/// </summary>
		public Exception Error { get; set; }

		/// <summary>
		/// If an error should be returned.
		/// </summary>
		public bool HasError => !(this.Error is null);
	}
}
