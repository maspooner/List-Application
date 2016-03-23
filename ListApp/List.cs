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
		//constructors
		internal MList(string name) {
			this.name = name;
			items = new List<ListItem>();
		}
		public MList(SerializationInfo info, StreamingContext context) {
			name = info.GetValue("name", typeof(string)) as string;
			items = info.GetValue("items", typeof(List<ListItem>)) as List<ListItem>;
		}
		//methods
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
		}
	}
	[Serializable]
	class ListItem : ISerializable {
		//members
		private string name;
		private List<ListItemField> fields;
		//constructors
		internal ListItem() {
			throw new NotImplementedException();
		}
		public ListItem(SerializationInfo info, StreamingContext context) {
			name = info.GetValue("name", typeof(string)) as string;
			fields = info.GetValue("fields", typeof(List<ListItemField>)) as List<ListItemField>;
		}
		//methods
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
