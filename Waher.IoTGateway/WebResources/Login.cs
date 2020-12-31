using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Script;
using Waher.Security;
using Waher.Security.Users;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Provides a resource that allows the caller to login to the gateway through a POST method call.
	/// </summary>
	public class Login : HttpSynchronousResource, IHttpPostMethod
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
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			if (!Request.HasData || Request.Session is null)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
			string From;

			if (!(Obj is Dictionary<string, string> Form) ||
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

			LoginResult Result = await Users.Login(UserName, Password, Request.RemoteEndPoint, "Web");

			switch (Result.Type)
			{
				case LoginResultType.PermanentlyBlocked:
					StringBuilder sb = new StringBuilder();

					sb.Append("This endpoint (");
					sb.Append(Request.RemoteEndPoint);
					sb.Append(") has been blocked from the system.");

					Request.Session["LoginError"] = sb.ToString();
					throw new SeeOtherException(Request.Header.Referer.Value);

				case LoginResultType.TemporarilyBlocked:
					sb = new StringBuilder();
					DateTime TP = Result.Next.Value;
					DateTime Today = DateTime.Today;

					sb.Append("Too many failed login attempts in a row registered. Try again after ");
					sb.Append(TP.ToLongTimeString());

					if (TP.Date != Today)
					{
						if (TP.Date == Today.AddDays(1))
							sb.Append(" tomorrow");
						else
						{
							sb.Append(", ");
							sb.Append(TP.ToShortDateString());
						}
					}

					sb.Append('.');

					Request.Session["LoginError"] = sb.ToString();
					throw new SeeOtherException(Request.Header.Referer.Value);

				case LoginResultType.NoPassword:
					Request.Session["LoginError"] = "No password provided.";
					throw new SeeOtherException(Request.Header.Referer.Value);

				case LoginResultType.InvalidCredentials:
				default:
					Request.Session["LoginError"] = "Invalid login credentials provided.";
					throw new SeeOtherException(Request.Header.Referer.Value);

				case LoginResultType.Success:
					DoLogin(Request, From, Result.User);
					break;
			}
		}

		internal static void DoLogin(HttpRequest Request, string From, User User)
		{
			DoLogin(Request, From, false, User);
		}

		internal static void DoLogin(HttpRequest Request, string From)
		{
			DoLogin(Request, From, true, new InternalUser());
		}

		private static void DoLogin(HttpRequest Request, string From, bool AutoLogin, IUser User)
		{
			Request.Session["User"] = User;
			Request.Session[" AutoLogin "] = AutoLogin;
			Request.Session.Remove("LoginError");

			if (!string.IsNullOrEmpty(From))
				throw new SeeOtherException(From);
		}

		private class InternalUser : IUser
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
					return Gateway.XmppClient?.UserName;
				}
			}

			public bool HasPrivilege(string Privilege)
			{
				return true;
			}
		}

	}
}
