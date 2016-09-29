using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace ListApp {
	/// <summary>
	/// Models a dialog Window that can be shown to generate a new <seealso cref="MItem"/>
	/// </summary>
	public partial class AddItemDialog : Window {
		//constants
		private static string UI_ELEMENT_SUFFIX = "_ui";
		//members
		/// <summary>
		/// Holds references to all framework elements added, accessed by name
		/// </summary>
		private Dictionary<string, FrameworkElement> register;
		//constructors
		/// <summary>
		/// Creates new dialog to add a new ListItem
		/// </summary>
		/// <param name="mw">A reference to the MainWindow</param>
		public AddItemDialog() {
			register = new Dictionary<string, FrameworkElement>();

			InitializeComponent();
		}
		//methods
		/// <summary>
		/// Shows this dialog, and adds or modifies a <seealso cref="MItem"/>
		/// from the given <seealso cref="MList"/>.
		/// If a <seealso cref="MItem"/> is given, the item will be modified,
		/// otherwise, a new item will be created and added to the list.
		/// </summary>
		/// <param name="mainWindow">The main window</param>
		/// <param name="list">the list to add or modify</param>
		/// <param name="existingItem">(optional) an existing item to modify 
		/// instead of creating a new item</param>
		/// <returns>true if the element was modified or added, false if the dialog was canceled</returns>
		internal bool ShowDialogForItem(MainWindow mainWindow, MList list, MItem existingItem = null) {
			Owner = mainWindow;
			Dictionary<string, FieldTemplateItem> template = list.Template;
			//create the UI elements for editting fields
			CreateUIElements(template, existingItem);
			//show the dialog and wait for a close or ok
			ShowDialog();
			Console.WriteLine("CLOSED DIALOG");
			//ok button pressed
			if (DialogResult.Value) {
				//modify a new item if there is no existing item available
				MItem toModifyItem = existingItem == null ? list.Add() : existingItem;
				//for every field to add/modify
				foreach (string fieldName in template.Keys) {
					//find the ui element in the register with the content to parse
					FrameworkElement field = register[fieldName + UI_ELEMENT_SUFFIX];
					//set the field data to the parsed data from the ui element
					toModifyItem[fieldName].Value = ParseFieldData(field, template[fieldName].Type);
				}
				return true; //success
			}
			return false; //fail
		}
		/// <summary>
		/// Given a <seealso cref="FrameworkElement"/>, the data of a given <seealso cref="FieldType"/> is parsed
		/// </summary>
		/// <param name="uiElement">The ui element to parse data from</param>
		/// <param name="fieldType">The type of field</param>
		/// <returns>the data parsed from the field</returns>
		private object ParseFieldData(FrameworkElement uiElement, FieldType fieldType) {
			switch (fieldType) {
				case FieldType.BASIC:	return (uiElement as TextBox).Text;
				case FieldType.DATE:	return (uiElement as DatePicker).SelectedDate;
				case FieldType.ENUM:	return (uiElement as ComboBox).SelectedIndex;
				case FieldType.IMAGE:	return (uiElement as BackupImage).GetBackup();
				case FieldType.NUMBER:	return (uiElement as NumberTextBox).ParseValue();
				case FieldType.DECIMAL:	return (uiElement as DecimalTextBox).ParseValue();
				default:				throw new NotImplementedException();
			}
		}
		/// <summary>
		/// Creates and adds ui elements for each field in a <seealso cref="List{FieldTemplateItem}"/>
		/// (with possible starting values if a <seealso cref="MItem"/> is given).
		/// </summary>
		/// <param name="template">a list of template items to use to create ui elements</param>
		/// <param name="item">a possibly null value that will be used to fill in ui 
		/// elements with previous values</param>
		private void CreateUIElements(Dictionary<string, FieldTemplateItem> template, MItem item) {
			foreach (string fieldName in template.Keys) {
				FieldTemplateItem fti = template[fieldName];
				//create the main ui element for each template item
				//i.e. the ui element that holds the content of each field
				FrameworkElement uiField = CreateMainUIElement(fti.Type, fti.Metadata);
				//add this ui element to the register for access later, appending the suffix "_ui"
				//to differentiate it as the main ui element
				register.Add(fieldName + UI_ELEMENT_SUFFIX, uiField);
				//if using values from before
				if (item != null) {
					//fill in the ui element with the field's value
					FillValueIn(uiField, item[fieldName]);
				}
				//wrap up the element for presentation and add it to the content panel
				contentPanel.Children.Add(WrapUpElement(fti.Name, fti.Type, uiField, item));
			}
		}
		/// <summary>
		/// Creates a ui element for a given field type and metadata
		/// </summary>
		/// <param name="fieldType">the type of field to create a ui element for</param>
		/// <param name="metadata">metadata to aid in the creation of the ui element</param>
		/// <returns>the main ui element for the field</returns>
		private FrameworkElement CreateMainUIElement(FieldType fieldType, object metadata) {
			switch (fieldType) {
				case FieldType.BASIC:	return new TextBox();
				case FieldType.NUMBER:	return new NumberTextBox(metadata as NumberMetadata);
				case FieldType.DECIMAL: return new DecimalTextBox(metadata as DecimalMetadata);
				case FieldType.IMAGE:	return new BackupImage(metadata as ImageMetadata);
				case FieldType.DATE:
					DatePicker dp = new DatePicker();
					dp.SelectedDateFormat = DatePickerFormat.Short;
					return dp;
				case FieldType.ENUM:
					ComboBox cb = new ComboBox();
					//enum possabilities are combo box options
					cb.ItemsSource = (metadata as EnumMetadata).Entries;
					return cb;
				default: throw new NotImplementedException();
			}
		}
		/// <summary>
		/// Fills a ui element with the data from a field
		/// </summary>
		/// <param name="uiField">the ui element to fill</param>
		/// <param name="field">the field to get data from</param>
		private void FillValueIn(FrameworkElement uiField, MField field) {
			if (uiField is TextBox) {
				//includes TextBox, NumberTextBox, DecimalTextBox
				(uiField as TextBox).Text = field.Value.ToString();
			}
			else if(uiField is DatePicker) {
				(uiField as DatePicker).SelectedDate = (DateTime)field.Value;
			}
			else if(uiField is ComboBox) {
				(uiField as ComboBox).SelectedIndex = (int)field.Value;
			}
			else if (uiField is BackupImage) {
				//set the source to a BitmapImage, and store the XImage for later
				(uiField as BackupImage).SetSourceAndBackup((field as ImageField).Value as XImage);
			}
		}
		//FIXME REFACTOR HERE
		private FrameworkElement WrapUpElement(string fieldName, FieldType fieldType, FrameworkElement mainUI, MItem item) {
			switch (fieldType) {
				case FieldType.BASIC:
				case FieldType.DATE:
				case FieldType.NUMBER:
				case FieldType.DECIMAL:
				case FieldType.ENUM:
					DockPanel dp = new DockPanel();
					Label l = new Label();
					l.Content = fieldName + ": ";
					dp.Children.Add(l);
					DockPanel.SetDock(l, Dock.Left);
					dp.Children.Add(mainUI);
					DockPanel.SetDock(mainUI, Dock.Right);
					return dp;
				case FieldType.IMAGE:
					return CreateImageUI(fieldName, mainUI, item);
				default: throw new NotImplementedException();
			}
		}
		private Grid CreateImageUI(string fieldName, FrameworkElement mainUI, MItem item) {
			Button browse = new Button();
			browse.Content = "Browse...";
			browse.Click += BrowseButton_Click;
			browse.Name = fieldName + "_bb";
			Button clear = new Button();
			clear.Content = "Clear";
			clear.Name = fieldName + "_clr";
			clear.Click += ClearButton_Click;
			Label file = new Label();
			file.Content = item != null ? "<Cached file>" : "<No file>";
			register.Add(fieldName + "_lab", file);

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
			mainUI.SetValue(Grid.ColumnProperty, 1);
			mainUI.SetValue(Grid.RowProperty, 1);
			g.Children.Add(browse);
			g.Children.Add(clear);
			g.Children.Add(file);
			g.Children.Add(mainUI);
			return g;
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
                BackupImage img = register[fieldName + UI_ELEMENT_SUFFIX] as BackupImage;
				img.SetSourceAndBackup(xi);
				Label lab = register[fieldName + "_lab"] as Label;
				lab.Content = ofd.FileName.Substring(ofd.FileName.LastIndexOf('\\'));
			}
		}
		private void ClearButton_Click(object sender, RoutedEventArgs e) {
			Button cb = sender as Button;
			string fieldName = cb.Name.Substring(0, cb.Name.Length - 4);
			BackupImage img = register[fieldName + UI_ELEMENT_SUFFIX] as BackupImage;
			img.SetSourceAndBackup(null);
			Label lab = register[fieldName + "_lab"] as Label;
			lab.Content = "<No file>";
		}
	}
}
