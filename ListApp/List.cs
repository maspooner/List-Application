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
		private List<ItemTemplateItem> template;
		//constructors
		internal MList(string name) {
			this.name = name;
			items = new List<ListItem>();
			template = new List<ItemTemplateItem>();
		}
		public MList(SerializationInfo info, StreamingContext context) {
			name = info.GetValue("name", typeof(string)) as string;
			items = info.GetValue("items", typeof(List<ListItem>)) as List<ListItem>;
			template = info.GetValue("template", typeof(List<ItemTemplateItem>)) as List<ItemTemplateItem>;
		}
		//properties
		internal string Name { get { return name; } }
		internal int Count { get { return items.Count; } }
		//methods
		internal ListItem this[int i] { get { return items[i]; } }
		internal void AddToTemplate(string fieldName, ItemType type, object metadata) {
			template.Add(new ItemTemplateItem(fieldName, type, metadata));
		}
		internal void DeleteFromTemplate(int i) {
			template.RemoveAt(i);
		}
		internal void ReorderTemplate(int oi, int ni) {
			int ani = ni > oi ? ni - 1 : ni;
			ItemTemplateItem item = template[oi];
			template.RemoveAt(oi);
			template.Insert(ani, item);
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
		internal ListItem(string name, List<ItemTemplateItem> template) {
			this.name = name;
			fields = new List<ListItemField>();
			for(int i = 0; i < template.Count; i++) {
				string n = template[i].Name;
				fields.Add(CreateField(template[i]));
			}
		}
		public ListItem(SerializationInfo info, StreamingContext context) {
			name = info.GetValue("name", typeof(string)) as string;
			fields = info.GetValue("fields", typeof(List<ListItemField>)) as List<ListItemField>;
		}
		//properties
		internal string Name { get { return name; } }
		//methods
		private ListItemField CreateField(ItemTemplateItem item) {
			switch (item.Type) {
				case ItemType.BASIC: return new BasicField(item.Name, null);
				case ItemType.DATE: return new DateField(item.Name, DateTime.MinValue);
				case ItemType.IMAGE: return new ImageField(item.Name, null);
				case ItemType.ENUM: return new EnumField(item.Name, 0);
				default: return null;
			}
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
		internal void ChangeTemplate(List<ItemTemplateItem> template) {
			List<ListItemField> newFields = new List<ListItemField>();
			for(int i = 0; i < template.Count; i++) {
				ListItemField match = fields.Find(x => x.Name.Equals(template[i].Name));
				newFields.Add(match != null ? match : CreateField(template[i]));
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
	class ItemTemplateItem : ISerializable {
		//members
		private string field;
		private ItemType type;
		private object metadata;
		//constructors
		internal ItemTemplateItem(string field, ItemType type, object metadata) {
			this.field = field;
			this.type = type;
			this.metadata = metadata;
		}
		public ItemTemplateItem(SerializationInfo info, StreamingContext context) {
			field = info.GetValue("field", typeof(string)) as string;
			type = (ItemType) info.GetValue("type", typeof(ItemType));
			metadata = info.GetValue("metadata", typeof(object));
		}
		//properties
		internal string Name { get { return field; } }
		internal ItemType Type { get { return type; } }
		internal object Metadata {
			get { return metadata; }
			set { metadata = value; }
		}
		//methods
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("field", field);
			info.AddValue("type", type);
			info.AddValue("metadata", metadata);
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
