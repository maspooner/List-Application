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
		private SyncSchema schema;
		//constructors
		internal SyncList(string name, SchemaType schemaType, object schemaData) : base(name) {
			this.schemaType = schemaType;
			this.schema = FindSchema(schemaType, schemaData);
		}
		internal SyncSchema Schema { get { return schema; } }
		//methods
		private SyncSchema FindSchema(SchemaType type, object schemaData) {
			switch (type) {
				case SchemaType.ANIME_LIST: return new AnimeListSchema(schemaData as string);
			}
			return null;
		}
		internal SchemaOption SchemaOptionAt(int i) { return schema.Options[i]; }
		internal int GetSchemaLength() { return schema.Options.Length; }
		internal override string GetTypeID() {
			return "SyncList";
		}
		internal void SaveSchemaOptions() {
			Dictionary<string, SyncTemplateItem> updatedValues = schema.GenerateTemplate(this);
			foreach(string fieldName in updatedValues.Keys) {
				Template[fieldName] = updatedValues[fieldName];
			}
			//TODO
			//foreach (string fieldName in Template.Keys) {
			//	if (!updatedValues.ContainsKey(fieldName)) {
			//		DeleteFromTemplate(fieldName);
			//	}
			//}
		}
		
	}
}
