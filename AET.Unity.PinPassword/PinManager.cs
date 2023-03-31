using System;
using AET.Unity.SimplSharp;
using AET.Unity.SimplSharp.FileIO;

namespace AET.Unity.PinPassword {
  public class PinManager {
    private readonly Pins pins = new Pins();

    private string enteredPin = string.Empty;

    private int changingPin = 0;

    #region Constructors
    public PinManager() {
      FileIo = new CrestronFileIO();
      PinLength = 4;
    }

    internal PinManager(IFileIO fileIo) {
      FileIo = fileIo;
      PinLength = 4;
      CreateTestDelegates();
    }
    #endregion

    private void CreateTestDelegates() {
      SetMessage = delegate { };
      SetPinDisplay = delegate { };
      SetPinStars = delegate { };
      PulseValidPinEntered = delegate { };
      PulseBackdoorPinEntered = delegate { };
      PulseInvalidPinEntered =  delegate { };
      PulseValidPinEnteredIndex = delegate { };
      PulseChangePinCancelled = delegate { };
      PulseChangePinSuccess = delegate { };
      SetChangingPin = delegate { };
      SetChangingPinActive = delegate { };
      SetChangingPinInactive = delegate { };
    }

    #region Properties

    public IFileIO FileIo { get; set; }

    public string FilePath { get; set; }

    public string BackdoorPin {
      get { return pins.BackdoorPin; }
      set { pins.BackdoorPin = value; }
    }

    public int ChangingPin {
      get { return changingPin; }
      private set {
        if (changingPin > 0) {
          SetChangingPin((ushort)changingPin, 0);
          if(value != changingPin) Clear();
        }
        if(value > 0) SetChangingPin((ushort)value, 1);
        changingPin = value;
        SetChangingPinInactive((value == 0).ToUshort());
        SetChangingPinActive((value > 0).ToUshort());
      }
    }



    #endregion 

    #region Splus Methods

    public void Init() {
      ChangingPin = 0;
    }

    public void PressDigit(ushort digit) {
      enteredPin += (digit % 10).ToString();
      UpdatePinDisplay();
      if (ChangingPin > 0) {
        if (enteredPin.Length > PinLength) {
          enteredPin = enteredPin.Substring(0, PinLength);
          UpdatePinDisplay();
        }
      } else if (enteredPin.Length >= PinLength) {
        CheckPin();
      }
    }

    private void UpdatePinDisplay() {
      SetPinDisplay(enteredPin);
      SetPinStars(new string('*', enteredPin.Length));
    }

    public void PressBackspace() {
      if (enteredPin.Length == 0) return;
      enteredPin = enteredPin.Substring(0, enteredPin.Length - 1);
      UpdatePinDisplay();
    }

    public void PressSavePin() {
      if (enteredPin.Length < PinLength) {
        SetMessage(string.Format("PIN must be {0} digits. Please enter more digits.", PinLength));
        return;
      }
      pins[ChangingPin].Pin = enteredPin.Substring(0,PinLength);
      Clear();
      SaveConfig();
      ChangingPin = 0;
      PulseChangePinSuccess();
    }
    private void CheckPin() {
      PinItem pin;
      if (pins.PinIsValid(enteredPin, out pin)) {
        PulseValidPinEntered();
        if (pin.IsBackdoorPin) PulseBackdoorPinEntered();
        else PulseValidPinEnteredIndex((ushort)pin.Position);
        Clear();
      }
      else {
        SetMessage("Invalid PIN Entered. Please try again.");
        PulseInvalidPinEntered();
        Clear();
      }
    }

    public bool PinIsValid(string pin) {
      return pins.PinIsValid(pin);
    }

    public void PressClear() {
      if (ChangingPin > 0) {
        PulseChangePinCancelled();
        ChangingPin = 0;
      } else Clear();
    }

    private void Clear() {
      enteredPin = string.Empty;
      SetMessage("");
      SetPinDisplay("");
      SetPinStars("");
    }
    public ushort PinLength { get; set; }

    public void PressChangePin(ushort index) {
      Clear();
      SetMessage(string.Format("Please enter new {0}-digit PIN", PinLength));
      ChangingPin = index;
    }
    #endregion

    #region SPlus Outputs
    public SetStringOutputDelegate SetPinDisplay { get; set; }
    public SetStringOutputDelegate SetPinStars { get; set; }
    public SetStringOutputDelegate SetMessage { get; set; }
    public TriggerDelegate PulseChangePinCancelled { get; set; }
    public TriggerDelegate PulseChangePinSuccess { get; set; }
    public SetUshortOutputArrayDelegate SetChangingPin { get; set; }
    public TriggerDelegate PulseValidPinEntered { get; set; }
    public TriggerDelegate PulseBackdoorPinEntered { get; set; }
    public TriggerDelegate PulseInvalidPinEntered { get; set; }
    public TriggerArrayDelegate PulseValidPinEnteredIndex { get; set; }
    public SetUshortOutputDelegate SetChangingPinActive { get; set; }
    public SetUshortOutputDelegate SetChangingPinInactive { get; set; }
    
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

    public void SaveConfig() {
      FileIo.WriteText(FilePath, pins.SerializePinsJson());
    }
    #endregion 

  }
}
