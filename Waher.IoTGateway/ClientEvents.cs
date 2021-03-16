using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Networking.HTTP.WebSockets;
using Waher.Persistence.Serialization;
using Waher.Runtime.Cache;
using Waher.Runtime.Threading;
using Waher.Script;
using Waher.Security;

namespace Waher.IoTGateway
{
	/// <summary>
	/// The ClientEvents class allows applications to push information asynchronously to web clients connected to the server.
	/// It keeps track of all open web clients that have the Events.js Javascript file loaded. When Events.js is loaded, it
	/// creates a unique Tab ID which it reports back to the ClientEvents resource. There, it is paired with the current resource
	/// being displayed, including any query parameters. Any application can then use ClientEvents to push information, either
	/// text strings or JSON objects, to any JavaScript function currently loaded by the client. Subsets of connected web clients
	/// can be made, either on resource or query string parameters.
	/// 
	/// Note: The use of Tab IDs allows you to uniquely identify each open tab, even when multiple tabs are open at the same time in
	/// the same browser.
	/// 
	/// Static methods of interest:
	/// 
	/// GetOpenLocations() allows you to get a list of resources currently being viewed.
	/// 
	/// GetTabIDsForLocation() allows you to get the set of Tab IDs viewing a particular resource, optionally filtered by query parameters.
	/// 
	/// GetTabIDsForLocations() allows you to get the set of Tab IDs viewing a set of resources.
	/// 
	/// GetTabIDs() returns the set of Tab IDs fr all open web clients.
	/// 
	/// PushEvent() allows you to push information to a set of clients, identified by their Tab IDs. Events.js performs constant long-polling
	/// of the server, to retrieve any events it might push to the client. Events include a Type and a Data element. The Type is translated into
	/// a Javascript function name. Data is passed to the function, as a parameter. The Data parameter can be either a string, or a JSON object.
	/// You can use the static JSON class available in the Waher.Content library to encode and decode JSON.
	/// 
	/// ReportAsynchronousResult() allows you to report asynchronously evaluated results back to clients.
	/// </summary>
	public class ClientEvents : HttpAsynchronousResource, IHttpGetMethod, IHttpPostMethod
	{
		/// <summary>
		/// Resource managing asynchronous events to web clients.
		/// </summary>
		public ClientEvents()
			: base("/ClientEvents")
		{
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => true;

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

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
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			if (string.IsNullOrEmpty(Request.SubPath))
				throw new BadRequestException("Sub-path missing.");

			ContentQueue Queue;
			string Id = Request.SubPath.Substring(1);

			lock (requestsByContentID)
			{
				if (requestsByContentID.TryGetValue(Id, out Queue))
				{
					if (Queue.Content is null)
					{
						Queue.Response = Response;
						return;
					}

					requestsByContentID.Remove(Id);
				}
				else
				{
					requestsByContentID[Id] = new ContentQueue(Id)
					{
						Response = Response
					};

					return;
				}
			}

			Response.ContentType = Queue.ContentType;
			await Response.Write(Queue.Content, 0, Queue.Content.Length);
			await Response.SendResponse();
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			if (!Request.HasData || Request.Session is null)
				throw new BadRequestException("POST request missing data.");

			// TODO: Check User authenticated

			object Obj = Request.DecodeData();
			if (!(Obj is string Location))
				throw new BadRequestException("Expected location.");

			string TabID = Request.Header["X-TabID"];
			if (string.IsNullOrEmpty(TabID))
				throw new BadRequestException("Expected X-TabID header.");

			TabQueue Queue = Register(Request, null, Location, TabID);
			StringBuilder Json = null;

			Response.ContentType = "application/json";

			if (!await Queue.SyncObj.TryBeginWrite(10000))
				throw new InternalServerErrorException("Unable to get access to queue.");

			try
			{
				if (!(Queue.Queue.First is null))
				{
					foreach (string Event in Queue.Queue)
					{
						if (Json is null)
							Json = new StringBuilder("[");
						else
							Json.Append(',');

						Json.Append(Event);
					}

					Queue.Queue.Clear();
					Queue.Response = null;
				}
				else
					Queue.Response = Response;
			}
			finally
			{
				await Queue.SyncObj.EndWrite();
			}

			if (!(Json is null))
			{
				timeoutByTabID.Remove(TabID);

				Json.Append(']');
				await Response.Write(Json.ToString());
				await Response.SendResponse();
				Response.Dispose();
			}
			else
				timeoutByTabID[TabID] = Queue;
		}

