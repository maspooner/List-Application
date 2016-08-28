using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ListApp {
	[Serializable]
	class ListItem : IEnumerable<ListItemField> {
		//members
		private List<ListItemField> fields;
		//constructors
		internal ListItem(List<ItemTemplateItem> template) {
			fields = new List<ListItemField>();
			for (int i = 0; i < template.Count; i++) {
				fields.Add(CreateField(template[i]));
			}
		}
		//properties
		internal ListItemField this[int i] { get { return fields[i]; } }
		internal int Count { get { return fields.Count; } }
		//methods
		private ListItemField CreateField(ItemTemplateItem item) {
			switch (item.Type) {
				case ItemType.BASIC: return new BasicField(item.Name);
				case ItemType.DATE: return new DateField(item.Name);
				case ItemType.IMAGE: return new ImageField(item.Name);
				case ItemType.ENUM: return new EnumField(item.Name);
				default: return null;
			}
		}
		internal ListItemField FindField(string name) {
			return fields.Find(x => x.Name.Equals(name));
		}
		internal void SetFieldData(string fieldName, object value) {
			ListItemField lif = FindField(fieldName);
			if (lif == null) {
				throw new InvalidOperationException();
			}
			else {
				lif.Value = value;
			}
		}
		internal void ChangeTemplate(List<ItemTemplateItem> template) {
			List<ListItemField> newFields = new List<ListItemField>();
			for (int i = 0; i < template.Count; i++) {
				ListItemField match = FindField(template[i].Name);
				newFields.Add(match != null ? match : CreateField(template[i]));
			}
			this.fields = newFields;
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
	class ListItemComparer : IComparer {
		private string name;
		private ListSortDirection lsd;
		internal ListItemComparer(string name, ListSortDirection lsd) {
			this.name = name;
			this.lsd = lsd;
		}
		public int Compare(object x, object y) {
			int comp = (x as ListItem).FindField(name).CompareTo((y as ListItem).FindField(name));
			return lsd.Equals(ListSortDirection.Ascending) ? comp : comp * -1;
		}
	}
	class SyncListItem : ListItem {
		//members
		private string id;
		//constructors
		internal SyncListItem(string id, List<ItemTemplateItem> template) : base(template) {
			this.id = id;
		}
		//methods
	}
}
