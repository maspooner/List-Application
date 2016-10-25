using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ListApp {
	[Serializable]
	class FieldTemplateItem : IRecoverable {
		//members
		internal FieldType Type { get; private set; }
		internal IMetadata Metadata { get; set; }
		internal Space Space { get; set; }
		private int listPos; //FIXME implement
		//constructors
		internal FieldTemplateItem(FieldType type, IMetadata metadata, Space space) {
			Type = type;
			Metadata = metadata;
			Space = space;
		}
		internal FieldTemplateItem(Dictionary<string, string> decoded) {
			Console.WriteLine("PARSING FIELDTEMPLATEITEM");
			Type = (FieldType) Enum.Parse(typeof(FieldType), decoded[nameof(Type)]);
			Metadata = ParseMetadata(Utils.Base64DecodeDict(decoded[nameof(Metadata)]));
			Space = new Space(Utils.Base64DecodeDict(decoded[nameof(Space)]));
		}
		//properties
		internal int X { get { return Space.X; } }
		internal int Y { get { return Space.Y; } }
		internal int Width { get { return Space.Width; } }
		internal int Height { get { return Space.Height; } }
		//methods
		internal bool Intersects(Space otherSpace) { return Space.Intersects(otherSpace); }
		private IMetadata ParseMetadata(Dictionary<string, string> decoded) {
			switch (Type) {
				case FieldType.BASIC:
				case FieldType.DATE:
					return null;
				case FieldType.DECIMAL:		return new DecimalMetadata(decoded);
				case FieldType.ENUM:		return new EnumMetadata(decoded);
				case FieldType.IMAGE:		return new ImageMetadata(decoded);
				case FieldType.NUMBER:		return new NumberMetadata(decoded);
				default:					throw new NotImplementedException();
			}
		}
		public string ToRecoverable() {
			return Utils.Base64Encode(
					nameof(Type), ((int)Type).ToString(),
					nameof(Metadata), Metadata == null ? "" : Metadata.ToRecoverable(),
					nameof(Space), Space.ToRecoverable()
				);
		}
	}
	[Serializable]
	class SyncTemplateItem : FieldTemplateItem {
		//members
		private string backName;
		private object syncMeta;
		//constructors
		internal SyncTemplateItem(FieldType type, IMetadata metadata, Space space, string backName, object syncMeta) 
			: base(type, metadata, space) {
			this.backName = backName;
			this.syncMeta = syncMeta;
		}
		//properties
		internal string BackName { get { return backName; } }
		internal object SyncMeta { get { return syncMeta; } }
		//methods
	}
}
