using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
		public string[] ToContentTypes => new string[] { "*" };

		/// <summary>
		/// How well the content is converted.
		/// </summary>
		public Grade ConversionGrade => Grade.Ok;

		/// <summary>
		/// Performs the actual conversion.
		/// </summary>
		/// <param name="State">State of the current conversion.</param>
		/// <returns>If the result is dynamic (true), or only depends on the source (false).</returns>
		public async Task<bool> ConvertAsync(ConversionState State)
		{
			DateTime TP = File.GetLastWriteTime(State.FromFileName);
			Expression Exp = null;

			lock (parsed)
			{
				if (parsed.TryGetValue(State.FromFileName, out KeyValuePair<Expression, DateTime> Rec) && TP == Rec.Value)
					Exp = Rec.Key;
			}

			if (Exp is null)
			{
				string Script;

				using (StreamReader rd = new StreamReader(State.From))
				{
					Script = await rd.ReadToEndAsync();
				}

				Exp = new Expression(Script, State.FromFileName);

				lock (parsed)
				{
					parsed[State.FromFileName] = new KeyValuePair<Expression, DateTime>(Exp, TP);
				}
			}

			object Result = await Exp.EvaluateAsync(State.Session ?? new Variables());

			if (!(Result is null))
			{
				if (!InternetContent.Encodes(Result, out Grade _, out IContentEncoder Encoder, State.ToContentType))
				{
					bool AlternativeFound = false;

					if (!(State.PossibleContentTypes is null))
					{
						foreach (string Alternative in State.PossibleContentTypes)
						{
							if (Alternative != State.ToContentType &&
								InternetContent.Encodes(Result, out Grade _, out Encoder, Alternative))
							{
								State.ToContentType = Alternative;
								AlternativeFound = true;
								break;
							}
						}
					}

					if (!AlternativeFound)
					{
						State.Error = new NotAcceptableException("Unable to encode objects of type " + Result.GetType().FullName + " to Internet Content Type " + State.ToContentType);
						return false;
					}
				}

				ContentResponse P = await Encoder.EncodeAsync(Result, Encoding.UTF8, State.ToContentType);
				await State.To.WriteAsync(P.Encoded, 0, P.Encoded.Length);
				State.ToContentType = P.ContentType;
			}

			return true;
		}

		private static readonly Dictionary<string, KeyValuePair<Expression, DateTime>> parsed = new Dictionary<string, KeyValuePair<Expression, DateTime>>();

	}
}
