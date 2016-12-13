using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ListApp;
using System.Collections.Generic;

namespace ListTests {
	[TestClass]
	public class UtilTests {
		/// <summary>
		/// Test encoding with standard data
		/// </summary>
		[TestMethod]
		public void EncodePair_NoSpecialChar() {
			string k = "key", v = "value";
			Console.WriteLine(Utils.EncodePair(k, v));
			Assert.AreEqual(Utils.EncodePair(k, v), "a2V5:dmFsdWU=");
		}
		/// <summary>
		/// Test encoding with characters that are used in the encoding process
		/// </summary>
		[TestMethod]
		public void EncodePair_HasSpecialChar() {
			string k = "key::", v = "val::ue";
			Console.WriteLine(Utils.EncodePair(k, v));
			Assert.AreEqual(Utils.EncodePair(k, v), "a2V5Ojo=:dmFsOjp1ZQ==");
		}
		/// <summary>
		/// Test encoding a null value
		/// </summary>
		[TestMethod]
		public void EncodePair_NullValue() {
			string k = "key::", v = null;
			Console.WriteLine(Utils.EncodePair(k, v));
			Assert.AreEqual(Utils.EncodePair(k, v), "a2V5Ojo=:&");
		}
		/// <summary>
		/// Test decoding with standard data
		/// </summary>
		[TestMethod]
		public void DecodePair_NoSpecialChar() {
			string e = "a2V5:dmFsdWU=";
			KeyValuePair<string, string> actual = Utils.DecodePair(e);
			Assert.AreEqual("key", actual.Key);
			Assert.AreEqual("value", actual.Value);
		}
		/// <summary>
		/// Test decoding with characters that are used in the decoding process
		/// </summary>
		[TestMethod]
		public void DecodePair_HasSpecialChar() {
			string e = "a2V5Ojo=:dmFsOjp1ZQ==";
			KeyValuePair<string, string> actual = Utils.DecodePair(e);
			Assert.AreEqual("key::", actual.Key);
			Assert.AreEqual("val::ue", actual.Value);
		}
		/// <summary>
		/// Test decoding a null value
		/// </summary>
		[TestMethod]
		public void DecodePair_NullValue() {
			string e = "a2V5Ojo=:&";
			KeyValuePair<string, string> actual = Utils.DecodePair(e);
			Assert.AreEqual("key::", actual.Key);
			Assert.AreEqual(null, actual.Value);
		}
		/// <summary>
		/// Does decoding the encoding of a key value pair yield the original key value pair?
		/// </summary>
		[TestMethod]
		public void EncodeDecodePair_AreEqual() {
			string k = "key::", v = "val::ue";
			KeyValuePair<string, string> actual = Utils.DecodePair(Utils.EncodePair(k, v));
            Assert.AreEqual(k, actual.Key);
			Assert.AreEqual(v, actual.Value);
		}

		/// <summary>
		/// Test encoding an empty dictionary
		/// </summary>
		[TestMethod]
		public void EncodeMultiple_Empty() {
			Dictionary<string, string> kv = new Dictionary<string, string>();
			Assert.AreEqual("", Utils.EncodeMultiple(kv));
		}
		/// <summary>
		/// Test decoding an empty string
		/// </summary>
		[TestMethod]
		public void DecodeMultiple_Empty() {
			Assert.IsTrue(Utils.DecodeMultiple("").Count == 0);
		}
		/// <summary>
		/// Does decoding the encoding of a multiple value dictionary yield the original dictionary?
		/// </summary>
		[TestMethod]
		public void EncodeDecodeMultiple_AreEqual() {
			Dictionary<string, string> kv = new Dictionary<string, string>();
			kv.Add("val1::", "22333");
			kv.Add("ekdlk", "3dddddd");
			kv.Add("skdfklsdf", "name334");
			Dictionary<string, string> actual = Utils.DecodeMultiple(Utils.EncodeMultiple(kv));
            foreach (string key in kv.Keys) {
				if (!actual.ContainsKey(key) && !actual[key].Equals(kv[key])) {
					Assert.Fail();
				}
			}
		}

		/// <summary>
		/// Test encoding an empty sequence
		/// </summary>
		[TestMethod]
		public void EncodeSequence_Empty() {
			List<string> ls = new List<string>();
			Assert.AreEqual("", Utils.EncodeSequence(ls));
		}
		/// <summary>
		/// Test decoding an empty sequence
		/// </summary>
		[TestMethod]
		public void DecodeSequence_Empty() {
			Assert.IsTrue(Utils.DecodeSequence("").Count == 0);
		}
		/// <summary>
		/// Does the encoding and decoding of an ordered sequence yield the original sequence in order?
		/// </summary>
		[TestMethod]
		public void EncodeDecodeSequence_AreEqual() {
			List<string> ls = new List<string>();
			ls.Add("val1::");
			ls.Add("ekdlk");
			ls.Add("skdfklsdf");
			List<string> actual = Utils.DecodeSequence(Utils.EncodeSequence(ls));
			for(int i = 0; i < ls.Count; i++) {
				if (!actual[i].Equals(ls[i])) {
					Assert.Fail();
				}
			}
		}
	}
}