		private static TabQueue Register(HttpRequest Request, WebSocket Socket, string Location, string TabID)
		{
			Uri Uri = new Uri(Location);
			string Resource = Uri.LocalPath;
			(string, string, string)[] Query = null;
			Variables Session = Request.Session;
			string s;

			if (!string.IsNullOrEmpty(Uri.Query))
			{

				s = Uri.Query;
				if (s.StartsWith("?"))
					s = s.Substring(1);

				string[] Parts = s.Split('&');
				Query = new (string, string, string)[Parts.Length];
				int i, j = 0;

				foreach (string Part in Parts)
				{
					i = Part.IndexOf('=');
					if (i < 0)
						Query[j++] = (Part, string.Empty, string.Empty);
					else
					{
						string s2 = Part.Substring(i + 1);
						Query[j++] = (Part.Substring(0, i), s2, System.Net.WebUtility.UrlDecode(s2));
					}
				}
			}

			if (eventsByTabID.TryGetValue(TabID, out TabQueue Queue))
				Queue.WebSocket = Socket;
			else
			{
				HttpFieldCookie Cookie = Request.Header.Cookie;
				string HttpSessionID = Cookie is null ? string.Empty : Cookie["HttpSessionID"];

				Queue = new TabQueue(TabID, HttpSessionID, Session)
				{
					WebSocket = Socket
				};

				eventsByTabID[TabID] = Queue;
			}

			lock (locationByTabID)
			{
				if (!locationByTabID.TryGetValue(TabID, out s) || s != Resource)
					locationByTabID[TabID] = Resource;
			}

			lock (tabIdsByLocation)
			{
				if (!tabIdsByLocation.TryGetValue(Resource, out Dictionary<string, (string, string, string)[]> TabIds))
				{
					TabIds = new Dictionary<string, (string, string, string)[]>();
					tabIdsByLocation[Resource] = TabIds;
				}

				TabIds[TabID] = Query;
			}

			Type UserType;
			object UserObject;

			if (!(Session is null) && 
				Session.TryGetVariable(" User ", out Variable UserVariable) &&
				!((UserObject = UserId(UserVariable.ValueObject)) is null))
			{
				lock (usersByTabID)
				{
					if (!usersByTabID.TryGetValue(TabID, out object Obj2) || !Obj2.Equals(UserObject))
						usersByTabID[TabID] = UserObject;
				}

				UserType = UserObject.GetType();

				lock (tabIdsByUser)
				{
					if (!tabIdsByUser.TryGetValue(UserType, out Dictionary<object, Dictionary<string, (string, string, string)[]>> UserObjects))
					{
						UserObjects = new Dictionary<object, Dictionary<string, (string, string, string)[]>>();
						tabIdsByUser[UserType] = UserObjects;
					}

					if (!UserObjects.TryGetValue(UserObject, out Dictionary<string, (string, string, string)[]> TabIds))
					{
						TabIds = new Dictionary<string, (string, string, string)[]>();
						UserObjects[UserObject] = TabIds;
					}

					TabIds[TabID] = Query;
				}
			}
			else
			{
				lock (usersByTabID)
				{
					if (!usersByTabID.TryGetValue(TabID, out UserObject))
						UserObject = null;
				}

				if (!(UserObject is null))
				{
					UserType = UserObject.GetType();

					lock (tabIdsByUser)
					{
						if (tabIdsByUser.TryGetValue(UserType, out Dictionary<object, Dictionary<string, (string, string, string)[]>> UserObjects))
						{
							if (UserObjects.Remove(UserObject) && UserObjects.Count == 0)
								tabIdsByUser.Remove(UserType);
						}
					}
				}
			}

			return Queue;
		}

		private static object UserId(object User)
		{
			if (User is IUser User2)
				return User2.UserName;
			else if (User is GenericObject GenObj)
				return GenObj.ObjectId;
			else
				return User;
		}

		internal static async Task RegisterWebSocket(WebSocket Socket, string Location, string TabID)
		{
			TabQueue Queue = Register(Socket.HttpRequest, Socket, Location, TabID);
			LinkedList<string> ToSend = null;

			if (!await Queue.SyncObj.TryBeginWrite(10000))
				throw new InternalServerErrorException("Unable to get access to queue.");

			try
			{
				if (!(Queue.Queue.First is null))
				{
					ToSend = new LinkedList<string>();

					foreach (string s2 in Queue.Queue)
						ToSend.AddLast(s2);

					Queue.Queue.Clear();
				}
			}
			finally
			{
				await Queue.SyncObj.EndWrite();
			}

			if (!(ToSend is null))
			{
				foreach (string s2 in ToSend)
					Socket.Send(s2, 4096);
			}
		}

