﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;

namespace ListApp {
	public partial class MainWindow : Window {
		//members
		private const string FILE_PATH = @"C:\Users\Matt\Documents\Visual Studio 2015\Projects\ListApp\lists.bin"; //TODO adjustable
		private List<MList> lists;
		//constructors
		public MainWindow() {
			InitializeComponent();
			LoadLists();
			
			MList list1 = new MList("group a");
			list1.AddToTemplate("notes", ItemType.BASIC);
			list1.AddToTemplate("date", ItemType.DATE);
			list1.AddToTemplate("img", ItemType.IMAGE);
			ListItem li1a = list1.Add("1a");
			li1a.SetFieldData("notes", "There are many things here");
			li1a.SetFieldData("date", DateTime.Now);
			li1a.SetFieldData("img", new Image());
			ListItem li2a = list1.Add("2a");
			li2a.SetFieldData("notes", "More notes");
			li2a.SetFieldData("date", DateTime.Today);
			li2a.SetFieldData("img", new Image());
			lists.Add(list1);

			PrintLists();
			list1.DeleteFromTemplate(0);
			list1.AddToTemplate("title", ItemType.BASIC);
			list1.ReorderTemplate(0, 2);
			list1.ResolveFieldFields();
			PrintLists();
			SaveLists();
		}
		//methods
		private void PrintLists() {
			foreach (MList m in lists) {
				Console.WriteLine(m.Name);
				foreach (ListItem li in m) {
					Console.WriteLine("\t" + li.Name);
					foreach (ListItemField lif in li) {
						Console.WriteLine("\t\t" + lif.Name + ": " + lif.GetValue());
					}
				}
			}
		}
		public void SaveLists() {
			Stream stream = File.Open(FILE_PATH, FileMode.Create);
			BinaryFormatter bformatter = new BinaryFormatter();
			bformatter.Serialize(stream, lists);
			stream.Close();
		}
		public void LoadLists() {
			if (new FileInfo(FILE_PATH).Exists) {
				Stream stream = File.Open(FILE_PATH, FileMode.Open);
				BinaryFormatter bformatter = new BinaryFormatter();
				lists = (List<MList>)bformatter.Deserialize(stream);
				stream.Close();
			}
			else {
				lists = new List<MList>();
			}
		}
	}
}
