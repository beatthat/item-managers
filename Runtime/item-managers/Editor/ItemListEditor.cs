using BeatThat.ManagePrefabInstances;
using UnityEditor;

namespace BeatThat.ItemManagers
{
    [CustomEditor(typeof(ItemList), true)]
	[CanEditMultipleObjects]
	public class ItemListEditor : UnityEditor.Editor 
	{

		override public void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var p = (target as ManagesPrefabInstances);
			p.OnInspectorGUI_EditPrefabs ();
		}

	}
}

