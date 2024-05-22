using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Content.Text;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Security;

namespace Waher.WebService.Script
{
	/// <summary>
	/// Web service that can be used to execute script on the server.
	/// </summary>
	public class ScriptService : HttpAsynchronousResource, IHttpPostMethod
	{
		private readonly HttpAuthenticationScheme[] authenticationSchemes;

		/// <summary>
		/// Web service that can be used to execute script on the server.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="AuthenticationSchemes">Authentication schemes.</param>
		public ScriptService(string ResourceName, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base(ResourceName)
		{
			this.authenticationSchemes = AuthenticationSchemes;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => true;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			IUser User = null;

			if (Request.Session is null ||
				!Request.Session.TryGetVariable("User", out Variable v) ||
				((User = v.ValueObject as IUser) is null) ||
				!User.HasPrivilege("Admin.Lab.Script"))
			{
				throw ForbiddenException.AccessDenied(this.ResourceName, User?.UserName ?? string.Empty, "Admin.Lab.Script");
			}

			object Obj = Request.HasData ? await Request.DecodeDataAsync() : null;
			string s = Obj as string;
			string Tag = Request.Header["X-TAG"];

			if (string.IsNullOrEmpty(Tag))
				throw new BadRequestException();

			State State;

			if (string.IsNullOrEmpty(s))
			{
				if (!string.IsNullOrEmpty(s = Request.Header["X-X"]) && int.TryParse(s, out int x) &&
					!string.IsNullOrEmpty(s = Request.Header["X-Y"]) && int.TryParse(s, out int y))
				{
					if (!Request.Session.TryGetVariable("Graphs", out v) ||
						!(v.ValueObject is Dictionary<string, KeyValuePair<Graph, object[]>> Graphs))
					{
						throw new NotFoundException("Graphs not found.");
					}

					KeyValuePair<Graph, object[]> Rec;

					lock (Graphs)
					{
						if (!Graphs.TryGetValue(Tag, out Rec))
							throw new NotFoundException("Graph not found.");
					}

					s = Rec.Key.GetBitmapClickScript(x, y, Rec.Value);

					Response.ContentType = PlainTextCodec.DefaultContentType;
					await Response.Write(s);
					await Response.SendResponse();
				}
				else
				{
					if (!State.TryGetState(Tag, out State))
						throw new NotFoundException("Expression not found.");

					State.SetRequestResponse(Request, Response, User);
				}
			}
			else
			{
				if (!Request.Session.TryGetVariable("Timeout", out v) ||
					!(v.ValueObject is double Timeout) || Timeout <= 0)
				{
					Timeout = 5 * 60 * 1000;    // 5 minutes
				}

				State = new State(Request, Response, Tag, (int)(Timeout + 0.5), User);

				try
				{
					Expression Exp = new Expression(s);

					if (!Exp.ForAll(this.IsAuthorized, User, SearchMethod.TreeOrder))
					{
						State.NewResult(new ObjectValue(new ForbiddenException("Unauthorized to execute expression.")));
						return;
					}

					State.SetExpression(Exp);
				}
				catch (Exception ex)
				{
					State.NewResult(new ObjectValue(ex));
					return;
				}

				State.Start();
			}
		}

		private bool IsAuthorized(ScriptNode Node, out ScriptNode NewNode, object State)
		{
			NewNode = null;

			if (Node is null)
				return true;
			else if (State is IUser User)
				return User.HasPrivilege("Script." + Node.GetType().FullName);
			else
				return false;
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
