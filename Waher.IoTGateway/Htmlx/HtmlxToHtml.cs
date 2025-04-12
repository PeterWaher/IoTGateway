using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Html;
using Waher.Content.Html.Elements;
using Waher.Content.Html.JavaScript;
using Waher.Events;
using Waher.IoTGateway.Cssx;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script;

namespace Waher.IoTGateway.Htmlx
{
	/// <summary>
	/// Converts HTMLX-files to HTML, by evaluating emebedded script and replacing it with results.
	/// </summary>
	public class HtmlxToHtml : IContentConverter
	{
		/// <summary>
		/// Converts HTMLX-files to HTML, by evaluating emebedded script and replacing it with results.
		/// </summary>
		public HtmlxToHtml()
		{
		}

		/// <summary>
		/// Converts content from these content types.
		/// </summary>
		public string[] FromContentTypes => new string[] { HtmlxDecoder.DefaultContentType };

		/// <summary>
		/// Converts content to these content types. 
		/// </summary>
		public string[] ToContentTypes => new string[] { HtmlCodec.DefaultContentType };

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
			string Htmlx;

			using (StreamReader rd = new StreamReader(State.From))
			{
				Htmlx = await rd.ReadToEndAsync();
			}

			if (!(State.Progress is null))
			{
				string Head = HtmlDocument.GetHead(Htmlx);
				if (!string.IsNullOrEmpty(Head))
				{
					try
					{
						HtmlDocument Doc = new HtmlDocument("<html><head>" + Head + "</head><body></body></html>");

						if (Doc.Head?.HasChildren ?? false)
						{
							foreach (HtmlNode N in Doc.Head.Children)
							{
								if (!(N is HtmlElement E))
									continue;

								switch (E.LocalName.ToUpper())
								{
									case "LINK":
										string Rel = E.GetAttribute("rel");
										if (Rel == "stylesheet")
										{
											string HRef = E.GetAttribute("href");

											if (!string.IsNullOrEmpty(HRef))
											{
												await State.Progress.EarlyHint(HRef, "preload",
													new KeyValuePair<string, string>("as", "style"));
											}
										}
										break;

									case "SCRIPT":
										string Type = E.GetAttribute("type");
										if (Type == JavaScriptCodec.DefaultContentType)
										{
											string Src = E.GetAttribute("src");

											if (!string.IsNullOrEmpty(Src))
											{
												await State.Progress.EarlyHint(Src, "preload",
													new KeyValuePair<string, string>("as", "script"));
											}
										}
										break;
								}
							}
						}
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}

				await State.Progress.HeaderProcessed();
			}

			string Html = await Convert(Htmlx, State, State.FromFileName);
			if (State.HasError)
				return false;

			if (!(State.Progress is null))
				await State.Progress.BodyProcessed();

			byte[] Data = Strings.Utf8WithBom.GetBytes(Html);
			await State.To.WriteAsync(Data, 0, Data.Length);
			State.ToContentType += "; charset=utf-8";

			return false;
		}

		/// <summary>
		/// Converts HTMLX to HTML, using the current theme
		/// </summary>
		/// <param name="Htmlx">HTMLX</param>
		/// <param name="State">Conversion state.</param>
		/// <param name="FileName">Source file name.</param>
		/// <returns>HTML</returns>
		public static Task<string> Convert(string Htmlx, ConversionState State, string FileName)
		{
			return Convert(Htmlx, State, State.Session, FileName, true);
		}

		/// <summary>
		/// Converts HTMLX to HTML, using the current theme
		/// </summary>
		/// <param name="Htmlx">HTMLX</param>
		/// <param name="Session">Current session</param>
		/// <param name="FileName">Source file name.</param>
		/// <returns>HTML</returns>
		public static Task<string> Convert(string Htmlx, Variables Session, string FileName)
		{
			return Convert(Htmlx, null, Session, FileName, true);
		}

		/// <summary>
		/// Converts HTMLX to HTML, using the current theme
		/// </summary>
		/// <param name="Htmlx">HTMLX</param>
		/// <param name="State">Conversion state, if available.</param>
		/// <param name="Session">Current session</param>
		/// <param name="FileName">Source file name.</param>
		/// <param name="LockSession">If the session should be locked (true),
		/// or if the caller has already locked the session (false).</param>
		/// <returns>HTML</returns>
		internal static Task<string> Convert(string Htmlx, ConversionState State, Variables Session, string FileName, bool LockSession)
		{
			return CssxToCss.Convert(Htmlx, "{{", "}}", State, Session, FileName, LockSession);
		}

	}
}
