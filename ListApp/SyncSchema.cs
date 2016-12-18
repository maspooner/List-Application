using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using HtmlAgilityPack;

namespace ListApp {
	interface ISchema {
		//TODO RAD
		int GetProgress();
		int GetTotalProgress();
		List<SchemaOption> GenerateOptions(); //gives user option to choose from preloaded SyncTemplate items
		void PrepareRefresh(SyncTask type, SyncList sl, string[] paramList); //initialize all web/xml components
		void RefreshOne(SyncList l, SyncItem i); //refreshes a single item
		void FindNew(SyncList l);	 //finds new items
		void RefreshAll(SyncList l); //finds new items and refreshes current ones
		void RefreshCurrent(SyncList l); //refreshes current items
		void FinishRefresh();         //dispose of all components
	}
	class AnimeListSchema : ISchema {
		//constants
		private const string WEBSITE_PARAM = "WEB";
		private const string XML_PARAM = "XML";
		private const int WEBSITE_HUH_INDEX = 0;
		//members
		private XmlNodeList animeNodes;
		private WebClient webClient;
		private int prog;
		private int totalProg;
		private int i;
		//constructors
		internal AnimeListSchema() {
			animeNodes = null;
			webClient = null;
			prog = 1;
			totalProg = 0;
			i = 0;
		}
		//methods
		public int GetProgress() {
			return prog;
		}
		public int GetTotalProgress() {
			return totalProg;
		}
		public List<SchemaOption> GenerateOptions() {
			List<SchemaOption> opts = new List<SchemaOption>();
			opts.Add(new SchemaOption("series_title", "title", FieldType.BASIC, null, new string[] { XML_PARAM }));
			opts.Add(new SchemaOption("series_episodes", "episodes", FieldType.NUMBER, new NumberMetadata(), new string[] { XML_PARAM }));
			opts.Add(new SchemaOption("my_start_date", "start date", FieldType.DATE, null, new string[] { XML_PARAM }));
			opts.Add(new SchemaOption("series_image", "image", FieldType.IMAGE, new ImageMetadata(C.DEFAULT_IMAGE_DISPLAY_HEIGHT), new string[] { XML_PARAM }));
			opts.Add(new SchemaOption("my_finish_date", "end date", FieldType.DATE, null, new string[] { XML_PARAM }));
			opts.Add(new SchemaOption("my_status", "watch status", FieldType.ENUM, new EnumMetadata("ERROR", "Watching", "Completed", "On Hold", "Dropped", "ERROR", "Plan to Watch"), new string[] { XML_PARAM }));


			opts.Add(new SchemaOption("pub_rater_count", "rating count", FieldType.NUMBER, new NumberMetadata(), new string[] { WEBSITE_PARAM }));


			opts.Sort();
			return opts;
			//TODO RAD add all
			//return new SchemaOption2[] {
			//	new SchemaOption2("id", "series_animedb_id", FieldType.NUMBER, new NumberMetadata(), true),
			//	new SchemaOption2("synonyms", "series_synonyms", FieldType.BASIC, null, true),
			//	new SchemaOption2("watched #", "my_watched_episodes", FieldType.NUMBER, new NumberMetadata(), true),
			//	new SchemaOption2("score", "my_score", FieldType.ENUM, new EnumMetadata("-", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"), true),
			//	new SchemaOption2("rating", "pub_rating", FieldType.DECIMAL, new DecimalMetadata(), false),
			//};
		}
		public void PrepareRefresh(SyncTask type, SyncList sl, string[] paramList) {
			webClient = new WebClient();
			webClient.Encoding = Encoding.UTF8;
			string username = paramList[0];
			string xmlStr = webClient.DownloadString("http://myanimelist.net/malappinfo.php?u=" + username + "&status=all&type=anime");
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlStr);
			this.animeNodes = xmlDoc.GetElementsByTagName("anime");

			switch (type) {
				case SyncTask.ALL: totalProg = animeNodes.Count; break;
				case SyncTask.NEW: totalProg = animeNodes.Count; break;
				case SyncTask.ONE: totalProg = 1; break;
				case SyncTask.UPDATE: totalProg = animeNodes.Count; break;
			}

			i = 0;
			prog = 1;
			
		}
		public void RefreshOne(SyncList l, SyncItem si) {
			//TODO implement
			prog++;
		}
		public void RefreshAll(SyncList l) {
			XmlNode xmlNode = animeNodes.Item(i++); //TODO BUG check upper limits
			string id = xmlNode.FindChild("series_animedb_id").InnerText;
			SyncItem si = l.FindWithId(id);
			if (si == null) {
				si = l.AddSync(id);
			}
			FillItem(si, l.Template, xmlNode);
			prog++;
		}
		public void FindNew(SyncList l) {
			XmlNode xmlNode = animeNodes.Item(i++); //TODO BUG check upper limits
			string id = xmlNode.FindChild("series_animedb_id").InnerText;
			SyncItem si = l.FindWithId(id);
			if (si == null) {
				si = l.AddSync(id);
				FillItem(si, l.Template, xmlNode);
			}
			prog++;
		}

