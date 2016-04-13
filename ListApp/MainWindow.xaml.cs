using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ListApp {
	public partial class MainWindow : Window {
		//members
		private const string FILE_PATH = @"C:\Users\Matt\Documents\Visual Studio 2015\Projects\ListApp\"; //TODO adjustable
		private GridLength lastHeight, lastWidth;
		private List<MList> lists;
		private int shownList;
		private ContextMenu itemsMenu;
		//constructors
		public MainWindow() {
			InitializeComponent();
			LoadImages();

			lastHeight = lastWidth = GridLength.Auto;
			shownList = -1;

			LoadTestLists();
			//LoadLists();
			for (int i = 0; i < lists.Count; i++) {
				leftPanel.Children.Add(CreateListLabel(lists[i], i));
			}
			DisplayList(0);
			DisplayItem(0);
			itemsMenu = new ContextMenu();
			itemsMenu.Items.Add("Edit");
			itemsMenu.Items.Add("Reorder");
			itemsMenu.Items.Add("Delete");
			//TODO commands
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
			ListItem li1b = list2.Add(new object[] { "There are many things here", DateTime.Now, new Bitmap(System.Drawing.Image.FromFile(FILE_PATH + "a.jpg")).ConvertToBitmapImage() });
			ListItem li2b = list2.Add();
			li2b.SetFieldData("notes", "More notes");
			li2b.SetFieldData("date", DateTime.Today);
			li2b.SetFieldData("img", new Bitmap(System.Drawing.Image.FromFile(FILE_PATH + "a.jpg")).ConvertToBitmapImage());
			lists.Add(list2);

			//PrintLists();
			//list1.DeleteFromTemplate(0);
			list1.AddToTemplate("status", ItemType.ENUM, new string[] {"completed", "started", "on hold" });
			list1.AddToTemplate("a", ItemType.BASIC, null);
			list1.AddToTemplate("b", ItemType.BASIC, null);
			list1.AddToTemplate("c", ItemType.BASIC, null);
			list1.AddToTemplate("d", ItemType.BASIC, null);
			list1.AddToTemplate("e", ItemType.BASIC, null);
			list1.AddToTemplate("f", ItemType.BASIC, null);
			list1.SetMetadata("status", new string[] { "a", "b", "c", "d" });
			list1.ResolveFieldFields();
			list1.Add();
			li1a.SetFieldData("status", 1);
			//list2.ReorderTemplate(2, 0);
			//list2.ResolveFieldFields();
			//li2a.SetFieldData("status", 1);
		}
		//methods
		private void LoadImages() {
			addImage.Source = Properties.Resources.addIcon.ConvertToBitmapImage();
			generalOptionsImage.Source = Properties.Resources.optionIcon.ConvertToBitmapImage();
			listOptionImage.Source = Properties.Resources.optionIcon.ConvertToBitmapImage();
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
				FrameworkElement fe;
				if(lif is ImageField) {
					System.Windows.Controls.Image img = new System.Windows.Controls.Image();
					img.BeginInit();
					img.Source = (lif as ImageField).GetBitmap();
					img.EndInit();
					fe = img;
				}
				else if (lif is EnumField) {
					fe = new Label();
					(fe as Label).Content = (lif as EnumField).GetSelectedValue(list.Template.Find(x => lif.Name.Equals(x.Name)).Metadata);
				}
				else {
					fe = new Label();
					(fe as Label).Content = lif.GetValue();
				}
				fe.SetValue(Grid.RowProperty, i + 1);
				fe.SetValue(Grid.ColumnProperty, j);
				fe.ContextMenu = itemsMenu;
				listItemGrid.Children.Add(fe);
			}
		}
		//WPF
		private void AddImage_MouseUp(object sender, MouseButtonEventArgs e) {
			object[] data = new AddItemDialog(this).ShowAndGetItem(lists[shownList].Template);
			if(data != null) {
				MList l = lists[shownList];
				ListItem li = l.Add();
				for(int i = 0; i < l.Template.Count; i++) {
					li.SetFieldData(l.Template[i].Name, data[i]);
				}
				AddListItemRow(lists[shownList], l.Count - 1);
			}
		}
		private void DisplayItem(int i) {
			MList l = lists[shownList];
			ListItem li = l[i];
			contentPanel.ClearGrid();
			//add new
			Utils.SetupContentGrid(contentPanel, l.Template);
			//TODO
			foreach (ListItemField lif in li) {
				FrameworkElement fe = null;
				switch (lif.) {
					case ItemType.BASIC:
					case ItemType.DATE:
					case ItemType.ENUM:
						fe = new Label();
						(fe as Label).Content = lif.
						break;
					case ItemType.IMAGE:

						break;
				}
			}
		}
		private void DisplayList(int id) {
			MList list = lists[id];
			listTitleLabel.Content = list.Name;
			listItemGrid.ClearGrid();
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
			shownList = id;
		}
		private void ListNameLabel_MouseUp(object sender, MouseButtonEventArgs e) {
			Label l = sender as Label;
			int listID = int.Parse(l.Name.Substring(l.Name.Length - 1));
            if (shownList != listID) {
				DisplayList(listID);
			}
		}
		private void GeneralOptionImage_MouseUp(object sender, MouseButtonEventArgs e) {
			//TODO
		}
		private void ListOptionImage_MouseUp(object sender, MouseButtonEventArgs e) {
			//TODO move to inside a dialog
			List<ItemTemplateItem> newTemplate = new EditLayoutDialog(this).ShowAndGetTemplate(lists[shownList].Template);
			if(newTemplate != null) {
				lists[shownList].ClearTemplate();
				foreach (ItemTemplateItem iti in newTemplate) {
					lists[shownList].AddToTemplate(iti);
				}
				//TODO rearrange content panel
			}
		}
		private void CollapseLeft_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			Console.WriteLine("COLLAPSE");
			ColumnDefinition fc = mainGrid.ColumnDefinitions[0];
			if (fc.MinWidth == 0) {
				fc.MinWidth = 100; //TODO adjust
				fc.MaxWidth = double.PositiveInfinity;
				fc.Width = lastWidth;
			}
			else {
				fc.MinWidth = 0;
				fc.MaxWidth = 0;
				lastWidth = fc.Width;
				fc.Width = new GridLength(0);
			}
		}
		private void CollapseBottom_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			Console.WriteLine("COLLAPSE");
			RowDefinition fr = rightGrid.RowDefinitions[2];
			if (fr.MinHeight == 0) {
				fr.MinHeight = 100; //TODO adjust
				fr.MaxHeight = double.PositiveInfinity;
				fr.Height = lastHeight;
			}
			else {
				fr.MinHeight = 0;
				fr.MaxHeight = 0;
				lastHeight = fr.Height;
				fr.Height = new GridLength(0);
			}
		}
	}
}
