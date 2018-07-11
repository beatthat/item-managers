using UnityEngine;
using UnityEngine.Events;


namespace BeatThat.ItemManagers
{
    public interface IHasItemAddedGoEvent 
	{
		UnityEvent<GameObject> itemAddedGO { get; }
	}
}
