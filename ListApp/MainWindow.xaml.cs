using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CImage = System.Windows.Controls.Image;

namespace ListApp {
	/*
	 * TODO sorting messes up display panel
	 * TODO option to cancel mid-way through download
	 * FIXME major refactoring of every file
	 */
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

			data = ListData.LoadTestLists();
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
			generalOptionsImage.Source = Properties.Resources.optionIcon.ConvertToBitmapImage();
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
		private CImage CreateActionImage(System.Windows.Media.ImageSource source, MouseButtonEventHandler handler) {
			CImage ci = new CImage();
			ci.Source = source;
			ci.MouseUp += handler;
			ci.Height = 30;
			ci.Margin = new Thickness(0, 0, 5, 0);
			DockPanel.SetDock(ci, Dock.Right);
			return ci;
		}
		private void ReloadActionBar(string name, bool isSync) {
			listActionBar.Children.Clear();
			Label title = new Label();
			title.Content = name;
			DockPanel.SetDock(title, Dock.Left);

			listActionBar.Children.Add(CreateActionImage(Properties.Resources.optionIcon.ConvertToBitmapImage(), listOptionImg_MouseUp));
			listActionBar.Children.Add(CreateActionImage(Properties.Resources.addIcon.ConvertToBitmapImage(), addNewImg_MouseUp));

			if (isSync) {
				listActionBar.Children.Add(CreateActionImage(Properties.Resources.reloadIcon.ConvertToBitmapImage(), syncListImg_MouseUp));
			}
		}

		//WPF
		private void addNewImg_MouseUp(object sender, MouseButtonEventArgs e) {
			MList l = data[shownList];
			bool wasModification = new AddItemDialog().ShowDialogForItem(this, l);
			if (wasModification) {
				Refresh();
			}
		}
		private void syncListImg_MouseUp(object sender, MouseButtonEventArgs e) {
			//TODO refreshments
			(data[shownList] as SyncList).StartRefreshAllTask(this, syncBar, messageLabel, syncCancel);
			//(l as XMLList).LoadValues("http://myanimelist.net/malappinfo.php?u=progressivespoon&status=all&type=anime", true);
			//(l as XmlList).LoadValues(@"F:\Documents\Visual Studio 2015\Projects\ListApp\al.xml");
			DisplayList(shownList);
			Refresh();
		}
		private void listOptionImg_MouseUp(object sender, MouseButtonEventArgs e) {
			Dictionary<string, Space> newSpaces = new EditLayoutDialog(this).ShowAndGetTemplate(data[shownList].Template);
			if (newSpaces != null) {
				foreach(string fieldName in newSpaces.Keys) {
					data[shownList].Template[fieldName].Space = newSpaces[fieldName];
				}
				DisplayItem(listItemGrid.SelectedIndex);
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
				MItem li = l[i];
				//add new
				Utils.SetupContentGrid(contentPanel, l.Template.Values.Select(fti => fti.Space));
				//TODO
				foreach (string fieldName in l.Template.Keys) {
					MField lif = li[fieldName];
					FrameworkElement fe = null;
					FieldTemplateItem fti = l.Template[fieldName];
					if (lif is ImageField) {
						fe = new CImage();
						(fe as CImage).Source = (lif as ImageField).GetVisibleValue(fti.Metadata)
							as System.Windows.Media.Imaging.BitmapImage;
					}
					//else if (lif is EnumField) {
					//	fe = new Label();
					//	(fe as Label).Content = (lif as EnumField).GetSelectedValue(fti.Metadata as EnumMetadata);
					//}
					else {
						fe = new Label();
						(fe as Label).Content = lif.Value == null ? "" : lif.GetVisibleValue(fti.Metadata).ToString();
					}
					Grid.SetColumn(fe, fti.X);
					Grid.SetRow(fe, fti.Y);
					Grid.SetColumnSpan(fe, fti.Width);
					Grid.SetRowSpan(fe, fti.Height);
					contentPanel.Children.Add(fe);
				}
			}
		}
		private void DisplayList(int id) {
			MList list = data[id];
			ReloadActionBar(list.Name, list is SyncList);

			listItemGrid.Columns.Clear();
			listItemGrid.ItemsSource = list.Items;
			//listItemGrid.Items.Clear();
			//foreach(ListItem li in list) {
			//	listItemGrid.Items.Add(li);
			//}
			foreach (string fieldName in list.Template.Keys) {
				listItemGrid.Columns.Add(DefineColumn(list.Template[fieldName]));
			}
			shownList = id;
			DisplayItem(0);
		}
		private DataGridTemplateColumn DefineColumn(FieldTemplateItem iti) {
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
				case FieldType.DATE:
				case FieldType.BASIC:
					bind.Converter = new ListItemToValueConverter();
					break;
				case FieldType.ENUM:
					bind.Converter = new ListItemToEnumConverter();
					break;
				case FieldType.IMAGE:
					uiType = typeof(CImage);
					bind.Converter = new ListItemToImageConverter();
					break;
				case FieldType.NUMBER:
					bind.Converter = new ListItemToNumberConverter();
					break;
				case FieldType.DECIMAL:
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
				fef.SetValue(CImage.MaxHeightProperty, (iti.Metadata as ImageMetadata).MaxHeight);
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
