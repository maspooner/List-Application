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
		//nonserialized
		[NonSerialized]
		private string baseDir;
		//constructors
		internal ListData() {
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
				foreach (ListItem li in m) {
					s += "\n";
					foreach (ListItemField lif in li) {
						s += "\t" + lif.Name + ": " + lif.Value + "\n";
					}
				}
			}
			return s;
		}
		public IEnumerator<MList> GetEnumerator() {
			foreach (MList ml in lists) {
				yield return ml;
			}
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		private void WriteToBackupTxt() {
			//TODO
			using(TextWriter tw = new StreamWriter(baseDir + "/backup.txt")) {
				foreach (MList ml in lists) {
					tw.WriteLine(ml.Name);
					tw.WriteLine("=================");
					foreach(ListItem li in ml) {
						tw.WriteLine("Item:");
						for(int i = 0; i < ml.Template.Count; i++) {
							ListItemField lif = li[i];
							FieldTemplateItem iti = ml.Template[i];
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
	}
}
