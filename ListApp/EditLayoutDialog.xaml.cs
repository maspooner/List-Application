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
			//TODO
			for(int i = 0; i < template.Count; i++) {
				ItemTemplateItem iti = template[i];
				Color cBack = Utils.RandomColor(rand);
				SolidColorBrush back = new SolidColorBrush(cBack);
				
				Label c = new Label();
				c.HorizontalContentAlignment = HorizontalAlignment.Center;
				c.VerticalContentAlignment = VerticalAlignment.Center;
				c.Content = i + 1;
				c.FontSize = 16;
				c.Foreground = Utils.ShouldUseWhite(cBack) ? Brushes.White : Brushes.Black;
				c.FontWeight = FontWeights.Bold;
				c.Background = back;
				c.Name = iti.Name;

				Grid.SetColumn(c, iti.X);
				Grid.SetRow(c, iti.Y);
				Grid.SetColumnSpan(c, iti.Width);
				Grid.SetRowSpan(c, iti.Height);

				Console.WriteLine(iti.X + " " + iti.Y);
				layoutContent.Children.Add(c);
				register.Add(iti.Name, c);

				nameList.Items.Add(iti.Name);
			}
			nameList.SelectedIndex = 0;
			Console.WriteLine(layoutContent.ColumnDefinitions.Count);
			Console.WriteLine(layoutContent.RowDefinitions.Count);
		}
		private bool Move(ItemTemplateItem oIti, int cDest, int rDest) {
			Console.WriteLine("move");
			if(isBlockFree(cDest, rDest, oIti.Width, oIti.Height, oIti)) {
				oIti.Move(cDest, rDest);
				Grid.SetColumn(register[oIti.Name], cDest);
				Grid.SetRow(register[oIti.Name], rDest);
				return true;
			}
			else {
				return false;
			}
		}
		private bool Resize(ItemTemplateItem oIti, bool isWidth, int newDim) {
			//TODO
			if(isWidth) {
				int dw = newDim - oIti.Width;
				if (dw == 0) return true;
				if (dw < 0 || isBlockFree(oIti.X + oIti.Width, oIti.Y, dw, oIti.Height, oIti)) {
					Grid.SetColumnSpan(register[oIti.Name], oIti.Width + dw);
					oIti.Resize(oIti.Width + dw, oIti.Height);
					return true;
				}
			}
			else {
				int dh = newDim - oIti.Height;
				if (dh == 0) return true;
				if (dh < 0 || isBlockFree(oIti.X, oIti.Y + oIti.Height, oIti.Width, dh, oIti)) {
					Grid.SetRowSpan(register[oIti.Name], oIti.Height + dh);
					oIti.Resize(oIti.Width, oIti.Height + dh);
					return true;
				}
			}
			return false;
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
		private void LayoutContent_SizeChanged(object sender, SizeChangedEventArgs e) {
			double width = layoutContent.ColumnDefinitions[0].ActualWidth;
			foreach (RowDefinition rd in layoutContent.RowDefinitions) {
				rd.MinHeight = width;
			}
		}
		private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}

		private void nameList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ListView lw = sender as ListView;
			ItemTemplateItem iti = items[lw.SelectedIndex];
			xIn.Text = iti.X.ToString();
			yIn.Text = iti.Y.ToString();
			wIn.Text = iti.Width.ToString();
			hIn.Text = iti.Height.ToString();
			xIn.Focus();
			xIn.SelectAll();
		}

		private void window_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key.Equals(Key.Down)) {
				nameList.SelectedIndex += nameList.SelectedIndex == nameList.Items.Count - 1 ? 0 : 1;
				nameList.ScrollIntoView(nameList.SelectedItem);
			}
			else if(e.Key.Equals(Key.Up)) {
				nameList.SelectedIndex -= nameList.SelectedIndex == 0 ? 0 : 1;
				nameList.ScrollIntoView(nameList.SelectedItem);
			}
		}

		private void textIn_GotFocus(object sender, RoutedEventArgs e) {
			TextBox tb = sender as TextBox;
			tb.SelectAll();
		}

		private void textIn_LostFocus(object sender, RoutedEventArgs e) {
			TextBox tb = sender as TextBox;
			int dim = 0;
			bool isNum = int.TryParse(tb.Text, out dim);
			ItemTemplateItem iti = items[nameList.SelectedIndex];
            if (tb.Name.StartsWith("x")) {
				if (!isNum || !Move(iti, dim, iti.Y)) {
					tb.Text = iti.X.ToString();
				}
			}
			else if (tb.Name.StartsWith("y")) {
				if (!isNum || !Move(iti, iti.X, dim)) {
					tb.Text = iti.Y.ToString();
				}
			}
			else if (tb.Name.StartsWith("w")) {
				if (!isNum || !Resize(iti, true, dim)) {
					tb.Text = iti.Width.ToString();
				}
			}
			else if (tb.Name.StartsWith("h")) {
				if (!isNum || !Resize(iti, false, dim)) {
					tb.Text = iti.Height.ToString();
				}
			}
		}
	}
}
