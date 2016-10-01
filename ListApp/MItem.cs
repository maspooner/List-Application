using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ListApp {
	/// <summary>
	/// Represents an item in an <seealso cref="MList"/>
	/// that contains a set of <seealso cref="MField"/>s
	/// </summary>
	[Serializable]
	class MItem {
		//members
		private Dictionary<string, MField> fields;
		//constructors
		/// <summary>
		/// Constructs an <seealso cref="MItem"/> against a template
		/// of <seealso cref="FieldTemplateItem"/>s, creating a new
		/// <seealso cref="MField"/> for each
		/// </summary>
		/// <param name="template">the template to base the field creation on</param>
		internal MItem(Dictionary<string, FieldTemplateItem> template) {
			fields = new Dictionary<string, MField>();
			//for each fieldName in the template
			foreach(string fieldName in template.Keys) {
				//create a new field from each template item and add it to the fields
				AddField(template[fieldName]);
			}
		}
		//properties
		/// <summary>
		/// Indexer for accessing an <seealso cref="MField"/> by its fieldName
		/// </summary>
		/// <param name="fieldName">the name of the field to get</param>
		/// <returns>the field with the given fieldName</returns>
		internal MField this[string fieldName] { get { return fields[fieldName]; } }
		//methods
		private MField CreateField(FieldType fieldType) {
			switch (fieldType) {
				case FieldType.NUMBER:
				case FieldType.BASIC:
				case FieldType.DATE:
				case FieldType.DECIMAL:
					return new MField(fieldType);
				case FieldType.IMAGE: return new ImageField();
				case FieldType.ENUM: return new EnumField();
				default: throw new NotImplementedException();
			}
		}
		internal void AddField(FieldTemplateItem fti) {
			fields.Add(fti.Name, CreateField(fti.Type));
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
	class SyncListItem : MItem {
		//members
		private string id;
		//constructors
		internal SyncListItem(string id, Dictionary<string, FieldTemplateItem> template) : base(template) {
			this.id = id;
		}
		//methods
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
}
