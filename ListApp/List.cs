using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ListApp {
	[Serializable]
	class MList : IEnumerable<ListItem>, ISerializable{
		//members
		private string name;
		private List<ListItem> items;
		private ItemTemplate template;
		//constructors
		internal MList(string name) {
			this.name = name;
			items = new List<ListItem>();
			template = new ItemTemplate();
		}
		public MList(SerializationInfo info, StreamingContext context) {
			name = info.GetValue("name", typeof(string)) as string;
			items = info.GetValue("items", typeof(List<ListItem>)) as List<ListItem>;
			template = info.GetValue("template", typeof(ItemTemplate)) as ItemTemplate;
		}
		//properties
		internal string Name { get { return name; } }
		//methods
		internal void AddToTemplate(string fieldName, ItemType type) {
			template.Add(fieldName, type);
		}
		internal void DeleteFromTemplate(int i) {
			template.Remove(i);
		}
		internal void ReorderTemplate(int oi, int ni) {
			template.Reorder(oi, ni);
		}
		internal void ResolveFieldFields() {
			foreach(ListItem li in items) {
				li.ChangeTemplate(template);
			}
		}
		internal ListItem Add(string name) {
			ListItem li = new ListItem(name, template);
            items.Add(li);
			return li;
		}
		internal void Delete(int i) {
			items.RemoveAt(i);
		}
		internal void Reorder(int oi, int ni) {
			int ani = ni > oi ? ni - 1 : ni;
			ListItem li = items[oi];
			items.RemoveAt(oi);
			items.Insert(ani, li);
		}
		public IEnumerator<ListItem> GetEnumerator() {
			foreach (ListItem li in items) {
				yield return li;
			}
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("name", name);
			info.AddValue("items", items);
            info.AddValue("template", template);
		}
	}
	[Serializable]
	class ListItem : IEnumerable<ListItemField>, ISerializable {
		//members
		private string name;
		private List<ListItemField> fields;
		//constructors
		internal ListItem(string name, ItemTemplate template) {
			this.name = name;
			fields = new List<ListItemField>();
			for(int i = 0; i < template.Count; i++) {
				string n = template.FieldAt(i);
				fields.Add(CreateField(template.FieldAt(i), template.TypeAt(i)));
			}
		}
		public ListItem(SerializationInfo info, StreamingContext context) {
			name = info.GetValue("name", typeof(string)) as string;
			fields = info.GetValue("fields", typeof(List<ListItemField>)) as List<ListItemField>;
		}
		//properties
		internal string Name { get { return name; } }
		//methods
		private ListItemField CreateField(string fn, ItemType it) {
			switch (it) {
				case ItemType.BASIC: return new BasicField(fn, null);
				case ItemType.DATE: return new DateField(fn, DateTime.MinValue);
				case ItemType.IMAGE: return new ImageField(fn, null);
			}
			return null;
		}
		internal void SetFieldData(string fieldName, object value) {
			ListItemField lif = fields.Find(x => x.Name.Equals(fieldName));
			if(lif == null) {
				throw new InvalidOperationException();
			}
			else {
				lif.SetValue(value);
			}
		}
		internal void ChangeTemplate(ItemTemplate template) {
			List<ListItemField> newFields = new List<ListItemField>();
			for(int i = 0; i < template.Count; i++) {
				ListItemField match = fields.Find(x => x.Name.Equals(template.FieldAt(i)));
				newFields.Add(match != null ? match : CreateField(template.FieldAt(i), template.TypeAt(i)));
			}
			this.fields = newFields;
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("name", name);
			info.AddValue("fields", fields);
		}
		public IEnumerator<ListItemField> GetEnumerator() {
			foreach (ListItemField lif in fields) {
				yield return lif;
			}
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
	[Serializable]
	class ItemTemplate : ISerializable {
		//members
		private List<string> fields;
		private List<ItemType> types;
		//constructors
		internal ItemTemplate() {
			fields = new List<string>();
			types = new List<ItemType>();
		}
		public ItemTemplate(SerializationInfo info, StreamingContext context) {
			fields = info.GetValue("fields", typeof(List<string>)) as List<string>;
			types = info.GetValue("types", typeof(List<ItemType>)) as List<ItemType>;
		}
		//properties
		internal int Count {
			get { return fields.Count; }
		}
		//methods
		internal string FieldAt(int i) {
			return fields[i];
		}
		internal ItemType TypeAt(int i) {
			return types[i];
		}
		internal void Add(string field, ItemType it) {
			fields.Add(field);
			types.Add(it);
		}
		internal void Remove(int i) {
			fields.RemoveAt(i);
			types.RemoveAt(i);
		}
		internal void Reorder(int oi, int ni) {
			int ani = ni > oi ? ni - 1 : ni;
			string field = fields[oi];
			ItemType type = types[oi];
			fields.RemoveAt(oi);
			types.RemoveAt(oi);
			fields.Insert(ani, field);
			types.Insert(ani, type);
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("fields", fields);
			info.AddValue("types", types);
		}
	}
	class ListIndex {
		//members
		private Dictionary<string, List<string>> index;
		//constructors
		internal ListIndex(MList list, object sortType) {
			index = new Dictionary<string, List<string>>();
			foreach (ListItem li in list) {
				throw new NotImplementedException();
			}
		}
		//methods
		internal void Add(string key, string value) {
			if (index.ContainsKey(key)) {
				index[key].Add(value);
			}
			else {
				List<string> l = new List<string>();
				l.Add(value);
				index.Add(key, l);
			}
		}
	}
}
