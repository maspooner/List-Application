using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ListApp {
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
			type = (ItemType)info.GetValue("type", typeof(ItemType));
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
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
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
	[Serializable]
	class SyncTemplateItem : ItemTemplateItem, ISerializable {
		private delegate object SyncDelegate();
		//members
		private SyncDelegate method;
		//constructors
		public SyncTemplateItem(SerializationInfo info, StreamingContext context) : base(info, context) {
			
		}
		//properties
		//methods
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			base.GetObjectData(info, context);

		}
	}
}
