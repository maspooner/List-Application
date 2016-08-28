using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ListApp {
	[Serializable]
	class MList : IEnumerable<ListItem>{
		//members
		private string name;
		private List<ListItem> items;
		private List<ItemTemplateItem> template;
		//constructors
		internal MList(string name) {
			this.name = name;
			items = new List<ListItem>();
			template = new List<ItemTemplateItem>();
		}
		//properties
		internal string Name { get { return name; } }
		internal int Count { get { return items.Count; } }
		internal List<ListItem> Items { get { return items; } }
		internal ListItem this[int i] { get { return items[i]; } }
		internal List<ItemTemplateItem> Template { get { return template; } }
		//methods
		internal virtual void AddToTemplate(ItemTemplateItem iti) {
			template.Add(iti);
		}
		internal virtual void AddToTemplate(string fieldName, ItemType type, object metadata) {
			template.Add(new ItemTemplateItem(fieldName, type, metadata, FindOpenLocation()));
		}
		internal virtual void DeleteFromTemplate(int i) {
			template.RemoveAt(i);
		}
		internal virtual void ClearTemplate() {
			template.Clear();
		}
		internal void ReorderTemplate(int oi, int ni) {
			int ani = ni > oi ? ni - 1 : ni;
			ItemTemplateItem item = template[oi];
			template.RemoveAt(oi);
			template.Insert(ani, item);
		}
		internal Location FindOpenLocation() {
			Location loc = null;
			int row = 0;
			if (template.Count == 0) {
				loc = new Location(0, 0);
			}
			while(loc == null) {
				for (int i = 0; i < C.FIELD_GRID_WIDTH && loc == null; i++) {
					bool fits = true;
					foreach(ItemTemplateItem iti in template) {
						for(int k = 0; k < iti.Occupied.Count; k++) {
							Location l = iti.Occupied[k];
                            if (l.X == i && l.Y == row) {
								fits = false;
							}
						}
					}
					if (fits) {
						loc = new Location(i, row);
					}
				}
				row++;
			}
			return loc;
		}
		internal void ResolveFieldFields() {
			foreach(ListItem li in items) {
				li.ChangeTemplate(template);
			}
		}
		internal ListItem Add() {
			ListItem li = new ListItem(template);
            items.Add(li);
			return li;
		}
		internal ListItem Add(object[] data) {
			ListItem li = new ListItem(template);
			for(int i = 0; i < template.Count; i++) {
				li[i].Value = data[i];
			}
			items.Add(li);
			return li;
		}
		internal void Delete(int i) {
			items.RemoveAt(i);
		}
		internal void Clear() {
			items.Clear();
		}
		internal void SetMetadata(string fieldName, object metadata) {
			ItemTemplateItem iti = template.Find(x => x.Name.Equals(fieldName));
			if (iti == null) {
				throw new InvalidOperationException();
			}
			else {
				iti.Metadata = metadata;
				//TODO adjust affected fields
			}
		}
		public IEnumerator<ListItem> GetEnumerator() {
			foreach (ListItem li in items) {
				yield return li;
			}
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
