using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListApp {
	abstract class ListItemField : IComparable<ListItemField> {
		//members
		private string fieldName;
		//constructors
		internal ListItemField(string fieldName) {
			this.fieldName = fieldName;
		}
		//methods
		public abstract int CompareTo(ListItemField other);
	}
	class BasicField : ListItemField {
		//members
		private string value;
		//constructors
		internal BasicField(string fieldName, string value) : base(fieldName){
			this.value = value;
		}
		//methods
		public override int CompareTo(ListItemField other) {
			return other is BasicField ? this.value.CompareTo((other as BasicField).value) : -1;
		}
	}
	class DateField : ListItemField {
		//members
		private DateTime value;
		//constructors
		internal DateField(string fieldName, DateTime value) : base(fieldName) {
			this.value = value;
		}
		//methods
		public override int CompareTo(ListItemField other) {
			return other is DateField ? this.value.CompareTo((other as DateField).value) : -1;
		}
	}
}
