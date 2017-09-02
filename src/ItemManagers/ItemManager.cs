using System.Collections.Generic;

namespace BeatThat.UI
{
	public interface ItemManager<ItemType> : IHasItems, IHasRootItems
	{
		void GetAll(ICollection<ItemType> result);

		int AddItems(ICollection<ItemType> items);

		bool GetLastItem<T>(out T item) where T : class;
	}


}