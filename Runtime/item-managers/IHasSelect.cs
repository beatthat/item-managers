using BeatThat.Properties;

namespace BeatThat.ItemManagers
{
    /// <summary>
    /// useful to have a handle on something that has both a list of items and a selected item
    /// </summary>
    public interface IHasSelect : IHasItems, IHasSelectedItem
	{
		void Select(int index, PropertyEventOptions opts = PropertyEventOptions.SendOnChange);
	}
}
