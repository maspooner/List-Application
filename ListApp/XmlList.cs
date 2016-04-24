using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ListApp {
	[Serializable]
	class XmlList : MList, ISerializable {
		//members
		private string itemTag;
		private List<string> tagNames;
		//constructors
		internal XmlList(string name, string itemTag) : base(name) {
			this.itemTag = itemTag;
			tagNames = new List<string>();
		}
		internal XmlList(SerializationInfo info, StreamingContext context) : base(info, context) {
			tagNames = info.GetValue("tagNames", typeof(List<string>)) as List<string>;
		}
		//methods
		internal void RegisterTagName(int i, string tag) {
			tagNames[i] = tag;
		}
		internal void AddToTemplate(string fieldName, ItemType type, object metadata, string tagName) {
			base.AddToTemplate(fieldName, type, metadata);
			tagNames.Add(tagName);
		}
		internal override void AddToTemplate(string fieldName, ItemType type, object metadata) {
			AddToTemplate(fieldName, type, metadata, "");
		}
		internal override void AddToTemplate(ItemTemplateItem iti) {
			base.AddToTemplate(iti);
			tagNames.Add("");
		}
		internal override void DeleteFromTemplate(int i) {
			base.DeleteFromTemplate(i);
			tagNames.RemoveAt(i);
		}
		internal override void ClearTemplate() {
			base.ClearTemplate();
			tagNames.Clear();
		}
		internal void LoadValues(string fileName) {
			//TODO
			//TODO replace add new item button with reload button
			Clear();
			XmlDocument doc = new XmlDocument();
			doc.Load(fileName);
			XmlNodeList items = doc.GetElementsByTagName(itemTag);
			foreach (XmlNode n in items) {
				ListItem li = Add();
				for(int i = 0; i < tagNames.Count; i++) {
					XmlNode foundNode = FindNode(tagNames[i], n.ChildNodes);
					if(foundNode != null) {
						object data = null;
						switch (Template[i].Type) {
							case ItemType.BASIC: data = foundNode.InnerText; break;
							case ItemType.DATE: data = DateTime.ParseExact(foundNode.InnerText, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture); break;
							case ItemType.ENUM:
								string[] choices = Template[i].Metadata as string[];
								for(int j = 0; data == null && j < choices.Length; j++) {
									if (choices[j].Equals(foundNode.InnerText))
										data = j;
								}
								if (data == null) data = 0;
								break;
						}
						li.SetFieldData(Template[i].Name, data);
					}
				}
			}
		}
		private XmlNode FindNode(string tag, XmlNodeList list) {
			foreach(XmlNode n in list) {
				if (n.Name.Equals(tag))
					return n;
			}
			return null;
		}
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			base.GetObjectData(info, context);
			info.AddValue("tagNames", tagNames);
		}
	}
}
