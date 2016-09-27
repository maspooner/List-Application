using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using HtmlAgilityPack;

namespace ListApp {
	[Serializable]
	abstract class SyncSchema {
		//members
		private SchemaOption[] options;
		//constructors
		internal SyncSchema() {
			this.options = GenerateOptions();
		}
		//properties
		internal SchemaOption[] Options { get { return options; } }
		//methods
		protected abstract SchemaOption[] GenerateOptions();
		internal abstract void PrepareRefresh();
		internal abstract IEnumerable<SyncListItem> CreateNewItems(List<FieldTemplateItem> template);
		internal abstract int GetItemCount();
        internal abstract IEnumerable<SyncTemplateItem> GenerateTemplate(SyncList list);
		internal abstract void RefreshAll(List<FieldTemplateItem> template);
		//internal abstract Task<List<SyncListItem>> CreateItems(List<ItemTemplateItem> template);
		
	}
	[Serializable]
	class AnimeListSchema : SyncSchema {
		//members
		private string username;
		[NonSerialized]
		private XmlNodeList animeNodes;
		//constructors
		internal AnimeListSchema(string username) {
			this.username = username;
			this.animeNodes = null;
		}
		//methods
		protected override SchemaOption[] GenerateOptions() {
			return new SchemaOption[] {
				new SchemaOption("title", "series_title", FieldType.BASIC, null, true),
				new SchemaOption("episodes", "series_episodes", FieldType.NUMBER, new NumberMetadata(), true),
				new SchemaOption("start date", "my_start_date", FieldType.DATE, null, true),
				new SchemaOption("end date", "my_finish_date", FieldType.DATE, null, true),
				new SchemaOption("image", "series_image", FieldType.IMAGE, new ImageMetadata(50.0), true),
				new SchemaOption("watch status", "my_status", FieldType.ENUM, new EnumMetadata("ERROR", "Watching", "Completed", "On Hold", "Dropped", "ERROR", "Plan to Watch"), true),
				new SchemaOption("id", "series_animedb_id", FieldType.NUMBER, new NumberMetadata(), true),
				new SchemaOption("synonyms", "series_synonyms", FieldType.BASIC, null, true),
				new SchemaOption("watched #", "my_watched_episodes", FieldType.NUMBER, new NumberMetadata(), true),
				new SchemaOption("score", "my_score", FieldType.ENUM, new EnumMetadata("-", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"), true),
				new SchemaOption("rating", "pub_rating", FieldType.DECIMAL, new DecimalMetadata(), false),
				new SchemaOption("rating count", "pub_rater_count", FieldType.NUMBER, new NumberMetadata(), false)
			};
		}
		internal override IEnumerable<SyncTemplateItem> GenerateTemplate(SyncList list) {
			foreach (SchemaOption so in Options) {
				if (so.Enabled) {
					yield return new SyncTemplateItem(so.Name, so.Type, so.Metadata, list.FindOpenLocation(), so.BackName, so.SyncMeta);
				}
			}
		}
		internal override void RefreshAll(List<FieldTemplateItem> template) {
			
		}
		private int? ParseNullable(string s, string fail) {
			if (s.Equals(fail)) {
				return null;
			}
			else {
				return int.Parse(s);
			}
		}
		private object FindDataFromXML(SyncTemplateItem sti, XmlNode namedChild) {
			string content = namedChild.InnerText;
			switch (sti.Type) {
				case FieldType.BASIC:
					return content;
				case FieldType.DATE:
					DateTime dateTime;
					bool success = DateTime.TryParseExact(content, "yyyy-MM-dd",
						System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateTime);
					if (success) {
						return dateTime;
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
		private object FindDataFromHTML(HtmlDocument htmlDoc, SyncTemplateItem sti) {
			//IEnumerable<HtmlNode> sideBarNodes = htmlDoc.DocumentNode.Descendants().
			//	Where(n => n.Attributes.Contains("class") && n.Attributes["class"].Value.Contains("js-scrollfix-bottom"));
			//foreach(HtmlNode n in sideBarNodes) {
			//	Console.WriteLine(n.InnerText);
			//}
			if (sti.BackName.Equals("pub_rating")) {
				return float.Parse(FindNodesWithAttribute(htmlDoc, "itemprop", "ratingValue").FirstOrDefault().InnerHtml);
            }
			else if (sti.BackName.Equals("pub_rater_count")) {
				return int.Parse(FindNodesWithAttribute(htmlDoc, "itemprop", "ratingCount").FirstOrDefault().InnerHtml,
					System.Globalization.NumberStyles.AllowThousands);
			}
			throw new InvalidOperationException();
			//TODO
		}

  //      internal override Task<List<SyncListItem>> CreateItems(List<ItemTemplateItem> template) {
		//	List<SyncListItem> items = new List<SyncListItem>();
		//	using (WebClient client = new WebClient()) {
		//		string xmlStr = client.DownloadString("http://myanimelist.net/malappinfo.php?u=" + username + "&status=all&type=anime");
		//		XmlDocument xmlDoc = new XmlDocument();
		//		xmlDoc.LoadXml(xmlStr);
		//		XmlNodeList animeNodes = xmlDoc.GetElementsByTagName("anime");
		//		//foreach (XmlNode xmlNode in animeNodes) {
		//		//	Console.WriteLine("Creating task");
		//		//	await CreateItemAsync(xmlNode, template);
		//		//}
		//		Task[] tasks = new Task[animeNodes.Count];
		//		for (int i = 0; i < tasks.Length; i++) {
		//			tasks[i] = CreateItemAsync(animeNodes[i], template);
		//			Console.WriteLine("Creating task");
		//		}
		//		Console.WriteLine("Awaiting all tasks");
		//		Parallel.ForEach(animeNodes.Cast<XmlNode>(), new ParallelOptions { MaxDegreeOfParallelism = 1 }, async (xmlNode) => {
		//			Task<SyncListItem> result = CreateItemAsync(xmlNode, template);
		//			items.Add(await result);
		//		});
				
		//		//Parallel.Invoke(new ParallelOptions() { MaxDegreeOfParallelism = 3 }, )
		//		//new Task(, TaskCreationOptions.)
		//		//await Task.Factory.StartNew(()=>Task.WaitAll(tasks));
		//		Console.WriteLine("Done with all tasks");
		//	}
		//	return items;
		//}
		//private SyncListItem CreateItemAsync(XmlNode xmlNode, List<ItemTemplateItem> template) {
		//	try {
		//		string id = xmlNode.FindChild("series_animedb_id").InnerText;
		//		Console.WriteLine("anime node id: " + id);
		//		SyncListItem sli = new SyncListItem(id, template);
		//		HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
		//		WebClient client = new WebClient();
		//		string htmlStr = client.DownloadString("http://myanimelist.net/anime/" + id + "/");
		//		htmlDoc.LoadHtml(htmlStr);
		//		//htmlDoc.DocumentNode.
		//		foreach (ItemTemplateItem iti in template) {
		//			if (iti is SyncTemplateItem) {
		//				SyncTemplateItem sti = iti as SyncTemplateItem;
		//				sli.FindField(sti.Name).Value = (bool)sti.SyncMeta ? FindDataFromXML(sti, xmlNode.FindChild(sti.BackName)) : FindDataFromHTML(sti);
		//			}
		//		}
		//		client.Dispose();
		//		return sli;
		//	}
		//	catch (Exception e) {
		//		Console.WriteLine(e);
		//	}
		//	return null;
		//}
		internal override void PrepareRefresh() {
			using (WebClient client = new WebClient()) {
				client.Encoding = Encoding.UTF8;
				string xmlStr = client.DownloadString("http://myanimelist.net/malappinfo.php?u=" + username + "&status=all&type=anime");
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xmlStr);
				this.animeNodes = xmlDoc.GetElementsByTagName("anime");
			}
		}
		internal override IEnumerable<SyncListItem> CreateNewItems(List<FieldTemplateItem> template) {
			int xyz = 0; //TODO remove
			foreach (XmlNode xmlNode in animeNodes) {
				string id = xmlNode.FindChild("series_animedb_id").InnerText;
				Console.WriteLine("anime node id: " + id);
				SyncListItem sli = new SyncListItem(id, template);
				HtmlDocument htmlDoc = new HtmlDocument();
				using (WebClient client = new WebClient()) {
					System.Diagnostics.Stopwatch s = System.Diagnostics.Stopwatch.StartNew();
					string htmlStr = client.DownloadString("http://myanimelist.net/anime/" + id + "/");
					htmlDoc.LoadHtml(htmlStr);
					foreach (FieldTemplateItem iti in template) {
						if (iti is SyncTemplateItem) {
							SyncTemplateItem sti = iti as SyncTemplateItem;
							sli.FindField(sti.Name).Value = (bool)sti.SyncMeta ? 
								FindDataFromXML(sti, xmlNode.FindChild(sti.BackName)) : FindDataFromHTML(htmlDoc, sti);
						}
					}
					Console.WriteLine("time: " + s.ElapsedMilliseconds);
					if(xyz++ > 10) {
						yield break;
					}
				}
				yield return sli;
			}
		}
		internal override int GetItemCount() {
			return animeNodes.Count;
		}
	}
	[Serializable]
	class SchemaOption {
		//members
		private string name;
		private string backName;
		private FieldType type;
		private object metadata;
		private object syncMeta;
		private bool enabled;
		//constructors
		internal SchemaOption(string name, string backName, FieldType type, object metadata, object syncMeta) {
			this.name = name;
			this.backName = backName;
			this.type = type;
			this.metadata = metadata;
			this.syncMeta = syncMeta;
			enabled = false;
		}
		//properties
		internal string Name { get { return name; } }
		internal string BackName { get { return backName; } }
		internal FieldType Type { get { return type; } }
		internal object Metadata { get { return metadata; } }
		internal object SyncMeta { get { return syncMeta; } }
		internal bool Enabled { get { return enabled; } set { this.enabled = value; } }
		//methods
	}
}
