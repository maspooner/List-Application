using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ListApp {
	public partial class EditLayoutDialog : Window {
		//members
		private MainWindow mainWindow;
		private Dictionary<string, FrameworkElement> register;
		private List<ItemTemplateItem> items;
		//constructors
		public EditLayoutDialog(MainWindow mw) {
			InitializeComponent();
			this.mainWindow = mw;
			register = new Dictionary<string, FrameworkElement>();
		}
		//methods
		internal List<ItemTemplateItem> ShowAndGetTemplate(List<ItemTemplateItem> template) {
			Owner = mainWindow;
			this.items = new List<ItemTemplateItem>();
			foreach(ItemTemplateItem iti in template) {
				items.Add(new ItemTemplateItem(iti));
			}
			CreateElements(items);
			ShowDialog();
			foreach(ItemTemplateItem iti in items) {
				Console.WriteLine(iti.X + " " + iti.Y);
			}
			return DialogResult.Value ? items : null; 
		}
		private void CreateElements(List<ItemTemplateItem> template) {
			Utils.SetupContentGrid(layoutContent, template);
			layoutContent.Drop += EmptyBox_Drop;
			//TODO
			foreach (ItemTemplateItem iti in template) {
				DockPanel comp = new DockPanel();
				comp.Name = iti.Name;
				Grid.SetColumn(comp, iti.X);
				Grid.SetRow(comp, iti.Y);
				Grid.SetColumnSpan(comp, iti.Width);
				Grid.SetRowSpan(comp, iti.Height);
				
				Label c = new Label();
				c.HorizontalContentAlignment = HorizontalAlignment.Center;
				c.VerticalContentAlignment = VerticalAlignment.Center;
				c.Content = iti.Name + "\n(" + iti.Type + ")";
				c.Name = iti.Name + "_c";
				c.MouseMove += Box_MouseMove;
				c.Background = System.Windows.Media.Brushes.LightGray;

				Label r = new Label();
				DockPanel.SetDock(r, Dock.Right);
				r.Cursor = Cursors.SizeWE;
				r.Width = 5;
				r.Name = iti.Name + "_r";
				r.BorderThickness = new Thickness(0, 0, 3, 0);
				r.Background = System.Windows.Media.Brushes.Aqua;

				comp.Children.Add(r);
				comp.Children.Add(c);

				Console.WriteLine(iti.X + " " + iti.Y);
				layoutContent.Children.Add(comp);
				register.Add(iti.Name, comp);
			}
			for(int i = 0; i < layoutContent.ColumnDefinitions.Count; i++) {
				for(int j = 0; j < layoutContent.RowDefinitions.Count; j++) {
					bool empty = true;
					foreach (ItemTemplateItem iti in items) {
						foreach(Location l in iti.Occupied) {
							if (l.X == i && l.Y == j) {
								empty = false;
								goto Empty;
							}
						}
					}
					Empty:
					if (empty) {
						Label e = new Label();
						e.Background = System.Windows.Media.Brushes.Chocolate;
						Grid.SetColumn(e, i);
						Grid.SetRow(e, j);
						e.AllowDrop = true;
						e.Drop += EmptyBox_Drop;
						layoutContent.Children.Add(e);
					}
				}
			}
			Console.WriteLine(layoutContent.ColumnDefinitions.Count);
			Console.WriteLine(layoutContent.RowDefinitions.Count);
		}
		private void EmptyBox_Drop(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent("moveData")) {
				Console.WriteLine("move");
				int cDest = Grid.GetColumn(sender as Label);
				int rDest = Grid.GetRow(sender as Label);
				ItemTemplateItem oIti = e.Data.GetData("moveData") as ItemTemplateItem;
				foreach (ItemTemplateItem iti in items) {
					if(iti != oIti) {
						foreach (Location l in iti.Occupied) {
							if (l.X == cDest && l.Y == rDest) {
								goto Filled;
							}
						}
					}
				}
				//clear to move
				for (int i = 0; i < oIti.Width; i++) {
					for (int j = 0; j < oIti.Height; j++) {
						FrameworkElement fe = layoutContent.FindAt(cDest + i, rDest + j);
						Grid.SetColumn(fe, oIti.X + i);
						Grid.SetRow(fe, oIti.Y + j);
					}
				}
				oIti.Move(cDest, rDest);
				Grid.SetColumn(register[oIti.Name], cDest);
				Grid.SetRow(register[oIti.Name], rDest);
				Filled:
				e.Handled = true;
			}
		}
		private void Box_MouseMove(object sender, MouseEventArgs e) {
			if (e.LeftButton.Equals(MouseButtonState.Pressed)) {
				string name = (sender as Label).Name;
				name = name.Substring(0, name.Length - 2);
				DataObject dragData = new DataObject("moveData", items.Find(iti => iti.Name.Equals(name)));
				DragDrop.DoDragDrop(register[name], dragData, DragDropEffects.Move);
			}
		}
		private void LayoutContent_SizeChanged(object sender, SizeChangedEventArgs e) {
			double width = layoutContent.ColumnDefinitions[0].ActualWidth;
			foreach (RowDefinition rd in layoutContent.RowDefinitions) {
				rd.MinHeight = width;
			}
		}
		private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}
	}
}
