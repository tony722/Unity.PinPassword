using AET.Unity.SimplSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace AET.Unity.PinPassword {
  internal class Pins : IEnumerable<PinItem> {
    private List<PinItem> pins;
    private readonly PinItem backdoorPinItem = new PinItem { Position = 0, IsBackdoorPin = true };
    public Pins() {
      pins = new List<PinItem>();
    }

    public Pins(string jsonPins, string backdoorPin) {
      BackdoorPin = backdoorPin;
      LoadPinsJson(jsonPins);
    }

    public PinItem this[int index] {
      get {
        var pin = pins.FirstOrDefault(p => p.Position == index);
        if (pin == null) {
          pin = new PinItem { Position = index };
          pins.Add(pin);
        }
        return pin;
      }
    }

    public string BackdoorPin {
      get { return backdoorPinItem.Pin; }
      set { backdoorPinItem.Pin = value; }
    }

    public void LoadPinsJson(string json) {
      if (json.IsNullOrWhiteSpace()) {
        pins.Clear();
        return;
      }
      pins = JsonConvert.DeserializeObject<List<PinItem>>(json);
    }

    public void LoadPinsCsv(string pinsCsv) {
      if (pinsCsv.IsNullOrWhiteSpace()) {
        pins = new List<PinItem>();
        return;
      }
      var newPins = pinsCsv.Split(',');
      pins = newPins.Select((p,i) => new PinItem { Pin = StripAllButDigits(p), Position = i + 1}).ToList();
    }

    public bool PinIsValid(string pin, out PinItem pinItem) {
      if (backdoorPinItem.Pin == pin) {
        pinItem = backdoorPinItem;
        return true;
      }
      pinItem = pins.FirstOrDefault(x => x.Pin == pin);
      return pinItem != null;
    }

    public string SerializePinsJson() {
      var json = JsonConvert.SerializeObject(pins.OrderBy(p => p.Position));
      return json;
    }

    public bool PinIsValid(string pin) {
      if (backdoorPinItem.Pin == pin) return true;
      return pins.Any(p => p.Pin == pin);
    }

    public bool PinIsValid(string pin, int position) {
      if (position == 0) return backdoorPinItem.Pin == pin;
      return pins.Any(p => p.Position == position && p.Pin == pin);
    }

    private static string StripAllButDigits(string value) {
      return new string(value.Where(char.IsDigit).ToArray());
    }

    public int Count {
      get { return pins.Count; }
    }

    public IEnumerator<PinItem> GetEnumerator() {
      return pins.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}
