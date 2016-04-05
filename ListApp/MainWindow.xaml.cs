using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ListApp {
	public partial class MainWindow : Window {
		//members
		private const string FILE_PATH = @"C:\Users\Matt\Documents\Visual Studio 2015\Projects\ListApp\"; //TODO adjustable
		private List<MList> lists;
		private int shownList;
		private AddItemDialog addItemDialog;
		//constructors
		public MainWindow() {
			InitializeComponent();
			LoadImages();
			addItemDialog = new AddItemDialog(this);
			shownList = -1;
			//LoadTestLists();
			LoadLists();
			for (int i = 0; i < lists.Count; i++) {
				leftPanel.Children.Add(CreateListLabel(lists[i], i));
			}
			PrintLists();
			SaveLists();
		}
		//test methods
		private void LoadTestLists() {
			lists = new List<MList>();
			shownList = -1;
			MList list1 = new MList("group a");
			list1.AddToTemplate("notes", ItemType.BASIC, null);
			list1.AddToTemplate("date", ItemType.DATE, null);
			ListItem li1a = list1.Add(new object[] { "There are many things here", DateTime.Now });
			ListItem li2a = list1.Add(new object[] { "More notes", DateTime.Today });
			li2a.SetFieldData("notes", "More notes");
			li2a.SetFieldData("date", DateTime.Today);
			lists.Add(list1);

			MList list2 = new MList("group b");
			list2.AddToTemplate("notes", ItemType.BASIC, null);
			list2.AddToTemplate("date", ItemType.DATE, null);
			list2.AddToTemplate("img", ItemType.IMAGE, null);
			ListItem li1b = list2.Add(new object[] { "There are many things here", DateTime.Now, new Bitmap(System.Drawing.Image.FromFile(FILE_PATH + "a.jpg")) });
			ListItem li2b = list2.Add();
			li2b.SetFieldData("notes", "More notes");
			li2b.SetFieldData("date", DateTime.Today);
			li2b.SetFieldData("img", new Bitmap(System.Drawing.Image.FromFile(FILE_PATH + "a.jpg")));
			lists.Add(list2);

			//PrintLists();
			//list1.DeleteFromTemplate(0);
			//list1.AddToTemplate("status", ItemType.ENUM, new string[] {"completed", "started", "on hold" });
			//list1.SetMetadata("status", new string[] { "a", "b", "c", "d" });
			//list2.ReorderTemplate(2, 0);
			//list2.ResolveFieldFields();
			//li2a.SetFieldData("status", 1);
		}
		//methods
		private void LoadImages() {
			addImage.Source = Properties.Resources.addIcon.ConvertToWPFImage();
		}
		private void PrintLists() {
			foreach (MList m in lists) {
				Console.WriteLine(m.Name);
				foreach (ListItem li in m) {
					Console.WriteLine();
					foreach (ListItemField lif in li) {
						Console.WriteLine("\t" + lif.Name + ": " + lif.GetValue());
					}
				}
			}
		}
		public void SaveLists() {
			Stream stream = File.Open(FILE_PATH + "lists.bin", FileMode.Create);
			BinaryFormatter bformatter = new BinaryFormatter();
			bformatter.Serialize(stream, lists);
			stream.Close();
		}
		public void LoadLists() {
			if (new FileInfo(FILE_PATH + "lists.bin").Exists) {
				Stream stream = File.Open(FILE_PATH + "lists.bin", FileMode.Open);
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
		private void AddListItemRow(MList list, int i) {
			ListItem item = list[i];
			listItemGrid.RowDefinitions.Add(new RowDefinition());
			//rest of fields
			for (int j = 0; j < item.Count; j++) {
				ListItemField lif = item[j];
				UIElement uie;
				if(lif is ImageField) {
					Console.WriteLine("img");
					System.Windows.Controls.Image img = new System.Windows.Controls.Image();
					img.BeginInit();
					img.Source = (lif as ImageField).GetBitmap();
					img.EndInit();
					uie = img;
				}
				else if (lif is EnumField) {
					uie = new Label();
					(uie as Label).Content = (lif as EnumField).GetSelectedValue(list.Template.Find(x => lif.Name.Equals(x.Name)).Metadata);
				}
				else {
					uie = new Label();
					(uie as Label).Content = lif.GetValue();
				}
				uie.SetValue(Grid.RowProperty, i + 1);
				uie.SetValue(Grid.ColumnProperty, j);
				listItemGrid.Children.Add(uie);
			}
		}
		//WPF
		private void AddImage_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			ListItem li = addItemDialog.ShowAndGetItem(lists[shownList].Template);
			Console.WriteLine("closed");
			if(li == null) {

			}
			else {

			}
		}
		private void ListNameLabel_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			Label l = sender as Label;
			int listID = int.Parse(l.Name.Substring(l.Name.Length - 1));
            if (shownList != listID) {
				MList list = lists[listID];
				listTitleLabel.Content = list.Name;
				listItemGrid.Children.Clear();
				listItemGrid.ColumnDefinitions.Clear();
				listItemGrid.RowDefinitions.Clear();
				//add new
				listItemGrid.RowDefinitions.Add(new RowDefinition());
				for (int i = 0; i < list.Template.Count; i++) {
					Label title = new Label();
					title.SetValue(Grid.RowProperty, 0);
					title.SetValue(Grid.ColumnProperty, i);
					title.Content = list.Template[i].Name;
					listItemGrid.ColumnDefinitions.Add(new ColumnDefinition());
					listItemGrid.Children.Add(title);
				}
				for (int i = 0; i < list.Count; i++) {
					AddListItemRow(list, i);
				}
				shownList = listID;
			}
			
		}
	}
}
