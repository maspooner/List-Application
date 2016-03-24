using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ListApp {
	public partial class MainWindow : Window {
		//members
		private const string FILE_PATH = "lists.bin"; //TODO adjustable
		private List<MList> lists;
		//constructors
		public MainWindow() {
			InitializeComponent();
			LoadLists();
		}
		//methods
		public void SaveLists() {
			Stream stream = File.Open(FILE_PATH, FileMode.Create);
			BinaryFormatter bformatter = new BinaryFormatter();

			bformatter.Serialize(stream, lists);
			stream.Close();
		}
		public void LoadLists() {
			Stream stream = File.Open(FILE_PATH, FileMode.Open);
			BinaryFormatter bformatter = new BinaryFormatter();

			lists = (List<MList>)bformatter.Deserialize(stream);
			stream.Close();
		}
	}
}
