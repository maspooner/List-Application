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
		private List<string> fieldTemplate;
		private List<ItemType> typeTemplate;
		//constructors
		internal MList(string name) {
			this.name = name;
			items = new List<ListItem>();
			fieldTemplate = new List<string>();
			typeTemplate = new List<ItemType>();
		}
		public MList(SerializationInfo info, StreamingContext context) {
			name = info.GetValue("name", typeof(string)) as string;
			items = info.GetValue("items", typeof(List<ListItem>)) as List<ListItem>;
			fieldTemplate = info.GetValue("fieldTemplate", typeof(List<string>)) as List<string>;
			typeTemplate = info.GetValue("typeTemplate", typeof(List<ItemType>)) as List<ItemType>;
		}
		//methods
		public void AddToTemplate(string fieldName, ItemType type) {
			fieldTemplate.Add(fieldName);
			typeTemplate.Add(type);
			//TODO add to items
			throw new NotImplementedException();
		}
		public void DeleteFromTemplate(int i) {
			fieldTemplate.RemoveAt(i);
			typeTemplate.RemoveAt(i);
			//TODO remove from items
			throw new NotImplementedException();
		}
		public void ResolveFieldFields() {
			throw new NotImplementedException();
		}
		public void Add(ListItem li, int i) {
			items.Insert(i, li);
		}
		public void Delete(int i) {
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
            info.AddValue("typeTemplate", typeTemplate);
			info.AddValue("fieldTemplate", fieldTemplate);
		}
	}
	[Serializable]
	class ListItem : ISerializable {
		//members
		private string name;
		private List<ListItemField> fields;
		//constructors
		internal ListItem(string name, List<string> fieldNames, List<ItemType> template) {
			this.name = name;
			fields = new List<ListItemField>();
			for(int i = 0; i < fieldNames.Count; i++) {
				switch (template[i]) {
					case ItemType.BASIC: fields.Add(new BasicField(fieldNames[i], null)); break;
					case ItemType.DATE: fields.Add(new DateField(fieldNames[i], DateTime.MinValue)); break;
					case ItemType.IMAGE: fields.Add(new ImageField(fieldNames[i], null)); break;
				}
			}
		}
		public ListItem(SerializationInfo info, StreamingContext context) {
			name = info.GetValue("name", typeof(string)) as string;
			fields = info.GetValue("fields", typeof(List<ListItemField>)) as List<ListItemField>;
		}
		//methods
		public void UpdateTemplate(List<ItemType> template) {
			throw new NotImplementedException();
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("name", name);
			info.AddValue("fields", fields);
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
