using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SkiaSharp;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Runtime.Inventory;
using Waher.Script;

namespace Waher.Content.Markdown.Web.WebScript
{
	/// <summary>
	/// Converts Web Script-files to desired output, by evaluating the web script and encoding the results in accordance with accept headers.
	/// </summary>
	public class WsToX : IContentConverter
	{
		/// <summary>
		/// Converts Web Script-files to desired output, by evaluating the web script and encoding the results in accordance with accept headers.
		/// </summary>
		public WsToX()
		{
		}

		/// <summary>
		/// Converts content from these content types.
		/// </summary>
		public string[] FromContentTypes => new string[] { "application/x-webscript" };

		/// <summary>
		/// Converts content to these content types. 
		/// </summary>
		public string[] ToContentTypes => InternetContent.CanEncodeContentTypes;

		/// <summary>
		/// How well the content is converted.
		/// </summary>
		public Grade ConversionGrade => Grade.Ok;

		/// <summary>
		/// Performs the actual conversion.
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
		/// <param name="PossibleContentTypes">Possible content types the converter is allowed to convert to. 
		/// Can be null if there are no alternatives.</param>
		/// <returns>If the result is dynamic (true), or only depends on the source (false).</returns>
		public bool Convert(string FromContentType, Stream From, string FromFileName, string LocalResourceName, string URL,
			ref string ToContentType, Stream To, Variables Session, params string[] PossibleContentTypes)
		{
			DateTime TP = File.GetLastWriteTime(FromFileName);
			Expression Exp = null;

			lock (parsed)
			{
				if (parsed.TryGetValue(FromFileName, out KeyValuePair<Expression, DateTime> Rec) && TP == Rec.Value)
					Exp = Rec.Key;
			}

			if (Exp is null)
			{
				string Script;

				using (StreamReader rd = new StreamReader(From))
				{
					Script = rd.ReadToEnd();
				}

				Exp = new Expression(Script);

				lock (parsed)
				{
					parsed[FromFileName] = new KeyValuePair<Expression, DateTime>(Exp, TP);
				}
			}

			if (Session is null)
				Session = new Variables();

			object Result = Exp.Evaluate(Session);

			if (!InternetContent.Encodes(Result, out Grade _, out IContentEncoder Encoder, ToContentType))
			{
				bool AlternativeFound = false;

				if (!(PossibleContentTypes is null))
				{
					foreach (string Alternative in PossibleContentTypes)
					{
						if (Alternative != ToContentType &&
							InternetContent.Encodes(Result, out Grade _, out Encoder, Alternative))
						{
							ToContentType = Alternative;
							AlternativeFound = true;
							break;
						}
					}
				}

				if (!AlternativeFound)
					throw new NotAcceptableException("Unable to encode objects of type " + Result.GetType().FullName + " to Internet Content Type " + ToContentType);
			}

			byte[] Data = Encoder.Encode(Result, Encoding.UTF8, out string _, ToContentType);
			To.Write(Data, 0, Data.Length);

			return true;
		}

		private static readonly Dictionary<string, KeyValuePair<Expression, DateTime>> parsed = new Dictionary<string, KeyValuePair<Expression, DateTime>>();

	}
}
