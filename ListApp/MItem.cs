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
	class MItem : IRecoverable {
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
				AddField(fieldName, template[fieldName]);
			}
		}
		/// <summary>
		/// Recovers an MItem from a decoded dictionary set
		/// </summary>
		internal MItem(Dictionary<string, string> decoded) {
			fields = new Dictionary<string, MField>();
			foreach(string fieldName in decoded.Keys) {
				if (!fieldName.Equals(C.TYPE_ID_KEY)) {
					//decode the MField's data and add the field
					MField mf = new MField(Utils.DecodeMultiple(decoded[fieldName]));
					fields.Add(fieldName, mf);
				}
			}
		}
		//properties
		/// <summary>
		/// Indexer for accessing an <seealso cref="MField"/> by its fieldName
		/// </summary>
		internal MField this[string fieldName] { get { return fields[fieldName]; } }
		//methods
		/// <summary>
		/// Creates an <seealso cref="MField"/> of the specified field type
		/// </summary>
		/// <param name="fieldType">the type of field to create</param>
		/// <returns>the newly created field</returns>
		private MField CreateField(FieldType fieldType) {
			switch (fieldType) {
				//just create an MField of the specified type
				case FieldType.NUMBER:
				case FieldType.BASIC:
				case FieldType.DECIMAL:
					return new MField(fieldType);
				//special handling for enumerations, dates, and images
				case FieldType.DATE: return new DateField();
				case FieldType.IMAGE: return new ImageField();
				case FieldType.ENUM: return new EnumField();
				default: throw new NotImplementedException();
			}
		}
		/// <summary>
		/// Adds a field to this item based on a template
		/// </summary>
		/// <param name="fieldName">The field name to use</param>
		/// <param name="fti">the template to use to construct the item</param>
		internal void AddField(string fieldName, FieldTemplateItem fti) {
			//create and add a field based on the type of item
			fields.Add(fieldName, CreateField(fti.Type));
		}
		/// <summary>
		/// Removes a field from this item based on the fieldName
		/// </summary>
		/// <param name="fieldName">the name of the field to remove</param>
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
		public virtual void AddRecoveryData(Dictionary<string, string> rec) {
			rec.Add(C.TYPE_ID_KEY, nameof(MItem));
		}
		public string ToRecoverable() {
			Dictionary<string, string> rec = new Dictionary<string, string>();
			AddRecoveryData(rec);
			foreach (string fieldName in fields.Keys) {
				rec.Add(fieldName, fields[fieldName].ToRecoverable());
			}
			return Utils.EncodeMultiple(rec);
		}
	}


	[Serializable]
	class SyncItem : MItem {
		//members
		internal string Id { get; private set; }
		private bool userEdited;
		//constructors
		internal SyncItem(string id, Dictionary<string, FieldTemplateItem> template) : base(template) {
			Id = id;
			userEdited = false;
		}
		internal SyncItem(Dictionary<string, string> decoded) : base(decoded) {
			Id = decoded[nameof(Id)];
			userEdited = bool.Parse(decoded[nameof(userEdited)]);
		}
		//methods
		public override void AddRecoveryData(Dictionary<string, string> rec) {
			rec.Add(C.TYPE_ID_KEY, nameof(SyncItem));
			rec.Add(nameof(Id), Id);
			rec.Add(nameof(userEdited), userEdited.ToString());
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
			int comp = (x as MItem)[name].CompareTo((y as MItem)[name]);
			return lsd.Equals(ListSortDirection.Ascending) ? comp : comp * -1;
		}
	}
}
