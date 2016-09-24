using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CImage = System.Windows.Controls.Image;

namespace ListApp {
	//TODO sorting messes up display panel
	//TODO option to cancel mid-way through download
	public partial class MainWindow : Window {
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
			//data = ListData.Load();
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
			list2.AddToTemplate("img", ItemType.IMAGE, 50.0);
			ListItem li1b = list2.Add(new object[] { "There are many things here", DateTime.Now,
				new XImage(@"F:\Documents\Visual Studio 2015\Projects\ListApp\a.jpg", false) });
			ListItem li2b = list2.Add();
			li2b.SetFieldData("notes", "More notes");
			li2b.SetFieldData("date", DateTime.Today);
			li2b.SetFieldData("img", new XImage("http://images2.fanpop.com/images/photos/8300000/Rin-Kagamine-Vocaloid-Wallpaper-vocaloids-8316875-1024-768.jpg", true));
			data.Lists.Add(list2);

			//XMLList list3 = new XMLList("group c (xml)", "anime");
			//list3.AddToTemplate("title", ItemType.BASIC, null, "series_title");
			//list3.AddToTemplate("episodes", ItemType.BASIC, null, "series_episodes");
			//list3.AddToTemplate("status", ItemType.ENUM, new string[] {"ERROR", "Watching", "Completed", "On Hold", "Dropped", "ERROR", "Plan to Watch" }, "my_status");
			//list3.AddToTemplate("startedDate", ItemType.DATE, null, "my_start_date");
			//list3.AddToTemplate("endedDate", ItemType.DATE, null, "my_finish_date");
			//list3.AddToTemplate("watchedEpisodes", ItemType.BASIC, null, "my_watched_episodes");
			//list3.AddToTemplate("score", ItemType.BASIC, null, "my_score");

			//data.Lists.Add(list3);

			SyncList list4 = new SyncList("AnimeSchema (Sync)", SyncList.SchemaType.ANIME_LIST, "progressivespoon");
			list4.AddToTemplate("random tag", ItemType.ENUM, new string[] { "one", "two", "three" });
			for(int i = 0; i < list4.GetSchemaLength(); i++) {
				list4.SchemaOptionAt(i).Enabled = true;
			}
			list4.SaveSchemaOptions();

			data.Lists.Add(list4);

