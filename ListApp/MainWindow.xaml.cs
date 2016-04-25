using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CImage = System.Windows.Controls.Image;

namespace ListApp {
	public partial class MainWindow : Window {
		internal const int SHOWN_AUTOSAVE_TEXT_TIME = 5000; //TODO adjust
		//members
		private GridLength lastHeight, lastWidth;
		private ListData data;
		private int shownList;
		private Thread autoSaveThread;
		private ContextMenu itemsMenu;
		private bool done;
		//constructors
		public MainWindow() {
			InitializeComponent();
			LoadImages();

			lastHeight = lastWidth = GridLength.Auto;
			shownList = -1;
			done = false;

			LoadTestLists();
			//LoadLists();
			for (int i = 0; i < data.Count; i++) {
				leftPanel.Children.Add(CreateListLabel(data[i], i));
			}
			DisplayList(0);
			itemsMenu = new ContextMenu();
			itemsMenu.Items.Add("Edit");
			itemsMenu.Items.Add("Reorder");
			itemsMenu.Items.Add("Delete");
			//TODO commands
			Console.WriteLine(data);
			data.Save();
			autoSaveThread = new Thread(AutoSaveThreadStart);
			autoSaveThread.Start();
		}
		//test methods
		private void LoadTestLists() {
			data = new ListData();
			shownList = -1;
			MList list1 = new MList("group a");
			list1.AddToTemplate("notes", ItemType.BASIC, null);
			list1.AddToTemplate("date", ItemType.DATE, null);
			ListItem li1a = list1.Add(new object[] { "There are many things here", DateTime.Now });
			ListItem li2a = list1.Add(new object[] { "More notes", DateTime.Today });
			li2a.SetFieldData("notes", "More notes");
			li2a.SetFieldData("date", DateTime.Today);
			data.Lists.Add(list1);

			MList list2 = new MList("group b");
			list2.AddToTemplate("notes", ItemType.BASIC, null);
			list2.AddToTemplate("date", ItemType.DATE, null);
			list2.AddToTemplate("img", ItemType.IMAGE, null);
			ListItem li1b = list2.Add(new object[] { "There are many things here", DateTime.Now,
				new Bitmap(System.Drawing.Image.FromFile(ListData.FILE_PATH + "a.jpg")).ConvertToBitmapImage() });
			ListItem li2b = list2.Add();
			li2b.SetFieldData("notes", "More notes");
			li2b.SetFieldData("date", DateTime.Today);
			li2b.SetFieldData("img", new Bitmap(System.Drawing.Image.FromFile(ListData.FILE_PATH + "a.jpg")).ConvertToBitmapImage());
			data.Lists.Add(list2);

			XmlList list3 = new XmlList("group c (xml)", "anime");
			list3.AddToTemplate("title", ItemType.BASIC, null, "series_title");
			list3.AddToTemplate("episodes", ItemType.BASIC, null, "series_episodes");
			list3.AddToTemplate("status", ItemType.ENUM, new string[] {"", "Plan to Watch", "Watching", "Completed", "Dropped" }, "my_status");

			data.Lists.Add(list3);
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
		private void AutoSaveThreadStart() {
			while (!done) {
				try {
					Thread.Sleep(data.WaitSaveTime);
					data.Save();
					Console.WriteLine("Saving");
					Dispatcher.Invoke(new Action(() => { messageLabel.Content = "Autosave complete"; }));
					Thread.Sleep(SHOWN_AUTOSAVE_TEXT_TIME);
					Dispatcher.Invoke(new Action(() => { messageLabel.Content = ""; }));
				}
				catch(Exception e) {
					Console.WriteLine(e.Message);
					Thread.Sleep(5000); //TODO remove
				}
			}
		}
		private void LoadImages() {
			listActionImage.Source = Properties.Resources.addIcon.ConvertToBitmapImage();
			generalOptionsImage.Source = Properties.Resources.optionIcon.ConvertToBitmapImage();
			listOptionImage.Source = Properties.Resources.optionIcon.ConvertToBitmapImage();
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
					CImage img = new CImage();
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
		private void ListActionImage_MouseUp(object sender, MouseButtonEventArgs e) {
			MList l = data[shownList];
			if (l is XmlList) {
				//TODO choose file
				(l as XmlList).LoadValues(@"C:\Users\Matt\Documents\Visual Studio 2015\Projects\ListApp\al.xml");
				DisplayList(shownList);
			}
			else {
				object[] fields = new AddItemDialog(this).ShowAndGetItem(data[shownList].Template);
				if (fields != null) {
					ListItem li = l.Add();
					for (int i = 0; i < l.Template.Count; i++) {
						li.SetFieldData(l.Template[i].Name, fields[i]);
					}
					AddListItemRow(data[shownList], l.Count - 1);
				}
			}
		}
		private void DisplayItem(int i) {
			contentPanel.ClearGrid();
			MList l = data[shownList];
			if(l.Count == 0 || i == -1) {
				Label q = new Label();
				q.Content = "No Item Selected";
				contentPanel.Children.Add(q);
			}
			else {
				ListItem li = l[i];
				//add new
				Utils.SetupContentGrid(contentPanel, l.Template);
				//TODO
				for (int k = 0; k < l.Template.Count; k++) {
					ListItemField lif = li[k];
					FrameworkElement fe = null;
					ItemTemplateItem iti = l.Template[k];
					if (lif is ImageField) {
						fe = new CImage();
						(fe as CImage).Source = (lif as ImageField).GetBitmap();
					}
					else if (lif is EnumField) {
						fe = new Label();
						(fe as Label).Content = (lif as EnumField).GetSelectedValue(iti.Metadata);
					}
					else {
						fe = new Label();
						object val = lif.GetValue();
						(fe as Label).Content = val == null ? "" : val.ToString();
					}
					Grid.SetColumn(fe, iti.X);
					Grid.SetRow(fe, iti.Y);
					Grid.SetColumnSpan(fe, iti.Width);
					Grid.SetRowSpan(fe, iti.Height);
					contentPanel.Children.Add(fe);
				}
			}
		}
		private void DisplayList(int id) {
			MList list = data[id];
			listActionImage.Source = list is XmlList ?
				Properties.Resources.reloadIcon.ConvertToBitmapImage() : Properties.Resources.addIcon.ConvertToBitmapImage();
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
			DisplayItem(0);
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
			List<ItemTemplateItem> newTemplate = new EditLayoutDialog(this).ShowAndGetTemplate(data[shownList].Template);
			if(newTemplate != null) {
				data[shownList].ClearTemplate();
				foreach (ItemTemplateItem iti in newTemplate) {
					data[shownList].AddToTemplate(iti);
				}
				DisplayItem(0);
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
		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);
			done = true;
			autoSaveThread.Interrupt();
			autoSaveThread.Join();
		}
	}
}
