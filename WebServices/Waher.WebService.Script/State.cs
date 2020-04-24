using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using SkiaSharp;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Security;

namespace Waher.WebService.Script
{
	/// <summary>
	/// Internal script execution state.
	/// </summary>
	internal class State
	{
		private static readonly Dictionary<string, State> expressions = new Dictionary<string, State>();

		private readonly string tag;
		private readonly Stopwatch watch = new Stopwatch();
		private readonly int timeout;
		private readonly Variables variables;
		private readonly StringBuilder printOutput;
		private Expression expression;
		private HttpRequest request;
		private HttpResponse response;
		private IUser user;
		private Timer watchdog = null;
		private Thread thread = null;
		private int counter;
		private bool previewing;

		/// <summary>
		/// Internal script execution state.
		/// </summary>
		/// <param name="Request">HTTP Request object.</param>
		/// <param name="Response">HTTP Response object for request.</param>
		/// <param name="Tag">Client-side tag.</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <param name="User">User executing the script.</param>
		public State(HttpRequest Request, HttpResponse Response, string Tag, int Timeout, IUser User)
		{
			this.request = Request;
			this.response = Response;
			this.tag = Tag;
			this.timeout = Timeout;
			this.user = User;
			this.expression = null;
			this.previewing = false;
			this.counter = 0;

			this.variables = new Variables();
			Request.Session.CopyTo(this.variables);

			this.variables["Request"] = Request;
			this.variables["Response"] = Response;

			this.printOutput = new StringBuilder();
			this.variables.ConsoleOut = new StringWriter(this.printOutput);
		}

		internal static bool TryGetState(string Tag, out State State)
		{
			lock (expressions)
			{
				return expressions.TryGetValue(Tag, out State);
			}
		}

		internal void SetExpression(Expression Expression)
		{
			if (!(this.expression is null))
				throw new InvalidOperationException("Expression already set.");

			this.expression = Expression;
			this.expression.Tag = this;
			this.expression.OnPreview += Expression_OnPreview;
		}

		private void Expression_OnPreview(object Sender, PreviewEventArgs e)
		{
			if (!(this.response?.HeaderSent ?? true))
			{
				this.previewing = true;
				this.SendResponse(e.Preview, true);
			}
		}

		internal void SetRequestResponse(HttpRequest Request, HttpResponse Response, IUser User)
		{
			this.request = Request;
			this.response = Response;
			this.user = User;
		}

		/// <summary>
		/// Starts the watch.
		/// </summary>
		public void Start()
		{
			if (!(this.thread is null))
				throw new InvalidOperationException("Evaluation already started.");

			this.thread = new Thread(this.Execute);
			this.watchdog = new Timer(this.WatchdogTimer, null, 1000, 1000);

			lock (expressions)
			{
				expressions[this.tag] = this;
			}

			this.thread.Start();
		}

		/// <summary>
		/// Stops the watch.
		/// </summary>
		public void Stop()
		{
			this.watch.Stop();
		}

		/// <summary>
		/// Elapsed milliseconds
		/// </summary>
		public double Milliseconds
		{
			get => (this.watch.ElapsedTicks * 1000.0) / Stopwatch.Frequency;
		}

