using System;
using Newtonsoft.Json;

namespace toDrive.Domain.Models
{
  public class TrackState
  {
    public DateTime DateTime { get; set; }
    public float AclX { get; set; }
    public float AclY { get; set; }
    public float AclZ { get; set; }
    public float GvtX { get; set; }
    public float GvtY { get; set; }
    public float GvtZ { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    public float Spd { get; set; } 
    public string Provider { get; set; }
    public float Acc { get; set; }

    [JsonIgnore]
    public TrackStateType Type
    {
      get
      {
        var type = TrackStateType.Undefined;

        if (this.AclX != 0 || this.AclY != 0 || this.AclZ != 0)
          type |= TrackStateType.Acceleration;

        if (this.GvtX != 0 || this.GvtY != 0 || this.GvtZ != 0)
          type |= TrackStateType.Gravity;

        if ((this.Lat != 0 || this.Lon != 0))
          type |= TrackStateType.Location;

        return type;
      }
    }
  }
}