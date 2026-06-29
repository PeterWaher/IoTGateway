using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Events;
using Waher.Runtime.Collections;

namespace Waher.Networking.HTTP.OAuth.MetaData
{
	/// <summary>
	/// Defines scopes supported by an OAUTH web service.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class OAuthScopesSupportedAttribute : OAuthMetaDataAttribute
	{
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
		/// Adds available meta-data to a dictionary of meta-data.
		/// </summary>
		/// <param name="Resource">Resource to add meta-data for.</param>
		/// <param name="MetaData">Dictionary to add meta-data to.</param>
		public override void AddMetaData(HttpResource Resource,
			Dictionary<string, object> MetaData)
		{
			string[] Scopes;
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

							Obj = MI.Invoke(Resource, Array.Empty<object>());
						}
						else
							Obj = FI.GetValue(Resource);
					}
					else
						Obj = PI.GetValue(Resource);

					if (Obj is IEnumerable<string> DynamicScopes)
						ScopesList.AddRange(DynamicScopes);
					else if (Obj is string DynamicScope)
						ScopesList.Add(DynamicScope);
					else
						Log.Warning("Dynamic scopes returned an object that was not of expected type: " + T.FullName, Resource.ResourceName);
				}

				Scopes = ScopesList.ToArray();
			}
			else
				Scopes = this.ScopesSupported;

			if (MetaData.TryGetValue("scopes_supported", out Obj) &&
				Obj is string[] Scopes2)
			{
				MetaData["scopes_supported"] = Scopes2.Join(Scopes);
			}
			else
				MetaData["scopes_supported"] = Scopes;
		}
	}
}
