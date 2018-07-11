using System.Collections.Generic;

namespace BeatThat.ItemManagers
{
    /// <summary>
    /// Something like a grid may be composed of items (cells) and root items (grid rows)
    /// </summary>
    public interface IHasRootItems 
	{
		int rootItemCount { get; }

		int GetRootItems<T>(ICollection<T> items) where T : class;
	}
}
