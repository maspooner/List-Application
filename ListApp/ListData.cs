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
		internal const string FILE_PATH = @"C:\Users\Matt\Documents\Visual Studio 2015\Projects\ListApp\"; //TODO adjustable
		//members
		private List<MList> lists;
		private bool txtBackup;
		private int waitSaveTime;
		//constructors
		internal ListData() {
			lists = new List<MList>();
			txtBackup = false;
			waitSaveTime =  10 * 60 * 1000; //10 min
		}
		private ListData(SerializationInfo info, StreamingContext context) {
			lists = info.GetValue("lists", typeof(List<MList>)) as List<MList>;
			txtBackup = info.GetBoolean("txtBackup");
		}
		//properties
		internal List<MList> Lists { get { return lists; } }
		internal int Count { get { return lists.Count; } }
		internal MList this[int i] { get { return lists[i]; } }
		internal int WaitSaveTime { get { return waitSaveTime - MainWindow.SHOWN_AUTOSAVE_TEXT_TIME; } } //save time for showing text on same thread
		//methods
		internal void Save() {
			Stream stream = File.Open(FILE_PATH + "lists.bin", FileMode.Create);
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
						s += "\t" + lif.Name + ": " + lif.GetValue() + "\n";
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
			using(TextWriter tw = new StreamWriter(FILE_PATH + "backup.txt")) {
				foreach (MList ml in lists) {
					tw.WriteLine(ml.Name);
					tw.WriteLine("=================");
					foreach(ListItem li in ml) {
						tw.WriteLine("Item:");
						for(int i = 0; i < ml.Template.Count; i++) {
							ListItemField lif = li[i];
							ItemTemplateItem iti = ml.Template[i];
							tw.Write("\t" + lif.Name + ": ");
							if(lif is EnumField) {
								tw.WriteLine((lif as EnumField).GetSelectedValue(iti.Metadata));
							}
							else {
								tw.WriteLine(lif.GetValue());
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
		internal ListData Load() {
			if (new FileInfo(FILE_PATH + "lists.bin").Exists) {
				Stream stream = File.Open(FILE_PATH + "lists.bin", FileMode.Open);
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