		internal static void Ping(string TabID)
		{
			if (eventsByTabID.TryGetValue(TabID, out TabQueue TabQueue))
				Gateway.HttpServer?.GetSession(TabQueue.SessionID, false);
		}

		internal static async Task UnregisterWebSocket(WebSocket Socket, string Location, string TabID)
		{
			if (eventsByTabID.TryGetValue(TabID, out TabQueue Queue) && Queue.WebSocket == Socket)
			{
				if (!await Queue.SyncObj.TryBeginWrite(10000))
					throw new InternalServerErrorException("Unable to get access to queue.");

				try
				{
					Queue.WebSocket = null;
				}
				finally
				{
					await Queue.SyncObj.EndWrite();
				}
			}

			Uri Uri = new Uri(Location);
			Remove(TabID, Uri.LocalPath);
		}


		private static readonly Cache<string, TabQueue> eventsByTabID = GetTabQueueCache();
		private static readonly Cache<string, TabQueue> timeoutByTabID = GetTabTimeoutCache();
		private static readonly Cache<string, ContentQueue> requestsByContentID = GetContentCache();
		private static readonly Dictionary<string, string> locationByTabID = new Dictionary<string, string>();
		private static readonly Dictionary<string, object> usersByTabID = new Dictionary<string, object>();
		private static readonly Dictionary<string, Dictionary<string, (string, string, string)[]>> tabIdsByLocation =
			new Dictionary<string, Dictionary<string, (string, string, string)[]>>(StringComparer.OrdinalIgnoreCase);
		private static readonly Dictionary<Type, Dictionary<object, Dictionary<string, (string, string, string)[]>>> tabIdsByUser =
			new Dictionary<Type, Dictionary<object, Dictionary<string, (string, string, string)[]>>>();

		private static Cache<string, TabQueue> GetTabTimeoutCache()
		{
			Cache<string, TabQueue> Result = new Cache<string, TabQueue>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromSeconds(20), true);
			Result.Removed += TimeoutCacheItem_Removed;
			return Result;
		}

		private static async void TimeoutCacheItem_Removed(object Sender, CacheItemEventArgs<string, TabQueue> e)
		{
			if (e.Reason == RemovedReason.NotUsed)
			{
				HttpResponse Response = e.Value.Response;

				if (!(Response is null))
				{
					try
					{
						e.Value.Response = null;

						await Response.Write("[{\"type\":\"NOP\"}]");
						await Response.SendResponse();
					}
					catch (Exception)
					{
						// Ignore
					}
					finally
					{
						Response.Dispose();
					}
				}
			}
		}

