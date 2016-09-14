using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

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
		internal abstract IEnumerable<SyncListItem> CreateNewItems(List<ItemTemplateItem> template);
		internal abstract int GetItemCount();
        internal abstract IEnumerable<SyncTemplateItem> GenerateTemplate(SyncList list);
		internal abstract void RefreshAll(List<ItemTemplateItem> template);
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
				new SchemaOption("title", "series_title", ItemType.BASIC, null, true),
				new SchemaOption("episodes", "series_episodes", ItemType.BASIC, null, true),
			};
		}
		internal override IEnumerable<SyncTemplateItem> GenerateTemplate(SyncList list) {
			foreach (SchemaOption so in Options) {
				if (so.Enabled) {
					yield return new SyncTemplateItem(so.Name, so.Type, so.Metadata, list.FindOpenLocation(), so.BackName, so.SyncMeta);
				}
			}
		}
		internal override void RefreshAll(List<ItemTemplateItem> template) {
			
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
				case ItemType.BASIC:
					return content;
				case ItemType.DATE:
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
				case ItemType.ENUM:
					string[] choices = sti.Metadata as string[];
                    for (int j = 0; j < choices.Length; j++) {
						if (choices[j].Equals(content))
							return j;
					}
					int Itry = 0;
					success = int.TryParse(content, out Itry);
					return success ? Itry : 0;
				case ItemType.IMAGE:
					return new XImage(content, true);
			}
			return null;
		}
		private object FindDataFromHTML(SyncTemplateItem sti) {
			return null; //TODO
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
		internal override IEnumerable<SyncListItem> CreateNewItems(List<ItemTemplateItem> template) {
			int xyz = 0; //TODO remove
			foreach (XmlNode xmlNode in animeNodes) {
				string id = xmlNode.FindChild("series_animedb_id").InnerText;
				Console.WriteLine("anime node id: " + id);
				SyncListItem sli = new SyncListItem(id, template);
				HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
				using (WebClient client = new WebClient()) {
					System.Diagnostics.Stopwatch s = System.Diagnostics.Stopwatch.StartNew();
					string htmlStr = client.DownloadString("http://myanimelist.net/anime/" + id + "/");
					htmlDoc.LoadHtml(htmlStr);
					foreach (ItemTemplateItem iti in template) {
						if (iti is SyncTemplateItem) {
							SyncTemplateItem sti = iti as SyncTemplateItem;
							sli.FindField(sti.Name).Value = (bool)sti.SyncMeta ? FindDataFromXML(sti, xmlNode.FindChild(sti.BackName)) : FindDataFromHTML(sti);
						}
					}
					Console.WriteLine("time: " + s.ElapsedMilliseconds);
					if(xyz++ > 5) {
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
		private ItemType type;
		private object metadata;
		private object syncMeta;
		private bool enabled;
		//constructors
		internal SchemaOption(string name, string backName, ItemType type, object metadata, object syncMeta) {
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
		internal ItemType Type { get { return type; } }
		internal object Metadata { get { return metadata; } }
		internal object SyncMeta { get { return syncMeta; } }
		internal bool Enabled { get { return enabled; } set { this.enabled = value; } }
		//methods
	}
}
