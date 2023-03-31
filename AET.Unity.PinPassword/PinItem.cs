using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace AET.Unity.PinPassword {
  internal class PinItem {
    public string Pin;

    /// <summary>
    /// One-Based Position of Pin In List
    /// (Position 0 is BackdoorPin)
    /// </summary>
    public int Position;

    [JsonIgnore]
    public bool IsBackdoorPin;
  }
}
