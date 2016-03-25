using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
		public void AddToTemplate(string fieldName, ItemType type) {
			template.Add(fieldName, type);
		}
		public void DeleteFromTemplate(int i) {
			template.Remove(i);
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
				switch (template.TypeAt(i)) {
					case ItemType.BASIC: fields.Add(new BasicField(n, null)); break;
					case ItemType.DATE: fields.Add(new DateField(n, DateTime.MinValue)); break;
					case ItemType.IMAGE: fields.Add(new ImageField(n, null)); break;
				}
			}
		}
		public ListItem(SerializationInfo info, StreamingContext context) {
			name = info.GetValue("name", typeof(string)) as string;
			fields = info.GetValue("fields", typeof(List<ListItemField>)) as List<ListItemField>;
		}
		//properties
		internal string Name { get { return name; } }
		//methods
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

			throw new NotImplementedException();
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
