using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ListApp {
	[Serializable]
	class ItemTemplateItem {
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
	class SyncTemplateItem : ItemTemplateItem {
		//members
		private string backName;
		private object syncMeta;
		//constructors
		internal SyncTemplateItem(string field, ItemType type, object metadata, Location loc, string backName, object syncMeta) 
			: base(field, type, metadata, loc) {
			this.backName = backName;
			this.syncMeta = syncMeta;
		}
		internal SyncTemplateItem(SyncTemplateItem sti) : base(sti) {
			this.backName = sti.backName.Clone() as string;
			this.syncMeta = sti.syncMeta;
		}
		//properties
		internal string BackName { get { return backName; } }
		internal object SyncMeta { get { return syncMeta; } }
		//methods
	}
}
