using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Linq;

namespace ListApp {
	/// <summary>
	/// Holds List data and options for all <seealso cref="MList"/>s
	/// </summary>
	[Serializable]
	class ListData : IEnumerable<MList> {
		//members
		[NonSerialized]
		private static string baseDirectory = Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);

		private List<MList> lists;
		private int waitSaveTime;
		//constructors
		/// <summary>
		/// Constructs new <seealso cref="ListData"/> with default
		/// options and no <seealso cref="MList"/>s
		/// </summary>
		private ListData() {
			lists = new List<MList>();
			waitSaveTime =  10 * 60 * 1000; //10 min //TODO adjustable
		}
		//properties
		internal int Count { get { return lists.Count; } }
		internal MList this[int i] { get { return lists[i]; } }
		internal int WaitSaveTime { get { return waitSaveTime - C.SHOWN_AUTOSAVE_TEXT_TIME; } } //save time for showing text on same thread
		//methods
		internal void AddList(MList ml) { lists.Add(ml); }
		internal void Recover(string name) {
			//string templateStr = File.ReadAllText(baseDirectory + C.BACKUPS_FOLDER + name + "_tmpl.csv");
			string file = File.ReadAllText(baseDirectory + C.BACKUPS_FOLDER + name + ".txt");
			Dictionary<string, string> decoded = Utils.DecodeMultiple(file);
			if (decoded[C.TYPE_ID_KEY].Equals(nameof(MList))) {
				AddList(new MList(name, decoded));
			}
			else if (decoded[C.TYPE_ID_KEY].Equals(nameof(SyncList))) {
				AddList(new SyncList(name, decoded));
			}
			else {
				throw new InvalidDataException("List type not found!");
			}
		}
		internal void Save() {
			Stream stream = File.Open(baseDirectory + "/lists.bin", FileMode.Create);
			BinaryFormatter bformatter = new BinaryFormatter();
			bformatter.Serialize(stream, this);
			stream.Close();

			SaveReadable();
			SaveAllRecovery();
		}
		private void SaveReadable() {
			//TODO
			using (TextWriter tw = new StreamWriter(baseDirectory + "/backup.txt")) {
				foreach (MList ml in lists) {
					tw.WriteLine(ml.Name);
					tw.WriteLine("=================");
					foreach (MItem mi in ml) {
						tw.WriteLine("Item:");
						foreach (string fieldName in ml.Template.Keys) {
							MField lif = mi[fieldName];
							FieldTemplateItem fti = ml.Template[fieldName];
							tw.Write("\t" + fieldName + ": ");
							tw.Write(lif.ToReadable(fti.Metadata));
						}
					}
				}
			}
		}
		private void SaveAllRecovery() {
			DirectoryInfo backupsDir = new DirectoryInfo(baseDirectory + C.BACKUPS_FOLDER);
			if (!backupsDir.Exists) {
				Directory.CreateDirectory(backupsDir.FullName);
			}
			foreach (MList ml in lists) {
				SaveRecovery(baseDirectory + C.BACKUPS_FOLDER + ml.Name + ".txt", ml.ToRecoverable());
			}
		}
		private void SaveRecovery(string fileName, string text) {
			using (TextWriter tw = new StreamWriter(new FileInfo(fileName).Open(FileMode.Create))) {
				tw.Write(text);
			}
		}
		public IEnumerator<MList> GetEnumerator() { return lists.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		//statics
		internal static ListData Load() {
			string listsFile = baseDirectory + "/lists.bin";
			if (new FileInfo(listsFile).Exists) {
				ListData ld = null;
				using(Stream stream = File.Open(listsFile, FileMode.Open)) {
					BinaryFormatter bformatter = new BinaryFormatter();
					ld = (ListData)bformatter.Deserialize(stream);
				}
				return ld;
			}
			else {
				return new ListData();
			}
		}
		internal static ListData RecoverLists() {
			ListData data = new ListData();
			data.Recover("group a");
			data.Recover("group b");
			data.Recover("AnimeSchema (Sync)");
			return data;
		}
		internal static ListData LoadTestLists() {
			ListData data = new ListData();
			
			MList list1 = new MList("group a");
			list1.AddToTemplate("notes", FieldType.BASIC, null);
			list1.AddToTemplate("date", FieldType.DATE, null);
			list1.AddToTemplate("dec", FieldType.DECIMAL, new DecimalMetadata(2, 3.14f, 10.26f));
			list1.AddToTemplate("num", FieldType.NUMBER, new NumberMetadata(0, 10));
			MItem li1a = list1.Add();
			li1a["notes"].Value = "There are many things here";
			li1a["date"].Value = new XDate(DateTime.Today);
			li1a["dec"].Value = 50f;
			li1a["num"].Value = 6;
			MItem li2a = list1.Add();
			li2a["notes"].Value = "more notes";
			li2a["date"].Value = new XDate(DateTime.Today);
			li2a["dec"].Value = 40f;
			li2a["num"].Value = 5;
			data.AddList(list1);

			MList list2 = new MList("group b");
			list2.AddToTemplate("notes", FieldType.BASIC, null);
			list2.AddToTemplate("date", FieldType.DATE, null);
			list2.AddToTemplate("img", FieldType.IMAGE, new ImageMetadata(50.0));
			MItem li1b = list2.Add();
			li1b["notes"].Value = "More notes";
			li1b["date"].Value = new XDate(DateTime.Today);
			li1b["img"].Value = new XImage("http://images2.fanpop.com/images/photos/8300000/Rin-Kagamine-Vocaloid-Wallpaper-vocaloids-8316875-1024-768.jpg", true);
            MItem li2b = list2.Add();
			li2b["notes"].Value = "More notes";
			li2b["date"].Value = new XDate(DateTime.Today);
			li2b["img"].Value = new XImage(@"F:\Documents\Visual Studio 2015\Projects\ListApp\a.jpg", false);
			data.AddList(list2);

			SyncList list4 = new SyncList("AnimeSchema (Sync)", SyncList.SchemaType.ANIME_LIST, "progressivespoon");
			List<SchemaOption> opts = list4.Schema.GenerateOptions();
			foreach(SchemaOption so in opts) {
				list4.AddToTemplate(so);
			}
			list4.AddToTemplate("random tag", FieldType.ENUM, new EnumMetadata("one", "two", "three"));

			data.AddList(list4);

			//PrintLists();
			//list1.DeleteFromTemplate(0);
			list1.AddToTemplate("status", FieldType.ENUM, new EnumMetadata("completed", "started", "on hold"));
			list1.AddToTemplate("a", FieldType.BASIC, null);
			list1.AddToTemplate("b", FieldType.BASIC, null);
			list1.AddToTemplate("f", FieldType.BASIC, null);
			list1.AddToTemplate("q", FieldType.IMAGE, new ImageMetadata(10.0));
			list1.AddToTemplate("z", FieldType.DATE, null);
			list1.AddToTemplate("adfsd", FieldType.DATE, null);
			list1.AddToTemplate("ccccc", FieldType.DATE, null);
			list1.AddToTemplate("a333", FieldType.DATE, null);
			list1.AddToTemplate("a2334", FieldType.DATE, null);
			list1.AddToTemplate("a3aaaa4", FieldType.DATE, null);
			list1.AddToTemplate("a32aaaaaaaaa4", FieldType.DATE, null);
			list1.AddToTemplate("a445fd", FieldType.DATE, null);
			list1.AddToTemplate("zxd", FieldType.DATE, null);
			list1.AddToTemplate("a32ddd", FieldType.DATE, null);
			list1.AddToTemplate("hytrd", FieldType.DATE, null);
			list1.AddToTemplate("a44ree", FieldType.DATE, null);
			list1.AddToTemplate("aaaaaaaa", FieldType.DATE, null);
			list1.SetMetadata("status", new EnumMetadata("a", "b", "c", "d"));
			li1a["status"].Value = 1;
			//list2.ReorderTemplate(2, 0);
			//list2.ResolveFieldFields();
			//li2a.SetFieldData("status", 1);

			return data;
		}
	}
}
