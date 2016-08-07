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
		private Random rand;
		//constructors
		public EditLayoutDialog(MainWindow mw) {
			InitializeComponent();
			this.mainWindow = mw;
			register = new Dictionary<string, FrameworkElement>();
			rand = new Random(C.COLOR_SEED);
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
			layoutContent.Drop += Box_Drop;
			//TODO
			for(int i = 0; i < template.Count; i++) {
				ItemTemplateItem iti = template[i];
				Color cBack = Utils.RandomColor(rand);
				SolidColorBrush back = new SolidColorBrush(cBack);

				DockPanel comp = new DockPanel();
				
				Label c = new Label();
				c.HorizontalContentAlignment = HorizontalAlignment.Center;
				c.VerticalContentAlignment = VerticalAlignment.Center;
				c.Content = i + 1;
				c.FontSize = 16;
				c.Foreground = Utils.ShouldUseWhite(cBack) ? Brushes.White : Brushes.Black;
				c.FontWeight = FontWeights.Bold;
				c.Name = iti.Name + "_c";
				c.MouseDown += Box_MouseDown;
				c.Background = back;

				Label r = new Label();
				DockPanel.SetDock(r, Dock.Right);
				r.Cursor = Cursors.SizeWE;
				r.Width = 5;
				r.Name = iti.Name + "_r";
				r.MouseDown += BoxStretch_MouseDown;
				r.Background = back;
				Label b = new Label();
				DockPanel.SetDock(b, Dock.Bottom);
				b.Cursor = Cursors.SizeNS;
				b.Height = 5;
				b.Name = iti.Name + "_b";
				b.MouseDown += BoxStretch_MouseDown;
				b.Background = back;

				comp.Children.Add(b);
				comp.Children.Add(r);
				comp.Children.Add(c);

				comp.Name = iti.Name;
				comp.AllowDrop = true;

				Grid.SetColumn(comp, iti.X);
				Grid.SetRow(comp, iti.Y);
				Grid.SetColumnSpan(comp, iti.Width);
				Grid.SetRowSpan(comp, iti.Height);

				Console.WriteLine(iti.X + " " + iti.Y);
				layoutContent.Children.Add(comp);
				register.Add(iti.Name, comp);
			}
			for(int i = 0; i < layoutContent.ColumnDefinitions.Count; i++) {
				for(int j = 0; j < layoutContent.RowDefinitions.Count; j++) {
					if (IsSpaceFree(i, j)) {
						Label e = new Label();
						e.Background = Brushes.Chocolate;
						Grid.SetColumn(e, i);
						Grid.SetRow(e, j);
						e.AllowDrop = true;
						e.Drop += Box_Drop;
						layoutContent.Children.Add(e);
					}
				}
			}
			Console.WriteLine(layoutContent.ColumnDefinitions.Count);
			Console.WriteLine(layoutContent.RowDefinitions.Count);
		}
		private void Move(ItemTemplateItem oIti, int cDest, int rDest) {
			Console.WriteLine("move");
			if(isBlockFree(cDest, rDest, oIti.Width, oIti.Height, oIti)) {
				//clear to move
				for (int i = 0; i < oIti.Width; i++) {
					for (int j = 0; j < oIti.Height; j++) {
						//move single blocks to old spot
						FrameworkElement fe = layoutContent.FindAt(cDest + i, rDest + j);
						if(fe != null) {
							Grid.SetColumn(fe, oIti.X + i);
							Grid.SetRow(fe, oIti.Y + j);
						}
					}
				}
				oIti.Move(cDest, rDest);
				Grid.SetColumn(register[oIti.Name], cDest);
				Grid.SetRow(register[oIti.Name], rDest);
			}
		}
		private void Resize(ItemTemplateItem oIti, bool vertical, int cDest, int rDest) {
			//TODO
			if (vertical) {
				if(rDest > oIti.Y) {
					int dh = rDest - oIti.Y - oIti.Height + 1;
					if(dh > 0) {
						if (isBlockFree(oIti.X, oIti.Y + oIti.Height, oIti.Width, dh, oIti)) {
							for (int cx = 0; cx < oIti.Width; cx++) {
								for (int cy = 0; cy < dh; cy++) {
									int c = oIti.X + cx;
									int r = oIti.Y + oIti.Height + cy;
									layoutContent.Children.Remove(layoutContent.FindAt(c, r));
								}
							}
							Grid.SetRowSpan(register[oIti.Name], oIti.Height + dh);
							oIti.Resize(oIti.Width, oIti.Height + dh);
						}
					}
				}
			}
			else {
				if(cDest > oIti.X) {
					int dw = cDest - oIti.X - oIti.Width + 1;
					if(dw > 0) {
						if(isBlockFree(oIti.X + oIti.Width, oIti.Y, dw, oIti.Height, oIti)) {
							for (int cx = 0; cx < dw; cx++) {
								for (int cy = 0; cy < oIti.Height; cy++) {
									int c = oIti.X + oIti.Width + cx;
									int r = oIti.Y + cy;
									layoutContent.Children.Remove(layoutContent.FindAt(c, r));
								}
							}
							Grid.SetColumnSpan(register[oIti.Name], oIti.Width + dw);
							oIti.Resize(oIti.Width + dw, oIti.Height);
						}
					}
				}
			}
		}
		private bool isBlockFree(int x, int y, int w, int h, ItemTemplateItem thisIti) {
			bool isSpace = true;
			for (int cx = 0; cx < w && isSpace; cx++) {
				for (int cy = 0; cy < h && isSpace; cy++) {
					int c = x + cx;
					int r = y + cy;
					isSpace = IsSpaceFree(c, r, thisIti) && c < C.FIELD_GRID_WIDTH && r < layoutContent.RowDefinitions.Count;
				}
			}
			return isSpace;
		}
		private bool IsSpaceFree(int c, int r, ItemTemplateItem thisIti = null) {
			foreach (ItemTemplateItem iti in items) {
				if(thisIti == null || thisIti != iti) {
					foreach (Location l in iti.Occupied) {
						if (l.X == c && l.Y == r) {
							return false;
						}
					}
				}
			}
			return true;
		}
		private void Box_Drop(object sender, DragEventArgs e) {
			int cDest = Grid.GetColumn(sender as FrameworkElement);
			int rDest = Grid.GetRow(sender as FrameworkElement);
			if (e.Data.GetDataPresent("moveData")) {
				Console.WriteLine("move");
				ItemTemplateItem oIti = e.Data.GetData("moveData") as ItemTemplateItem;
				Move(oIti, cDest, rDest);
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
