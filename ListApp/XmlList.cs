using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ListApp {
	[Serializable]
	class XMLList : MList, ISerializable {
		//members
		private string itemTag;
		private List<string> tagNames;
		//constructors
		internal XMLList(string name, string itemTag) : base(name) {
			this.itemTag = itemTag;
			tagNames = new List<string>();
		}
		internal XMLList(SerializationInfo info, StreamingContext context) : base(info, context) {
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
		internal int? ParseNullable(string s, string fail) {
			if (s.Equals(fail)) {
				return null;
			}
			else {
				return int.Parse(s);
			}
		}
		internal void LoadValues(string fileName, bool fromWeb = false) {
			//TODO
			Clear();
			XmlDocument doc = new XmlDocument();
			if (fromWeb) {
				using (WebClient client = new WebClient()) {
					doc.LoadXml(client.DownloadString(fileName));
				}
			}
			else {
				doc.Load(fileName);
			}
			XmlNodeList items = doc.GetElementsByTagName(itemTag);
			foreach (XmlNode n in items) {
				ListItem li = Add();
				for(int i = 0; i < tagNames.Count; i++) {
					XmlNode foundNode = FindNode(tagNames[i], n.ChildNodes);
					if(foundNode != null) {
						object data = null;
						switch (Template[i].Type) {
							case ItemType.BASIC: data = foundNode.InnerText; break;
							case ItemType.DATE:
								DateTime dateTime;
								bool success = DateTime.TryParseExact(foundNode.InnerText, "yyyy-MM-dd",
									System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTime);
								if (success) {
									data = dateTime;
								}
								else {
									int? year = ParseNullable(foundNode.InnerText.Substring(0, 4), "0000");
									int? month = ParseNullable(foundNode.InnerText.Substring(5, 2), "00");
									int? day = ParseNullable(foundNode.InnerText.Substring(8, 2), "00");
									data = new XDate(year, month, day, 0);
								}
								break;
							case ItemType.ENUM:
								string[] choices = Template[i].Metadata as string[];
								for(int j = 0; data == null && j < choices.Length; j++) {
									if (choices[j].Equals(foundNode.InnerText))
										data = j;
								}
								if (data == null) {
									int Itry = 0;
									success = int.TryParse(foundNode.InnerText, out Itry);
									data = success ? Itry : 0;
								}
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
