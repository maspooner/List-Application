using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

namespace ListApp {
	/// <summary>
	/// A serializable <seealso cref="IEnumerable{ListItem}"/> that models a list
	/// containing <seealso cref="MItem"/> objects based off of a series of
	/// <seealso cref="FieldTemplateItem"/> objects that serve as a template for
	/// fields in an <seealso cref="MItem"/>.
	/// </summary>
	[Serializable]
	class MList : IEnumerable<MItem> {
		//members
		private string name;
		private List<MItem> items;
		//unique identifier, item
		private Dictionary<string, FieldTemplateItem> template;
		//constructors
		internal MList(string name) {
			this.name = name;
			items = new List<MItem>();
			template = new Dictionary<string, FieldTemplateItem>();
		}
		internal MList(string name, string[] csvItems, string[] csvTemplate) : this(name) {
			//TODO implement import
			if(csvItems != null) {
				foreach (string item in csvItems) {
					items.Add(new MItem(Utils.Base64DecodeDict(item)));
				}
			}
			if(csvTemplate != null) {
				for (int i = 0; i < csvTemplate.Length / 2; i++) {
					template.Add(Utils.Base64Decode(csvTemplate[i * 2]),
						new FieldTemplateItem(Utils.Base64DecodeDict(csvTemplate[2 * i + 1])));
				}
			}
		}
		//properties
		internal string Name { get { return name; } }
		internal int Count { get { return items.Count; } }
		internal List<MItem> Items { get { return items; } }
		//Indexer for accessing MItems
		internal MItem this[int i] { get { return items[i]; } }
		internal Dictionary<string, FieldTemplateItem> Template { get { return template; } }
		//methods
		/// <summary>
		/// Adds a new <seealso cref="FieldTemplateItem"/> to the list's template,
		/// and adds a new field based off of the <seealso cref="FieldTemplateItem"/>
		/// to each item already in the list.
		/// </summary>
		/// <param name="fieldName">the name of the new field</param>
		/// <param name="fti">the template item to add</param>
		internal void AddToTemplate(string fieldName, FieldTemplateItem fti) {
			//can't add new field with same name
			if (template.ContainsKey(fieldName)) {
				throw new NotSupportedException("There exists a field with name " + fieldName + " already.");
			}
			else {
				//add the new template for the field
				template.Add(fieldName, fti);
				//add the field to each item already in the list
				foreach (MItem mi in items) {
					mi.AddField(fieldName, fti);
				}
			}
		}
		/// <summary>
		/// Takes the passed in parameters to contstruct a new <seealso cref="FieldTemplateItem"/>
		/// to add to this list's template. Then, adds a new field based off of the
		/// <seealso cref="FieldTemplateItem"/> to each item already in the list.
		/// </summary>
		/// <param name="fieldName">the name of the new field</param>
		/// <param name="type">the type of the new field</param>
		/// <param name="metadata">any metadata associated with the new field</param>
		internal void AddToTemplate(string fieldName, FieldType type, IMetadata metadata) {
			AddToTemplate(fieldName, new FieldTemplateItem(type, metadata, FindOpenSpace(1, 1)));
		}
		/// <summary>
		/// Removes the <seealso cref="FieldTemplateItem"/> from this list's template 
		/// with the specified name, and removes the field with that name from each item
		/// already in the list
		/// </summary>
		/// <param name="fieldName"></param>
		internal void DeleteFromTemplate(string fieldName) {
			//remove the template item 
			template.Remove(fieldName);
			//remove the field from each item already in the list
			foreach (MItem mi in items) {
				mi.RemoveField(fieldName);
			}
		}
		/// <summary>
		/// Finds an open space between all the template items
		/// to place a new <seealso cref="Space"/> of a given size
		/// </summary>
		/// <param name="width">the width of the new space</param>
		/// <param name="height">the height of the new space</param>
		/// <returns></returns>
		internal Space FindOpenSpace(int width, int height) {
			//construct a space to use as an iterator for moving between coordinates
			//starting at (0, 0)
			Space open = new Space(0, 0, width, height);
			//if there aren't any template items, just place it at (0, 0)
			if (template.Count == 0) return open;
			//loop while the y position isn't at the max (due to an error)
			while (open.Y < int.MaxValue) {
				//assign the x to 0 and go up to the grid width
				for (open.X = 0; open.X < C.FIELD_GRID_WIDTH; open.X++) {
					bool fits = true;
					foreach(string fieldName in template.Keys) {
						//if a template item intersects the proposed space
						if (template[fieldName].Intersects(open)) {
							//the space doesn't fit
							fits = false;
							break;
						}
					}
					if (fits) {
						//no conflicts? the space is open
						return open;
					}
				}
				//done with the row, move to the next
				open.Y++;
			}
			//should throw error if loop runs for too long
			throw new NotSupportedException();
		}
		/// <summary>
		/// Creates and returns a new <seealso cref="MItem"/>, adding it
		/// to this <seealso cref="MList"/>. The item's fields are all
		/// given default values
		/// </summary>
		internal MItem Add() {
			//create based off of the template
			MItem li = new MItem(template);
			items.Add(li);
			return li;
		}
		/// <summary>
		/// Removes an item at a specified index
		/// </summary>
		/// <param name="i">the index of the item to remove</param>
		internal void Delete(int i) {
			items.RemoveAt(i);
		}
		//FIXME REFACTOR HERE
		internal bool VerifyFieldData(string fieldName, IMetadata metadata) {
			foreach(MItem mi in items) {
				if (!metadata.Verify(mi[fieldName].Value)) {
					return false;
				}
			}
			return true;
		}
		internal void SetMetadata(string fieldName, IMetadata metadata) {
			template[fieldName].Metadata = metadata;
			foreach(MItem mi in items) {
				MField mf = mi[fieldName];
				if (!metadata.Verify(mf.Value)) {
					mf.Value = mf.StartingValue();
				}
			}
		}
		internal string ToCSV() {
			return Utils.Base64Encode(GetTypeID()) + "," +
				Utils.Base64Encode(ItemsToCSV()) + "," + 
				Utils.Base64Encode(TemplateToCSV());
		}
		internal virtual string GetTypeID() { return "MList"; }
		private string ItemsToCSV() {
			return string.Join(",", items.Select(mi => mi.ToRecoverable()));
		}
		private string TemplateToCSV() {
			string csv = GetTypeID();
			foreach (string fieldName in template.Keys) {
				csv += Utils.Base64Encode(fieldName) + "," + template[fieldName].ToRecoverable() + ",";
			}
			return csv.Substring(0, csv.Length - 1);
		}
		//Methods to allow enumeration through items
		public IEnumerator<MItem> GetEnumerator() { return items.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}
