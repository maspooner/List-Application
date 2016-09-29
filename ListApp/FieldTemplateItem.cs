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
		private string field;
		private FieldType type;
		private int listPos; //FIXME implement
		internal IMetadata Metadata { get; set; }
		internal Space Space { get; set; }
		//constructors
		internal FieldTemplateItem(string field, FieldType type, IMetadata metadata, Space space) {
			this.field = field;
			this.type = type;
			Metadata = metadata;
			Space = space;
		}
		//properties
		internal string Name { get { return field; } }
		internal FieldType Type { get { return type; } }
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
