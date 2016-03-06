using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;

namespace Waher.WebService.Script
{
	public class ScriptService : HttpAsynchronousResource, IHttpPostMethod
	{
		private HttpAuthenticationScheme[] authenticationSchemes;

		public ScriptService(string ResourceName, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base(ResourceName)
		{
			this.authenticationSchemes = AuthenticationSchemes;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void POST(HttpRequest Request, HttpResponse Response)
		{
			if (!Request.HasData || Request.Session == null)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
			string s = Obj as string;

			if (s == null)
				throw new BadRequestException();

			Variables Variables = Request.Session;
			TextWriter Bak = Variables.ConsoleOut;
			StringBuilder sb = new StringBuilder();

			Variables["Request"] = Request;
			Variables["Response"] = Response;

			Variables.Lock();
			Variables.ConsoleOut = new StringWriter(sb);
			try
			{
				Expression Exp = new Expression(s);
				IElement Result;

				try
				{
					Result = Exp.Root.Evaluate(Variables);
				}
				catch (ScriptReturnValueException ex)
				{
					Result = ex.ReturnValue;
				}

				Request.Session["Ans"] = Result;

				Graph G = Result as Graph;
				if (G != null)
				{
					GraphSettings Settings = new GraphSettings();
					Variable v;
					double d;

					if (Variables.TryGetVariable("GraphWidth", out v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
					{
						Settings.Width = (int)Math.Round(d);
						Settings.MarginLeft = (int)Math.Round(15 * d / 640);
						Settings.MarginRight = Settings.MarginLeft;
					}
					else if (!Variables.ContainsVariable("GraphWidth"))
						Variables["GraphWidth"] = (double)Settings.Width;
						

					if (Variables.TryGetVariable("GraphHeight", out v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
					{
						Settings.Height = (int)Math.Round(d);
						Settings.MarginTop = (int)Math.Round(15 * d / 480);
						Settings.MarginBottom = Settings.MarginTop;
						Settings.LabelFontSize = 12 * d / 480;
					}
					else if (!Variables.ContainsVariable("GraphHeight"))
						Variables["GraphHeight"] = (double)Settings.Height;

					Bitmap Bmp = G.CreateBitmap(Settings);
					MemoryStream ms = new MemoryStream();
					Bmp.Save(ms, ImageFormat.Png);
					byte[] Data = ms.GetBuffer();
					s = System.Convert.ToBase64String(Data, 0, (int)ms.Position, Base64FormattingOptions.None);
					s = "<img border=\"2\" width=\"" + Settings.Width.ToString() + "\" height=\"" + Settings.Height.ToString() +
						"\" src=\"data:image/png;base64," + s + "\" />";
				}
				else
				{
					s = Result.ToString();
					s = "<div class='clickable' onclick='SetScript(\"" + s.ToString().Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\"", "\\\"").Replace("'", "\\'") +
						"\");'><p><font style=\"color:red\"><code>" + this.FormatText(XML.HtmlValueEncode(s)) + "</code></font></p></div>";
				}
			}
			catch (Exception ex)
			{
				s = "<p><font style=\"color:red;font-weight:bold\"><code>" + this.FormatText(XML.HtmlValueEncode(ex.Message)) + "</code></font></p>";
			}
			finally
			{
				Variables.ConsoleOut.Flush();
				Variables.ConsoleOut = Bak;
				Variables.Release();
			}

			string s2 = sb.ToString();
			if (!string.IsNullOrEmpty(s2))
				s = "<p><font style=\"color:blue\"><code>" + this.FormatText(XML.HtmlValueEncode(s2)) + "</code></font></p>" + s;

			Response.Return(s);
		}

		private string FormatText(string s)
		{
			return s.Replace("\r\n", "\n").Replace("\n", "<br/>").Replace("\r", "<br/>").
				Replace("\t", "&nbsp;&nbsp;&nbsp;").Replace(" ", "&nbsp;");
		}

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
		{
			return this.authenticationSchemes;
		}

	}
}
