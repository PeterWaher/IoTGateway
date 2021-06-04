using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Waher.IoTGateway.Svc
{
	/// <summary>
	/// Static interface for accessing performance counters.
	/// </summary>
	public static class PerformanceCounters
	{
		private static readonly string[] categoryNames = GetCategoryNames();
		private static readonly Dictionary<string, CategoryRec> categories = GetCategories();

		/// <summary>
		/// Gets available categories
		/// </summary>
		/// <returns>Dictionary of cateogry records</returns>
		private static Dictionary<string, CategoryRec> GetCategories()
		{
			Dictionary<string, CategoryRec> Result = new Dictionary<string, CategoryRec>();

			foreach (PerformanceCounterCategory Category in PerformanceCounterCategory.GetCategories())
			{
				Result[Category.CategoryName] = new CategoryRec()
				{
					Category = Category,
					Counters = null
				};
			}

			return Result;
		}

		/// <summary>
		/// Gets available category names
		/// </summary>
		/// <returns>Array of category names</returns>
		private static string[] GetCategoryNames()
		{
			PerformanceCounterCategory[] Categories = PerformanceCounterCategory.GetCategories();
			int i, c = Categories.Length;
			string[] Names = new string[c];

			for (i = 0; i < c; i++)
				Names[i] = Categories[i].CategoryName;

			return Names;
		}

		private class CategoryRec
		{
			public PerformanceCounterCategory Category;
			public PerformanceCounter[] Counters;
			public string[] InstanceNames;
			public Dictionary<string, InstanceRec> Instances;
			public Dictionary<string, PerformanceCounter> CountersByName;
		}

		private class InstanceRec
		{
			public PerformanceCounter[] Counters;
			public Dictionary<string, PerformanceCounter> CountersByName;
		}

		/// <summary>
		/// Available categories
		/// </summary>
		public static PerformanceCounterCategory[] Categories => PerformanceCounterCategory.GetCategories();

		/// <summary>
		/// Available category names
		/// </summary>
		public static string[] CategoryNames => categoryNames;

		/// <summary>
		/// Gets a performance category, given its name
		/// </summary>
		/// <param name="CategoryName">Category name</param>
		/// <returns>Performance category, if found, null otherwise.</returns>
		public static PerformanceCounterCategory GetCategory(string CategoryName)
		{
			if (categories.TryGetValue(CategoryName, out CategoryRec Rec))
				return Rec.Category;
			else
				return null;
		}

		/// <summary>
		/// Gets available performance counters, given their category name, and that the category does not consist of instances.
		/// </summary>
		/// <param name="CategoryName">Category name</param>
		/// <returns>Available performance counters</returns>
		public static PerformanceCounter[] GetCounters(string CategoryName)
		{
			if (!categories.TryGetValue(CategoryName, out CategoryRec Rec))
				return new PerformanceCounter[0];

			if (Rec.Counters is null)
				Rec.Counters = Rec.Category.GetCounters();

			return Rec.Counters;
		}

		/// <summary>
		/// Gets available performance counters names, given their category name, and that the category does not consist of instances.
		/// </summary>
		/// <param name="CategoryName">Category name</param>
		/// <returns>Available performance counters</returns>
		public static string[] GetCounterNames(string CategoryName)
		{
			PerformanceCounter[] Counters = GetCounters(CategoryName);
			int i, c = Counters.Length;
			string[] Names = new string[c];

			for (i = 0; i < c; i++)
				Names[i] = Counters[i].CounterName;

			return Names;
		}

		/// <summary>
		/// Gets a performance counter, given its category name and counter name, and that the category does not consist of instances.
		/// </summary>
		/// <param name="CategoryName">Category name</param>
		/// <param name="CounterName">Category name</param>
		/// <returns>Performance counter, if found, null otherwise.</returns>
		public static PerformanceCounter GetCounter(string CategoryName, string CounterName)
		{
			if (!categories.TryGetValue(CategoryName, out CategoryRec Rec))
				return null;

			if (Rec.CountersByName is null)
			{
				if (Rec.Counters is null)
					Rec.Counters = Rec.Category.GetCounters();

				Dictionary<string, PerformanceCounter> ByName = new Dictionary<string, PerformanceCounter>();

				foreach (PerformanceCounter Counter in Rec.Counters)
					ByName[Counter.CounterName] = Counter;

				Rec.CountersByName = ByName;
			}

			if (Rec.CountersByName.TryGetValue(CounterName, out PerformanceCounter Result))
				return Result;
			else
				return null;
		}

		/// <summary>
		/// Gets available instance name, given a performance category name.
		/// </summary>
		/// <param name="CategoryName">Category name</param>
		/// <returns>Available instance names</returns>
		public static string[] GetInstanceNames(string CategoryName)
		{
			if (!categories.TryGetValue(CategoryName, out CategoryRec Rec))
				return new string[0];

			if (Rec.InstanceNames is null)
				Rec.InstanceNames = Rec.Category.GetInstanceNames();

			return Rec.InstanceNames;
		}

		/// <summary>
		/// Gets available performance counters, given their category and instance names.
		/// </summary>
		/// <param name="CategoryName">Category name</param>
		/// <param name="InstanceName">Instance name</param>
		/// <returns>Available performance counters</returns>
		public static PerformanceCounter[] GetCounters(string CategoryName, string InstanceName)
		{
			if (!categories.TryGetValue(CategoryName, out CategoryRec CategoryRec))
				return new PerformanceCounter[0];

			if (CategoryRec.Instances is null)
			{
				Dictionary<string, InstanceRec> Instances = new Dictionary<string, InstanceRec>();

				if (CategoryRec.InstanceNames is null)
					CategoryRec.InstanceNames = CategoryRec.Category.GetInstanceNames();

				foreach (string Name in CategoryRec.InstanceNames)
				{
					Instances[Name] = new InstanceRec()
					{
						Counters = CategoryRec.Category.GetCounters(Name)
					};
				}

				CategoryRec.Instances = Instances;
			}

			if (!CategoryRec.Instances.TryGetValue(InstanceName, out InstanceRec InstanceRec))
				return new PerformanceCounter[0];

			return InstanceRec.Counters;
		}

		/// <summary>
		/// Gets available performance counter names, given their category and instance names.
		/// </summary>
		/// <param name="CategoryName">Category name</param>
		/// <returns>Available performance counters</returns>
		public static string[] GetCounterNames(string CategoryName, string InstanceName)
		{
			PerformanceCounter[] Counters = GetCounters(CategoryName, InstanceName);
			int i, c = Counters.Length;
			string[] Names = new string[c];

			for (i = 0; i < c; i++)
				Names[i] = Counters[i].CounterName;

			return Names;
		}

		/// <summary>
		/// Gets a performance counter, given its category, instance and counter names.
		/// </summary>
		/// <param name="CategoryName">Category name</param>
		/// <param name="InstanceName">Instance name</param>
		/// <param name="CounterName">Counter name</param>
		/// <returns>Performance counter, if found, null otherwise.</returns>
		public static PerformanceCounter GetCounter(string CategoryName, string InstanceName, string CounterName)
		{
			if (!categories.TryGetValue(CategoryName, out CategoryRec CategoryRec))
				return null;

			if (CategoryRec.Instances is null)
			{
				Dictionary<string, InstanceRec> Instances = new Dictionary<string, InstanceRec>();

				if (CategoryRec.InstanceNames is null)
					CategoryRec.InstanceNames = CategoryRec.Category.GetInstanceNames();

				foreach (string Name in CategoryRec.InstanceNames)
				{
					Instances[Name] = new InstanceRec()
					{
						Counters = CategoryRec.Category.GetCounters(Name)
					};
				}

				CategoryRec.Instances = Instances;
			}

			if (!CategoryRec.Instances.TryGetValue(InstanceName, out InstanceRec InstanceRec))
				return null;

			if (InstanceRec.CountersByName is null)
			{
				if (InstanceRec.Counters is null)
					InstanceRec.Counters = CategoryRec.Category.GetCounters(InstanceName);

				Dictionary<string, PerformanceCounter> ByName = new Dictionary<string, PerformanceCounter>();

				foreach (PerformanceCounter Counter in InstanceRec.Counters)
					ByName[Counter.CounterName] = Counter;

				InstanceRec.CountersByName = ByName;
			}

			if (InstanceRec.CountersByName.TryGetValue(CounterName, out PerformanceCounter Result))
				return Result;
			else
				return null;
		}

	}
}
