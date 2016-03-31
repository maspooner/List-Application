using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;

namespace ListApp {
	public partial class MainWindow : Window {
		//members
		private const string FILE_PATH = @"C:\Users\Matt\Documents\Visual Studio 2015\Projects\ListApp\lists.bin"; //TODO adjustable
		private List<MList> lists;
		private int shownList;
		//constructors
		public MainWindow() {
			InitializeComponent();
			//LoadLists();
			lists = new List<MList>();
			shownList = -1;

			MList list1 = new MList("group a");
			list1.AddToTemplate("notes", ItemType.BASIC, null);
			list1.AddToTemplate("date", ItemType.DATE, null);
			list1.AddToTemplate("img", ItemType.IMAGE, null);
			ListItem li1a = list1.Add("1a");
			li1a.SetFieldData("notes", "There are many things here");
			li1a.SetFieldData("date", DateTime.Now);
			li1a.SetFieldData("img", new Image());
			ListItem li2a = list1.Add("2a");
			li2a.SetFieldData("notes", "More notes");
			li2a.SetFieldData("date", DateTime.Today);
			li2a.SetFieldData("img", new Image());
			lists.Add(list1);

			MList list2 = new MList("group b");
			list2.AddToTemplate("notes", ItemType.BASIC, null);
			list2.AddToTemplate("date", ItemType.DATE, null);
			list2.AddToTemplate("img", ItemType.IMAGE, null);
			ListItem li1b = list2.Add("1b");
			li1b.SetFieldData("notes", "There are many things here");
			li1b.SetFieldData("date", DateTime.Now);
			li1b.SetFieldData("img", new Image());
			ListItem li2b = list2.Add("2b");
			li2b.SetFieldData("notes", "More notes");
			li2b.SetFieldData("date", DateTime.Today);
			li2b.SetFieldData("img", new Image());
			lists.Add(list2);

			PrintLists();
			list1.DeleteFromTemplate(0);
			list1.AddToTemplate("status", ItemType.ENUM, new string[] {"completed", "started", "on hold" });
			list1.SetMetadata("status", new string[] { "a", "b", "c", "d" });
			list1.ResolveFieldFields();
			li2a.SetFieldData("status", 1);

			for (int i = 0; i < lists.Count; i++) {
				leftPanel.Children.Add(CreateListLabel(lists[i], i));
			}

			PrintLists();
			SaveLists();
		}
		//methods
		private void PrintLists() {
			foreach (MList m in lists) {
				Console.WriteLine(m.Name);
				foreach (ListItem li in m) {
					Console.WriteLine("\t" + li.Name);
					foreach (ListItemField lif in li) {
						Console.WriteLine("\t\t" + lif.Name + ": " + lif.GetValue());
					}
				}
			}
		}
		public void SaveLists() {
			Stream stream = File.Open(FILE_PATH, FileMode.Create);
			BinaryFormatter bformatter = new BinaryFormatter();
			bformatter.Serialize(stream, lists);
			stream.Close();
		}
		public void LoadLists() {
			if (new FileInfo(FILE_PATH).Exists) {
				Stream stream = File.Open(FILE_PATH, FileMode.Open);
				BinaryFormatter bformatter = new BinaryFormatter();
				lists = (List<MList>)bformatter.Deserialize(stream);
				stream.Close();
			}
			else {
				lists = new List<MList>();
			}
		}
		private Label CreateListLabel(MList list, int i) {
			Label l = new Label();
			l.Name = "list_" + i;
			l.Content = list.Name;
			l.MouseUp += ListNameLabel_MouseUp;
			return l;
		}
		private Label CreateListItemLabel(ListItem item, int i) {
			Label l = new Label();
			l.Name = "listItem_" + i;
			l.Content = item.Name;
			return l;
		}
		//WPF
		private void ListNameLabel_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			Label l = sender as Label;
			int listID = int.Parse(l.Name.Substring(l.Name.Length - 1));
            if (shownList != listID) {
				rightPanel.Children.Clear();
				MList list = lists[listID];
				for (int i = 0; i < list.Count; i++) {
					rightPanel.Children.Add(CreateListItemLabel(list[i], i));
				}
				Console.WriteLine(l.Name + '.');
				shownList = listID;
			}
			
		}
	}
}
