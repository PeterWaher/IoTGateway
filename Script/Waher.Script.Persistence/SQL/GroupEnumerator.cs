using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL
{
	/// <summary>
	/// Enumerator that groups items into groups, and returns aggregated elements.
	/// </summary>
	public class GroupEnumerator : IEnumerator
	{
		private readonly ScriptNode[] groupBy;
		private readonly ScriptNode[] groupNames;
		private readonly Variables variables;
		private readonly IEnumerator e;
		private bool processLast = false;
		private Type lastType = null;
		private IEnumerable<PropertyInfo> properties = null;
		private IEnumerable<FieldInfo> fields = null;
		private object current = null;

		/// <summary>
		/// Enumerator that groups items into groups, and returns aggregated elements.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Variables">Current set of variables</param>
		/// <param name="GroupBy">Group on these fields</param>
		/// <param name="GroupNames">Names given to grouped fields</param>
		public GroupEnumerator(IEnumerator ItemEnumerator, Variables Variables, ScriptNode[] GroupBy, ScriptNode[] GroupNames)
		{
			this.e = ItemEnumerator;
			this.variables = Variables;
			this.groupBy = GroupBy;
			this.groupNames = GroupNames;
		}

		/// <summary>
		/// <see cref="IEnumerator.Current"/>
		/// </summary>
		public object Current => this.current;

		/// <summary>
		/// <see cref="IEnumerator.MoveNext"/>
		/// </summary>
		public bool MoveNext()
		{
			Dictionary<string, List<object>> Aggregated = null;
			ObjectProperties Variables = null;
			IElement E;
			object[] Last = null;
			int i, c = this.groupBy.Length;
			object o1, o2;

			while (this.processLast || e.MoveNext())
			{
				this.processLast = false;

				Variables = new ObjectProperties(e.Current, this.variables);

				if (Last is null)
				{
					Last = new object[c];

					for (i = 0; i < c; i++)
					{
						E = this.groupBy[i].Evaluate(Variables);
						Last[i] = E.AssociatedObjectValue;
					}
				}
				else
				{
					for (i = 0; i < c; i++)
					{
						E = this.groupBy[i].Evaluate(Variables);

						o1 = Last[i];
						o2 = E.AssociatedObjectValue;

						if (o1 is null ^ o2 is null)
							break;

						if (o1 != null && !o1.Equals(o2))
							break;
					}

					if (i < c)
					{
						this.processLast = true;
						break;
					}
				}

				o1 = e.Current;

				Type T = o1.GetType();
				if (T != this.lastType)
				{
					this.lastType = T;
					this.properties = T.GetRuntimeProperties();
					this.fields = T.GetRuntimeFields();
				}

				if (Aggregated is null)
					Aggregated = new Dictionary<string, List<object>>();

				foreach (PropertyInfo PI in this.properties)
				{
					if (!PI.CanRead || !PI.CanWrite)
						continue;

					if (!Aggregated.TryGetValue(PI.Name, out List<object> List))
					{
						List = new List<object>();
						Aggregated[PI.Name] = List;
					}

					List.Add(PI.GetValue(o1));
				}
			}

			if (Aggregated is null)
				return false;

			Dictionary<string, object> Result = new Dictionary<string, object>();
			bool First = true;

			foreach (KeyValuePair<string, List<object>> Rec in Aggregated)
			{
				object[] A = Rec.Value.ToArray();
				Result[Rec.Key] = A;

				if (First)
				{
					First = false;
					Result[" First "] = A;
				}
			}

			if (this.groupNames != null)
			{
				for (i = 0; i < c; i++)
				{
					ScriptNode Node = this.groupNames[i];
					if (Node is null)
						continue;

					if (Node is VariableReference Ref)
						Result[Ref.VariableName] = Last[i];
					else
					{
						E = this.groupNames[i]?.Evaluate(Variables);
						if (E != null && E is StringValue S)
							Result[S.Value] = Last[i];
					}
				}
			}

			this.current = Result;

			return true;
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset"/>
		/// </summary>
		public void Reset()
		{
			this.e.Reset();
			this.processLast = false;
			this.current = null;
		}
	}
}
