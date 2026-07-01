using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Collections;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.OAuth.MetaData
{
	/// <summary>
	/// Defines scopes supported by an OAUTH web service.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class OAuthScopesSupportedAttribute : OAuthMetaDataAttribute
	{
		private static readonly SortedDictionary<string, bool> scopesAvailable = new SortedDictionary<string, bool>();

		/// <summary>
		/// Defines scopes supported by an OAUTH web service.
		/// </summary>
		/// <param name="ScopesSupported">Scopes supported.</param>
		public OAuthScopesSupportedAttribute(params string[] ScopesSupported)
			: this(false, ScopesSupported)
		{
		}

		/// <summary>
		/// Defines scopes supported by an OAUTH web service.
		/// </summary>
		/// <param name="DynamicScopes">If the scopes point to a field, property or method, 
		/// that returns the scopes that are supported.</param>
		/// <param name="ScopesSupported">Scopes supported.</param>
		public OAuthScopesSupportedAttribute(bool DynamicScopes, params string[] ScopesSupported)
		{
			this.DynamicScopes = DynamicScopes;
			this.ScopesSupported = ScopesSupported;
		}

		/// <summary>
		/// If the scopes point to a field, property or method, that returns the scopes 
		/// that are supported.
		/// </summary>
		public bool DynamicScopes { get; }

		/// <summary>
		/// Scopes supported.
		/// </summary>
		public string[] ScopesSupported { get; }

		/// <summary>
		/// Gets available scopes for a given resource.
		/// </summary>
		/// <param name="Resource">Resource to get scopes for.</param>
		/// <returns>Array of available scopes.</returns>
		public async Task<string[]> GetScopes(HttpResource Resource)
		{
			object Obj;

			if (this.DynamicScopes)
			{
				ChunkedList<string> ScopesList = new ChunkedList<string>();
				Type T = Resource.GetType();

				foreach (string Scope in this.ScopesSupported)
				{
					PropertyInfo PI = T.GetProperty(Scope, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
					if (PI is null)
					{
						FieldInfo FI = T.GetField(Scope, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

						if (FI is null)
						{
							MethodInfo MI = T.GetMethod(Scope, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, new Type[0], null);
							if (MI is null)
								continue;

							Obj = await ScriptNode.WaitPossibleTask(MI.Invoke(Resource, Array.Empty<object>()));
						}
						else
							Obj = await ScriptNode.WaitPossibleTask(FI.GetValue(Resource));
					}
					else
						Obj = await ScriptNode.WaitPossibleTask(PI.GetValue(Resource));

					if (Obj is IEnumerable<string> DynamicScopes)
						ScopesList.AddRange(DynamicScopes);
					else if (Obj is string DynamicScope)
						ScopesList.Add(DynamicScope);
					else
						Log.Warning("Dynamic scopes returned an object that was not of expected type: " + T.FullName, Resource.ResourceName);
				}

				return ScopesList.ToArray();
			}
			else
				return this.ScopesSupported;
		}

		/// <summary>
		/// Registers any meta-data used that requires registration.
		/// </summary>
		/// <param name="Resource">Resource containing meta-data to be registered.</param>
		public override async Task RegisterMetaData(HttpResource Resource)
		{
			string[] Scopes = await this.GetScopes(Resource);

			lock (scopesAvailable)
			{
				foreach (string Scope in Scopes)
					scopesAvailable[Scope] = true;
			}
		}

		/// <summary>
		/// Registered scopes.
		/// </summary>
		public static string[] RegisteredScopes
		{
			get
			{
				lock (scopesAvailable)
				{
					int c = scopesAvailable.Count;
					string[] Result = new string[c];
					
					scopesAvailable.Keys.CopyTo(Result, 0);
				
					return Result;
				}
			}
		}

		/// <summary>
		/// Adds available meta-data to a dictionary of meta-data.
		/// </summary>
		/// <param name="Resource">Resource to add meta-data for.</param>
		/// <param name="MetaData">Dictionary to add meta-data to.</param>
		public override async Task AddMetaData(HttpResource Resource,
			Dictionary<string, object> MetaData)
		{
			string[] Scopes = await this.GetScopes(Resource);

			if (MetaData.TryGetValue("scopes_supported", out object Obj) &&
				Obj is string[] Scopes2)
			{
				MetaData["scopes_supported"] = Scopes2.Join(Scopes);
			}
			else
				MetaData["scopes_supported"] = Scopes;
		}
	}
}
