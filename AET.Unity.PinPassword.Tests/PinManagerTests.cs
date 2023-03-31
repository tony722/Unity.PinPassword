using AET.Unity.SimplSharp.FileIO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using FluentAssertions;

namespace AET.Unity.PinPassword.Tests {
  [TestClass]
  public class PinManagerTests {
    private PinManager pin;
    private bool pinValid, pinInvalid;
    private int validIndex;
    private string pinDisplay, pinStars;
    private readonly int[] changingPinFeedback = new int[4];
    private bool changePinSuccess, changePinCancelled;
    
  [TestInitialize]
    public void TestInit() {
      pin = new PinManager(new TestFileIO {SimulatedFileContents = "[{\"Pin\":\"1234\",\"Position\":1},{\"Pin\":\"5678\",\"Position\":2}]" }) {
        PinLength = 4,
        BackdoorPin = "0000"
      };
      pin.ReadConfigFile();
      pin.PulseValidPinEnteredIndex = (idx) => {
        pinValid = true;
        validIndex = idx;
      };
      pin.PulseInvalidPinEntered = () => pinInvalid = true;
      pin.SetPinDisplay = (s) => pinDisplay = s.ToString();
      pin.SetPinStars = (s) => pinStars = s.ToString();
      pin.SetChangingPin = (i, v) => changingPinFeedback[i - 1] = v;
      pin.PulseChangePinCancelled = () => changePinCancelled = true;
      pin.PulseChangePinSuccess = () => changePinSuccess = true;
    }

    #region Pin Checking Tests
    [TestMethod]
    public void PressDigit_4DigitsEntered_PinChecked() {
      pin.PressDigit(5);
      pin.PressDigit(6);
      pin.PressDigit(7);
      pin.PressDigit(8);
      pinValid.Should().BeTrue();
      validIndex.Should().Be(2);
    }

    [TestMethod]
    public void PressDigit_4IncorrectDigitsEntered_PinInvalidShown() {
      pin.PressDigit(5);
      pin.PressDigit(6);
      pin.PressDigit(7);
      pin.PressDigit(2);
      pinValid.Should().BeFalse();
      pinInvalid.Should().BeTrue();
    }
    #endregion

    #region Display Tests

    [TestMethod]
    public void SetPinDisplay_DigitsPressed_DisplaysPressedDigits() {
      pin.PressDigit(5);
      pin.PressDigit(6);
      pin.PressDigit(7);
      pinDisplay.Should().Be("567");
      pinStars.Should().Be("***");
    }

    [TestMethod]
    public void SetPinDisplay_IndexTenPressed_DisplaysZero() {
      pin.PressDigit(7);
      pin.PressDigit(10);
      pinDisplay.Should().Be("70");
      pinStars.Should().Be("**");
    }

    [TestMethod]
    public void PressBackspace_ClearsMostRecentCharacter() {
      pin.PressDigit(5);
      pin.PressDigit(6);
      pin.PressDigit(7);
      pin.PressBackspace();
      pinDisplay.Should().Be("56");
    }
    [TestMethod]
    public void PressBackspace_NoDigitsEntered_DoesNothing() {
      pin.PressBackspace();
      pinDisplay.Should().BeNullOrEmpty();
      pin.PressDigit(7);
      pin.PressBackspace();
      pin.PressBackspace();
      pinDisplay.Should().BeNullOrEmpty();
    }
    #endregion

    #region Change Pin Tests

    [TestMethod]
    public void ChangePin_DigitsEntered_PinChanged() {
      pin.PressChangePin(1);
      pinDisplay.Should().BeNullOrEmpty();
      pin.PressDigit(2);
      pin.PressDigit(2);
      pin.PressDigit(3);
      pin.PressDigit(3);
      pin.PressSavePin();
      pinDisplay.Should().BeNullOrEmpty();
      pin.PinIsValid("1234").Should().BeFalse();
      pin.PinIsValid("2233").Should().BeTrue();
      changePinCancelled.Should().BeFalse();
      changePinSuccess.Should().BeTrue();
      changingPinFeedback.Should().BeEquivalentTo(new[] { 0, 0, 0, 0 });
    }

    [TestMethod]
    public void ChangePin_FeedbackSet() {
      pin.PressChangePin(3);
      changingPinFeedback.Should().BeEquivalentTo(new [] { 0, 0, 1, 0 });
    }

    [TestMethod]
    public void ChangePin_ChangePinToChangeInMiddle_OutputClearedAndFeedbackSetCorrectly() {
      pin.PressChangePin(4);
      changingPinFeedback.Should().BeEquivalentTo(new [] { 0, 0, 0, 1 });
      pin.PressDigit(3);
      pin.PressDigit(3);
      pinDisplay.Should().Be("33");
      pin.PressChangePin(3);
      pinDisplay.Should().Be("", "because we changed which pin we're editing midstream");
      changingPinFeedback.Should().BeEquivalentTo(new [] { 0, 0, 1, 0 });
    }

    [TestMethod]
    public void ChangePin_Cancel_OutputClearedAndFeedbackCleared() {
      pin.PressChangePin(4);
      pin.PressDigit(3);
      pin.PressDigit(3);
      pin.PressClear();
      pinDisplay.Should().Be("");
      changingPinFeedback.Should().BeEquivalentTo(new [] { 0, 0, 0, 0 });
      changePinCancelled.Should().BeTrue();
      changePinSuccess.Should().BeFalse();
    }
    #endregion
  }
}
