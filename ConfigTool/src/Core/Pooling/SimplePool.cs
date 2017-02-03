using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Pooling {
	public partial class SimplePool : MonoBehaviour {
		private readonly string name;

		private Dictionary<string, Definition> definitionByPath = new Dictionary<string, Definition>();
		private Dictionary<string, Cache> cacheByPath = new Dictionary<string, Cache>();
		private Dictionary<SimplePoolItem, Cache> cacheByItem = new Dictionary<SimplePoolItem, Cache>();
#if UNITY_EDITOR
		private Display display;
#endif

		public SimplePool(string name = null) {
			this.name = name;
#if UNITY_EDITOR
			string s = "SimplePool";
			if (!string.IsNullOrEmpty(name)) {
				s = name + "-Display";
			}
			display = new GameObject(s).AddComponent<Display>();
#endif
		}

		public void AddDefinition(Definition def) {
			if (def.Prefab == null)
				throw new Exception(string.Format("Definition with path {0} has null prefab", def.Path));
			if (string.IsNullOrEmpty(def.Path))
				throw new Exception("Null or empty path");

			Definition d = null;
			if (definitionByPath.TryGetValue(def.Path, out d)) {
				if (d.Prefab != def.Prefab) {
					throw new Exception(string.Format(
						"Definition with path {0} is duplicated with different prefab", def.Path
					));
				}
			}
			else {
				definitionByPath[def.Path] = def;
			}

#if UNITY_EDITOR
			display.AddDefinition(def);
#endif
		}

		public SimplePoolItem Obtain(string path) {
			Definition definition = null;
			bool found = definitionByPath.TryGetValue(path, out definition);
			if (!found) {
				throw new Exception(string.Format(name + ": Cannot find definition for path: '{0}'", path));
			}

			Cache cache = null;
			bool isCacheFound = cacheByPath.TryGetValue(path, out cache);
			if (!isCacheFound) {
				cache = new Cache();
				cacheByPath[path] = cache;
				SimplePoolItem item = InstantiateItem(definition.Prefab);
				cache.items.Add(item);
				cache.activeItems.Add(item);
				cacheByItem[item] = cache;
#if UNITY_EDITOR
				display.AddItem(item);
				display.AddCache(cache);
#endif
				return item;
			}
			else {
				SimplePoolItem item = cache.FindInactiveItem();
				if (item == null) {
					item = InstantiateItem(definition.Prefab);
					cache.items.Add(item);
					cache.activeItems.Add(item);
					cacheByItem[item] = cache;
#if UNITY_EDITOR
					display.AddItem(item);
#endif
				}
				else {
					cache.inactiveItems.Remove(item);
					cache.activeItems.Add(item);
					item.gameObject.active = true;
				}

				return item;
			}
		}

		public void Return(SimplePoolItem item) {
			item.OnBeforeReturn();
			Cache cache = cacheByItem[item];
			cache.activeItems.Remove(item);
			cache.inactiveItems.Add(item);
			item.gameObject.active = false;
		}

		public void WarmUp() {
			ICollection<Definition> definitions = definitionByPath.Values;
			foreach (Definition d in definitions) {
				List<SimplePoolItem> items = new List<SimplePoolItem>();
				for (int kIndex = 0; kIndex < d.InitialSize; kIndex++) {
					items.Add(Obtain(d.Path));
				}

				for (int kIndex = 0; kIndex < items.Count; kIndex++) {
					Return(items[kIndex]);
				}
			}
		}

		private SimplePoolItem InstantiateItem(GameObject prefab) {
			GameObject go = GameObject.Instantiate(prefab);
			SimplePoolItem spi = go.AddComponent<SimplePoolItem>();
			Transform t = go.transform;
			Quaternion originalRotation = t.rotation;
			Vector3 originalScale = t.localScale;
			spi.AddDefaultCleanupProcedures(() => {
				t.rotation = originalRotation;
				t.localScale = originalScale;
			});
			return spi;
		}
	}

	public partial class SimplePool {
		[Serializable]
		public class Definition {
			[SerializeField]
			private string path;

			[SerializeField]
			private GameObject prefab;

			[SerializeField]
			private int initialSize = 1;

			public Definition(string path, GameObject prefab) {
				this.path = path;
				this.prefab = prefab;
			}

			public Definition(string path, GameObject prefab, int initialSize) {
				this.path = path;
				this.prefab = prefab;
				this.initialSize = initialSize;
			}

			public string Path {
				get { return path; }
			}

			public GameObject Prefab {
				get { return prefab; }
			}

			public int InitialSize {
				get { return initialSize; }
			}
		}

		public class Cache {
			public List<SimplePoolItem> items = new List<SimplePoolItem>();
			public HashSet<SimplePoolItem> activeItems = new HashSet<SimplePoolItem>();
			public HashSet<SimplePoolItem> inactiveItems = new HashSet<SimplePoolItem>();

			public SimplePoolItem FindInactiveItem() {
				foreach (SimplePoolItem inactiveItem in inactiveItems) {
					return inactiveItem;
				}

				return null;
			}
		}
	}
}