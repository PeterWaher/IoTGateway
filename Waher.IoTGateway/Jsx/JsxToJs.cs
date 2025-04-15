using System.IO;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Html.JavaScript;
using Waher.IoTGateway.Cssx;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script;

namespace Waher.IoTGateway.Jsx
{
	/// <summary>
	/// Converts JavascriptX-files to HTML, by evaluating emebedded script and replacing it with results.
	/// </summary>
	public class JsxToJs : IContentConverter
	{
		/// <summary>
		/// Converts JavascriptX-files to HTML, by evaluating emebedded script and replacing it with results.
		/// </summary>
		public JsxToJs()
		{
		}

		/// <summary>
		/// Converts content from these content types.
		/// </summary>
		public string[] FromContentTypes => new string[] { JsxDecoder.DefaultContentType };

		/// <summary>
		/// Converts content to these content types. 
		/// </summary>
		public string[] ToContentTypes => new string[] { JavaScriptCodec.DefaultContentType };

		/// <summary>
		/// How well the content is converted.
		/// </summary>
		public Grade ConversionGrade => Grade.Perfect;

		/// <summary>
		/// Performs the actual conversion.
		/// </summary>
		/// <param name="State">State of the current conversion.</param>
		/// <returns>If the result is dynamic (true), or only depends on the source (false).</returns>
		public async Task<bool> ConvertAsync(ConversionState State)
		{
			string Jsx;

			using (StreamReader rd = new StreamReader(State.From))
			{
				Jsx = await rd.ReadToEndAsync();
			}

			string Javascript = await Convert(Jsx, State, State.FromFileName);
			if (State.HasError)
				return false;

			byte[] Data = Strings.Utf8WithBom.GetBytes(Javascript);
			await State.To.WriteAsync(Data, 0, Data.Length);
			State.ToContentType += "; charset=utf-8";

			return false;
		}

		/// <summary>
		/// Converts JavascriptX to HTML, using the current theme
		/// </summary>
		/// <param name="Jsx">JavascriptX</param>
		/// <param name="State">Conversion state.</param>
		/// <param name="FileName">Source file name.</param>
		/// <returns>HTML</returns>
		public static Task<string> Convert(string Jsx, ConversionState State, string FileName)
		{
			return Convert(Jsx, State, State.Session, FileName, true);
		}

		/// <summary>
		/// Converts JavascriptX to HTML, using the current theme
		/// </summary>
		/// <param name="Jsx">JavascriptX</param>
		/// <param name="Session">Current session</param>
		/// <param name="FileName">Source file name.</param>
		/// <returns>HTML</returns>
		public static Task<string> Convert(string Jsx, Variables Session, string FileName)
		{
			return Convert(Jsx, null, Session, FileName, true);
		}

		/// <summary>
		/// Converts JavascriptX to HTML, using the current theme
		/// </summary>
		/// <param name="Jsx">JavascriptX</param>
		/// <param name="State">Conversion state, if available.</param>
		/// <param name="Session">Current session</param>
		/// <param name="FileName">Source file name.</param>
		/// <param name="LockSession">If the session should be locked (true),
		/// or if the caller has already locked the session (false).</param>
		/// <returns>HTML</returns>
		internal static Task<string> Convert(string Jsx, ConversionState State, Variables Session, string FileName, bool LockSession)
		{
			return CssxToCss.Convert(Jsx, "{{", "}}", State, Session, FileName, LockSession);
		}

	}
}
