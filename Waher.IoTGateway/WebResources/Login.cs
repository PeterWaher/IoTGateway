using System;
using System.Collections.Generic;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Script;
using Waher.Security;
using Waher.IoTGateway;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Provides a resource that allows the caller to login to the gateway through a POST method call.
	/// </summary>
	public class Login : HttpAsynchronousResource, IHttpPostMethod
	{
		/// <summary>
		/// Provides a resource that allows the caller to login to the gateway through a POST method call.
		/// </summary>
		public Login()
			: base("/Login")
		{
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
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async void POST(HttpRequest Request, HttpResponse Response)
		{
			try
			{
				if (!Request.HasData || Request.Session == null)
					throw new BadRequestException();

				object Obj = Request.DecodeData();
				Dictionary<string, string> Form = Obj as Dictionary<string, string>;
				string From;

				if (Form == null ||
					!Form.TryGetValue("UserName", out string UserName) ||
					!Form.TryGetValue("Password", out string Password))
				{
					throw new BadRequestException();
				}

				if (Request.Session.TryGetVariable("from", out Variable v))
				{
					From = v.ValueObject as string;
					if (string.IsNullOrEmpty(From))
						From = "/Index.md";
				}
				else
					From = "/Index.md";

				LoginResult LoginResult = await Gateway.DoMainXmppLogin(UserName, Password, Request.RemoteEndPoint);
				if (LoginResult == LoginResult.Successful)
				{
					Log.Informational("User logged in.", UserName, Request.RemoteEndPoint, "LoginSuccessful", EventLevel.Minor);
					DoLogin(Request, From);
				}
				else
				{
					if (LoginResult == LoginResult.InvalidLogin)
					{
						Log.Warning("Invalid login attempt.", UserName, Request.RemoteEndPoint, "LoginFailure", EventLevel.Minor);
						Request.Session["LoginError"] = "Invalid login credentials provided.";
					}
					else
					{
						Log.Error("Unable to connect to XMPP server to validate login credentials.");
						Request.Session["LoginError"] = "Unable to connect to XMPP server.";
					}

					throw new SeeOtherException(Request.Header.Referer.Value);
				}
			}
			catch (Exception ex)
			{
				Response.SendResponse(ex);
			}
			finally
			{
				Response.Dispose();
			}
		}

		internal static void DoLogin(HttpRequest Request, string From)
		{
			Request.Session["User"] = new User();
			Request.Session.Remove("LoginError");

			throw new SeeOtherException(From);
		}

		private class User : IUser
		{
			public string PasswordHash
			{
				get
				{
					return string.Empty;
				}
			}

			public string PasswordHashType
			{
				get
				{
					return string.Empty;
				}
			}

			public string UserName
			{
				get
				{
					return Gateway.XmppClient.UserName;
				}
			}

			public bool HasPrivilege(string Privilege)
			{
				return false;
			}
		}

	}
}
