using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Groups
{
	/// <summary>
	/// Enumerator that groups items into groups, and returns aggregated elements.
	/// </summary>
	public class GroupEnumerator : IResultSetEnumerator
	{
		private readonly ScriptNode[] groupBy;
		private readonly ScriptNode[] groupNames;
		private readonly Variables variables;
		private readonly IResultSetEnumerator e;
		private bool processLast = false;
		private GroupObject current = null;
		private ObjectProperties objectVariables = null;

		/// <summary>
		/// Enumerator that groups items into groups, and returns aggregated elements.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Variables">Current set of variables</param>
		/// <param name="GroupBy">Group on these fields</param>
		/// <param name="GroupNames">Names given to grouped fields</param>
		public GroupEnumerator(IResultSetEnumerator ItemEnumerator, Variables Variables, ScriptNode[] GroupBy, ScriptNode[] GroupNames)
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
			return this.MoveNextAsync().Result;
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public async Task<bool> MoveNextAsync()
		{
			List<object> Objects = null;
			IElement E;
			object[] Last = null;
			int i, c = this.groupBy.Length;
			object o1, o2;

			while (this.processLast || await e.MoveNextAsync())
			{
				this.processLast = false;

				if (this.objectVariables is null)
					this.objectVariables = new ObjectProperties(e.Current, this.variables);
				else
					this.objectVariables.Object = e.Current;

				if (Last is null)
				{
					Last = new object[c];

					for (i = 0; i < c; i++)
					{
						E = this.groupBy[i].Evaluate(this.objectVariables);
						Last[i] = E.AssociatedObjectValue;
					}
				}
				else
				{
					for (i = 0; i < c; i++)
					{
						E = this.groupBy[i].Evaluate(this.objectVariables);

						o1 = Last[i];
						o2 = E.AssociatedObjectValue;

						if (o1 is null ^ o2 is null)
							break;

						if (!(o1 is null) && !o1.Equals(o2))
							break;
					}

					if (i < c)
					{
						this.processLast = true;
						break;
					}
				}

				if (Objects is null)
					Objects = new List<object>();

				Objects.Add(e.Current);
			}

			if (Objects is null)
				return false;

			this.current = new GroupObject(Objects.ToArray(), Last, this.groupNames, this.variables);

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
