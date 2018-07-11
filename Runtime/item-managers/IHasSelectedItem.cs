using UnityEngine;
using UnityEngine.Events;


namespace BeatThat.ItemManagers
{
    public interface IHasSelectedItem 
	{
		UnityEvent selectedItemUpdated { get; }

		GameObject selectedGameObject { get; }
	}
}
