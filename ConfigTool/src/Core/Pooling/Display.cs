using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Pooling {
	public class Display : MonoBehaviour {
		public List<SimplePool.Definition> definitions = new List<SimplePool.Definition>();
		public List<GameObject> pool = new List<GameObject>();
		public List<CacheDisplay> cacheDisplays = new List<CacheDisplay>();
		private List<SimplePool.Cache> caches = new List<SimplePool.Cache>();

		public void AddDefinition(SimplePool.Definition d) {
			definitions.Add(d);
		}

		public void AddItem(SimplePoolItem item) {
			pool.Add(item.gameObject);
		}

		public void AddCache(SimplePool.Cache cache) {
			caches.Add(cache);
		}

		private void Update() {
			cacheDisplays.Clear();
			foreach (SimplePool.Cache cache in caches) {
				cacheDisplays.Add(new CacheDisplay() {
					items = cache.items.Select(item => item.gameObject).ToList(),
					activeItems = cache.activeItems.Select(item => item.gameObject).ToList(),
					inactiveItems = cache.inactiveItems.Select(item => item.gameObject).ToList()
				});
			}
		}

		[Serializable]
		public class CacheDisplay {
			public List<GameObject> items;
			public List<GameObject> activeItems;
			public List<GameObject> inactiveItems;
		}
	}
}