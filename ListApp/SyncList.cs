using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;

namespace ListApp {
	[Serializable]
	class SyncList : MList {
		internal enum SchemaType { ANIME_LIST }
		//members
		private SchemaType schemaType;
		[NonSerialized]
		private ISchema schema;
		private string[] schemaParams;
		[NonSerialized]
		private bool observable;
		//constructors
		internal SyncList(string name, SchemaType schemaType, string[] schemaParams) : base(name) {
			this.schemaType = schemaType;
			this.schema = FindSchema(schemaType);
			this.schemaParams = schemaParams;
			observable = true;
		}
		internal SyncList(string name, Dictionary<string, string> decoded) : base(name, decoded) {
			schemaType = (SchemaType)Enum.Parse(typeof(SchemaType), decoded[nameof(schemaType)]);
			schema = FindSchema(schemaType);
			schemaParams = Utils.DecodeSequence(decoded[nameof(schemaParams)]).ToArray();
			observable = true;
		}
		internal ISchema Schema { get { return schema; } }
		internal string[] SchemaParams { get { return schemaParams; } }
		//methods
		internal void SetObservable(bool observable) {
			this.observable = observable;
		}
		internal override bool CanObserve() {
			return observable;
		}
		public SyncItem FindWithId(string id) {
			foreach(MItem mi in Items) {
				if(mi is SyncItem) {
					SyncItem si = mi as SyncItem;
					if (si.Id.Equals(id)) {
						return si;
					}
				}
			}
			return null;
		}
		public void AddToTemplate(SchemaOption so) {
			AddToTemplate(so.DefaultName, so.ToTemplateItem(this));
		}
		public SyncItem AddSync(string id) {
			SyncItem si = new SyncItem(id, Template);
			Items.Add(si);
			return si;
		}
		public override void AddRecoveryData(Dictionary<string, string> rec) {
			//call base
			base.AddRecoveryData(rec);
			rec[C.TYPE_ID_KEY] = nameof(SyncList); //override with this class' name
			rec.Add(nameof(schemaType), schemaType.ToString());
			rec.Add(nameof(schemaParams), Utils.EncodeSequence(schemaParams));
		}
		private ISchema FindSchema(SchemaType type) {
			switch (type) {
				case SchemaType.ANIME_LIST: return new AnimeListSchema();
			}
			return null;
		}
		private object ParseSyncData(SchemaType type, string eData) {
			switch (type) {
				case SchemaType.ANIME_LIST: return eData;
			}
			return null;
		}
	}
}
