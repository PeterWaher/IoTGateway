using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Networking.HTTP.WebSockets;
using Waher.Runtime.Cache;
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
	/// </summary>
	public class ClientEvents : HttpAsynchronousResource, IHttpPostMethod
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
		public bool AllowsPOST
		{
			get { return true; }
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void POST(HttpRequest Request, HttpResponse Response)
		{
			if (!Request.HasData || Request.Session is null)
				throw new BadRequestException();

			// TODO: Check User authenticated

			object Obj = Request.DecodeData();
			string TabID = Request.Header["X-TabID"];

			if (!(Obj is string Location) || string.IsNullOrEmpty(TabID))
				throw new BadRequestException();

			Uri Uri = new Uri(Location);
			string Resource = Uri.LocalPath;
			List<(string, string, string)> Query = null;
			string s;

			if (!string.IsNullOrEmpty(Uri.Query))
			{
				Query = new List<(string, string, string)>();
				int i;

				s = Uri.Query;
				if (s.StartsWith("?"))
					s = s.Substring(1);

				foreach (string Part in s.Split('&'))
				{
					i = Part.IndexOf('=');
					if (i < 0)
						Query.Add((Part, string.Empty, string.Empty));
					else
					{
						string s2 = Part.Substring(i + 1);
						Query.Add((Part.Substring(0, i), s2, System.Net.WebUtility.UrlDecode(s2)));
					}
				}
			}

			Response.ContentType = "application/json";

			if (!eventsByTabID.TryGetValue(TabID, out TabQueue Queue))
			{
				HttpFieldCookie Cookie = Request.Header.Cookie;
				string HttpSessionID = Cookie is null ? string.Empty : Cookie["HttpSessionID"];

				Queue = new TabQueue(TabID, HttpSessionID, Request.Session);
				eventsByTabID[TabID] = Queue;
			}

			lock (locationByTabID)
			{
				if (!locationByTabID.TryGetValue(TabID, out s) || s != Resource)
					locationByTabID[TabID] = Resource;
			}

			lock (tabIdsByLocation)
			{
				if (!tabIdsByLocation.TryGetValue(Resource, out Dictionary<string, List<(string, string, string)>> TabIds))
				{
					TabIds = new Dictionary<string, List<(string, string, string)>>();
					tabIdsByLocation[Resource] = TabIds;
				}

				TabIds[TabID] = Query;
			}

			StringBuilder Json = null;

			lock (Queue)
			{
				if (Queue.Queue.First != null)
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

			if (Json != null)
			{
				timeoutByTabID.Remove(TabID);

				Json.Append(']');
				Response.Write(Json.ToString());
				Response.SendResponse();
				Response.Dispose();
			}
			else
				timeoutByTabID[TabID] = Queue;
		}

		internal static void RegisterWebSocket(WebSocket Socket, string Location, string TabID)
		{
			Uri Uri = new Uri(Location);
			string Resource = Uri.LocalPath;
			List<(string, string, string)> Query = null;
			string s;

			if (!string.IsNullOrEmpty(Uri.Query))
			{
				Query = new List<(string, string, string)>();
				int i;

				s = Uri.Query;
				if (s.StartsWith("?"))
					s = s.Substring(1);

				foreach (string Part in s.Split('&'))
				{
					i = Part.IndexOf('=');
					if (i < 0)
						Query.Add((Part, string.Empty, string.Empty));
					else
					{
						string s2 = Part.Substring(i + 1);
						Query.Add((Part.Substring(0, i), s2, System.Net.WebUtility.UrlDecode(s2)));
					}
				}
			}

			if (eventsByTabID.TryGetValue(TabID, out TabQueue Queue))
				Queue.WebSocket = Socket;
			else
			{
				HttpFieldCookie Cookie = Socket.HttpRequest.Header.Cookie;
				string HttpSessionID = Cookie is null ? string.Empty : Cookie["HttpSessionID"];

				Queue = new TabQueue(TabID, HttpSessionID, Socket.HttpRequest.Session)
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
				if (!tabIdsByLocation.TryGetValue(Resource, out Dictionary<string, List<(string, string, string)>> TabIds))
				{
					TabIds = new Dictionary<string, List<(string, string, string)>>();
					tabIdsByLocation[Resource] = TabIds;
				}

				TabIds[TabID] = Query;
			}

			LinkedList<string> ToSend = null;

			lock (Queue)
			{
				if (Queue.Queue.First != null)
				{
					ToSend = new LinkedList<string>();

					foreach (string s2 in Queue.Queue)
						ToSend.AddLast(s2);

					Queue.Queue.Clear();
				}
			}

			if (ToSend != null)
			{
				foreach (string s2 in ToSend)
					Socket.Send(s2, 4096);
			}
		}

		internal static void Ping(string TabID)
		{
			if (eventsByTabID.TryGetValue(TabID, out TabQueue TabQueue))
				Gateway.HttpServer.GetSession(TabQueue.SessionID, false);
		}

		internal static void UnregisterWebSocket(WebSocket Socket, string Location, string TabID)
		{
			Uri Uri = new Uri(Location);
			string Resource = Uri.LocalPath;

			if (eventsByTabID.TryGetValue(TabID, out TabQueue Queue))
			{
				lock (Queue)
				{
					Queue.WebSocket = null;
				}
			}

			lock (locationByTabID)
			{
				if (locationByTabID.TryGetValue(TabID, out string s) && s == Resource)
					locationByTabID.Remove(TabID);
				else
					return;
			}

			lock (tabIdsByLocation)
			{
				if (tabIdsByLocation.TryGetValue(Resource, out Dictionary<string, List<(string, string, string)>> TabIds))
				{
					if (TabIds.Remove(TabID) && TabIds.Count == 0)
						tabIdsByLocation.Remove(Resource);
				}
			}
		}


		private static Cache<string, TabQueue> eventsByTabID = GetQueueCache();
		private static Cache<string, TabQueue> timeoutByTabID = GetTimeoutCache();
		private static Dictionary<string, string> locationByTabID = new Dictionary<string, string>();
		private static Dictionary<string, Dictionary<string, List<(string, string, string)>>> tabIdsByLocation =
			new Dictionary<string, Dictionary<string, List<(string, string, string)>>>(StringComparer.OrdinalIgnoreCase);

		private static Cache<string, TabQueue> GetTimeoutCache()
		{
			Cache<string, TabQueue> Result = new Cache<string, TabQueue>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromSeconds(20));

			Result.Removed += TimeoutCacheItem_Removed;

			return Result;
		}

		private static void TimeoutCacheItem_Removed(object Sender, CacheItemEventArgs<string, TabQueue> e)
		{
			if (e.Reason == RemovedReason.NotUsed)
			{
				HttpResponse Response = e.Value.Response;

				if (Response != null)
				{
					try
					{
						e.Value.Response = null;

						Response.Write("[{\"type\":\"NOP\"}]");
						Response.SendResponse();
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

		private static Cache<string, TabQueue> GetQueueCache()
		{
			Cache<string, TabQueue> Result = new Cache<string, TabQueue>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromSeconds(30));

			Result.Removed += QueueCacheItem_Removed;

			return Result;
		}

		private static void QueueCacheItem_Removed(object Sender, CacheItemEventArgs<string, TabQueue> e)
		{
			TabQueue Queue = e.Value;
			string TabID = Queue.TabID;
			string Location;

			lock (Queue)
			{
				Queue.Queue.Clear();
			}

			lock (locationByTabID)
			{
				if (locationByTabID.TryGetValue(TabID, out Location))
					locationByTabID.Remove(TabID);
				else
					Location = null;
			}

			if (Location != null)
			{
				lock (tabIdsByLocation)
				{
					if (tabIdsByLocation.TryGetValue(Location, out Dictionary<string, List<(string, string, string)>> TabIDs))
					{
						if (TabIDs.Remove(TabID) && TabIDs.Count == 0)
							tabIdsByLocation.Remove(Location);
					}
				}
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
		/// <param name="QueryFilter">Query parameter filter.</param>
		/// <returns>Tab IDs</returns>
		public static string[] GetTabIDsForLocation(string Location, bool IgnoreCase,
			params KeyValuePair<string, string>[] QueryFilter)
		{
			string[] Result;

			lock (tabIdsByLocation)
			{
				if (tabIdsByLocation.TryGetValue(Location, out Dictionary<string, List<(string, string, string)>> TabIDs))
				{
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

						foreach (KeyValuePair<string, List<(string, string, string)>> P in TabIDs)
						{
							IsMatch = true;

							foreach (KeyValuePair<string, string> Q in QueryFilter)
							{
								if (Q.Value is null)
								{
									Found = true;

									if (P.Value != null)
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

									if (P.Value != null)
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
				}
				else
					Result = new string[0];
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
					Dictionary<string, List<(string, string, string)>> TabIDs;

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

		private class TabQueue
		{
			public string TabID;
			public string SessionID;
			public Variables Session;
			public LinkedList<string> Queue = new LinkedList<string>();
			public HttpResponse Response = null;
			public WebSocket WebSocket = null;

			public TabQueue(string ID, string SessionID, Variables Session)
			{
				this.TabID = ID;
				this.SessionID = SessionID;
				this.Session = Session;
			}
		}

		/// <summary>
		/// Puses an event to a set of Tabs, given their Tab IDs.
		/// </summary>
		/// <param name="TabIDs">Tab IDs.</param>
		/// <param name="Type">Event Type.</param>
		/// <param name="Data">Event Data (as plain text).</param>
		public static void PushEvent(string[] TabIDs, string Type, string Data)
		{
			PushEvent(TabIDs, Type, Data, false, null, null);
		}

		/// <summary>
		/// Puses an event to a set of Tabs, given their Tab IDs.
		/// </summary>
		/// <param name="TabIDs">Tab IDs.</param>
		/// <param name="Type">Event Type.</param>
		/// <param name="Data">Event Data (as plain text, or as JSON).</param>
		/// <param name="DataIsJson">If <paramref name="Data"/> is JSON or plain text.</param>
		public static void PushEvent(string[] TabIDs, string Type, string Data, bool DataIsJson)
		{
			PushEvent(TabIDs, Type, Data, DataIsJson, null, null);
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
		public static void PushEvent(string[] TabIDs, string Type, string Data, bool DataIsJson, string UserVariable, params string[] Privileges)
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
				if (TabID != null && eventsByTabID.TryGetValue(TabID, out TabQueue Queue))
				{
					if (!string.IsNullOrEmpty(UserVariable))
					{
						if (!Queue.Session.TryGetVariable(UserVariable, out Variable v) ||
							!(v.ValueObject is IUser User))
						{
							continue;
						}

						if (Privileges != null)
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

					lock (Queue)
					{
						if (Queue.WebSocket != null)
							Queue.WebSocket.Send(Json.ToString(), 4096);
						else if (Queue.Response != null)
						{
							try
							{
								Queue.Response.Write("[" + Json.ToString() + "]");
								Queue.Response.SendResponse();
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

					timeoutByTabID.Remove(TabID);
				}
			}
		}


	}
}
