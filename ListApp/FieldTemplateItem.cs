using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ListApp {
	[Serializable]
	class FieldTemplateItem {
		//members
		internal string Name { get; private set; }
		internal FieldType Type { get; private set; }
		internal IMetadata Metadata { get; set; }
		internal Space Space { get; set; }
		private int listPos; //FIXME implement
		//constructors
		internal FieldTemplateItem(string name, FieldType type, IMetadata metadata, Space space) {
			Name = name;
			Type = type;
			Metadata = metadata;
			Space = space;
		}
		//properties
		internal int X { get { return Space.X; } }
		internal int Y { get { return Space.Y; } }
		internal int Width { get { return Space.Width; } }
		internal int Height { get { return Space.Height; } }
		//methods
		internal bool Intersects(Space otherSpace) { return Space.Intersects(otherSpace); }
	}
	[Serializable]
	class SyncTemplateItem : FieldTemplateItem {
		//members
		private string backName;
		private object syncMeta;
		//constructors
		internal SyncTemplateItem(string field, FieldType type, IMetadata metadata, Space space, string backName, object syncMeta) 
			: base(field, type, metadata, space) {
			this.backName = backName;
			this.syncMeta = syncMeta;
		}
		//properties
		internal string BackName { get { return backName; } }
		internal object SyncMeta { get { return syncMeta; } }
		//methods
	}
}
