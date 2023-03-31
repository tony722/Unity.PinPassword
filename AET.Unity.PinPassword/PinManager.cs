using System;
using System.Collections.Generic;
using System.Linq;
using AET.Unity.SimplSharp;
using AET.Unity.SimplSharp.FileIO;

namespace AET.Unity.PinPassword {
  public class PinManager {
    private readonly Pins pins = new Pins();

    private string enteredPin = string.Empty;

    private int pinBeingChanged = 0;
    private ushort revealPin;

    #region Constructors
    public PinManager() {
      CreateDelegatePlaceholders();
      FileIo = new CrestronFileIO();
      PinLength = 4;
    }

    internal PinManager(IFileIO fileIo) {
      FileIo = fileIo;
      PinLength = 4;
      CreateDelegatePlaceholders();
    }
    #endregion

    private void CreateDelegatePlaceholders() {
      SetMessage = delegate { };
      SetPinDisplay = delegate { };
      PulseValidPinEntered = delegate { };
      PulseBackdoorPinEntered = delegate { };
      PulseInvalidPinEntered =  delegate { };
      PulseValidPinEnteredIndex = delegate { };
      PulseChangePinCancelled = delegate { };
      PulseChangePinSuccessful = delegate { };
      SetPinBeingChanged = delegate { };
      SetPinChangingActive = delegate { };
      SetPinChangingInactive = delegate { };
    }

    #region Properties

    public IFileIO FileIo { get; set; }

    public string FilePath { get; set; }

    public string BackdoorPin {
      get { return pins.BackdoorPin; }
      set { pins.BackdoorPin = value; }
    }

    public ushort PinLength { get; set; }

    private int PinBeingChanged {
      get { return pinBeingChanged; }
      set {
        if (pinBeingChanged > 0) {
          SetPinBeingChanged((ushort)pinBeingChanged, 0);
          if(value != pinBeingChanged) Clear();
        }
        if(value > 0) SetPinBeingChanged((ushort)value, 1);
        pinBeingChanged = value;
        SetPinChangingInactive((value == 0).ToUshort());
        SetPinChangingActive((value > 0).ToUshort());
      }
    }

    public ushort RevealPin {
      get { return revealPin; }
      set {
        if (value == revealPin) return;
        revealPin = value;
        UpdatePinDisplay();
      }
    }

    #endregion 

    #region Splus Methods

    public void Init() {
      PinBeingChanged = 0;
      SanityCheckPins();
    }

    private void SanityCheckPins() {
      if (!SanityCheckBackdoorPin()) return;

      var invalidPins = new List<PinItem>();
      
      foreach (var pin in pins) if (pin.Pin.Length != PinLength) invalidPins.Add(pin);
      if (invalidPins.Count > 0) {
        var pinText = string.Join(", #", invalidPins.Select(p => p.Position.ToString()).ToArray());
        SetMessage(string.Format("The following PIN(s) are invalid and will not work: #{1}. Must be exactly Pin_Length ({0} digits).", PinLength, pinText));
      }
    }

    private bool SanityCheckBackdoorPin() {
      if(BackdoorPin == null) return true;
      if(BackdoorPin.Length > 0 && BackdoorPin.Length != PinLength) {
        SetMessage(string.Format("Backdoor PIN is not correct length. Must be exactly Pin_Length ({0} digits).", PinLength));
        return false;
      }
      return true;
    }

    public void PressDigit(ushort digit) {
      AddNewDigitToPin(digit);
      UpdatePinDisplay();
      if (InPinChangeMode()) {
        TrimPinToMaxPinLength();
      } else if (enteredPin.Length >= PinLength) {
        CheckEnteredPin();
      }
    }
    #region PressDigit() methods
    private void AddNewDigitToPin(ushort digit) {
      enteredPin += (digit % 10).ToString();
    }

    private bool InPinChangeMode() {
      return PinBeingChanged > 0;
    }

