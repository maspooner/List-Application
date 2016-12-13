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
			Type = (FieldType) Enum.Parse(typeof(FieldType), decoded[nameof(Type)]);
			Metadata = ParseMetadata(Utils.DecodeMultiple(decoded[nameof(Metadata)]));
			Space = new Space(Utils.DecodeMultiple(decoded[nameof(Space)]));
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
		public virtual void AddData(Dictionary<string, string> rec) {
			rec.Add(nameof(Type), ((int)Type).ToString());
			rec.Add(nameof(Metadata), Metadata == null ? null : Metadata.ToRecoverable());
			rec.Add(nameof(Space), Space.ToRecoverable());
			rec.Add(C.TYPE_ID_KEY, nameof(FieldTemplateItem));
		}
		public string ToRecoverable() {
			Dictionary<string, string> rec = new Dictionary<string, string>();
			AddData(rec);
			return Utils.EncodeMultiple(rec);
		}
	}
	[Serializable]
	class SyncTemplateItem : FieldTemplateItem {
		//members
		internal string Name { get; private set; }
		internal string BackName { get; private set; }
		internal bool SyncMeta { get; private set; }
		//constructors
		internal SyncTemplateItem(Dictionary<string, string> decoded) : base(decoded){
			Name = decoded[nameof(Name)];
			BackName = decoded[nameof(BackName)];
			SyncMeta = bool.Parse(decoded[nameof(SyncMeta)]);
		}
		internal SyncTemplateItem(FieldType type, IMetadata metadata, Space space, string name, string backName, bool syncMeta) 
			: base(type, metadata, space) {
			Name = name;
			BackName = backName;
			SyncMeta = syncMeta;
		}
		//methods
		public override void AddData(Dictionary<string, string> rec) {
			//call base
			base.AddData(rec);
			rec.Add(nameof(Name), Name);
			rec.Add(nameof(BackName), BackName);
			rec.Add(nameof(SyncMeta), SyncMeta.ToString());
			rec[C.TYPE_ID_KEY] = nameof(SyncTemplateItem); //change to this class' name
		}
	}
}
