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
			IEnumerable<ModuleDependencyAttribute> AttrsX = TypeX.GetCustomAttributes<ModuleDependencyAttribute>();
			IEnumerable<ModuleDependencyAttribute> AttrsY = TypeY.GetCustomAttributes<ModuleDependencyAttribute>();
			bool XHasDependencies = false;
			bool YHasDependencies = false;
			bool XDependsOnY = false;
			bool YDependsOnX = false;

			foreach (ModuleDependencyAttribute AttrX in AttrsX)
			{
				XHasDependencies = true;

				if (AttrX.DependsOn(TypeY))
				{
					XDependsOnY = true;
					break;
				}
			}

			foreach (ModuleDependencyAttribute AttrY in AttrsY)
			{
				YHasDependencies = true;

				if (AttrY.DependsOn(TypeX))
				{
					YDependsOnX = true;
					break;
				}
			}

			if (XHasDependencies)
			{
				if (!YHasDependencies)
					return 1;
			}
			else if (YHasDependencies)
				return -1;

			int i = XDependsOnY.CompareTo(YDependsOnX);
			if (i != 0)
				return i;
			else
				return TypeX.FullName.CompareTo(TypeY.FullName);
		}
	}
}