		private static Cache<string, TabQueue> GetTabQueueCache()
		{
			Cache<string, TabQueue> Result = new Cache<string, TabQueue>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromSeconds(30), true);
			Result.Removed += QueueCacheItem_Removed;
			return Result;
		}

		private static void QueueCacheItem_Removed(object Sender, CacheItemEventArgs<string, TabQueue> e)
		{
			TabQueue Queue = e.Value;
			string TabID = Queue.TabID;

			Remove(TabID, null);
			Queue.Dispose();
		}

		private static void Remove(string TabID, string Resource)
		{
			string Location;
			object User;

			lock (locationByTabID)
			{
				if (locationByTabID.TryGetValue(TabID, out Location) && (Resource is null || Location == Resource))
					locationByTabID.Remove(TabID);
				else
					Location = null;
			}

			if (!(Location is null))
			{
				lock (tabIdsByLocation)
				{
					if (tabIdsByLocation.TryGetValue(Location, out Dictionary<string, (string, string, string)[]> TabIDs))
					{
						if (TabIDs.Remove(TabID) && TabIDs.Count == 0)
							tabIdsByLocation.Remove(Location);
					}
				}
			}

			lock (usersByTabID)
			{
				if (usersByTabID.TryGetValue(TabID, out User))
					usersByTabID.Remove(TabID);
				else
					User = null;
			}

			if (!(User is null))
			{
				Type UserType = User.GetType();

				lock (tabIdsByUser)
				{
					if (tabIdsByUser.TryGetValue(UserType, out Dictionary<object, Dictionary<string, (string, string, string)[]>> UserObjects) &&
						UserObjects.TryGetValue(User, out Dictionary<string, (string, string, string)[]> TabIDs))
					{
						if (TabIDs.Remove(TabID) && TabIDs.Count == 0 &&
							UserObjects.Remove(User) && UserObjects.Count == 0)
						{
							tabIdsByUser.Remove(UserType);
						}
					}
				}
			}
		}

		private static Cache<string, ContentQueue> GetContentCache()
		{
			Cache<string, ContentQueue> Result = new Cache<string, ContentQueue>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromSeconds(60), true);
			Result.Removed += ContentCacheItem_Removed;
			return Result;
		}

		private static async void ContentCacheItem_Removed(object Sender, CacheItemEventArgs<string, ContentQueue> e)
		{
			if (e.Reason != RemovedReason.Manual)
			{
				try
				{
					HttpResponse Response = e.Value.Response;

					Response.ContentType = "text/plain";
					await Response.Write("Request took too long to complete.");
					await Response.SendResponse();
				}
				catch (Exception)
				{
					// Ignore
				}
			}
		}

		/// <summary>
		/// Reports asynchronously evaluated result back to a client.
		/// </summary>
		/// <param name="Id">Content ID</param>
		/// <param name="ContentType">Content-Type of result.</param>
		/// <param name="Result">Binary encoding of result.</param>
		public static async Task ReportAsynchronousResult(string Id, string ContentType, byte[] Result)
		{
			try
			{
				ContentQueue Queue;

				lock (requestsByContentID)
				{
					if (requestsByContentID.TryGetValue(Id, out Queue))
					{
						if (Queue.Response is null)
							return;

						requestsByContentID.Remove(Id);
					}
					else
					{
						Queue = new ContentQueue(Id)
						{
							ContentType = ContentType,
							Content = Result
						};

						requestsByContentID[Id] = Queue;
						return;
					}
				}

				Queue.Response.ContentType = ContentType;
				await Queue.Response.Write(Result, 0, Result.Length);
				await Queue.Response.SendResponse();
			}
			catch (Exception)
			{
				// Ignore
			}
		}

		/// <summary>
		/// Returns a list of resources that are currently open.
		/// </summary>
		/// <returns>List of resources that are currently open.</returns>
		public static string[] GetOpenLocations()
		{
			string[] Result;

			lock (tabIdsByLocation)
			{
				Result = new string[tabIdsByLocation.Count];
				tabIdsByLocation.Keys.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Returns a list of active users
		/// </summary>
		/// <returns>List of active users.</returns>
		public static object[] GetActiveUsers()
		{
			List<object> Result = new List<object>();

			lock (tabIdsByUser)
			{
				foreach (KeyValuePair<Type, Dictionary<object, Dictionary<string, (string, string, string)[]>>> P in tabIdsByUser)
					Result.AddRange(P.Value.Keys);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that display a particular resource.
		/// </summary>
		/// <param name="Location">Resource.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForLocation(string Location)
		{
			return GetTabIDsForLocation(Location, new KeyValuePair<string, string>[0]);
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that display a particular resource.
		/// </summary>
		/// <param name="Location">Resource.</param>
		/// <param name="QueryParameter1">Name of query parameter 1 in query filter.</param>
		/// <param name="QueryParameterValue1">Value of query parameter 1 in query filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForLocation(string Location, string QueryParameter1, string QueryParameterValue1)
		{
			return GetTabIDsForLocation(Location, new KeyValuePair<string, string>(QueryParameter1, QueryParameterValue1));
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that display a particular resource.
		/// </summary>
		/// <param name="Location">Resource.</param>
		/// <param name="QueryParameter1">Name of query parameter 1 in query filter.</param>
		/// <param name="QueryParameterValue1">Value of query parameter 1 in query filter.</param>
		/// <param name="QueryParameter2">Name of query parameter 2 in query filter.</param>
		/// <param name="QueryParameterValue2">Value of query parameter 2 in query filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForLocation(string Location, string QueryParameter1, string QueryParameterValue1,
			string QueryParameter2, string QueryParameterValue2)
		{
			return GetTabIDsForLocation(Location,
				new KeyValuePair<string, string>(QueryParameter1, QueryParameterValue1),
				new KeyValuePair<string, string>(QueryParameter2, QueryParameterValue2));
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that display a particular resource.
		/// </summary>
		/// <param name="Location">Resource.</param>
		/// <param name="QueryFilter">Query parameter filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForLocation(string Location, params KeyValuePair<string, string>[] QueryFilter)
		{
			return GetTabIDsForLocation(Location, false, QueryFilter);
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that display a particular resource.
		/// </summary>
		/// <param name="Location">Resource.</param>
		/// <param name="IgnoreCase">If tag values are case insensitive.</param>
		/// <param name="QueryParameter1">Name of query parameter 1 in query filter.</param>
		/// <param name="QueryParameterValue1">Value of query parameter 1 in query filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForLocation(string Location, bool IgnoreCase,
			string QueryParameter1, string QueryParameterValue1)
		{
			return GetTabIDsForLocation(Location, IgnoreCase, new KeyValuePair<string, string>(QueryParameter1, QueryParameterValue1));
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that display a particular resource.
		/// </summary>
		/// <param name="Location">Resource.</param>
		/// <param name="IgnoreCase">If tag values are case insensitive.</param>
		/// <param name="QueryParameter1">Name of query parameter 1 in query filter.</param>
		/// <param name="QueryParameterValue1">Value of query parameter 1 in query filter.</param>
		/// <param name="QueryParameter2">Name of query parameter 2 in query filter.</param>
		/// <param name="QueryParameterValue2">Value of query parameter 2 in query filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForLocation(string Location, bool IgnoreCase,
			string QueryParameter1, string QueryParameterValue1,
			string QueryParameter2, string QueryParameterValue2)
		{
			return GetTabIDsForLocation(Location, IgnoreCase,
				new KeyValuePair<string, string>(QueryParameter1, QueryParameterValue1),
				new KeyValuePair<string, string>(QueryParameter2, QueryParameterValue2));
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that display a particular resource.
		/// </summary>
		/// <param name="Location">Resource.</param>
		/// <param name="IgnoreCase">If tag values are case insensitive.</param>
		/// <param name="QueryFilter">Query parameter filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForLocation(string Location, bool IgnoreCase, params KeyValuePair<string, string>[] QueryFilter)
		{
			lock (tabIdsByLocation)
			{
				if (tabIdsByLocation.TryGetValue(Location, out Dictionary<string, (string, string, string)[]> TabIDs))
					return ProcessQueryFilterLocked(TabIDs, QueryFilter, IgnoreCase);
				else
					return new string[0];
			}
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that a specific user views.
		/// </summary>
		/// <param name="User">User object.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForUser(object User)
		{
			return GetTabIDsForUser(User, null, false, new KeyValuePair<string, string>[0]);
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that a specific user views.
		/// </summary>
		/// <param name="User">User object.</param>
		/// <param name="QueryFilter">Query parameter filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForUser(object User, params KeyValuePair<string, string>[] QueryFilter)
		{
			return GetTabIDsForUser(User, null, false, QueryFilter);
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that a specific user views.
		/// </summary>
		/// <param name="User">User object.</param>
		/// <param name="IgnoreCase">If tag values are case insensitive.</param>
		/// <param name="QueryParameter1">Name of query parameter 1 in query filter.</param>
		/// <param name="QueryParameterValue1">Value of query parameter 1 in query filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForUser(object User, bool IgnoreCase,
			string QueryParameter1, string QueryParameterValue1)
		{
			return GetTabIDsForUser(User, IgnoreCase, new KeyValuePair<string, string>(QueryParameter1, QueryParameterValue1));
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that a specific user views.
		/// </summary>
		/// <param name="User">User object.</param>
		/// <param name="IgnoreCase">If tag values are case insensitive.</param>
		/// <param name="QueryParameter1">Name of query parameter 1 in query filter.</param>
		/// <param name="QueryParameterValue1">Value of query parameter 1 in query filter.</param>
		/// <param name="QueryParameter2">Name of query parameter 2 in query filter.</param>
		/// <param name="QueryParameterValue2">Value of query parameter 2 in query filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForUser(object User, bool IgnoreCase,
			string QueryParameter1, string QueryParameterValue1,
			string QueryParameter2, string QueryParameterValue2)
		{
			return GetTabIDsForUser(User, IgnoreCase,
				new KeyValuePair<string, string>(QueryParameter1, QueryParameterValue1),
				new KeyValuePair<string, string>(QueryParameter2, QueryParameterValue2));
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that a specific user views.
		/// </summary>
		/// <param name="User">User object.</param>
		/// <param name="IgnoreCase">If tag values are case insensitive.</param>
		/// <param name="QueryFilter">Query parameter filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForUser(object User, bool IgnoreCase, params KeyValuePair<string, string>[] QueryFilter)
		{
			return GetTabIDsForUser(User, null, IgnoreCase, QueryFilter);
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that a specific user views.
		/// </summary>
		/// <param name="User">User object.</param>
		/// <param name="Location">If restricting the tabs to a given location.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForUser(object User, string Location)
		{
			return GetTabIDsForUser(User, Location, new KeyValuePair<string, string>[0]);
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that a specific user views.
		/// </summary>
		/// <param name="User">User object.</param>
		/// <param name="Location">If restricting the tabs to a given location.</param>
		/// <param name="QueryParameter1">Name of query parameter 1 in query filter.</param>
		/// <param name="QueryParameterValue1">Value of query parameter 1 in query filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForUser(object User, string Location,
			string QueryParameter1, string QueryParameterValue1)
		{
			return GetTabIDsForUser(User, Location,
				new KeyValuePair<string, string>(QueryParameter1, QueryParameterValue1));
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that a specific user views.
		/// </summary>
		/// <param name="User">User object.</param>
		/// <param name="Location">If restricting the tabs to a given location.</param>
		/// <param name="QueryParameter1">Name of query parameter 1 in query filter.</param>
		/// <param name="QueryParameterValue1">Value of query parameter 1 in query filter.</param>
		/// <param name="QueryParameter2">Name of query parameter 2 in query filter.</param>
		/// <param name="QueryParameterValue2">Value of query parameter 2 in query filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForUser(object User, string Location,
			string QueryParameter1, string QueryParameterValue1,
			string QueryParameter2, string QueryParameterValue2)
		{
			return GetTabIDsForUser(User, Location,
				new KeyValuePair<string, string>(QueryParameter1, QueryParameterValue1),
				new KeyValuePair<string, string>(QueryParameter2, QueryParameterValue2));
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that a specific user views.
		/// </summary>
		/// <param name="User">User object.</param>
		/// <param name="Location">If restricting the tabs to a given location.</param>
		/// <param name="QueryFilter">Query parameter filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForUser(object User, string Location, params KeyValuePair<string, string>[] QueryFilter)
		{
			return GetTabIDsForUser(User, Location, false, QueryFilter);
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that a specific user views.
		/// </summary>
		/// <param name="User">User object.</param>
		/// <param name="Location">If restricting the tabs to a given location.</param>
		/// <param name="IgnoreCase">If tag values are case insensitive.</param>
		/// <param name="QueryParameter1">Name of query parameter 1 in query filter.</param>
		/// <param name="QueryParameterValue1">Value of query parameter 1 in query filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForUser(object User, string Location, bool IgnoreCase,
			string QueryParameter1, string QueryParameterValue1)
		{
			return GetTabIDsForUser(User, Location, IgnoreCase,
				new KeyValuePair<string, string>(QueryParameter1, QueryParameterValue1));
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that a specific user views.
		/// </summary>
		/// <param name="User">User object.</param>
		/// <param name="Location">If restricting the tabs to a given location.</param>
		/// <param name="IgnoreCase">If tag values are case insensitive.</param>
		/// <param name="QueryParameter1">Name of query parameter 1 in query filter.</param>
		/// <param name="QueryParameterValue1">Value of query parameter 1 in query filter.</param>
		/// <param name="QueryParameter2">Name of query parameter 2 in query filter.</param>
		/// <param name="QueryParameterValue2">Value of query parameter 2 in query filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForUser(object User, string Location, bool IgnoreCase,
			string QueryParameter1, string QueryParameterValue1,
			string QueryParameter2, string QueryParameterValue2)
		{
			return GetTabIDsForUser(User, Location, IgnoreCase,
				new KeyValuePair<string, string>(QueryParameter1, QueryParameterValue1),
				new KeyValuePair<string, string>(QueryParameter2, QueryParameterValue2));
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that a specific user views.
		/// </summary>
		/// <param name="User">User object.</param>
		/// <param name="Location">If restricting the tabs to a given location.</param>
		/// <param name="IgnoreCase">If tag values are case insensitive.</param>
		/// <param name="QueryFilter">Query parameter filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForUser(object User, string Location, bool IgnoreCase, params KeyValuePair<string, string>[] QueryFilter)
		{
			User = UserId(User);
			Type T = User.GetType();
			string[] Result;

			lock (tabIdsByUser)
			{
				if (tabIdsByUser.TryGetValue(T, out Dictionary<object, Dictionary<string, (string, string, string)[]>> UserObjects) &&
					UserObjects.TryGetValue(User, out Dictionary<string, (string, string, string)[]> TabIDs))
				{
					Result = ProcessQueryFilterLocked(TabIDs, QueryFilter, IgnoreCase);
				}
				else
					return new string[0];
			}

			if (string.IsNullOrEmpty(Location))
				return Result;

			List<string> Result2 = new List<string>();

			lock (tabIdsByLocation)
			{
				if (tabIdsByLocation.TryGetValue(Location, out Dictionary<string, (string, string, string)[]> TabIDs))
				{
					foreach (string TabID in Result)
					{
						if (TabIDs.ContainsKey(TabID))
							Result2.Add(TabID);
					}
				}
			}

			return Result2.ToArray();
		}

		private static string[] ProcessQueryFilterLocked(Dictionary<string, (string, string, string)[]> TabIDs,
			KeyValuePair<string, string>[] QueryFilter, bool IgnoreCase)
		{
			string[] Result;

			if (QueryFilter is null || QueryFilter.Length == 0)
			{
				Result = new string[TabIDs.Count];
				TabIDs.Keys.CopyTo(Result, 0);
			}
			else
			{
				List<string> Match = new List<string>();
				bool Found;
				bool IsMatch;

				foreach (KeyValuePair<string, (string, string, string)[]> P in TabIDs)
				{
					IsMatch = true;

					foreach (KeyValuePair<string, string> Q in QueryFilter)
					{
						if (Q.Value is null)
						{
							Found = true;

							if (!(P.Value is null))
							{
								foreach ((string, string, string) Q2 in P.Value)
								{
									if (Q2.Item1 == Q.Key)
									{
										Found = false;
										break;
									}
								}
							}

							if (!Found)
							{
								IsMatch = false;
								break;
							}
						}
						else
						{
							Found = false;

							if (!(P.Value is null))
							{
								foreach ((string, string, string) Q2 in P.Value)
								{
									if (Q2.Item1 == Q.Key &&
										(string.Compare(Q2.Item2, Q.Value, IgnoreCase) == 0 ||
										string.Compare(Q2.Item3, Q.Value, IgnoreCase) == 0))
									{
										Found = true;
										break;
									}
								}
							}

							if (!Found)
							{
								IsMatch = false;
								break;
							}
						}
					}

					if (IsMatch)
						Match.Add(P.Key);
				}

				Result = Match.ToArray();
			}

			return Result;
		}

		/// <summary>
		/// Gets the Tab IDs of all tabs that display a set of resources.
		/// </summary>
		/// <param name="Locations">Resources.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForLocations(params string[] Locations)
		{
			switch (Locations.Length)
			{
				case 0:
					return new string[0];

				case 1:
					return GetTabIDsForLocation(Locations[0]);

				default:
					Dictionary<string, bool> Result = new Dictionary<string, bool>();
					Dictionary<string, (string, string, string)[]> TabIDs;

					lock (tabIdsByLocation)
					{
						foreach (string Location in Locations)
						{
							if (tabIdsByLocation.TryGetValue(Location, out TabIDs))
							{
								foreach (string TabID in TabIDs.Keys)
									Result[TabID] = true;
							}
						}
					}

					string[] Result2 = new string[Result.Count];
					Result.Keys.CopyTo(Result2, 0);

					return Result2;
			}
		}

		/// <summary>
		/// Gets all open Tab IDs.
		/// </summary>
		/// <returns>Tab IDs.</returns>
		public static string[] GetTabIDs()
		{
			string[] Result;

			lock (locationByTabID)
			{
				Result = new string[locationByTabID.Count];
				locationByTabID.Keys.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Gets all open Tab IDs for logged in users.
		/// </summary>
		/// <returns>Tab IDs.</returns>
		public static string[] GetTabIDsForUsers()
		{
			Dictionary<string, bool> Result = new Dictionary<string, bool>();

			lock (tabIdsByUser)
			{
				foreach (Dictionary<object, Dictionary<string, (string, string, string)[]>> P in tabIdsByUser.Values)
				{
					foreach (Dictionary<string, (string, string, string)[]> P2 in P.Values)
					{
						foreach (string TabID in P2.Keys)
							Result[TabID] = true;
					}
				}
			}

			string[] Result2 = new string[Result.Count];
			Result.Keys.CopyTo(Result2, 0);
			return Result2;
		}

		private class TabQueue : IDisposable
		{
			public string TabID;
			public string SessionID;
			public Variables Session;
			public MultiReadSingleWriteObject SyncObj = new MultiReadSingleWriteObject();
			public LinkedList<string> Queue = new LinkedList<string>();
			public HttpResponse Response = null;
			public WebSocket WebSocket = null;

			public TabQueue(string ID, string SessionID, Variables Session)
			{
				this.TabID = ID;
				this.SessionID = SessionID;
				this.Session = Session;
			}

			public void Dispose()
			{
				this.SyncObj?.Dispose();
				this.SyncObj = null;

				this.Queue?.Clear();
			}
		}

		private class ContentQueue
		{
			public string ContentID;
			public string ContentType;
			public HttpResponse Response = null;
			public byte[] Content = null;

			public ContentQueue(string ID)
			{
				this.ContentID = ID;
			}
		}

		/// <summary>
		/// Puses an event to a set of Tabs, given their Tab IDs.
		/// </summary>
		/// <param name="TabIDs">Tab IDs.</param>
		/// <param name="Type">Event Type.</param>
		/// <param name="Data">Event Data.</param>
		/// <returns>Number of tabs event is actually sent to.</returns>
		public static Task<int> PushEvent(string[] TabIDs, string Type, object Data)
		{
			if (Data is string s)
				return PushEvent(TabIDs, Type, s, false, null, null);
			else
			{
				s = JSON.Encode(Data, false);
				return PushEvent(TabIDs, Type, s, true, null, null);
			}
		}

		/// <summary>
		/// Puses an event to a set of Tabs, given their Tab IDs.
		/// </summary>
		/// <param name="TabIDs">Tab IDs.</param>
		/// <param name="Type">Event Type.</param>
		/// <param name="Data">Event Data (as plain text).</param>
		/// <returns>Number of tabs event is actually sent to.</returns>
		public static Task<int> PushEvent(string[] TabIDs, string Type, string Data)
		{
			return PushEvent(TabIDs, Type, Data, false, null, null);
		}

		/// <summary>
		/// Puses an event to a set of Tabs, given their Tab IDs.
		/// </summary>
		/// <param name="TabIDs">Tab IDs.</param>
		/// <param name="Type">Event Type.</param>
		/// <param name="Data">Event Data (as plain text, or as JSON).</param>
		/// <param name="DataIsJson">If <paramref name="Data"/> is JSON or plain text.</param>
		/// <returns>Number of tabs event is actually sent to.</returns>
		public static Task<int> PushEvent(string[] TabIDs, string Type, string Data, bool DataIsJson)
		{
			return PushEvent(TabIDs, Type, Data, DataIsJson, null, null);
		}

		/// <summary>
		/// Puses an event to a set of Tabs, given their Tab IDs.
		/// </summary>
		/// <param name="TabIDs">Tab IDs.</param>
		/// <param name="Type">Event Type.</param>
		/// <param name="Data">Event Data (as plain text, or as JSON).</param>
		/// <param name="DataIsJson">If <paramref name="Data"/> is JSON or plain text.</param>
		/// <param name="UserVariable">Optional user variable. If provided, event is only pushed to clients with a session contining a variable 
		/// named <paramref name="UserVariable"/> having a value derived from <see cref="IUser"/>.</param>
		/// <param name="Privileges">Optional privileges. If provided, event is only pushed to clients with a user variable having
		/// the following set of privileges.</param>
		/// <returns>Number of tabs event is actually sent to.</returns>
		public static async Task<int> PushEvent(string[] TabIDs, string Type, string Data, bool DataIsJson, string UserVariable, params string[] Privileges)
		{
			int Result = 0;

			try
			{
				StringBuilder Json = new StringBuilder();

				Json.Append("{\"type\":\"");
				Json.Append(Type);
				Json.Append("\",\"data\":");

				if (DataIsJson)
					Json.Append(Data);
				else
				{
					Json.Append('"');
					Json.Append(JSON.Encode(Data));
					Json.Append('"');
				}

				Json.Append('}');

				string s = Json.ToString();

				if (TabIDs is null)
					TabIDs = eventsByTabID.GetKeys();

				foreach (string TabID in TabIDs)
				{
					if (!(TabID is null) && eventsByTabID.TryGetValue(TabID, out TabQueue Queue))
					{
						if (!string.IsNullOrEmpty(UserVariable))
						{
							if (!Queue.Session.TryGetVariable(UserVariable, out Variable v) ||
								!(v.ValueObject is IUser User))
							{
								continue;
							}

							if (!(Privileges is null))
							{
								bool HasPrivileges = true;

								foreach (string Privilege in Privileges)
								{
									if (!User.HasPrivilege(Privilege))
									{
										HasPrivileges = false;
										break;
									}
								}

								if (!HasPrivileges)
									continue;
							}
						}

						if (await Queue.SyncObj.TryBeginWrite(10000))
						{
							try
							{
								if (!(Queue.WebSocket is null))
									Queue.WebSocket.Send(Json.ToString(), 4096);
								else if (!(Queue.Response is null))
								{
									try
									{
										await Queue.Response.Write("[" + Json.ToString() + "]");
										await Queue.Response.SendResponse();
										Queue.Response.Dispose();
										Queue.Response = null;
									}
									catch (Exception)
									{
										// Ignore
									}
								}
								else
									Queue.Queue.AddLast(s);
							}
							finally
							{
								await Queue.SyncObj.EndWrite();
							}
						}

						timeoutByTabID.Remove(TabID);
						Result++;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return Result;
		}

	}
}
