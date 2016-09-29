using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ListApp {
	[Serializable]
	class ListData : IEnumerable<MList>, ISerializable{
		//members
		private List<MList> lists;
		private bool txtBackup;
		private int waitSaveTime;
		[NonSerialized]
		private string baseDir;
		//constructors
		private ListData() {
			lists = new List<MList>();
			txtBackup = true;
			waitSaveTime =  10 * 60 * 1000; //10 min //TODO adjustable
			baseDir = GetBaseDir();
		}
		private ListData(SerializationInfo info, StreamingContext context) {
			lists = info.GetValue("lists", typeof(List<MList>)) as List<MList>;
			txtBackup = info.GetBoolean("txtBackup");
		}
		//properties
		internal List<MList> Lists { get { return lists; } }
		internal int Count { get { return lists.Count; } }
		internal MList this[int i] { get { return lists[i]; } }
		internal int WaitSaveTime { get { return waitSaveTime - C.SHOWN_AUTOSAVE_TEXT_TIME; } } //save time for showing text on same thread
		//methods
		internal void Save() {
			Stream stream = File.Open(baseDir + "/lists.bin", FileMode.Create);
			BinaryFormatter bformatter = new BinaryFormatter();
			bformatter.Serialize(stream, this);
			stream.Close();
			if (txtBackup) {
				WriteToBackupTxt();
			}
		}
		public override string ToString() {
			string s = "";
			foreach (MList m in lists) {
				s += m.Name + "\n";
				foreach (MItem li in m) {
					s += "\n";
					foreach(string fieldName in m.Template.Keys) {
						s += "\t" + li[fieldName].Name + ": " + li[fieldName].Value + "\n";
					}
				}
			}
			return s;
		}
		public IEnumerator<MList> GetEnumerator() { return lists.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		private void WriteToBackupTxt() {
			//TODO
			using(TextWriter tw = new StreamWriter(baseDir + "/backup.txt")) {
				foreach (MList ml in lists) {
					tw.WriteLine(ml.Name);
					tw.WriteLine("=================");
					foreach(MItem mi in ml) {
						tw.WriteLine("Item:");
						foreach(string fieldName in ml.Template.Keys) {
							MField lif = mi[fieldName];
							FieldTemplateItem iti = ml.Template[fieldName];
							tw.Write("\t" + lif.Name + ": ");
							if(lif is EnumField) {
								tw.WriteLine((lif as EnumField).GetSelectedValue(iti.Metadata as EnumMetadata));
							}
							else {
								tw.WriteLine(lif.Value);
							}
						}
					}
				}
			}
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("lists", lists);
			info.AddValue("txtBackup", txtBackup);
		}
		//statics
		private static string GetBaseDir() { return Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath); }
		internal static ListData Load() {
			string listsFile = GetBaseDir() + "/lists.bin";
			if (new FileInfo(listsFile).Exists) {
				Stream stream = File.Open(listsFile, FileMode.Open);
				BinaryFormatter bformatter = new BinaryFormatter();
				ListData ld = (ListData) bformatter.Deserialize(stream);
				stream.Close();
				return ld;
			}
			else {
				return new ListData();
			}
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
			li1a["date"].Value = DateTime.Now;
			li1a["dec"].Value = 50f;
			li1a["num"].Value = 6;
			MItem li2a = list1.Add();
			li2a["notes"].Value = "more notes";
			li2a["date"].Value = DateTime.Today;
			li2a["dec"].Value = 40f;
			li2a["num"].Value = 5;
			data.Lists.Add(list1);

			MList list2 = new MList("group b");
			list2.AddToTemplate("notes", FieldType.BASIC, null);
			list2.AddToTemplate("date", FieldType.DATE, null);
			list2.AddToTemplate("img", FieldType.IMAGE, new ImageMetadata(50.0));
			MItem li1b = list2.Add();
			li1b["notes"].Value = "More notes";
			li1b["date"].Value = DateTime.Today;
			li1b["img"].Value = new XImage("http://images2.fanpop.com/images/photos/8300000/Rin-Kagamine-Vocaloid-Wallpaper-vocaloids-8316875-1024-768.jpg", true);
            MItem li2b = list2.Add();
			li2b["notes"].Value = "More notes";
			li2b["date"].Value = DateTime.Today;
			li2b["img"].Value = new XImage(@"F:\Documents\Visual Studio 2015\Projects\ListApp\a.jpg", false);
			data.Lists.Add(list2);

			SyncList list4 = new SyncList("AnimeSchema (Sync)", SyncList.SchemaType.ANIME_LIST, "progressivespoon");
			list4.AddToTemplate("random tag", FieldType.ENUM, new EnumMetadata("one", "two", "three"));
			for (int i = 0; i < list4.GetSchemaLength(); i++) {
				list4.SchemaOptionAt(i).Enabled = true;
			}
			list4.SaveSchemaOptions();

			data.Lists.Add(list4);

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
