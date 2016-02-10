using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Content
{
	/// <summary>
	/// Basic interface for Internet Content encoders. A class implementing this interface and having a default constructor, will be able
	/// to partake in object encodings through the static <see cref="InternetContent"/> class. No registration is required.
	/// </summary>
	public interface IContentConverter
	{
		/// <summary>
		/// Converts content from these content types.
		/// </summary>
		string[] FromContentTypes
		{
			get;
		}

		/// <summary>
		/// Converts content to these content types. 
		/// </summary>
		string[] ToContentTypes
		{
			get;
		}

		/// <summary>
		/// How well the content is converted.
		/// </summary>
		Grade ConversionGrade
		{
			get;
		}

		/// <summary>
		/// Performs the actual conversion.
		/// </summary>
		/// <param name="FromContentType">Content type of the content to convert from.</param>
		/// <param name="From">Stream pointing to binary representation of content.</param>
		/// <param name="FromFileName">If the content is coming from a file, this parameter contains the name of that file. 
		/// Otherwise, the parameter is the empty string.</param>
		/// <param name="ResourceName">Local resource name of file, if accessed from a web server.</param>
		/// <param name="ToContentType">Content type of the content to convert to.</param>
		/// <param name="To">Stream pointing to where binary representation of content is to be sent.</param>
		void Convert(string FromContentType, Stream From, string FromFileName, string ResourceName, string ToContentType, Stream To);
	}
}
