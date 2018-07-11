using System.Collections.Generic;

namespace BeatThat.ItemManagers
{
    public interface ItemManager : IHasItems, IHasRootItems {}

	public interface ItemManager<ItemType> : ItemManager
	{
		int GetAll(ICollection<ItemType> result);

		int AddItems(ICollection<ItemType> items);

		bool GetLastItem<T>(out T item) where T : class;
	}


}
