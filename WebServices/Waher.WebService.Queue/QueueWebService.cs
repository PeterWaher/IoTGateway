using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.Content.Multipart;
using Waher.Events;
using Waher.IoTGateway;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Runtime.Collections;
using Waher.Runtime.IO;
using Waher.Script;

namespace Waher.WebService.Queue
{
	/// <summary>
	/// Provides a REST web service interface for access to local queues.
	/// </summary>
	public class QueueWebService : HttpAsynchronousResource,
		IHttpGetMethod, IHttpPostMethod, IHttpPutMethod, IHttpDeleteMethod
	{
		private const string RootPrivilege = "Admin.Queues.";

		private readonly HttpAuthenticationScheme[] authenticationSchemes;

		/// <summary>
		/// Provides a REST web service interface for access to local queues.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		/// <param name="AuthenticationSchemes">Authentication schemes.</param>
		public QueueWebService(string ResourceName,
			params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base(ResourceName)
		{
			this.authenticationSchemes = AuthenticationSchemes;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => false;

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
		{
			if (Request.Header.Method.ToUpper() == "GET")
				return null;
			else
				return this.authenticationSchemes;
		}

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// If the PUT method is allowed.
		/// </summary>
		public bool AllowsPUT => true;

		/// <summary>
		/// If the DELETE method is allowed.
		/// </summary>
		public bool AllowsDELETE => true;

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			byte[] Data = Resources.LoadResource(
				typeof(QueueWebService).Namespace + ".Data.ApiDocumentation.md",
				typeof(QueueWebService).Assembly);

			string Markdown = Strings.GetString(Data, Encoding.UTF8);
			MarkdownSettings Settings = new MarkdownSettings(null, true, new Variables())
			{
				RootFolder = Gateway.RootFolder,
				ResourceMap = Gateway.HttpServer
			};
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings,
				null, Request.Resource.ResourceName, Request.Header.GetURL());
			string Html = await Doc.GenerateHTML();

			Response.ContentType = HtmlCodec.DefaultContentType;

			await Response.Write(Html);
			await Response.SendResponse();
		}

		/// <summary>
		/// Executes the PUT method on the resource. (Enqueue)
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task PUT(HttpRequest Request, HttpResponse Response)
		{
			if (Request.User is null)
			{
				await Response.SendResponse(new ForbiddenException("Access denied."));
				return;
			}

			string QueueName = GetQueueName(Request);
			if (string.IsNullOrEmpty(QueueName))
			{
				await Response.SendResponse(new BadRequestException("Invalid or unspecified Queue Name."));
				return;
			}

			string Privilege = RootPrivilege + QueueName + ".Enqueue";
			if (!Request.User.HasPrivilege(Privilege))
			{
				await Response.SendResponse(ForbiddenException.AccessDenied(Request,
					this.ResourceName, Request.User.UserName, Privilege));
				return;
			}

			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException("No payload."));
				return;
			}

			string s = Request.Header.ContentType?.Value ?? string.Empty;
			int i = s.IndexOf(';');
			if (i > 0)
				s = s[..i].Trim();

			if (HttpFolderResource.IsProtected(s))
			{
				await Response.SendResponse(new ForbiddenException("Protected Content Type."));
				return;
			}

			ContentResponse Payload = await Request.DecodeDataAsync();
			if (Payload.HasError)
			{
				await Response.SendResponse(new BadRequestException("Unable to decode payload: " + Payload.Error.Message));
				return;
			}

			int Timeout = 30000;

			if (Request.Header.TryGetQueryParameter("Timeout", out s))
			{
				if (int.TryParse(s, out i) && i >= 0 && i < 90000)
					Timeout = i;
				else
				{
					await Response.SendResponse(new BadRequestException("Invalid Timeout value."));
					return;
				}
			}

			object Decoded = Payload.Decoded;
			IPersistedQueue Queue = await Database.GetQueue(QueueName);

			Task _ = Task.Run(async () =>
			{
				try
				{
					if (Decoded is MultipartContent MultipartContent)
					{
						foreach (EmbeddedContent Content in MultipartContent.Content)
						{
							if (Content.Decoded is null)
								continue;

							if (!await Queue.Enqueue(Content.Decoded, Timeout))
							{
								await Response.SendResponse(new ServiceUnavailableException(
									"Unable to enqueue item within timeout period."));
								return;
							}
						}

						Response.StatusCode = 204;
						await Response.SendResponse();
					}
					else if (await Queue.Enqueue(Decoded, Timeout))
					{
						Response.StatusCode = 204;
						await Response.SendResponse();
					}
					else
					{
						await Response.SendResponse(new ServiceUnavailableException(
							"Unable to enqueue item within timeout period."));
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					await Response.SendResponse(new UnprocessableEntityException());
				}
			});
		}

