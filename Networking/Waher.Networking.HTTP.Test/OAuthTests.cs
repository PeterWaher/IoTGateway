using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Getters;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Networking.Sniffers;
using Waher.Security;
using Waher.Security.JWT;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class OAuthTests : IUserSource
	{
		private const string BaseUrl = "http://localhost:8081";
		private const string CallbackResource = "/Callback";
		private const string ProtectedResource = "/Hello";
		private const string Realm = "Test";
		private const string TestUserName = "User";
		private const string TestPassword = "Password";
		private const string TestClientId = "UnitTestClient";

		private HttpServer server;
		private ConsoleEventSink sink = null;
		private XmlFileSniffer xmlSniffer = null;
		private JwtFactory jwtFactory = null;

		/// <summary>
		/// Test context
		/// </summary>
		public TestContext TestContext { get; set; }

		[TestInitialize]
		public void TestInitialize()
		{
			string SnifferFileName = this.TestContext.TestName;
			if (string.IsNullOrEmpty(SnifferFileName))
				SnifferFileName = "OAuth";

			SnifferFileName = "Sniffers" + Path.DirectorySeparatorChar + SnifferFileName + ".xml";

			this.sink = new ConsoleEventSink();
			Log.Register(this.sink);

			File.Delete(SnifferFileName);
			this.xmlSniffer = new XmlFileSniffer(SnifferFileName,
				@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
				int.MaxValue, BinaryPresentationMethod.ByteCount);

			this.jwtFactory = JwtFactory.CreateHmacSha256();
			this.server = new HttpServer(8081, this.xmlSniffer);

			OAuthTokenResource TokenResource;
			OAuthAuthorizeResource AuthorizeResource;

			this.server.Register(new ProtectedResourceMetaData());
			this.server.Register(TokenResource = new OAuthTokenResource(this, this.jwtFactory));
			this.server.Register(AuthorizeResource = new OAuthAuthorizeResource(TokenResource, this.jwtFactory));
			this.server.Register(new AuthorizationServerMetaData(AuthorizeResource));

			this.server.Register(CallbackResource, Callback);
			this.server.Register(new Hello(this.jwtFactory, this));
		}

		private static async Task Callback(HttpRequest Request, HttpResponse Response)
		{
			if (!Request.Header.TryGetQueryParameter("code", out string Code) ||
				string.IsNullOrEmpty(Code))
			{
				await Response.SendResponse(new BadRequestException("Missing code."));
				return;
			}

			if (!Request.Header.TryGetQueryParameter("state", out string State))
				State = string.Empty;

			await Response.Return(new Dictionary<string, object>()
			{
				{ "code", Code },
				{ "state", State }
			});
		}

		[OAuthResourceName("Hello Test Web Service")]
		private class Hello(JwtFactory JwtFactory, IUserSource Users) 
			: HttpProtectedResource(ProtectedResource), IHttpGetMethod
		{
			private readonly HttpAuthenticationScheme[] authenticationSchemes =
				[
					new JwtAuthentication(Realm, Users, JwtFactory,
						new Uri(BaseUrl + ProtectedResourceMetaData.WellKnowResourcePath + ProtectedResource))
				];

			public bool AllowsGET => true;
			public override bool Synchronous => true;
			public override bool HandlesSubPaths => false;
			public override bool UserSessions => false;

			public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
			{
				return this.authenticationSchemes;
			}

			public async Task GET(HttpRequest Request, HttpResponse Response)
			{
				if (Request.User is null)
				{
					await Response.SendResponse(new ForbiddenException());
					return;
				}

				StringBuilder sb = new();

				sb.Append("Hello ");
				sb.Append(Request.User.UserName);
				sb.AppendLine(".");

				await Response.Return(sb.ToString());
			}
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.server is not null)
			{
				await this.server.DisposeAsync();
				this.server = null;
			}

			if (this.xmlSniffer is not null)
			{
				await this.xmlSniffer.DisposeAsync();
				this.xmlSniffer = null;
			}

			if (this.sink is not null)
			{
				Log.Unregister(this.sink);
				await this.sink.DisposeAsync();
				this.sink = null;
			}

			if (this.jwtFactory is not null)
			{
				this.jwtFactory.Dispose();
				this.jwtFactory = null;
			}
		}

		public Task<IUser> TryGetUser(string UserName)
		{
			if (UserName == "User")
				return Task.FromResult<IUser>(new User());
			else
				return Task.FromResult<IUser>(null);
		}


		[TestMethod]
		public async Task Test_01_Metadata_Discovery()
		{
			ContentResponse MetaDataResponse = await InternetContent.GetAsync(
				new Uri(BaseUrl + ProtectedResourceMetaData.WellKnowResourcePath + ProtectedResource));
			MetaDataResponse.AssertOk();

			object MetaData = MetaDataResponse.Decoded;
			Assert.AreEqual(BaseUrl + ProtectedResource, Required<string>(MetaData, "resource"));
			Assert.Contains(BaseUrl, Required<object[]>(MetaData, "authorization_servers"));
			Assert.Contains("header", Required<object[]>(MetaData, "bearer_methods_supported"));

			ContentResponse ServerMetaDataResponse = await InternetContent.GetAsync(
				new Uri(BaseUrl + AuthorizationServerMetaData.WellKnowResourcePath));
			ServerMetaDataResponse.AssertOk();

			object ServerMetaData = ServerMetaDataResponse.Decoded;
			Assert.AreEqual(BaseUrl, Required<string>(ServerMetaData, "issuer"));
			Assert.AreEqual(BaseUrl + "/oauth/authorize", Required<string>(ServerMetaData, "authorization_endpoint"));
			Assert.AreEqual(BaseUrl + "/oauth/token", Required<string>(ServerMetaData, "token_endpoint"));
			Assert.Contains("code", Required<object[]>(ServerMetaData, "response_types_supported"));
			Assert.Contains("authorization_code", Required<object[]>(ServerMetaData, "grant_types_supported"));
			Assert.Contains("S256", Required<object[]>(ServerMetaData, "code_challenge_methods_supported"));
		}

		[TestMethod]
		public async Task Test_02_AuthorizationCodeWithPkce()
		{
			string AccessToken = await GetAccessTokenWithAuthorizationCode();

			ContentResponse HelloResponse = await InternetContent.GetAsync(
				new Uri(BaseUrl + ProtectedResource),
				new KeyValuePair<string, string>("Authorization", "Bearer " + AccessToken));

			HelloResponse.AssertOk();
			Assert.AreEqual("Hello " + TestUserName + "." + Environment.NewLine, HelloResponse.Decoded);
		}

		[TestMethod]
		public async Task Test_03_HelloWithoutBearerToken_ReturnsUnauthorizedChallenge()
		{
			ContentResponse Response = await InternetContent.GetAsync(new Uri(BaseUrl + ProtectedResource));
			WebException Error = AssertWebException(Response, UnauthorizedException.Code);
			bool FoundChallenge = false;

			foreach (KeyValuePair<string, IEnumerable<string>> Header in Error.Headers)
			{
				if (Header.Key == "WWW-Authenticate")
				{
					foreach (string Value in Header.Value)
					{
						if (Value.StartsWith("Bearer "))
						{
							FoundChallenge = true;
							string Expected = "resource_metadata=\"" + BaseUrl + ProtectedResourceMetaData.WellKnowResourcePath + ProtectedResource + "\"";
							Assert.Contains(Expected, Value);
						}
					}
				}
			}

			Assert.IsTrue(FoundChallenge);
		}

		[TestMethod]
		public async Task Test_04_HelloWithInvalidBearerToken_ReturnsUnauthorized()
		{
			ContentResponse Response = await InternetContent.GetAsync(
				new Uri(BaseUrl + ProtectedResource),
				new KeyValuePair<string, string>("Authorization", "Bearer this-is-not-a-jwt"));

			AssertWebException(Response, UnauthorizedException.Code);
		}

		[TestMethod]
		public async Task Test_05_TokenEndpointWithInvalidAuthorizationCode_ReturnsForbidden()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + "/oauth/token"),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", "invalid-code" },
					{ "code_verifier", "not-used" }
				});

			WebException Error = AssertWebException(Response, ForbiddenException.Code);
			Assert.Contains("Invalid code", Error.Message);
		}

		[TestMethod]
		public async Task Test_06_TokenEndpointMissingPkceVerifier_ReturnsBadRequest()
		{
			string CodeVerifier = CreatePkceCodeVerifier();
			string CodeChallenge = CreateS256CodeChallenge(CodeVerifier);
			string Code = await GetAuthorizationCode(CodeChallenge, "S256");

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + "/oauth/token"),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", Code }
				});

			WebException Error = AssertWebException(Response, BadRequestException.Code);
			Assert.Contains("Missing code_verifier", Error.Message);
		}

		[TestMethod]
		public async Task Test_07_TokenEndpointWrongPkceVerifier_ReturnsForbidden()
		{
			string CodeVerifier = CreatePkceCodeVerifier();
			string CodeChallenge = CreateS256CodeChallenge(CodeVerifier);
			string Code = await GetAuthorizationCode(CodeChallenge, "S256");

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + "/oauth/token"),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", Code },
					{ "code_verifier", CreatePkceCodeVerifier() }
				});

			WebException Error = AssertWebException(Response, ForbiddenException.Code);
			Assert.Contains("Invalid code_verifier", Error.Message);
		}

		[TestMethod]
		public async Task Test_08_TokenEndpointReusedAuthorizationCode_ReturnsForbidden()
		{
			string CodeVerifier = CreatePkceCodeVerifier();
			string CodeChallenge = CreateS256CodeChallenge(CodeVerifier);
			string Code = await GetAuthorizationCode(CodeChallenge, "S256");

			ContentResponse FirstResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + "/oauth/token"),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", Code },
					{ "code_verifier", CodeVerifier }
				});
			FirstResponse.AssertOk();

			ContentResponse SecondResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + "/oauth/token"),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", Code },
					{ "code_verifier", CodeVerifier }
				});

			Assert.IsTrue(SecondResponse.HasError);
			WebException Error = AssertWebException(SecondResponse, ForbiddenException.Code);
			Assert.Contains("Invalid code", Error.Message);
		}

		[TestMethod]
		public async Task Test_09_AuthorizeEndpointInvalidLogin_ReturnsLoginFormWithError()
		{
			string CodeVerifier = CreatePkceCodeVerifier();
			string CodeChallenge = CreateS256CodeChallenge(CodeVerifier);
			string State = Guid.NewGuid().ToString("N");

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri(BaseUrl + "/oauth/authorize"),
				new Dictionary<string, string>()
				{
					{ "UserName", TestUserName },
					{ "Password", "wrong-password" },
					{ "From", BaseUrl + CallbackResource },
					{ "State", State },
					{ "CodeChallenge", CodeChallenge },
					{ "CodeChallengeMethod", "S256" }
				});

			Response.AssertOk();
			string Body = Encoding.UTF8.GetString(Response.Encoded);
			Assert.Contains("Invalid user name or password", Body);
		}

		private static WebException AssertWebException(
			ContentResponse Response, int ExpectedStatusCode)
		{
			Assert.IsTrue(Response.HasError);
			WebException Result = Response.Error as WebException;
			Assert.IsNotNull(Result);
			Assert.AreEqual(ExpectedStatusCode, (int)Result.StatusCode);
			return Result;
		}

		private static async Task<string> GetAccessTokenWithAuthorizationCode()
		{
			string CodeVerifier = CreatePkceCodeVerifier();
			string CodeChallenge = CreateS256CodeChallenge(CodeVerifier);
			string Code = await GetAuthorizationCode(CodeChallenge, "S256");

			ContentResponse TokenResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + "/oauth/token"),
				new Dictionary<string, string>()
				{
					{ "grant_type", "authorization_code" },
					{ "code", Code },
					{ "client_id", TestClientId },
					{ "code_verifier", CodeVerifier }
				});

			TokenResponse.AssertOk();
			object Parsed = JSON.Parse(Encoding.UTF8.GetString(TokenResponse.Encoded));
			Assert.AreEqual("Bearer", Required<string>(Parsed, "token_type"));

			string AccessToken = Required<string>(Parsed, "access_token");
			Assert.IsFalse(string.IsNullOrEmpty(AccessToken));

			return AccessToken;
		}

		private static async Task<string> GetAuthorizationCode(string CodeChallenge, string CodeChallengeMethod)
		{
			string State = Guid.NewGuid().ToString();
			string RedirectUri = BaseUrl + CallbackResource;
			string AuthorizeUri = BaseUrl + "/oauth/authorize" +
				"?response_type=code" +
				"&client_id=" + Uri.EscapeDataString(TestClientId) +
				"&state=" + Uri.EscapeDataString(State) +
				"&redirect_uri=" + Uri.EscapeDataString(RedirectUri) +
				"&code_challenge=" + Uri.EscapeDataString(CodeChallenge) +
				"&code_challenge_method=" + Uri.EscapeDataString(CodeChallengeMethod);

			ContentResponse LoginForm = await InternetContent.GetAsync(new Uri(AuthorizeUri));
			LoginForm.AssertOk();

			ContentResponse LoginResponse = await InternetContent.PostAsync(
				new Uri(BaseUrl + "/oauth/authorize"),
				new Dictionary<string, string>()
				{
					{ "UserName", TestUserName },
					{ "Password", TestPassword },
					{ "From", RedirectUri },
					{ "State", State },
					{ "CodeChallenge", CodeChallenge },
					{ "CodeChallengeMethod", CodeChallengeMethod }
				});

			LoginResponse.AssertOk();

			Dictionary<string, object> CallbackValues = LoginResponse.Decoded as Dictionary<string, object>;
			Assert.IsNotNull(CallbackValues);

			Assert.IsTrue(CallbackValues.TryGetValue("state", out object ReturnedState), "Callback response did not contain state.");
			Assert.AreEqual(State, ReturnedState);
			Assert.IsTrue(CallbackValues.TryGetValue("code", out object Code), "Callback response did not contain code.");
			string s = Code as string;
			Assert.IsFalse(string.IsNullOrEmpty(s));

			return s;
		}

		private static string CreatePkceCodeVerifier()
		{
			byte[] RandomBytes = new byte[32];
			using RandomNumberGenerator Rnd = RandomNumberGenerator.Create();
			Rnd.GetBytes(RandomBytes);
			return Base64Url.Encode(RandomBytes);
		}

		private static string CreateS256CodeChallenge(string CodeVerifier)
		{
			return Base64Url.Encode(Hashes.ComputeSHA256Hash(Encoding.UTF8.GetBytes(CodeVerifier)));
		}

		private static T Required<T>(object Dictionary, string Key)
		{
			object Result;

			if (Dictionary is IDictionary<string, object> Typed &&
				Typed.TryGetValue(Key, out object GenericValue))
			{
				Result = GenericValue;
			}
			else if (Dictionary is IDictionary Untyped && Untyped.Contains(Key))
				Result = Untyped[Key];
			else
				throw new Exception("Expected JSON object to contain key: " + Key);

			if (Result is null)
				throw new Exception("Property value is null: " + Key);

			if (Result is not T TypedResult)
			{
				throw new Exception("Property value not of expected type: " + Key +
					" (" + Result.GetType().FullName + ")");
			}

			return TypedResult;
		}
	}
}
