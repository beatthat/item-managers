using UnityEngine.Events;
using UnityEngine;


namespace BeatThat.UI
{
	public interface IHasSelectedItem 
	{
		UnityEvent selectedItemUpdated { get; }

		GameObject selectedGameObject { get; }
	}
}