		private void Execute(object P)
		{
			IElement Result;

			try
			{
				try
				{
					this.watch.Start();
					Result = this.expression.Root.Evaluate(this.variables);
				}
				catch (ScriptReturnValueException ex)
				{
					Result = ex.ReturnValue;
				}
				catch (ScriptAbortedException)
				{
					this.variables.CancelAbort();
					Result = new ObjectValue(new TimeoutException("Script forcefully aborted. You can control the timeout threshold, by setting the Timeout variable to the number of milliseconds to use."));
				}
				catch (Exception ex)
				{
					Result = new ObjectValue(ex);
				}
				finally
				{
					this.watch.Stop();
					this.watchdog?.Dispose();

					Log.Notice("Script evaluated.", this.request.Resource.ResourceName, this.user.UserName, "ScriptEval",
						new KeyValuePair<string, object>("RemoteEndPoint", this.request.RemoteEndPoint),
						new KeyValuePair<string, object>("Script", this.expression.Script),
						new KeyValuePair<string, object>("Milliseconds", this.Milliseconds));
				}

				if (!this.response.HeaderSent)
				{
					lock (expressions)
					{
						expressions.Remove(this.tag);
					}

					this.SendResponse(Result, false);
				}

				this.variables.CopyTo(this.request.Session);
			}
			catch (ThreadAbortException)
			{
				if (!this.response.HeaderSent)
				{
					lock (expressions)
					{
						expressions.Remove(this.tag);
					}

					this.SendResponse(new ObjectValue(new TimeoutException("Script forcefully aborted. You can control the timeout threshold, by setting the Timeout variable to the number of milliseconds to use.")),
						false);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
			finally
			{
				Timer Temp = this.watchdog;
				this.watchdog = null;
				Temp?.Dispose();
			}
		}

		private void WatchdogTimer(object State)
		{
			double ms = this.Milliseconds;

			this.counter++;

			if (!this.response.HeaderSent && !this.previewing)
			{
				this.response.SetHeader("X-More", "1");
				this.response.ContentType = "text/html";
				this.response.Write("<p><font style=\"color:green\"><code>" + new string('.', this.counter) + "</code></font></p>");
				this.response.SendResponse();
				this.response.Dispose();
			}

			if (ms >= this.timeout)
			{
				MethodInfo AbortInternal = null;

				foreach (MethodInfo MI in this.thread.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (MI.Name == "AbortInternal" && MI.GetParameters().Length == 0)
					{
						AbortInternal = MI;
						break;
					}
				}

				if (AbortInternal is null)
					this.variables?.Abort();
				else
				{
					try
					{
						AbortInternal.Invoke(this.thread, new object[0]);
					}
					catch (Exception)
					{
						this.variables?.Abort();
					}
				}

				Timer Temp = this.watchdog;
				this.watchdog = null;
				Temp?.Dispose();

				Log.Warning("Long-running script forcefully terminated.", this.request.Resource.ResourceName, this.user.UserName, "ScriptAbort",
					new KeyValuePair<string, object>("RemoteEndPoint", this.request.RemoteEndPoint),
					new KeyValuePair<string, object>("Script", this.expression.Script),
					new KeyValuePair<string, object>("Milliseconds", ms));
			}
			else if (this.counter == 5)     // 5 sceonds
			{
				Log.Notice("Long-running script.", this.request.Resource.ResourceName, this.user.UserName, "ScriptLong",
					new KeyValuePair<string, object>("RemoteEndPoint", this.request.RemoteEndPoint),
					new KeyValuePair<string, object>("Script", this.expression.Script),
					new KeyValuePair<string, object>("Milliseconds", ms));
			}
		}

		internal void SendResponse(IElement Result, bool More)
		{
			this.variables["Ans"] = Result;

			byte[] Bin;
			object Obj;
			string s;

			if (Result is Graph G)
			{
				GraphSettings Settings = new GraphSettings();
				Tuple<int, int> Size;
				double d;

				if ((Size = G.RecommendedBitmapSize) != null)
				{
					Settings.Width = Size.Item1;
					Settings.Height = Size.Item2;

					Settings.MarginLeft = (int)Math.Round(15.0 * Settings.Width / 640);
					Settings.MarginRight = Settings.MarginLeft;

					Settings.MarginTop = (int)Math.Round(15.0 * Settings.Height / 480);
					Settings.MarginBottom = Settings.MarginTop;
					Settings.LabelFontSize = 12.0 * Settings.Height / 480;
				}
				else
				{
					if (this.variables.TryGetVariable("GraphWidth", out Variable v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
					{
						Settings.Width = (int)Math.Round(d);
						Settings.MarginLeft = (int)Math.Round(15 * d / 640);
						Settings.MarginRight = Settings.MarginLeft;
					}
					else if (!this.variables.ContainsVariable("GraphWidth"))
						this.variables["GraphWidth"] = (double)Settings.Width;

					if (this.variables.TryGetVariable("GraphHeight", out v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
					{
						Settings.Height = (int)Math.Round(d);
						Settings.MarginTop = (int)Math.Round(15 * d / 480);
						Settings.MarginBottom = Settings.MarginTop;
						Settings.LabelFontSize = 12 * d / 480;
					}
					else if (!this.variables.ContainsVariable("GraphHeight"))
						this.variables["GraphHeight"] = (double)Settings.Height;
				}

				using (SKImage Bmp = G.CreateBitmap(Settings, out object[] States))
				{
					string Tag = Guid.NewGuid().ToString();
					SKData Data = Bmp.Encode(SKEncodedImageFormat.Png, 100);
					Bin = Data.ToArray();

					s = Convert.ToBase64String(Bin, 0, Bin.Length);
					s = "<figure><img border=\"2\" width=\"" + Settings.Width.ToString() + "\" height=\"" + Settings.Height.ToString() +
						"\" src=\"data:image/png;base64," + s + "\" onclick=\"GraphClicked(this,event,'" + Tag + "');\" /></figure>";

					Data.Dispose();

					if (!(this.variables["Graphs"] is Dictionary<string, KeyValuePair<Graph, object[]>> Graphs))
					{
						Graphs = new Dictionary<string, KeyValuePair<Graph, object[]>>();
						this.variables["Graphs"] = Graphs;
					}

					lock (Graphs)
					{
						Graphs[Tag] = new KeyValuePair<Graph, object[]>(G, States);
					}
				}
			}
			else if (Result.AssociatedObjectValue is SKImage Img)
			{
				SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100);
				Bin = Data.ToArray();

				s = Convert.ToBase64String(Bin, 0, Bin.Length);
				s = "<figure><img border=\"2\" width=\"" + Img.Width.ToString() + "\" height=\"" + Img.Height.ToString() +
					"\" src=\"data:image/png;base64," + s + "\" /></figure>";

				Data.Dispose();
			}
			else if (Result.AssociatedObjectValue is Exception ex)
			{
				ex = Log.UnnestException(ex);

				if (ex is AggregateException ex2)
				{
					StringBuilder sb2 = new StringBuilder();

					foreach (Exception ex3 in ex2.InnerExceptions)
					{
						sb2.Append("<p><font style=\"color:red;font-weight:bold\"><code>");
						sb2.Append(this.FormatText(XML.HtmlValueEncode(ex3.Message)));
						sb2.Append("</code></font></p>");
					}

					s = sb2.ToString();
				}
				else
					s = "<p><font style=\"color:red;font-weight:bold\"><code>" + this.FormatText(XML.HtmlValueEncode(ex.Message)) + "</code></font></p>";
			}
			else if (Result is ObjectMatrix M && M.ColumnNames != null)
			{
				StringBuilder Html = new StringBuilder();

				s = Result.ToString();

				Html.Append("<div class='clickable' onclick='SetScript(this);'><code style='display:none'>");
				Html.Append(XML.Encode(s));
				Html.Append("</code><table><thead><tr>");

				foreach (string Name in M.ColumnNames)
				{
					Html.Append("<th>");
					Html.Append(this.FormatText(XML.HtmlValueEncode(Name)));
					Html.Append("</th>");
				}

				Html.Append("</tr></thead><tbody>");

				int x, y;

				for (y = 0; y < M.Rows; y++)
				{
					Html.Append("<tr>");

					for (x = 0; x < M.Columns; x++)
					{
						Html.Append("<td>");

						object Item = M.GetElement(x, y).AssociatedObjectValue;
						if (Item != null)
						{
							if (Item is string s3)
								Html.Append(this.FormatText(XML.HtmlValueEncode(s3)));
							else
								Html.Append(this.FormatText(XML.HtmlValueEncode(Expression.ToString(Item))));
						}

						Html.Append("</td>");
					}

					Html.Append("</tr>");
				}

				Html.Append("</tbody></table></div>");
				s = Html.ToString();
			}
			else
			{
				s = Result.ToString();
				s = "<div class='clickable' onclick='SetScript(this);'><code style='display:none'>" + XML.Encode(s) +
					"</code><p><font style=\"color:red\"><code>" + this.FormatText(XML.HtmlValueEncode(s)) + "</code></font></p></div>";
			}

			string s2 = this.printOutput.ToString();
			if (!string.IsNullOrEmpty(s2))
				s = "<p><font style=\"color:blue\"><code>" + this.FormatText(XML.HtmlValueEncode(s2)) + "</code></font></p>" + s;

			Bin = Encoding.UTF8.GetBytes(s);

			this.response.ContentType = "text/html; charset=utf-8";
			this.response.ContentLength = Bin.Length;	// To avoid chunked transfer.
			this.response.SetHeader("X-More", More ? "1" : "0");
			this.response.Write(Bin);
			this.response.SendResponse();
			this.response.Dispose();
		}

		private string FormatText(string s)
		{
			return s.
				Replace("\r\n", "\n").
				Replace("\n", "<br/>").
				Replace("\r", "<br/>").
				Replace("\t", "&nbsp;&nbsp;&nbsp;").
				Replace(" ", "&nbsp;");
		}

	}
}
