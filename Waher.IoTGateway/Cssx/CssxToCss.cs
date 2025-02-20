using SkiaSharp;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Html.Css;
using Waher.IoTGateway.ScriptExtensions.Constants;
using Waher.IoTGateway.Setup;
using Waher.Runtime.Inventory;
using Waher.Script;

namespace Waher.IoTGateway.Cssx
{
	/// <summary>
	/// Converts CSSX-files to CSS, by evaluating emebedded script and replacing it with results.
	/// </summary>
	public class CssxToCss : IContentConverter
	{
		/// <summary>
		/// Converts CSSX-files to CSS, by evaluating emebedded script and replacing it with results.
		/// </summary>
		public CssxToCss()
		{
		}

		/// <summary>
		/// Converts content from these content types.
		/// </summary>
		public string[] FromContentTypes => new string[] { "text/x-cssx" };

		/// <summary>
		/// Converts content to these content types. 
		/// </summary>
		public string[] ToContentTypes => new string[] { CssCodec.ContentType };

		/// <summary>
		/// How well the content is converted.
		/// </summary>
		public Grade ConversionGrade => Grade.Perfect;

		/// <summary>
		/// Performs the actual conversion.
		/// </summary>
		/// <param name="State">State of the current conversion.</param>
		/// <param name="Progress">Optional progress reporting of encoding/decoding. Can be null.</param>
		/// <returns>If the result is dynamic (true), or only depends on the source (false).</returns>
		public async Task<bool> ConvertAsync(ConversionState State, ICodecProgress Progress)
		{
			string Cssx;

			using (StreamReader rd = new StreamReader(State.From))
			{
				Cssx = await rd.ReadToEndAsync();
			}

			string Css = await Convert(Cssx, State, State.FromFileName);
			if (State.HasError)
				return false;

			byte[] Data = Utf8WithBOM.GetBytes(Css);
			await State.To.WriteAsync(Data, 0, Data.Length);
			State.ToContentType += "; charset=utf-8";

			return false;
		}

		/// <summary>
		/// Converts CSSX to CSS, using the current theme
		/// </summary>
		/// <param name="Cssx">CSSX</param>
		/// <param name="State">Conversion state.</param>
		/// <param name="FileName">Source file name.</param>
		/// <returns>CSS</returns>
		public static Task<string> Convert(string Cssx, ConversionState State, string FileName)
		{
			return Convert(Cssx, State, State.Session, FileName, true);
		}

		/// <summary>
		/// Converts CSSX to CSS, using the current theme
		/// </summary>
		/// <param name="Cssx">CSSX</param>
		/// <param name="Session">Current session</param>
		/// <param name="FileName">Source file name.</param>
		/// <returns>CSS</returns>
		public static Task<string> Convert(string Cssx, Variables Session, string FileName)
		{
			return Convert(Cssx, null, Session, FileName, true);
		}

		/// <summary>
		/// Converts CSSX to CSS, using the current theme
		/// </summary>
		/// <param name="Cssx">CSSX</param>
		/// <param name="State">Conversion state, if available.</param>
		/// <param name="Session">Current session</param>
		/// <param name="FileName">Source file name.</param>
		/// <param name="LockSession">If the session should be locked (true),
		/// or if the caller has already locked the session (false).</param>
		/// <returns>CSS</returns>
		internal static async Task<string> Convert(string Cssx, ConversionState State, Variables Session, string FileName, bool LockSession)
		{
			bool Pushed = false;

			if (Session is null)
			{
				Exception ex = new ArgumentNullException("No session defined.", nameof(Session));

				if (State is null)
					throw ex;
				else
				{
					State.Error = ex;
					return null;
				}
			}

			if (LockSession)
				await Session.LockAsync();
			try
			{
				if (LockSession)
				{
					Session.Push();
					Pushed = true;
				}

				ThemeDefinition Def = Theme.GetCurrentTheme(Session);
				Def?.Prepare(Session);

				StringBuilder Result = new StringBuilder();
				Expression Exp;
				int i = 0;
				int c = Cssx.Length;
				int j, k;
				string Script;
				object Value;

				while (i < c)
				{
					j = Cssx.IndexOf('¤', i);
					if (j < 0)
						break;

					if (j > i)
						Result.Append(Cssx[i..j]);

					k = Cssx.IndexOf('¤', j + 1);
					if (k < 0)
						break;

					Script = Cssx.Substring(j + 1, k - j - 1);
					Exp = new Expression(Script, FileName);
					Value = await Exp.EvaluateAsync(Session);

					if (Value is SKColor Color)
					{
						if (Color.Alpha == 255)
						{
							Result.Append('#');
							Result.Append(Color.Red.ToString("X2"));
							Result.Append(Color.Green.ToString("X2"));
							Result.Append(Color.Blue.ToString("X2"));
						}
						else
						{
							Result.Append("rgba(");
							Result.Append(Color.Red.ToString());
							Result.Append(',');
							Result.Append(Color.Green.ToString());
							Result.Append(',');
							Result.Append(Color.Blue.ToString());
							Result.Append(',');
							Result.Append(Expression.ToString(Color.Alpha / 255.0));
							Result.Append(')');
						}
					}
					else if (Value is string s)
						Result.Append(s);
					else
						Result.Append(Expression.ToString(Value));

					i = k + 1;
				}

				if (i < c)
					Result.Append(Cssx[i..]);

				return Result.ToString();
			}
			finally
			{
				if (LockSession)
				{
					if (Pushed)
						Session.Pop();

					Session.Release();
				}
			}
		}

		internal static readonly System.Text.Encoding Utf8WithBOM = new UTF8Encoding(true);

	}
}
