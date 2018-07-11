using System.Collections.Generic;
using BeatThat.Pools;
using BeatThat.Properties;
using UnityEngine;

namespace BeatThat.ItemManagers
{
    /// <summary>
    /// Interface to expose a managed collection of items.
    /// </summary>
    public interface IHasItems 
	{
		int count { get; }

		int GetItems<T>(ICollection<T> items) where T : class;
	}

	public static class IHasItemsExtensions
	{
		public static void SetBoolAllItems<T>(this IHasItems hasItems, bool value, 
			MissingComponentOptions opts = MissingComponentOptions.AddAndWarn) where T : Component, IHasBool
		{
			using(var items = ListPool<Transform>.Get()) {
				hasItems.GetItems<Transform>(items);
				foreach(var item in items) {
					item.SetBool<T>(value, opts);
				}
			}
		}
	}
}

