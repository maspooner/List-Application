using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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
				c.MouseDown += Box_MouseDown;
				c.Background = Brushes.LightGray;

				Label r = new Label();
				DockPanel.SetDock(r, Dock.Right);
				r.Cursor = Cursors.SizeWE;
				r.Width = 5;
				r.Name = iti.Name + "_r";
				r.MouseDown += BoxStretch_MouseDown;
				r.Background = Brushes.Gray;
				Label b = new Label();
				DockPanel.SetDock(b, Dock.Bottom);
				b.Cursor = Cursors.SizeNS;
				b.Height = 5;
				b.Name = iti.Name + "_b";
				b.MouseDown += BoxStretch_MouseDown;
				b.Background = Brushes.Gray;

				comp.Children.Add(b);
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
						e.Background = Brushes.Chocolate;
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
		private void Move(ItemTemplateItem oIti, int cDest, int rDest) {
			Console.WriteLine("move");
			foreach (ItemTemplateItem iti in items) {
				if (iti != oIti) {
					foreach (Location l in iti.Occupied) {
						if (l.X == cDest && l.Y == rDest) {
							return;
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
		}
		private void Resize(ItemTemplateItem oIti, bool vertical, int cDest, int rDest) {
			//TODO
			if (vertical) {
				if (cDest == oIti.X && rDest > oIti.Y) {
					Console.WriteLine("valid");
					int dh = rDest - oIti.Y;
					Console.WriteLine("dh: " + dh);
					if (dh > 0) {
						bool space = true;
						for (int i = 0; i < oIti.Width; i++) {
							for (int j = 0; j < dh; j++) {
								int c = oIti.X + i;
								int r = oIti.Y + oIti.Width + j;
								foreach (ItemTemplateItem iti in items) {
									foreach (Location l in iti.Occupied) {
										if (l.X == c && l.Y == j) {
											space = false;
											goto Filled;
										}
									}
								}
							}
						}
					Filled:
						if (space) {
							//begin growth
							for (int i = 0; i < oIti.Width; i++) {
								for (int j = 0; j < dh; j++) {
									int c = oIti.X + i;
									int r = oIti.Y + oIti.Width + j;
									layoutContent.Children.Remove(layoutContent.FindAt(c, r));
								}
							}
							oIti.Resize(oIti.Height + dh, oIti.Width);
							Grid.SetRowSpan(register[oIti.Name], oIti.Height + dh);
						}
					}
					else {
						//don't check for space taken up by others
					}
				}
			}
			else {
				if (rDest == oIti.Y && cDest > oIti.X) {
					Console.WriteLine("valid");
				}
			}
		}
		private void EmptyBox_Drop(object sender, DragEventArgs e) {
			int cDest = Grid.GetColumn(sender as Label);
			int rDest = Grid.GetRow(sender as Label);
			if (e.Data.GetDataPresent("moveData")) {
				Console.WriteLine("move");
				ItemTemplateItem oIti = e.Data.GetData("moveData") as ItemTemplateItem;
				Move(e.Data.GetData("moveData") as ItemTemplateItem, cDest, rDest);
				e.Handled = true;
			}
			else if (e.Data.GetDataPresent("stretchData")) {
				StretchData sd = e.Data.GetData("stretchData") as StretchData;
				Resize(sd.Iti, sd.Vertical, cDest, rDest);
				e.Handled = true;
			}
		}
		private void Box_MouseDown(object sender, MouseEventArgs e) {
			string name = (sender as Label).Name;
			name = name.Substring(0, name.Length - 2);
			DataObject dragData = new DataObject("moveData", items.Find(iti => iti.Name.Equals(name)));
			DragDrop.DoDragDrop(register[name], dragData, DragDropEffects.Move);
		}
		private void BoxStretch_MouseDown(object sender, MouseEventArgs e) {
			string id = (sender as Label).Name;
			string name = id.Substring(0, id.Length - 2);
			bool vertical = id[id.Length - 1] == 'b';
			DataObject dragData = new DataObject("stretchData", new StretchData(items.Find(iti => iti.Name.Equals(name)), vertical));
			DragDrop.DoDragDrop(register[name], dragData, DragDropEffects.Link);
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
	class StretchData {
		//members
		private ItemTemplateItem iti;
		private bool vertical;
		//constructors
		internal StretchData(ItemTemplateItem iti, bool vertical) {
			this.iti = iti;
			this.vertical = vertical;
		}
		//properties
		internal ItemTemplateItem Iti { get { return iti; } }
		internal bool Vertical { get { return vertical; } }
	}
}
