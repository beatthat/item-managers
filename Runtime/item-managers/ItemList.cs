using System;
using System.Collections.Generic;
using BeatThat.Controllers;
using BeatThat.GetComponentsExt;
using BeatThat.ManagePrefabInstances;
using BeatThat.Pools;
using BeatThat.SafeRefs;
using BeatThat.TransformPathExt;
using BeatThat.UnityEvents;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BeatThat.ItemManagers
{

    public class ItemList<ItemType> : ItemList<ItemType, ItemType> where ItemType : Component {}

	/// <summary>
	/// Non generic, non-abstract base class for ItemManager<T> exists primarily to enable a default custom Unity Editor for ItemManagers.
	/// </summary>
	public class ItemList : Controller, ManagesPrefabInstances
	{
		public PrefabInstancePolicy m_defaultInstancePolicy;

		public PrefabInstancePolicy defaultInstancePolicy { get { return m_defaultInstancePolicy; } }

		#region ManagesPrefabInstances implementation
        
		virtual public bool supportsMultiplePrefabTypes { get { return false; } } 
         
		virtual public void GetPrefabInstances (ICollection<PrefabInstance> instances, bool ensureCreated = false)
		{
			throw new NotImplementedException ();
		}

		virtual public void GetPrefabTypes (ICollection<PrefabType> types)
		{
			throw new NotImplementedException ();
		}

		public void OnApplyPrefab_BeforeAllSiblings (GameObject prefabInstance, GameObject prefab) {}

		public void OnApplyPrefab (GameObject prefabInstance, GameObject prefab)
		{
			#if UNITY_EDITOR
			this.ApplyManagedPrefabInstancesThenRemoveFromParentPrefab();
			#endif
		}

		public void OnApplyPrefab_AfterAllSiblings (GameObject prefabInstance, GameObject prefab) {}
			
		#endregion
	}

	/// <summary>
	/// Simple scroll list that manages adding and cleaning up items. 
	/// Assumes all items are the same type/can be instantiated from a single prefab.
	/// </summary>
	public class ItemList<ItemType, ListItemType> : ItemList, ItemManager<ItemType>, IHasItemAddedGoEvent
		where ListItemType : Component
		where ItemType : class
	{

		override public bool supportsMultiplePrefabTypes { get { return true; } } 

		override public void GetPrefabInstances (ICollection<PrefabInstance> instances, bool ensureCreated = false)
		{
			#if UNITY_EDITOR
			if(Application.isPlaying) {
				return;
			}

			using(var foundItems = ListPool<ListItemType>.Get())
			using(var foundObjects = ListPool<GameObject>.Get())
			using(var prefabTypes = ListPool<PrefabType>.Get())  {
				(m_contentParent != null ? m_contentParent.transform: this.transform).GetComponentsInDirectChildren(foundItems, true);


				GetPrefabTypes(prefabTypes);

				if(ensureCreated && foundItems.Count == 0) {
					foreach(var pt in prefabTypes) {
						if(pt.prefab == null) {
							Debug.LogWarning("[" + Time.frameCount + "] encountered null prefab for type " + pt.prefabType);
							continue;
						}

						var prefab = pt.prefab as ListItemType;
						if(prefab == null) {
							Debug.LogWarning("[" + Time.frameCount + "] unable to cast prefab from type " + pt.prefabType.GetType() + " to type " + typeof(ListItemType));
							continue;
						}
						foundItems.Add(AddItem(prefab));
					}
				}

				foreach(var c in foundItems) {
					foundObjects.Add(c.gameObject);
				}

				this.ObjectsToPrefabInstances(foundObjects, prefabTypes, instances, this.defaultInstancePolicy);
			}

			#endif
		}

		override public void GetPrefabTypes (ICollection<PrefabType> types)
		{
			types.Add (new PrefabType {
				prefab = m_itemPrefab,
				prefabType = typeof(ListItemType),
				instancePolicy = m_defaultInstancePolicy
			});
		}

		public ListItemType m_itemPrefab;
		public GameObject m_contentParent;

		public enum SetItemNamesPolicy { Never = 0, InEditor = 1, Always = 2 }

		[Tooltip("Should instantiated items have their name set. By default YES, but only in the editor (for easier navigation)")]
		public SetItemNamesPolicy m_setItemNames = SetItemNamesPolicy.InEditor;

		[Tooltip("if TRUE, then when setItemNames is active, will pass the name of the item's prefab as parameter 0 (and item index is as paramter 1)")]
		public bool m_passPrefabNameToItemNameFormatAsParameter1 = true;

		[Tooltip("when item instances have their names set, what format should be used? By default it is {PrefabName}-{ItemIndex}")]
		public string m_itemNameFormat = "{0}-{1}";

		// Analysis disable ConvertToAutoProperty
		public SetItemNamesPolicy setItemNames { get { return m_setItemNames; } set { m_setItemNames = value; } }
		public string itemNameFormat { get { return m_itemNameFormat; } set { m_itemNameFormat = value; } }
		// Analysis restore ConvertToAutoProperty

		public bool m_clearItemsOnUnbind = true;

		public UnityEvent<GameObject> itemAddedGO { get { return m_itemAddedGO?? (m_itemAddedGO = new GameObjectEvent()); } set { m_itemAddedGO = value; } }
		[SerializeField]private UnityEvent<GameObject> m_itemAddedGO;

		sealed override protected void UnbindController()
		{
			if(m_clearItemsOnUnbind) {
				ClearItems();
			}
			base.UnbindController();
			UnbindScrollList();
		}

		/// <summary>
		/// Override to add behaviour on unbind
		/// </summary>
		virtual protected void UnbindScrollList()
		{
		}

		public ListItemType lastItem 
		{
			get {
				return m_listItems.Count == 0 ? null : m_listItems [m_listItems.Count - 1].value;
			}
		}

		public bool GetLastItem<T>(out T item) where T : class
		{
			item = this.lastItem as T;
			return (item != null);
		}

		public ListItemType GetListItem(int ix) 
		{
			return m_listItems[ix].value;
		}

		public void GetAllRootItems(ICollection<ListItemType> results)
		{
			foreach(var li in m_listItems) {
				var v = li.value;
				if(v != null) {
					results.Add(v);
				}
			}
		}

		public ItemType Get(int ix) 
		{
			// fix later with less wasteful search
			using(var items = ListPool<ItemType>.Get()) {
				GetAll(items);
				return items[ix];
			}
		}

		public int GetAll(ICollection<ItemType> result)
		{
			int nBefore = result.Count;
			foreach(var i in m_listItems) {
				var v = i.value;
				if(v == null) {
					continue;
				}
				GetItems(i.value, result);
			}
			return result.Count - nBefore;
		}

		#region IHasRootItems implementation
		public int rootItemCount { get { return m_listItems.Count; } }

		public int GetRootItems<T>(ICollection<T> results) where T : class
		{
			using(var items = ListPool<ListItemType>.Get()) {
				GetAllRootItems(items);
				return ExtractComponents<ListItemType, T>(items, results);
			}
		}
		#endregion


		#region IHasItems implementation
		public int count
		{ 
			get { 
				// TODO: optimize
				using(var items = ListPool<ItemType>.Get()) {
					GetItems(items);
					return items.Count;
				}
			}
		}

		public int GetItems<T>(ICollection<T> results) where T : class
		{
			using(var items = ListPool<ItemType>.Get()) {
				GetAll(items);
				return ExtractComponents<ItemType, T>(items, results);
			}
		}
		#endregion

		private static int ExtractComponents<ItemT, ExtractT>(ICollection<ItemT> itemsIn, ICollection<ExtractT> itemsOut) 
			where ItemT : class 
			where ExtractT : class
		{
			int n = 0;
			foreach(var ti in itemsIn) {
				if(ti == null) {
					continue;
				}

				var item = ti as ExtractT;
				if(item != null) {
					itemsOut.Add(item);
					n++;
					continue;
				}

				var c = ti as Component;
				if (c == null) {
					Debug.LogWarning("Unable to convert item to requested type " + typeof(ExtractT).Name);
					continue;
				}


				if(typeof(GameObject) == typeof(ExtractT) && (item = c.gameObject as ExtractT) != null) {
					itemsOut.Add(item);
					n++;
					continue;
				}
					
				if(c != null && (item = c.GetComponent<ExtractT>()) != null) {
					itemsOut.Add(item);
					n++;
					continue;
				}

				
			}
			return n;
		}

		public int AddItems(ICollection<ItemType> items)
		{
			return GetItems(AddItem(), items);
		}

		private static int GetItems(ListItemType listItem, ICollection<ItemType> items)
		{
			var asItem = listItem as ItemType;
			if(asItem != null) {
				items.Add(asItem);
				return 1;
			}

			var hasItems = listItem as IHasItems;
			if(hasItems == null) {
				throw new InvalidCastException("the ListItemType of a ScrollList must either be the same as the ItemType or it must implement IHasItems ItemType="
					+ typeof(ItemType).Name + " ListItemType=" + typeof(ListItemType).Name);
			}

			return hasItems.GetItems(items);
		}

		private T InstantiatePrefab<T>(T p) where T : UnityEngine.Object
		{
			#if UNITY_EDITOR
			if(!Application.isPlaying) {
				return UnityEditor.PrefabUtility.InstantiatePrefab (p) as T;
			}
			#endif

			return Instantiate (p);
		}

		/// <summary>
		/// Creates a new item and inserts it into the list at the given index
		/// </summary>
		/// <returns>The newly instantiated item</returns>
		/// <param name="index">Index.</param>
		/// <param name="prefab">Pass to override the default item prefab</param>
		public ListItemType InsertItemAt(int index, ListItemType prefab = null)
		{
			var prefabResolved = prefab ?? m_itemPrefab;
			var item = InstantiatePrefab (prefabResolved);

			if(index < m_listItems.Count) {
				// the actual transform sibling order of the items may be different from the list, e.g. if there are bookend items not in the list
				var siblingIndex = m_listItems[index].value.transform.GetSiblingIndex(); 
				item.transform.SetParent(this.contentParent.transform, false);
				item.transform.SetSiblingIndex(siblingIndex);
				m_listItems.Insert(index, new SafeRef<ListItemType>(item));
			}
			else {
				item.transform.SetParent(this.contentParent.transform, false);
				m_listItems.Add(new SafeRef<ListItemType>(item));
			}

			if(this.setItemNames == SetItemNamesPolicy.Always || (Application.isEditor && this.setItemNames == SetItemNamesPolicy.InEditor)) {
				item.name = (m_passPrefabNameToItemNameFormatAsParameter1)?
					string.Format(this.itemNameFormat, prefabResolved.name, m_listItems.Count):
					string.Format(this.itemNameFormat, m_listItems.Count);
			}

			if(m_itemAddedGO != null) {
				m_itemAddedGO.Invoke(item.gameObject);
			}
				
			return item;
		}

		public void EnsureItemCount(int count, ListItemType prefab = null)
		{
			if(this.count == count) {
				return;
			}

			while(this.count < count) {
				AddItem(prefab);
			}

			while(this.count > count) {
				RemoveItemAt(this.count - 1);
			}
		}

		public ListItemType RemoveItemAt(int index)
		{
			Debug.LogError("[" + Time.frameCount + "][" + this.Path() + "] RemoveItemAt " + index + "!");
			var item = m_listItems[index];
			m_listItems.RemoveAt(index);
			return item.value;
		}

		/// <summary>
		/// Creates a new item and adds it to the list as the last item
		/// </summary>
		/// <returns>The newly instantiated item</returns>
		/// <param name="prefab">Pass to override the default item prefab</param>
		public ListItemType AddItem(ListItemType prefab = null)
		{
			return InsertItemAt(m_listItems.Count, prefab);
		}

		public void ClearItems()
		{
			for(int i = m_listItems.Count - 1; i >= 0; i--) {
				var item = m_listItems[i].value;
				m_listItems.RemoveAt(i);
				if(item == null) {
					continue;
				}
				Destroy(item.gameObject);
			}
		}

		virtual protected Transform FindContentParent()
		{
			if (m_contentParent != null) {
				return m_contentParent.transform;
			}

			var sc = GetComponentInChildren<ScrollRect> (true);
			Transform t = sc != null ? sc.content : null;

			if (t == null) {
				t = this.transform;
			}

			m_contentParent = t.gameObject;

			return t;
		}

		public Transform contentParent 
		{ 
			get { 
				if (m_contentParent != null) {
					return m_contentParent.transform;
				}
				var cp = FindContentParent ();
				if (cp != null) {
					m_contentParent = cp.gameObject;
				}
				return cp;
			} 
		}

		private List<SafeRef<ListItemType>> m_listItems = new List<SafeRef<ListItemType>>();
	}

}






