using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Orders modules in dependency order.
	/// </summary>
	public class DependencyOrder : IComparer<IModule>
	{
		/// <summary>
		/// Compares two modules.
		/// </summary>
		/// <param name="x">Module 1</param>
		/// <param name="y">Module 2</param>
		/// <returns>Signed comparison result.</returns>
		public int Compare(IModule x, IModule y)
		{
			Type TypeX = x.GetType();
			Type TypeY = y.GetType();
			IEnumerable<ModuleDependencyAttribute> AttrsX = TypeX.GetTypeInfo().GetCustomAttributes<ModuleDependencyAttribute>();
			IEnumerable<ModuleDependencyAttribute> AttrsY = TypeY.GetTypeInfo().GetCustomAttributes<ModuleDependencyAttribute>();
			bool XDependsOnY = false;
			bool YDependsOnX = false;

			if (!(AttrsX is null))
			{
				foreach (ModuleDependencyAttribute AttrX in AttrsX)
				{
					if (AttrX.DependsOn(TypeY))
					{
						XDependsOnY = true;
						break;
					}
				}
			}

			if (!(AttrsY is null))
			{
				foreach (ModuleDependencyAttribute AttrY in AttrsY)
				{
					if (AttrY.DependsOn(TypeX))
					{
						YDependsOnX = true;
						break;
					}
				}
			}

			int i = YDependsOnX.CompareTo(XDependsOnY);
			if (i != 0)
				return i;
			else
				return TypeX.FullName.CompareTo(TypeY.FullName);
		}
	}
}
