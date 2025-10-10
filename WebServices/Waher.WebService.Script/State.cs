﻿using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content.Html;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Rendering;
using Waher.Content.Xml;
using Waher.Events;
using Waher.IoTGateway.ScriptExtensions.Constants;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Operators.Matrices;
using Waher.Security;
using Waher.Security.LoginMonitor;

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
		private readonly SessionVariables variables;
		private readonly StringBuilder printOutput;
		private readonly object synchObj = new object();
		private Expression expression;
		private HttpRequest request;
		private HttpResponse response;
		private IUser user;
		private Timer watchdog = null;
		private Thread thread = null;
		private IElement preview = null;
		private IElement queued = null;
		private bool sent = false;
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

			this.variables = new SessionVariables()
			{
				CurrentRequest = Request,
				CurrentResponse = Response
			};
			Request.Session.CopyTo(this.variables);

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
		}

		internal void NewResult(IElement Result)
		{
			lock (this.synchObj)
			{
				if (this.sent)
				{
					this.queued = Result;
					return;
				}
				else
					this.sent = true;
			}

			this.SendNewResult(Result);
		}

		private Task SendNewResult(IElement Result)
		{
			lock (expressions)
			{
				expressions.Remove(this.tag);
			}

			return this.SendResult(Result, false);
		}

		private Task SendNewPreview(IElement Result)
		{
			return this.SendResult(Result, true);
		}

		private Task Expression_OnPreview(object Sender, PreviewEventArgs e)
		{
			lock (this.synchObj)
			{
				this.preview = e.Preview;
			}

			return Task.CompletedTask;
		}

		internal void SetRequestResponse(HttpRequest Request, HttpResponse Response, IUser User)
		{
			IElement Result;
			bool Preview;

			lock (this.synchObj)
			{
				this.request = Request;
				this.response = Response;
				this.user = User;

				if (!(this.queued is null))
				{
					Result = this.queued;
					this.queued = null;
					this.sent = true;
					Preview = false;
				}
				else if (!(this.preview is null))
				{
					Result = this.preview;
					this.preview = null;
					this.previewing = true;
					this.sent = true;
					Preview = true;
				}
				else
				{
					this.sent = false;
					return;
				}
			}

			if (Preview)
				this.SendNewPreview(Result);
			else
				this.SendNewResult(Result);
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
			get => this.watch.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
		}

		private async void Execute(object P)
		{
			IElement Result;

			try
			{
				this.variables.OnPreview += this.Expression_OnPreview;
				try
				{
					this.watch.Start();
					Result = await this.expression.Root.EvaluateAsync(this.variables);
				}
				catch (ScriptReturnValueException ex)
				{
					Result = ex.ReturnValue;
					//ScriptReturnValueException.Reuse(ex);
				}
				catch (ScriptBreakLoopException ex)
				{
					Result = ex.LoopValue ?? ObjectValue.Null;
					//ScriptBreakLoopException.Reuse(ex);
				}
				catch (ScriptContinueLoopException ex)
				{
					Result = ex.LoopValue ?? ObjectValue.Null;
					//ScriptContinueLoopException.Reuse(ex);
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
					this.variables.OnPreview -= this.Expression_OnPreview;
					this.watch.Stop();

					Timer Temp = this.watchdog;
					this.watchdog = null;
					Temp?.Dispose();

					KeyValuePair<string, object>[] Tags = await LoginAuditor.Annotate(this.request.RemoteEndPoint,
						new KeyValuePair<string, object>("RemoteEndPoint", this.request.RemoteEndPoint),
						new KeyValuePair<string, object>("Script", this.expression.Script),
						new KeyValuePair<string, object>("Milliseconds", this.Milliseconds));

					Log.Notice("Script evaluated.", this.request.Resource.ResourceName, this.user.UserName, "ScriptEval", Tags);
				}

				this.NewResult(Result);

				this.variables.CopyTo(this.request.Session);
			}
			catch (ThreadAbortException)
			{
				this.NewResult(new ObjectValue(
					new TimeoutException("Script forcefully aborted. You can control the timeout threshold, by setting the Timeout variable to the number of milliseconds to use.")));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
			finally
			{
				Timer Temp = this.watchdog;
				this.watchdog = null;
				Temp?.Dispose();
			}
		}

		private async void WatchdogTimer(object State)
		{
			try
			{
				double ms = this.Milliseconds;
				bool SendProgress = false;
				IElement Preview = null;

				this.counter++;

				lock (this.synchObj)
				{
					if (!this.sent)
					{
						if (!(this.preview is null))
						{
							Preview = this.preview;
							this.preview = null;
							this.previewing = true;
							this.sent = true;
						}
						else if (!this.previewing)
						{
							SendProgress = true;
							this.sent = true;
						}
					}
				}

				if (!(Preview is null))
					await this.SendResult(Preview, true);
				else if (SendProgress)
				{
					this.response.SetHeader("X-More", "1");
					this.response.ContentType = HtmlCodec.DefaultContentType;
					await this.response.Write("<p><font style=\"color:green\"><code>" + new string('.', this.counter) + "</code></font></p>");
					await this.response.SendResponse();
					await this.response.DisposeAsync();
				}

				if (ms >= this.timeout)
				{
					Timer Temp = this.watchdog;
					this.watchdog = null;
					Temp?.Dispose();

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
							AbortInternal.Invoke(this.thread, Array.Empty<object>());
						}
						catch (Exception)
						{
							this.variables?.Abort();
						}
					}

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
			catch (Exception)
			{
				// Ignore
			}
		}

		private async Task SendResult(IElement Result, bool More)
		{
			try
			{
				if (!More)
					this.request.Session["Ans"] = Result;

				byte[] Bin;
				string s;

				if (Result.AssociatedObjectValue is IToMatrix ToMatrix)
					Result = ToMatrix.ToMatrix();

				if (Result is Graph G)
				{
					PixelInformation Pixels = G.CreatePixels(out object[] States);
					string Tag = Guid.NewGuid().ToString();
					Bin = Pixels.EncodeAsPng();

					s = Convert.ToBase64String(Bin, 0, Bin.Length);
					s = "<figure><img border=\"2\" width=\"" + G.Settings.Width.ToString() + "\" height=\"" + G.Settings.Height.ToString() +
						"\" src=\"data:image/png;base64," + s + "\" onclick=\"GraphClicked(this,event,'" + Tag + "');\" /></figure>";

					if (!(this.request.Session["Graphs"] is Dictionary<string, KeyValuePair<Graph, object[]>> Graphs))
					{
						Graphs = new Dictionary<string, KeyValuePair<Graph, object[]>>();
						this.request.Session["Graphs"] = Graphs;
					}

					lock (Graphs)
					{
						Graphs[Tag] = new KeyValuePair<Graph, object[]>(G, States);
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

					StringBuilder sb2 = new StringBuilder();
					string NegColor = Theme.GetCurrentTheme(this.variables)["NegColor"];

					if (ex is AggregateException ex2)
					{
						foreach (Exception ex3 in ex2.InnerExceptions)
						{
							sb2.Append("<p><font style=\"color:");
							sb2.Append(NegColor);
							sb2.Append(";font-weight:bold\"><code>");
							sb2.Append(this.FormatText(XML.HtmlValueEncode(ex3.Message)));
							sb2.Append("</code></font></p>");
						}
					}
					else
					{
						sb2.Append("<p><font style=\"color:");
						sb2.Append(NegColor);
						sb2.Append(";font-weight:bold\"><code>");
						sb2.Append(this.FormatText(XML.HtmlValueEncode(ex.Message)));
						sb2.Append("</code></font></p>");
					}

					s = sb2.ToString();
				}
				else if (Result is ObjectMatrix M && !(M.ColumnNames is null))
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
							if (Item is null)
								Html.Append("<code>null</code>");
							else
							{
								if (Item is string s3)
									Html.Append(this.FormatText(XML.HtmlValueEncode(s3)));
								else if (Item is CaseInsensitiveString cis)
								{
									if (cis.Value is null)
										Html.Append("<code>null</code>");
									else
										Html.Append(this.FormatText(XML.HtmlValueEncode(cis.Value)));
								}
								else if (Item is MarkdownElement Element)
								{
									using HtmlRenderer Renderer = new HtmlRenderer(Html, new HtmlSettings()
									{
										XmlEntitiesOnly = true
									});

									await Element.Render(Renderer);
								}
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

					StringBuilder sb2 = new StringBuilder();

					sb2.Append("<div class='clickable' onclick='SetScript(this);'><code style='display:none'>");
					sb2.Append(XML.Encode(s));
					sb2.Append("</code><p><font style=\"color:");
					sb2.Append(Theme.GetCurrentTheme(this.variables)["NegColor"]);
					sb2.Append("\"><code>");
					sb2.Append(this.FormatText(XML.HtmlValueEncode(s)));
					sb2.Append("</code></font></p></div>");

					s = sb2.ToString();
				}

				string s2 = this.printOutput.ToString();
				if (!string.IsNullOrEmpty(s2))
				{
					StringBuilder sb2 = new StringBuilder();
					sb2.Append("<p><font style=\"color:");
					IoTGateway.Cssx.CssxToCss.ColorToCss(sb2, Theme.GetCurrentTheme(this.variables).LinkColorUnvisited);
					sb2.Append("\"><code>");
					sb2.Append(this.FormatText(XML.HtmlValueEncode(s2)));
					sb2.Append("</code></font></p>");
					sb2.Append(s);

					s = sb2.ToString();
				}

				Bin = Encoding.UTF8.GetBytes(s);

				this.response.ContentType = "text/html; charset=utf-8";
				this.response.ContentLength = Bin.Length;   // To avoid chunked transfer.
				this.response.SetHeader("X-More", More ? "1" : "0");
				await this.response.Write(true, Bin);
				await this.response.SendResponse();
				await this.response.DisposeAsync();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
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
