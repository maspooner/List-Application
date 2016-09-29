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
	class MItem {
		//members
		private Dictionary<string, MField> fields;
		//constructors
		internal MItem(Dictionary<string, FieldTemplateItem> template) {
			fields = new Dictionary<string, MField>();
			foreach(string key in template.Keys) {
				fields.Add(key, CreateField(template[key]));
			}
		}
		//properties
		internal MField this[string fieldName] { get { return fields[fieldName]; } }
		//methods
		private MField CreateField(FieldTemplateItem item) {
			switch (item.Type) {
				case FieldType.BASIC: return new BasicField(item.Name);
				case FieldType.DATE: return new DateField(item.Name);
				case FieldType.IMAGE: return new ImageField(item.Name);
				case FieldType.ENUM: return new EnumField(item.Name);
				case FieldType.NUMBER: return new NumberField(item.Name);
				case FieldType.DECIMAL: return new DecimalField(item.Name);
				default: throw new NotImplementedException();
			}
		}
		internal void AddField(FieldTemplateItem fti) {
			fields.Add(fti.Name, CreateField(fti));
		}
		internal void RemoveField(string fieldName) {
			fields.Remove(fieldName);
		}
		//internal void ChangeTemplate(List<FieldTemplateItem> template) {
		//	List<ListItemField> newFields = new List<ListItemField>();
		//	for (int i = 0; i < template.Count; i++) {
		//		ListItemField match = FindField(template[i].Name);
		//		newFields.Add(match != null ? match : CreateField(template[i]));
		//	}
		//	this.fields = newFields;
		//}
	}
	class ListItemComparer : IComparer {
		private string name;
		private ListSortDirection lsd;
		internal ListItemComparer(string name, ListSortDirection lsd) {
			this.name = name;
			this.lsd = lsd;
		}
		public int Compare(object x, object y) {
			int comp = (x as MItem)[name].CompareTo((y as MItem)[name]);
			return lsd.Equals(ListSortDirection.Ascending) ? comp : comp * -1;
		}
	}
	class SyncListItem : MItem {
		//members
		private string id;
		//constructors
		internal SyncListItem(string id, Dictionary<string, FieldTemplateItem> template) : base(template) {
			this.id = id;
		}
		//methods
	}
}