    private void TrimPinToMaxPinLength() {
      if (enteredPin.Length > PinLength) {
        enteredPin = enteredPin.Substring(0, PinLength);
        UpdatePinDisplay();
      }
    }

    private void CheckEnteredPin() {
      PinItem pin;
      if (pins.PinIsValid(enteredPin, out pin)) {
        Clear();
        PulseValidPinEntered();
        if (pin.IsBackdoorPin) PulseBackdoorPinEntered();
        else PulseValidPinEnteredIndex((ushort)pin.Position);
      } else {
        Clear();
        PulseInvalidPinEntered();
        SetMessage("Invalid PIN Entered. Please try again.");
      }
    }

    #endregion

    private void UpdatePinDisplay() {
      if(RevealPin == 0) SetPinDisplay(new string('*', enteredPin.Length));
      else SetPinDisplay(enteredPin);
    }

    public void PressBackspace() {
      if (enteredPin.Length == 0) return;
      enteredPin = enteredPin.Substring(0, enteredPin.Length - 1);
      UpdatePinDisplay();
    }

    public void PressSavePin() {
      if (TriedToSavePinShorterThanPinLength()) {
        SetMessage(string.Format("PIN must be {0} digits. Please enter more digits.", PinLength));
        return;
      }
      pins[PinBeingChanged].Pin = EnteredPinTrimmedToPinLength();
      PinBeingChanged = 0;
      Clear();
      PulseChangePinSuccessful();
      SaveConfigFile();
    }
    #region PressSavePin() methods
    private bool TriedToSavePinShorterThanPinLength() {
      return enteredPin.Length < PinLength;
    }

    private string EnteredPinTrimmedToPinLength() {
      return enteredPin.Substring(0, PinLength);
    }
    #endregion

    public void PressDeletePin() {
      if (!InPinChangeMode()) return;
      pins.DeletePin(PinBeingChanged);
      PinBeingChanged = 0;
      Clear();
      PulseChangePinCancelled();
      SaveConfigFile();
    }

    public bool PinIsValid(string pin) {
      return pins.PinIsValid(pin);
    }

    public void PressClear() {
      if (PinBeingChanged > 0) {
        PulseChangePinCancelled();
        PinBeingChanged = 0;
      } 
      Clear();
    }

    private void Clear() {
      enteredPin = string.Empty;
      SetMessage("");
      SetPinDisplay("");
    }

    public void PressChangePin(ushort index) {
      Clear();
      SetMessage(string.Format("Please enter new {0}-digit PIN", PinLength));
      PinBeingChanged = index;
    }
    #endregion

    #region SPlus Outputs

    public SetStringOutputDelegate SetPinDisplay { get; set; }

    public SetStringOutputDelegate SetMessage { get; set; }

    public TriggerDelegate PulseChangePinCancelled { get; set; }

    public TriggerDelegate PulseChangePinSuccessful { get; set; }

    public SetUshortOutputArrayDelegate SetPinBeingChanged { get; set; }

    public TriggerDelegate PulseValidPinEntered { get; set; }

    public TriggerDelegate PulseBackdoorPinEntered { get; set; }

    public TriggerDelegate PulseInvalidPinEntered { get; set; }

    public TriggerArrayDelegate PulseValidPinEnteredIndex { get; set; }

    public SetUshortOutputDelegate SetPinChangingActive { get; set; }

    public SetUshortOutputDelegate SetPinChangingInactive { get; set; }

    #endregion

    #region File Handling

    public void ReadConfigFile(string filePath) {
      FilePath = filePath;
      ReadConfigFile();
    }

    public void ReadConfigFile() {
      if (!FileIo.Exists(FilePath)) return;
      var fileData = FileIo.ReadAllText(FilePath);
      LoadPins(fileData);
    }

    public void LoadPins(string jsonPins) {
      pins.LoadPinsJson(jsonPins);
    }

    public void SaveConfigFile() {
      FileIo.WriteText(FilePath, pins.SerializePinsJson());
    }
    #endregion 

  }
}
