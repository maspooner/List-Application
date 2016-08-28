using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;

namespace ListApp {
	[Serializable]
	class SyncList : MList {
		internal enum SchemaType { ANIME_LIST }
		//members
		private SchemaType schemaType;
		private SyncSchema schema;
		[NonSerialized]
		private ProgressBar syncBar;
		[NonSerialized]
		private Label messagesLabel;
		[NonSerialized]
		private MainWindow mainWindow;
		[NonSerialized]
		private BackgroundWorker itemWorker;
		//constructors
		internal SyncList(string name, SchemaType schemaType, object schemaData) : base(name) {
			this.schemaType = schemaType;
			this.schema = FindSchema(schemaType, schemaData);
			itemWorker = new BackgroundWorker();
			itemWorker.WorkerReportsProgress = true;
			itemWorker.WorkerSupportsCancellation = true;
			itemWorker.DoWork += ItemWorker_DoWork;
			itemWorker.ProgressChanged += ItemWorker_ProgressChanged;
			itemWorker.RunWorkerCompleted += ItemWorker_RunWorkerCompleted;
		}
		//methods
		private SyncSchema FindSchema(SchemaType type, object schemaData) {
			switch (type) {
				case SchemaType.ANIME_LIST: return new AnimeListSchema(schemaData as string);
			}
			return null;
		}
		internal SchemaOption SchemaOptionAt(int i) { return schema.Options[i]; }
		internal void SaveSchemaOptions() {
			//remove all old schema settings
			Template.RemoveAll((iti) => iti is SyncTemplateItem);
			//add all new schema settings
			Template.AddRange(schema.GenerateTemplate(this));
		}
		internal void StartRefreshAllTask(MainWindow mainWindow, ProgressBar pb, Label l) {
			this.mainWindow = mainWindow;
			syncBar = pb;
			messagesLabel = l;
			syncBar.Visibility = System.Windows.Visibility.Visible;
			itemWorker.RunWorkerAsync();
		}
		private void ItemWorker_DoWork(object sender, DoWorkEventArgs e) {
			BackgroundWorker bw = sender as BackgroundWorker;
			bw.ReportProgress(0, "Setting up...");
			schema.PrepareRefresh();
			IEnumerable<SyncListItem> itemEnum = schema.CreateNewItems(Template);
			int done = 0;
			int count = schema.GetItemCount();
			List<SyncListItem> list = new List<SyncListItem>();
			bw.ReportProgress(0, "Adding item #1 of " + count);
			foreach (SyncListItem sli in itemEnum) {
				if (bw.CancellationPending) {
					e.Cancel = true;
					break;
				}
				list.Add(sli);
				done++;
				Console.WriteLine("Adding item #" + (done + 1) + " of " + count);
				bw.ReportProgress(done * 100 / count, "Adding item #" + (done + 1) + " of " + count);
			}
			e.Result = list;
		}
		private void ItemWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			syncBar.Value = e.ProgressPercentage;
			messagesLabel.Content = e.UserState as string;
		}
		private void ItemWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			syncBar.Visibility = System.Windows.Visibility.Collapsed;
			messagesLabel.Content = "";
			if (e.Cancelled) {
				Console.WriteLine("Canceled");
			}
			else if (e.Error != null) {
				throw e.Error;
			}
			else {
				Console.WriteLine("Done");
				Items.Clear();
				Items.AddRange(e.Result as List<SyncListItem>);
				mainWindow.Refresh();
			}
		}
	}
}
