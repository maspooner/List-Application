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
		private Dictionary<string, Space> copySpaces;
		private Random rand;
		//constructors
		public EditLayoutDialog(MainWindow mw) {
			InitializeComponent();
			this.mainWindow = mw;
			register = new Dictionary<string, FrameworkElement>();
			copySpaces = new Dictionary<string, Space>();
			rand = new Random(C.COLOR_SEED);
		}
		//methods
		internal Dictionary<string, Space> ShowAndGetTemplate(Dictionary<string, FieldTemplateItem> template) {
			Owner = mainWindow;
			foreach(string fieldName in template.Keys) {
				copySpaces.Add(fieldName, new Space(template[fieldName].Space));
			}
			CreateElements();
			ShowDialog();
			
			foreach(Space sp in copySpaces.Values) {
				Console.WriteLine(sp.X + " " + sp.Y);
			}
			return DialogResult.Value ? copySpaces : null; 
		}
		private void CreateElements() {
			Utils.SetupContentGrid(layoutContent, copySpaces.Values);
			//TODO
			int num = 1;
			foreach(string fieldName in copySpaces.Keys) {
				Space sp = copySpaces[fieldName];
				Color cBack = Utils.RandomColor(rand);
				SolidColorBrush back = new SolidColorBrush(cBack);

				Label c = new Label();
				c.HorizontalContentAlignment = HorizontalAlignment.Center;
				c.VerticalContentAlignment = VerticalAlignment.Center;
				c.Content = num;
				c.FontSize = 16;
				c.Foreground = Utils.ShouldUseWhite(cBack) ? Brushes.White : Brushes.Black;
				c.FontWeight = FontWeights.Bold;
				c.Background = back;

				Grid.SetColumn(c, sp.X);
				Grid.SetRow(c, sp.Y);
				Grid.SetColumnSpan(c, sp.Width);
				Grid.SetRowSpan(c, sp.Height);

				Console.WriteLine(sp.X + " " + sp.Y);
				layoutContent.Children.Add(c);
				register.Add(fieldName, c);

				nameList.Items.Add(fieldName);
				num++;
			}
			nameList.SelectedIndex = 0;
			Console.WriteLine(layoutContent.ColumnDefinitions.Count);
			Console.WriteLine(layoutContent.RowDefinitions.Count);
		}
		private bool Move(string fieldName, int cDest, int rDest) {
			Space currSpace = copySpaces[fieldName];
			Console.WriteLine("move");
			if(isSpaceFor(new Space(cDest, rDest, currSpace.Width, currSpace.Height), currSpace)) {
				currSpace.X = cDest;
				currSpace.Y = rDest;
				Grid.SetColumn(register[fieldName], cDest);
				Grid.SetRow(register[fieldName], rDest);
				return true;
			}
			else {
				return false;
			}
		}
		private bool Resize(string fieldName, bool isWidth, int newDim) {
			Space currSpace = copySpaces[fieldName];
			//TODO
			if(isWidth) {
				int dw = newDim - currSpace.Width;
				if (dw == 0) return true;
				//if (dw < 0 || isBlockFree(oIti.X + oIti.Width, oIti.Y, dw, oIti.Height, oIti)) {
				//	Grid.SetColumnSpan(register[oIti.Name], oIti.Width + dw);
				//	oIti.Resize(oIti.Width + dw, oIti.Height);
				//	return true;
				//}
				if (dw < 0 || isSpaceFor(new Space(currSpace.X, currSpace.Y, dw + currSpace.Width, currSpace.Height), currSpace)) {
					Grid.SetColumnSpan(register[fieldName], currSpace.Width + dw);
					currSpace.Width += dw;
					return true;
				}
			}
			else {
				int dh = newDim - currSpace.Height;
				if (dh == 0) return true;
				//if (dh < 0 || isBlockFree(oIti.X, oIti.Y + oIti.Height, oIti.Width, dh, oIti)) {
				//	Grid.SetRowSpan(register[oIti.Name], oIti.Height + dh);
				//	oIti.Resize(oIti.Width, oIti.Height + dh);
				//	return true;
				//}
				if (dh < 0 || isSpaceFor(new Space(currSpace.X, currSpace.Y, currSpace.Width, dh + currSpace.Height), currSpace)) {
					Grid.SetRowSpan(register[fieldName], currSpace.Height + dh);
					currSpace.Height += dh;
					return true;
				}
			}
			return false;
		}
		private bool isSpaceFor(Space newSpace, Space currentSpace) {
			foreach (Space sp in copySpaces.Values) {
				if(sp != currentSpace) {
					if (sp.Intersects(newSpace)) {
						return false;
					}
				}
			}
			return true;
		}
		//private bool isBlockFree(int x, int y, int w, int h, FieldTemplateItem thisIti) {

		//	bool isSpace = true;
		//	for (int cx = 0; cx < w && isSpace; cx++) {
		//		for (int cy = 0; cy < h && isSpace; cy++) {
		//			int c = x + cx;
		//			int r = y + cy;
		//			isSpace = IsSpaceFree(c, r, thisIti) && c < C.FIELD_GRID_WIDTH && r < layoutContent.RowDefinitions.Count;
		//		}
		//	}
		//	return isSpace;
		//}
		//private bool IsSpaceFree(int c, int r, FieldTemplateItem thisIti) {
		//	foreach (FieldTemplateItem iti in items) {
		//		if(thisIti == null || thisIti != iti) {
		//			foreach (Location l in iti.Occupied) {
		//				if (l.X == c && l.Y == r) {
		//					return false;
		//				}
		//			}
		//		}
		//	}
		//	return true;
		//}
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
			Space sp = copySpaces[lw.SelectedValue as string];
			//FieldTemplateItem iti = copySpaces[lw.SelectedIndex];
			xIn.Text = sp.X.ToString();
			yIn.Text = sp.Y.ToString();
			wIn.Text = sp.Width.ToString();
			hIn.Text = sp.Height.ToString();
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
			//FieldTemplateItem iti = copySpaces[nameList.SelectedIndex];
			string fieldName = nameList.SelectedValue as string;
            Space sp = copySpaces[fieldName];
			if (tb.Name.StartsWith("x")) {
				if (!isNum || !Move(fieldName, dim, sp.Y)) {
					tb.Text = sp.X.ToString();
				}
			}
			else if (tb.Name.StartsWith("y")) {
				if (!isNum || !Move(fieldName, sp.X, dim)) {
					tb.Text = sp.Y.ToString();
				}
			}
			else if (tb.Name.StartsWith("w")) {
				if (!isNum || !Resize(fieldName, true, dim)) {
					tb.Text = sp.Width.ToString();
				}
			}
			else if (tb.Name.StartsWith("h")) {
				if (!isNum || !Resize(fieldName, false, dim)) {
					tb.Text = sp.Height.ToString();
				}
			}
		}
	}
}
