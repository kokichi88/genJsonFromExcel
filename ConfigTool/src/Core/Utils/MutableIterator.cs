using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Core.Utils {
	public class MutableIterator {
		private int index;
		private int count;
		public int Index {
			get {
				return this.index;
			}
			set {
				if (this.index == 0) {
					this.index = value;
				}
			}
		}
		public int Count {
			get {
				return this.count;
			}
		}
		public MutableIterator() {
			this.Reset();
		}
		public void Reset() {
			this.index = 0;
			this.count = 0;
		}
		public void Init(int count) {
			this.index = 0;
			this.count = count;
		}
		public void Init(ICollection list) {
			this.index = 0;
			this.count = list.Count;
		}
		public bool Active() {
			return this.index < this.count;
		}
		public void Next() {
			this.index++;
		}
		public void OnRemove(int i) {
			if (this.count > 0) {
				this.count--;
				if (i <= this.index) {
					this.index--;
				}
			}
		}
	}
}
