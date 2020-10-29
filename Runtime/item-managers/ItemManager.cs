using System.Collections.Generic;

namespace BeatThat.ItemManagers
{
    public interface ItemManager : IHasItems, IHasRootItems {}

	public interface ItemManager<ItemType> : ItemManager
	{
		int AddItems(ICollection<ItemType> items);
		int GetAll(ICollection<ItemType> result);
		IEnumerable<ItemType> GetAll();
		bool GetLastItem<T>(out T item) where T : class;
	}


}
