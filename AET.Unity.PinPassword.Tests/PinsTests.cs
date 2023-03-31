using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.NetworkInformation;
using FluentAssertions;

namespace AET.Unity.PinPassword.Tests {
  
  [TestClass]
  public class PinsTests {
    private readonly Pins pins = new Pins(@"[{'Pin':'5678','Position':2},{'Pin':'1234','Position':1}]".Replace("'", "\""), "0000");

    #region LoadPinsCsv Tests
    [TestMethod]
    public void LoadPins_ValidData_OldPasswordsCleared() {
      pins.LoadPinsJson(@"[{'Pin':'6666','Position':1},{'Pin':'7777','Position':2}]".Replace("'","\""));
      pins.PinIsValid("1234").Should().BeFalse("because this should be cleared");
    }

    [TestMethod]
    public void LoadPins_NullOrEmptyString_ClearsPasswordListWithoutExceptions() {
      pins.LoadPinsJson(null);
      pins.LoadPinsJson("");
      pins.Count.Should().Be(0);
    }

    [TestMethod]
    public void LoadPins_ValidData_NewPasswordsActive() {
      pins.LoadPinsJson(@"[{'Pin':'2222','Position':1},{'Pin':'3333','Position':2},{'Pin':'4444','Position':4}]".Replace("'","\""));
      pins.PinIsValid("2222").Should().BeTrue("because this is in the new config file");
      pins.PinIsValid("3333").Should().BeTrue("because this is in the new config file");
      pins.PinIsValid("4444").Should().BeTrue("because this is in the new config file");
    }
    #endregion

    #region PinIsValid Tests
    [TestMethod]
    public void PinIsValid_NullOrEmptyString_ReturnsFalse() {
      pins.PinIsValid(null).Should().BeFalse();
      pins.PinIsValid(string.Empty).Should().BeFalse();
    }

    [TestMethod]
    public void PinIsValid_NullOrEmptyString_ReturnedPinIsNull() {
      pins.PinIsValid(null, out var pin);
      pin.Should().BeNull();
    }

    [TestMethod]
    public void PinIsValid_ValidPin_ReturnsTrue() {
      pins.PinIsValid("1234", out var pin).Should().BeTrue("because this is a valid pin");
      pin.Position.Should().Be(1, "because position is one-based");
    }

    [TestMethod]
    public void PinIsValid_ValidPasswordForIndex_ReturnsTrue() {
      pins.PinIsValid("5678", 2).Should().BeTrue("because 5678 is the password for position 2 (one-based)");
    }

    [TestMethod]
    public void PinIsValid_ValidPasswordForOtherIndex_ReturnsFalse() {
      pins.PinIsValid("5678", 1).Should().BeFalse("because 5678 is the password for position 2, not 0");
    }
    #endregion

    #region Configuration Tests
    [TestMethod]
    public void GetConfigData_DefaultTestPasswords_ReturnsValidCsv() {
      pins.SerializePinsJson().Should().Be("[{\"Pin\":\"1234\",\"Position\":1},{\"Pin\":\"5678\",\"Position\":2}]");
    }


    #endregion
  }
}
