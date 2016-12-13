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
		private ISyncSchema schema;
		private object schemaData;
		//constructors
		internal SyncList(string name, SchemaType schemaType, object schemaData) : base(name) {
			this.schemaType = schemaType;
			this.schemaData = schemaData;
			this.schema = FindSchema(schemaType);
		}
		internal SyncList(string name, Dictionary<string, string> decoded) : base(name, decoded) {
			//TODO RAD
			schemaType = (SchemaType)Enum.Parse(typeof(SchemaType), decoded[nameof(schemaType)]);
			schemaData = ParseSyncData(schemaType, decoded[nameof(schemaData)]);
			schema = FindSchema(schemaType);
		}
		internal ISyncSchema Schema { get { return schema; } }
		//methods
		public void PrepareRefresh() {
			schema.PrepareRefresh(schemaData);
		}
		public override void AddRecoveryData(Dictionary<string, string> rec) {
			//call base
			base.AddRecoveryData(rec);
			rec[C.TYPE_ID_KEY] = nameof(SyncList); //override with this class' name
			rec.Add(nameof(schemaType), schemaType.ToString());
		}
		private ISyncSchema FindSchema(SchemaType type) {
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
