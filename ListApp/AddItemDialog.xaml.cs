using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

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
							data[i] = (field as BackupImage).GetBackup();
							break;
						case ItemType.NUMBER:
							data[i] = (field as NumberTextBox).ParseValue();
							break;
						case ItemType.DECIMAL:
							data[i] = (field as DecimalTextBox).ParseValue();
							break;
						default:
							throw new NotImplementedException();
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
						break;
					case ItemType.NUMBER:
						if (iti.Metadata == null) {
							fe = new NumberTextBox();
						}
						else {
							object[] fields = iti.Metadata as object[];
							fe = new NumberTextBox((int)fields[0], (int)fields[1]);
						}
                        break;
					case ItemType.DECIMAL:
						if (iti.Metadata == null) {
							fe = new DecimalTextBox();
						}
						else {
							object[] fields = iti.Metadata as object[];
							if(fields.Length == 1) {
								fe = new DecimalTextBox((int)fields[0]);
							}
							else if (fields.Length == 3) {
								fe = new DecimalTextBox((int)fields[0], (float)fields[1], (float)fields[2]);
							}
							else {
								throw new NotSupportedException();
							}
						}
						break;
					case ItemType.DATE:
						DatePicker dp = new DatePicker();
						dp.SelectedDateFormat = DatePickerFormat.Short;
						dp.SelectedDate = li == null ? DateTime.Today : (DateTime)li[i].Value;
						fe = dp;
						break;
					case ItemType.ENUM:
						ComboBox cb = new ComboBox();
						cb.ItemsSource = iti.Metadata as string[];
						cb.SelectedIndex = li == null ? 0 : (int)li[i].Value;
						fe = cb;
						break;
					case ItemType.IMAGE:
						BackupImage bi = new BackupImage();
						if(li != null) {
							bi.SetSourceAndBackup((li[i] as ImageField).Value as XImage);
						}
						fe = bi;
						break;
					default:
						throw new NotImplementedException();
				}
				if(fe is TextBox) {
					if (li != null) {
						(fe as TextBox).Text = li[i].Value.ToString();
					}
				}
				register.Add(iti.Name + "_ui", fe);
				switch (iti.Type) {
					case ItemType.BASIC:
					case ItemType.DATE:
					case ItemType.NUMBER:
					case ItemType.DECIMAL:
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
					default:
						throw new NotImplementedException();
				}
			}
		}
		private bool IsValidInput() {
			foreach (FrameworkElement fe in register.Values) {
				if(fe is NumberTextBox) {
					if(!(fe as NumberTextBox).IsValid()) {
						return false;
					}
				}
				else if(fe is DecimalTextBox) {
					if (!(fe as DecimalTextBox).IsValid()) {
						return false;
					}
				}
				else if (fe is TextBox) {
					if ((fe as TextBox).Text.Equals("")) {
						return false;
					}
				}
				else if (fe is DatePicker) {
					if ((fe as DatePicker).SelectedDate == null) {
						return false;
					}
				}
				else if (fe is BackupImage) {
					if((fe as BackupImage).GetBackup() == null) {
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
				XImage xi = new XImage(ofd.FileName, false);
				string fieldName = bb.Name.Substring(0, bb.Name.Length - 3);
                BackupImage img = register[fieldName + "_ui"] as BackupImage;
				img.SetSourceAndBackup(xi);
				Label lab = register[fieldName + "_lab"] as Label;
				lab.Content = ofd.FileName.Substring(ofd.FileName.LastIndexOf('\\'));
			}
		}
		private void ClearButton_Click(object sender, RoutedEventArgs e) {
			Button cb = sender as Button;
			string fieldName = cb.Name.Substring(0, cb.Name.Length - 4);
			BackupImage img = register[fieldName + "_ui"] as BackupImage;
			img.SetSourceAndBackup(null);
			Label lab = register[fieldName + "_lab"] as Label;
			lab.Content = "<No file>";
		}
	}
}