		private static string GetQueueName(HttpRequest Request)
		{
			if (string.IsNullOrEmpty(Request.SubPath))
				return null;

			string s = Request.SubPath[1..];

			if (s.IndexOf('/') >= 0 ||
				s.IndexOf('.') >= 0 ||
				s.IndexOfAny(CommonTypes.ControlCharacters) >= 0)
			{
				return null;
			}

			return s;
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			DateTime Start = DateTime.UtcNow;

			if (Request.User is null)
			{
				await Response.SendResponse(new ForbiddenException("Access denied."));
				return;
			}

			string QueueName = GetQueueName(Request);
			if (string.IsNullOrEmpty(QueueName))
			{
				await Response.SendResponse(new BadRequestException("Invalid or unspecified Queue Name."));
				return;
			}

			string Privilege = RootPrivilege + QueueName + ".Dequeue";
			if (!Request.User.HasPrivilege(Privilege))
			{
				await Response.SendResponse(ForbiddenException.AccessDenied(Request,
					this.ResourceName, Request.User.UserName, Privilege));
				return;
			}

			if (Request.HasData)
			{
				await Response.SendResponse(new BadRequestException("Request accepts no payload."));
				return;
			}

			int Timeout = 30000;
			int Count = 1;
			bool Multipart = false;
			int i;

			if (Request.Header.TryGetQueryParameter("Timeout", out string s))
			{
				if (int.TryParse(s, out i) && i >= 0 && i < 90000)
					Timeout = i;
				else
				{
					await Response.SendResponse(new BadRequestException("Invalid Timeout value."));
					return;
				}
			}

			if (Request.Header.TryGetQueryParameter("Count", out s))
			{
				if (int.TryParse(s, out i) && i > 0)
				{
					Count = i;
					Multipart = true;
				}
				else
				{
					await Response.SendResponse(new BadRequestException("Invalid Count value."));
					return;
				}
			}

			int MinTimeout = Timeout;

			if (Request.Header.TryGetQueryParameter("MinTimeout", out s))
			{
				if (int.TryParse(s, out i) && i > 0)
					MinTimeout = Math.Min(i, Timeout);
				else
				{
					await Response.SendResponse(new BadRequestException("Invalid MinTimeout value."));
					return;
				}
			}

			bool Peek = false;

			if (Request.Header.TryGetQueryParameter("Peek", out s))
			{
				if (int.TryParse(s, out i) && i >= 0 && i <= 1)
				{
					Peek = i == 1;

					if (Peek && Multipart)
					{
						await Response.SendResponse(new BadRequestException("Peek and Count parameters cannot be used simultanerously."));
						return;
					}
				}
				else
				{
					await Response.SendResponse(new BadRequestException("Invalid Peek value."));
					return;
				}
			}

			IPersistedQueue Queue = await Database.GetQueue(QueueName, true);
			if (Queue is null)
			{
				await Response.SendResponse(new NotFoundException("Queue not found."));
				return;
			}

			if (Peek)
			{
				object Item = await Queue.Peek();

				if (Item is null)
				{
					Response.StatusCode = 204;
					await Response.SendResponse();
				}
				else
					await Response.Return(Item);
			}

			Task _ = Task.Run(async () =>
			{
				try
				{
					if (Multipart)
					{
						ChunkedList<EmbeddedContent> Items = new ChunkedList<EmbeddedContent>();
						int TimeLeft = Timeout;

						while (TimeLeft >= 0 && Count > 0)
						{
							DateTime TP = DateTime.UtcNow;
							TimeLeft = Timeout - (int)(TP - Start).TotalMilliseconds;

							object Item = await Queue.Dequeue(Math.Max(0, TimeLeft));
							if (Item is null)
								break;

							ContentResponse Encoded = await InternetContent.EncodeAsync(Item, Encoding.UTF8);
							if (Encoded.HasError)
							{
								await Response.SendResponse(Encoded.Error);
								return;
							}

							Count--;
							Timeout = MinTimeout;

							Items.Add(new EmbeddedContent()
							{
								Raw = Encoded.Encoded,
								ContentType = Encoded.ContentType
							});
						}

						await Response.Return(new MixedContent(Items.ToArray()));
					}
					else
					{
						object Item = await Queue.Dequeue(Timeout);

						if (Item is null)
						{
							Response.StatusCode = 204;
							await Response.SendResponse();
						}
						else
							await Response.Return(Item);
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					await Response.SendResponse(ex);
				}
			});
		}

		/// <summary>
		/// Executes the DELETE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task DELETE(HttpRequest Request, HttpResponse Response)
		{
			// TODO
			await this.GET(Request, Response);
		}

	}
}
