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
	 * FIXME major refactoring of every file
	 * TODO number of backups to set after every major sync operation
	 * TODO disable edits when updating lists
	 */
	public partial class MainWindow : Window {
		//members
		private GridLength lastHeight, lastWidth;
		private ListData data;
		private int shownList;
		private SyncManager syncManager;
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

			//data = ListData.LoadTestLists();
			//data = ListData.Load();
			data = ListData.RecoverLists();
			ReloadMListList();
			DisplayList(0);
			itemsMenu = new ContextMenu();
			itemsMenu.Items.Add("Edit");
			itemsMenu.Items.Add("Reorder");
			itemsMenu.Items.Add("Refresh");
			itemsMenu.Items.Add("Delete");
			//TODO commands
			Console.WriteLine(data);
			data.Save();
			autoSaveThread = new Thread(AutoSaveThreadStart);
			autoSaveThread.Start();
		}
		//methods
		private void LoadImages() {
			generalOptionsImage.Source = Properties.Resources.optionIcon.ConvertToBitmapImage();
		}
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
		
		private Label CreateListLabel(MList list) {
			Label l = new Label();
			l.Content = list.Name;
			return l;
		}
		private void ReloadMListList() {
			leftPanel.Items.Clear();
			foreach (MList ml in data) {
				leftPanel.Items.Add(CreateListLabel(ml));
			}
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

		private void addNewImg_MouseUp(object sender, MouseButtonEventArgs e) {
			MList l = data[shownList];
			if (l.CanObserve()) {
				bool wasModification = new AddItemDialog().ShowDialogForItem(this, l);
				if (wasModification) {
					Refresh();
				}
			}
		}
		
		private void listOptionImg_MouseUp(object sender, MouseButtonEventArgs e) {
			if (data[shownList].CanObserve()) {
				Dictionary<string, Space> newSpaces = new EditLayoutDialog(this).ShowAndGetTemplate(data[shownList].Template);
				if (newSpaces != null) {
					foreach (string fieldName in newSpaces.Keys) {
						data[shownList].Template[fieldName].Space = newSpaces[fieldName];
					}
					DisplayItem(listItemGrid.SelectedIndex);
				}
			}
		}
		
		/// <summary>
		/// Displays the list with the specified ID into the listItemGrid
		/// If the list is not obvervable, a screen specifying so is displayed
		/// </summary>
		private void DisplayList(int id) {
			if (data.Count > 0 && data[id].CanObserve()) {
				listItemGrid.Visibility = Visibility.Visible;
				noListsLabel.Visibility = Visibility.Collapsed;
				MList list = data[id];
				//change the action bar's actions to match the current list
				// (and remove it if the list is not observable)
				ReloadActionBar(list);
				//load the items from this list
				listItemGrid.ItemsSource = list.Items;
				//setup columns
				listItemGrid.Columns.Clear();
				foreach (string fieldName in list.Template.Keys) {
					listItemGrid.Columns.Add(DefineColumn(fieldName, list.Template[fieldName]));
				}
			}
			else {
				listItemGrid.Visibility = Visibility.Collapsed;
				noListsLabel.Visibility = Visibility.Visible;
				ReloadActionBar(null);
				//clear the items there
				listItemGrid.ItemsSource = null;
				//display the list is currently inaccessable
				noListsLabel.Content = data.Count > 0 ? "Currently Syncing" : "No lists!";
			}
			if(data.Count > 0) {
				//update shown list
				shownList = id;
				//try to display the first item
				DisplayItem(0);
			}
		}
		private void DisplayItem(int i) {
			itemContentPanel.ClearGrid();
			MList ml = data[shownList];
			if(ml.Count == 0 || i == -1) {
				itemContentPanel.Visibility = Visibility.Collapsed;
				noItemsLabel.Visibility = Visibility.Visible;
				noItemsLabel.Content = i == -1 ? "No Item Selected" : "No Items!";
			}
			else {
				itemContentPanel.Visibility = Visibility.Visible;
				noItemsLabel.Visibility = Visibility.Collapsed;
				MItem item = ml[i];
				Utils.SetupContentGrid(itemContentPanel, ml.Template.Values.Select(fti => fti.Space));
				//TODO
				foreach (string fieldName in ml.Template.Keys) {
					MField lif = item[fieldName];
					FrameworkElement fe = null;
					FieldTemplateItem fti = ml.Template[fieldName];
					if (lif.FieldType.Equals(FieldType.IMAGE)) {
						fe = new CImage();
						(fe as CImage).Source = (lif as ImageField).ToVisibleValue(fti.Metadata)
							as System.Windows.Media.Imaging.BitmapImage;
					}
					//else if (lif is EnumField) {
					//	fe = new Label();
					//	(fe as Label).Content = (lif as EnumField).GetSelectedValue(fti.Metadata as EnumMetadata);
					//}
					else {
						fe = new Label();
						(fe as Label).Content = lif.Value == null ? "" : lif.ToVisibleValue(fti.Metadata).ToString();
					}
					Grid.SetColumn(fe, fti.X);
					Grid.SetRow(fe, fti.Y);
					Grid.SetColumnSpan(fe, fti.Width);
					Grid.SetRowSpan(fe, fti.Height);
					itemContentPanel.Children.Add(fe);
				}
			}
		}
		private void ReloadActionBar(MList list) {
			listActionBar.Children.Clear();
			if (list != null && list.CanObserve()) {
				Label title = new Label();
				title.Content = list.Name;
				DockPanel.SetDock(title, Dock.Left);
				listActionBar.Children.Add(title);

				listActionBar.Children.Add(CreateActionImage(Properties.Resources.optionIcon.ConvertToBitmapImage(), listOptionImg_MouseUp));
				listActionBar.Children.Add(CreateActionImage(Properties.Resources.addIcon.ConvertToBitmapImage(), addNewImg_MouseUp));
				if (list is SyncList) {
					listActionBar.Children.Add(CreateActionImage(Properties.Resources.reloadIcon.ConvertToBitmapImage(), SyncActionButton_OnClick));
				}
			}
		}
		private void Refresh() {
			ListCollectionView lcv = CollectionViewSource.GetDefaultView(listItemGrid.ItemsSource) as ListCollectionView;
			lcv.CustomSort = null;
			foreach (DataGridColumn dgc in listItemGrid.Columns) {
				dgc.SortDirection = null;
			}
			listItemGrid.Items.Refresh();
		}
		private DataGridTemplateColumn DefineColumn(string fieldName, FieldTemplateItem fti) {
			//CImage img = new CImage();
			//img.BeginInit();
			//img.Source = (lif as ImageField).GetBitmap();
			//img.EndInit();
			//fe = img;
			DataGridTemplateColumn dgc = new DataGridTemplateColumn();
			Binding bind = new Binding();
			bind.Mode = BindingMode.OneWay;
			bind.ConverterParameter = new ConverterData() { Name = fieldName, Template = fti };
			Type uiType = typeof(TextBlock);
			switch (fti.Type) {
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
				fef.SetValue(CImage.MaxHeightProperty, (fti.Metadata as ImageMetadata).MaxHeight);
			}
			else {
				throw new NotImplementedException();
			}
			DataTemplate dataTemp = new DataTemplate();
			dataTemp.VisualTree = fef;
			dataTemp.DataType = typeof(DataGridTemplateColumn);
			dgc.CellTemplate = dataTemp;
			dgc.Header = fieldName;
			dgc.CanUserSort = true;
			dgc.SortMemberPath = fieldName;

			return dgc;
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




		//handlers
		private void MItemGrid_OnSort(object sender, DataGridSortingEventArgs e) {
			//don't pass the event to other handlers, we want to handle all the sorting
			e.Handled = true;
			//set the sorting direction to either Ascending or Descending
			ListSortDirection dir = e.Column.SortDirection != ListSortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
			e.Column.SortDirection = dir;

			ListCollectionView lcv = CollectionViewSource.GetDefaultView(listItemGrid.ItemsSource) as ListCollectionView;
			lcv.CustomSort = new ListItemComparer(e.Column.Header as string, dir);
		}
		private void SyncActionButton_OnClick(object sender, MouseButtonEventArgs e) {
			//TODO refreshments
			//start up the sync manager if not already up
			if (syncManager == null) {
				syncManager = new SyncManager(this, syncBar, messageLabel, syncCancel);
			}

			//syncManager.StartRefreshAllTask(data[shownList] as SyncList);
			bool started = syncManager.StartRefreshNewTask(data[shownList] as SyncList, shownList);

			//(l as XMLList).LoadValues("http://myanimelist.net/malappinfo.php?u=progressivespoon&status=all&type=anime", true);
			//(l as XmlList).LoadValues(@"F:\Documents\Visual Studio 2015\Projects\ListApp\al.xml");

			//clear the shown list
			if (started) {
				DisplayList(shownList);
			}
		}
		internal void SyncOver_Callback() {
			//refresh the list if it is the current one
			if(syncManager.SyncingList == shownList) {
				DisplayList(shownList);
			}
		}
		/// <summary>
		/// Called when user opts to cancel a scheduled task
		/// </summary>
		private void SyncCancelButton_OnClick(object sender, RoutedEventArgs e) {
			syncManager.CancelTask();
		}
		/// <summary>
		/// Called when the user changes the selected list
		/// </summary>
		private void ListNameListPanel_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ListView lv = sender as ListView;
			int selected = lv.SelectedIndex;
			//only work to display if not changing to the same list
            if (shownList != selected) {
				DisplayList(selected);
			}
		}
		/// <summary>
		/// Called when the window is closed
		/// </summary>
		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);
			done = true;
			//gracefully close the autosave thread and join it with this one
			autoSaveThread.Interrupt();
			autoSaveThread.Join();
		}
	}
}