		public void RefreshCurrent(SyncList l) {
			XmlNode xmlNode = animeNodes.Item(i++); //TODO BUG check upper limits
			string id = xmlNode.FindChild("series_animedb_id").InnerText;
			SyncItem si = l.FindWithId(id);
			if (si != null) {
				FillItem(si, l.Template, xmlNode);
			}
			prog++;
		}
		private void FillItem(SyncItem si, Dictionary<string, FieldTemplateItem> template, XmlNode animeNode) {
			//Console.WriteLine("anime node id: " + si.Id);
			HtmlDocument htmlDoc = new HtmlDocument();
			System.Diagnostics.Stopwatch s = System.Diagnostics.Stopwatch.StartNew();
			string htmlStr = webClient.DownloadString("http://myanimelist.net/anime/" + si.Id + "/");
			htmlDoc.LoadHtml(htmlStr);
			foreach (string fieldName in template.Keys) {
				FieldTemplateItem fti = template[fieldName];
				if (fti is SyncTemplateItem) {
					SyncTemplateItem sti = fti as SyncTemplateItem;
					si[fieldName].Value = sti.ParamList[0].Equals("XML") ?
						FindDataFromXML(sti, animeNode.FindChild(sti.Id)) : FindDataFromHTML(htmlDoc, sti);
				}
			}
			//Console.WriteLine("time: " + s.ElapsedMilliseconds);
		}

		public void FinishRefresh() {
			webClient.Dispose();
		}




		private int? ParseNullable(string s, string fail) {
			if (s.Equals(fail)) {
				return null;
			}
			else {
				return int.Parse(s);
			}
		}
		private IComparable FindDataFromXML(SyncTemplateItem sti, XmlNode namedChild) {
			string content = namedChild.InnerText;
			switch (sti.Type) {
				case FieldType.BASIC:
					return content;
				case FieldType.DATE:
					DateTime dateTime;
					bool success = DateTime.TryParseExact(content, "yyyy-MM-dd",
						System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTime);
					if (success) {
						return new XDate(dateTime);
					}
					else {
						int? year = ParseNullable(content.Substring(0, 4), "0000");
						int? month = ParseNullable(content.Substring(5, 2), "00");
						int? day = ParseNullable(content.Substring(8, 2), "00");
						return new XDate(year, month, day, 0);
					}
				case FieldType.ENUM:
					EnumMetadata data = sti.Metadata as EnumMetadata;
					for (int j = 0; j < data.Entries.Length; j++) {
						if (data.Entries[j].Equals(content))
							return j;
					}
					int Itry = 0;
					success = int.TryParse(content, out Itry);
					return success ? Itry : 0;
				case FieldType.IMAGE:
					return new XImage(content, true);
				case FieldType.NUMBER:
					return int.Parse(content);
				case FieldType.DECIMAL:
					return float.Parse(content);
				default:
					throw new NotImplementedException();
			}
		}
		private IEnumerable<HtmlNode> FindNodesWithAttribute(HtmlDocument htmlDoc, string attriName, string attriValue) {
			return htmlDoc.DocumentNode.Descendants().Where(n => n.Attributes.Contains(attriName)
				&& n.Attributes[attriName].Value.Equals(attriValue));
		}
		private IComparable FindDataFromHTML(HtmlDocument htmlDoc, SyncTemplateItem sti) {
			//IEnumerable<HtmlNode> sideBarNodes = htmlDoc.DocumentNode.Descendants().
			//	Where(n => n.Attributes.Contains("class") && n.Attributes["class"].Value.Contains("js-scrollfix-bottom"));
			//foreach(HtmlNode n in sideBarNodes) {
			//	Console.WriteLine(n.InnerText);
			//}
			if (sti.Id.Equals("pub_rating")) {
				return float.Parse(FindNodesWithAttribute(htmlDoc, "itemprop", "ratingValue").FirstOrDefault().InnerHtml);
			}
			else if (sti.Id.Equals("pub_rater_count")) {
				return int.Parse(FindNodesWithAttribute(htmlDoc, "itemprop", "ratingCount").FirstOrDefault().InnerHtml,
					System.Globalization.NumberStyles.AllowThousands);
			}
			throw new InvalidOperationException();
			//TODO
		}
	}
	class SchemaOption : IComparable<SchemaOption> {
		//properties
		internal string DefaultName { get; private set; }
		internal string Id { get; private set; }
		internal FieldType Type { get; private set; }
		internal IMetadata Metadata { get; set; }
		internal string[] ParamList { get; private set; }
		//constructors
		internal SchemaOption(string id, string name, FieldType type, IMetadata metadata, string[] paramList) {
			DefaultName = name;
			Id = id;
			Type = type;
			Metadata = metadata;
			ParamList = paramList;
		}
		//methods
		public SyncTemplateItem ToTemplateItem(MList ml) {
			return new SyncTemplateItem(Type, Metadata, ml.FindOpenSpace(1, 1), DefaultName, Id, ParamList);
		}
		public int CompareTo(SchemaOption other) {
			return Id.CompareTo(other.Id);
		}
	}
}
