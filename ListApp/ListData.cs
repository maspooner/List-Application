using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ListApp {
	class ListData : IEnumerable<MList>, ISerializable{
		private const string FILE_PATH = @"C:\Users\Matt\Documents\Visual Studio 2015\Projects\ListApp\"; //TODO adjustable
		//members
		private List<MList> lists;
		private bool txtBackup;
		//constructors
		internal ListData() {
			lists = new List<MList>();
			txtBackup = false;
		}
		private ListData(SerializationInfo info, StreamingContext context) {
			lists = info.GetValue("lists", typeof(List<MList>)) as List<MList>;
			txtBackup = info.GetBoolean("txtBackup");
		}
		//properties
		internal List<MList> Lists { get { return lists; } }
		internal int Count { get { return lists.Count; } }
		internal MList this[int i] { get { return lists[i]; } }
		//methods
		internal void Save() {
			Stream stream = File.Open(FILE_PATH + "lists.bin", FileMode.Create);
			BinaryFormatter bformatter = new BinaryFormatter();
			bformatter.Serialize(stream, this);
			stream.Close();
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
