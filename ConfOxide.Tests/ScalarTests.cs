﻿using System;
using System.ComponentModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfOxide.Tests {
	[TestClass]
	public class ScalarTests {
		sealed class DefaultValues : SettingsBase<DefaultValues> {
			[DefaultValue(null)]
			public byte? DefNull { get; set; }
			public int DefZero { get; set; }
			[DefaultValue("42.1234")]
			public decimal? DefFourtyTwo { get; set; }
			[DefaultValue("Hello")]
			public string Greeting { get; set; }
			[DefaultValue("2000-01-01")]
			public DateTime Century { get; set; }
		}

		[TestMethod]
		public void ScalarDefaultValues() {
			var instance = new DefaultValues();
			instance.DefNull.Should().NotHaveValue();
			instance.DefZero.Should().Be(0);
			instance.DefFourtyTwo.Should().Be(42.1234m);
			instance.Greeting.Should().Be("Hello");
			instance.Century.Should().Be(new DateTime(2000, 01, 01));
		}

		[TestMethod]
		public void ScalarReset() {
			var instance = new DefaultValues { DefNull = 12, Greeting = "Hola", Century = DateTime.Now, DefZero = 12 };
			instance.ResetValues();

			instance.DefNull.Should().NotHaveValue();
			instance.DefZero.Should().Be(0);
			instance.Greeting.Should().Be("Hello");
			instance.Century.Should().Be(new DateTime(2000, 01, 01));
		}

		[TestMethod]
		public void ScalarCopy() {
			var source = new DefaultValues { Greeting = "Hola", Century = DateTime.Now, DefZero = 12 };
			var target = source.CreateCopy();

			target.DefZero.Should().Be(12);
			target.Greeting.Should().Be("Hola");
			target.Century.Should().Be(source.Century);
		}

		[TestMethod]
		public void ScalarEquality() {
			var source = new DefaultValues { Greeting = "Hola", Century = DateTime.Now, DefZero = 12 };
			var target = source.CreateCopy();
			target.IsEquivalentTo(source).Should().BeTrue();
			source.IsEquivalentTo(target).Should().BeTrue();
		}
	}
}