			//PrintLists();
			//list1.DeleteFromTemplate(0);
			list1.AddToTemplate("status", ItemType.ENUM, new string[] {"completed", "started", "on hold" });
			list1.AddToTemplate("a", ItemType.BASIC, null);
			list1.AddToTemplate("b", ItemType.BASIC, null);
			list1.AddToTemplate("c", ItemType.BASIC, null);
			list1.AddToTemplate("d", ItemType.BASIC, null);
			list1.AddToTemplate("e", ItemType.BASIC, null);
			list1.AddToTemplate("f", ItemType.BASIC, null);
			list1.AddToTemplate("q", ItemType.IMAGE, 10.0);
			list1.AddToTemplate("z", ItemType.DATE, null);
			list1.AddToTemplate("adfsd", ItemType.DATE, null);
			list1.AddToTemplate("ccccc", ItemType.DATE, null);
			list1.AddToTemplate("a333", ItemType.DATE, null);
			list1.AddToTemplate("a1123", ItemType.DATE, null);
			list1.AddToTemplate("a324", ItemType.DATE, null);
			list1.AddToTemplate("a3243", ItemType.DATE, null);
			list1.AddToTemplate("a2334", ItemType.DATE, null);
			list1.AddToTemplate("a3aaaa4", ItemType.DATE, null);
			list1.AddToTemplate("a32aaaaaaaaa4", ItemType.DATE, null);
			list1.AddToTemplate("a445fd", ItemType.DATE, null);
			list1.AddToTemplate("zxd", ItemType.DATE, null);
			list1.AddToTemplate("a32ddd", ItemType.DATE, null);
			list1.AddToTemplate("hytrd", ItemType.DATE, null);
			list1.AddToTemplate("a44ree", ItemType.DATE, null);
			list1.AddToTemplate("aaaaaaaa", ItemType.DATE, null);
			list1.SetMetadata("status", new string[] { "a", "b", "c", "d" });
			list1.ResolveFieldFields();
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
					Thread.Sleep(C.SHOWN_AUTOSAVE_TEXT_TIME);
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
		internal void Refresh() {
			ListCollectionView lcv = CollectionViewSource.GetDefaultView(listItemGrid.ItemsSource) as ListCollectionView;
			lcv.CustomSort = null;
			foreach (DataGridColumn dgc in listItemGrid.Columns) {
				dgc.SortDirection = null;
			}
			listItemGrid.Items.Refresh();
		}
		//WPF
		private void ListActionImage_MouseUp(object sender, MouseButtonEventArgs e) {
			MList l = data[shownList];
			if (l is SyncList) {
				//TODO refreshments
				(l as SyncList).StartRefreshAllTask(this, syncBar, messageLabel, syncCancel);
				//(l as XMLList).LoadValues("http://myanimelist.net/malappinfo.php?u=progressivespoon&status=all&type=anime", true);
				//(l as XmlList).LoadValues(@"F:\Documents\Visual Studio 2015\Projects\ListApp\al.xml");
				DisplayList(shownList);
				Refresh();
			}
			else {
				object[] fields = new AddItemDialog(this).ShowAndGetItem(data[shownList].Template);
				if (fields != null) {
					ListItem li = l.Add();
					for (int i = 0; i < l.Template.Count; i++) {
						li.SetFieldData(l.Template[i].Name, fields[i]);
					}
					Refresh();
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
						(fe as CImage).Source = (lif as ImageField).GetBitmapImage();
					}
					else if (lif is EnumField) {
						fe = new Label();
						(fe as Label).Content = (lif as EnumField).GetSelectedValue(iti.Metadata);
					}
					else {
						fe = new Label();
						(fe as Label).Content = lif.Value == null ? "" : lif.Value.ToString();
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
			listActionImage.Source = list is SyncList ?
				Properties.Resources.reloadIcon.ConvertToBitmapImage() : Properties.Resources.addIcon.ConvertToBitmapImage();
			listTitleLabel.Content = list.Name;
			listItemGrid.Columns.Clear();
			listItemGrid.ItemsSource = list.Items;
			//listItemGrid.Items.Clear();
			//foreach(ListItem li in list) {
			//	listItemGrid.Items.Add(li);
			//}
			foreach (ItemTemplateItem iti in list.Template) {
				listItemGrid.Columns.Add(DefineColumn(iti));
			}
			shownList = id;
			DisplayItem(0);
		}
		private DataGridTemplateColumn DefineColumn(ItemTemplateItem iti) {
			//CImage img = new CImage();
			//img.BeginInit();
			//img.Source = (lif as ImageField).GetBitmap();
			//img.EndInit();
			//fe = img;
			DataGridTemplateColumn dgc = new DataGridTemplateColumn();
			Binding bind = new Binding();
			bind.Mode = BindingMode.OneWay;
			bind.ConverterParameter = iti;
			Type uiType = typeof(TextBlock);
			switch (iti.Type) {
				case ItemType.DATE:
				case ItemType.BASIC:
					bind.Converter = new ListItemToValueConverter();
					break;
				case ItemType.ENUM:
					bind.Converter = new ListItemToEnumConverter();
					break;
				case ItemType.IMAGE:
					uiType = typeof(CImage);
					bind.Converter = new ListItemToImageConverter();
					break;
				case ItemType.NUMBER:
					bind.Converter = new ListItemToNumberConverter();
					break;
				case ItemType.DECIMAL:
					bind.Converter = new ListItemToDecimalConverter();
					break;
				default:
					throw new NotImplementedException();
			}
			FrameworkElementFactory fef = new FrameworkElementFactory(uiType);
			if (uiType.Name.Equals("TextBlock")) {
				fef.SetBinding(TextBlock.TextProperty, bind);
			}
			else if(uiType.Name.Equals("Image")) {
				//TODO
				fef.SetBinding(CImage.SourceProperty, bind);
				fef.SetValue(CImage.MaxHeightProperty, iti.Metadata);
			}
			else {
				throw new NotImplementedException();
			}
			DataTemplate dataTemp = new DataTemplate();
			dataTemp.VisualTree = fef;
			dataTemp.DataType = typeof(DataGridTemplateColumn);
			dgc.CellTemplate = dataTemp;
			dgc.Header = iti.Name;
			dgc.CanUserSort = true;
			dgc.SortMemberPath = iti.Name;

			return dgc;
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
				DisplayItem(listItemGrid.SelectedIndex);
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

		private void listItemGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			DisplayItem(listItemGrid.SelectedIndex);
		}

		private void listItemGrid_Sorting(object sender, DataGridSortingEventArgs e) {
            e.Handled = true;
			ListSortDirection dir = e.Column.SortDirection != ListSortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            e.Column.SortDirection = dir;
			Console.WriteLine(CollectionViewSource.GetDefaultView(listItemGrid.ItemsSource));
			ListCollectionView lcv = CollectionViewSource.GetDefaultView(listItemGrid.ItemsSource) as ListCollectionView;
			lcv.CustomSort = new ListItemComparer(e.Column.Header as string, dir);
		}

		private void syncCancel_Click(object sender, RoutedEventArgs e) {
			if(data[shownList] is SyncList) {
				(data[shownList] as SyncList).CancelRefreshAllTask();
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
