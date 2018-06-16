using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SkiaSharp;
using Waher.Content;
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
		public string[] FromContentTypes => new string[] { "text/x.cssx" };

		/// <summary>
		/// Converts content to these content types. 
		/// </summary>
		public string[] ToContentTypes => new string[] { "text/css" };

		/// <summary>
		/// How well the content is converted.
		/// </summary>
		public Grade ConversionGrade => Grade.Perfect;

		/// <summary>
		/// Performs the actual conversion.
		/// </summary>
		/// <param name="FromContentType">Content type of the content to convert from.</param>
		/// <param name="From">Stream pointing to binary representation of content.</param>
		/// <param name="FromFileName">If the content is coming from a file, this parameter contains the name of that file. 
		/// Otherwise, the parameter is the empty string.</param>
		/// <param name="LocalResourceName">Local resource name of file, if accessed from a web server.</param>
		/// <param name="URL">URL of resource, if accessed from a web server.</param>
		/// <param name="ToContentType">Content type of the content to convert to.</param>
		/// <param name="To">Stream pointing to where binary representation of content is to be sent.</param>
		/// <param name="Session">Session states.</param>
		/// <returns>If the result is dynamic (true), or only depends on the source (false).</returns>
		public bool Convert(string FromContentType, Stream From, string FromFileName, string LocalResourceName, string URL, string ToContentType,
			Stream To, Variables Session)
		{
			string Cssx;

			using (StreamReader rd = new StreamReader(From))
			{
				Cssx = rd.ReadToEnd();
			}

			Session.Push();
			try
			{
				ThemeDefinition Def = Theme.CurrerntTheme;
				if (Def != null)
				{
					Session["HeaderColor"] = Def.HeaderColor;
					Session["HeaderTextColor"] = Def.HeaderTextColor;
					Session["ButtonColor"] = Def.ButtonColor;
					Session["ButtonTextColor"] = Def.ButtonTextColor;
					Session["MenuTextColor"] = Def.MenuTextColor;
					Session["EditColor"] = Def.EditColor;
					Session["LinkColorUnvisited"] = Def.LinkColorUnvisited;
					Session["LinkColorVisited"] = Def.LinkColorVisited;
					Session["LinkColorHot"] = Def.LinkColorHot;

					foreach (KeyValuePair<string, string> P in Def.GetCustomProperties())
						Session[P.Key] = P.Value;
				}

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
						Result.Append(Cssx.Substring(i, j - i));

					k = Cssx.IndexOf('¤', j + 1);
					if (k < 0)
						break;

					Script = Cssx.Substring(j + 1, k - j - 1);
					Exp = new Expression(Script);
					Value = Exp.Evaluate(Session);

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
					else
						Result.Append(Expression.ToString(Value));

					i = k + 1;
				}

				if (i < c)
					Result.Append(Cssx.Substring(i));

				byte[] Data = Utf8WithBOM.GetBytes(Result.ToString());
				To.Write(Data, 0, Data.Length);
			}
			finally
			{
				Session.Pop();
			}

			return false;
		}

		internal static readonly Encoding Utf8WithBOM = new UTF8Encoding(true);

	}
}
