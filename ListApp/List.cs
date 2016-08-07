using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
			name = info.GetString("name");
			items = info.GetValue("items", typeof(List<ListItem>)) as List<ListItem>;
			template = info.GetValue("template", typeof(List<ItemTemplateItem>)) as List<ItemTemplateItem>;
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
		private Location FindOpenLocation() {
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
				li[i].SetValue(data[i]);
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
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("name", name);
			info.AddValue("items", items);
            info.AddValue("template", template);
		}
	}
	[Serializable]
	class ListItem : IEnumerable<ListItemField>, ISerializable {
		//members
		private List<ListItemField> fields;
		//constructors
		internal ListItem(List<ItemTemplateItem> template) {
			fields = new List<ListItemField>();
			for(int i = 0; i < template.Count; i++) {
				fields.Add(CreateField(template[i]));
			}
		}
		public ListItem(SerializationInfo info, StreamingContext context) {
			fields = info.GetValue("fields", typeof(List<ListItemField>)) as List<ListItemField>;
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
				ListItemField match = FindField(template[i].Name);
				newFields.Add(match != null ? match : CreateField(template[i]));
			}
			this.fields = newFields;
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
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
	[Serializable]
	class ItemTemplateItem : ISerializable {
		//members
		private string field;
		private ItemType type;
		private object metadata;
		private Location loc;
		private List<Location> occupiedCells;
		private int width, height;
		//constructors
		internal ItemTemplateItem(string field, ItemType type, object metadata, Location loc, int width, int height) {
			this.field = field;
			this.type = type;
			this.metadata = metadata;
			this.loc = loc;
			this.width = width;
			this.height = height;
			occupiedCells = new List<Location>();
			CalculateOccupied();
		}
		internal ItemTemplateItem(string field, ItemType type, object metadata, Location loc) : this(field, type, metadata, loc, 1, 1) {
		}
		internal ItemTemplateItem(ItemTemplateItem iti) : this(iti.field.Clone() as string, iti.type, iti.metadata, new Location(iti.X, iti.Y), iti.width, iti.height) {
		}
		public ItemTemplateItem(SerializationInfo info, StreamingContext context) {
			field = info.GetString("field");
			type = (ItemType) info.GetValue("type", typeof(ItemType));
			metadata = info.GetValue("metadata", typeof(object));
		}
		//properties
		internal string Name { get { return field; } }
		internal ItemType Type { get { return type; } }
		internal int X { get { return loc.X; } }
		internal int Y { get { return loc.Y; } }
		internal int Width { get { return width; } }
		internal int Height { get { return height; } }
		internal List<Location> Occupied { get { return occupiedCells; } }
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
		internal void Move(int x, int y) {
			loc.X = x;
			loc.Y = y;
			CalculateOccupied();
		}
		internal void Resize(int width, int height) {
			this.width = width;
			this.height = height;
			CalculateOccupied();
		}
		private void CalculateOccupied() {
			occupiedCells.Clear();
			for (int i = 0; i < height; i++) {
				for (int j = 0; j < width; j++) {
					occupiedCells.Add(new Location(loc.X + j, loc.Y + i));
				}
			}
		}
	}
}
