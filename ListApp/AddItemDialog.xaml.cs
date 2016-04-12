using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using CImage = System.Windows.Controls.Image;

namespace ListApp {
	public partial class AddItemDialog : Window {
		//methods
		private MainWindow mainWindow;
		private Dictionary<string, FrameworkElement> register;
		//constructors
		public AddItemDialog(MainWindow mw) {
			mainWindow = mw;
			register = new Dictionary<string, FrameworkElement>();
			InitializeComponent();
		}
		//methods
		internal object[] ShowAndGetItem(List<ItemTemplateItem> template, ListItem li = null) {
			Owner = mainWindow;
			CreateElements(template, li);
			ShowDialog();
			Console.WriteLine("CLOSED DIALOG");
			if (DialogResult.Value) {
				Console.WriteLine("OK");
				//TODO implement
				object[] data = new object[template.Count];
				for(int i = 0; i < template.Count; i++) {
					ItemTemplateItem iti = template[i];
					FrameworkElement field = register[iti.Name + "_ui"];
					switch (iti.Type) {
						case ItemType.BASIC:
							data[i] = (field as TextBox).Text;
							break;
						case ItemType.DATE:
							data[i] = (field as DatePicker).SelectedDate;
							break;
						case ItemType.ENUM:
							data[i] = (field as ComboBox).SelectedIndex;
							break;
						case ItemType.IMAGE:
							data[i] = (field as CImage).Source as System.Windows.Media.Imaging.BitmapImage;
							break;
					}
				}
				return data;
			}
			else {
				return null;
			}
		}
		private void CreateElements(List<ItemTemplateItem> template, ListItem li) {
			for (int i = 0; i < template.Count; i++) {
				ItemTemplateItem iti = template[i];
				FrameworkElement fe = null;
				switch (iti.Type) {
					case ItemType.BASIC:
						fe = new TextBox();
						if(li != null) {
							(fe as TextBox).Text = li[i].GetValue().ToString();
						}
						break;
					case ItemType.DATE:
						DatePicker dp = new DatePicker();
						dp.SelectedDateFormat = DatePickerFormat.Short;
						dp.SelectedDate = li == null ? DateTime.Today : (DateTime)li[i].GetValue();
						fe = dp;
						break;
					case ItemType.ENUM:
						ComboBox cb = new ComboBox();
						cb.ItemsSource = iti.Metadata as string[];
						cb.SelectedIndex = li == null ? 0 : (int)li[i].GetValue();
						fe = cb;
						break;
					case ItemType.IMAGE:
						fe = new CImage();
						if(li != null) {
							(fe as CImage).Source = (li[i] as ImageField).GetBitmap();
						}
						break;
				}
				register.Add(iti.Name + "_ui", fe);
				switch (iti.Type) {
					case ItemType.BASIC:
					case ItemType.DATE:
					case ItemType.ENUM:
						DockPanel dp = new DockPanel();
						Label l = new Label();
						l.Content = iti.Name + ": ";
						dp.Children.Add(l);
						DockPanel.SetDock(l, Dock.Left);
						dp.Children.Add(fe);
						DockPanel.SetDock(fe, Dock.Right);
						contentPanel.Children.Add(dp);
						break;
					case ItemType.IMAGE:
						Button browse = new Button();
						browse.Content = "Browse...";
						browse.Click += BrowseButton_Click;
						browse.Name = iti.Name + "_bb";
						Button clear = new Button();
						clear.Content = "Clear";
						clear.Name = iti.Name + "_clr";
						clear.Click += ClearButton_Click;
						Label file = new Label();
						file.Content = li != null ? "<Cached file>" : "<No file>";
						register.Add(iti.Name + "_lab", file);

						Grid g = new Grid();
						g.ColumnDefinitions.Add(new ColumnDefinition());
						g.ColumnDefinitions.Add(new ColumnDefinition());
						g.RowDefinitions.Add(new RowDefinition());
						g.RowDefinitions.Add(new RowDefinition());
						file.SetValue(Grid.ColumnProperty, 0);
						file.SetValue(Grid.RowProperty, 0);
						browse.SetValue(Grid.ColumnProperty, 1);
						browse.SetValue(Grid.RowProperty, 0);
						clear.SetValue(Grid.ColumnProperty, 0);
						clear.SetValue(Grid.RowProperty, 1);
						fe.SetValue(Grid.ColumnProperty, 1);
						fe.SetValue(Grid.RowProperty, 1);
						g.Children.Add(browse);
						g.Children.Add(clear);
						g.Children.Add(file);
						g.Children.Add(fe);
						contentPanel.Children.Add(g);
						break;
				}
			}
		}
		private bool IsValidInput() {
			foreach (FrameworkElement fe in register.Values) {
				if (fe is TextBox) {
					if ((fe as TextBox).Text.Equals("")) {
						return false;
					}
				}
				else if (fe is DatePicker) {
					if ((fe as DatePicker).SelectedDate == null) {
						return false;
					}
				}
				else if (fe is CImage) {
					if((fe as CImage).Source == null) {
						return false;
					}
				}
			}
			return true;
		}
		//WPF
		private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
			if (IsValidInput()) {
				DialogResult = true;
			}
			else {
				//TODO highlight missing fields
			}
		}
		private void BrowseButton_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog ofd = new OpenFileDialog();
			Button bb = sender as Button;
			ofd.DefaultExt = ".png";
			ofd.Filter = "Image Files|*.jpeg;*.png;*.jpg;*.gif|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";
			if(ofd.ShowDialog() == true) {
				Bitmap b = new Bitmap(Bitmap.FromFile(ofd.FileName) as Bitmap, new System.Drawing.Size(300, 100)); //TODO adjustable
				string fieldName = bb.Name.Substring(0, bb.Name.Length - 3);
                CImage img = register[fieldName + "_ui"] as CImage;
				img.Source = b.ConvertToBitmapImage();
				Label lab = register[fieldName + "_lab"] as Label;
				lab.Content = ofd.FileName.Substring(ofd.FileName.LastIndexOf('\\'));
			}
		}
		private void ClearButton_Click(object sender, RoutedEventArgs e) {
			Button cb = sender as Button;
			string fieldName = cb.Name.Substring(0, cb.Name.Length - 4);
			CImage img = register[fieldName + "_ui"] as CImage;
			img.Source = null;
			Label lab = register[fieldName + "_lab"] as Label;
			lab.Content = "<No file>";
		}
	}
}
