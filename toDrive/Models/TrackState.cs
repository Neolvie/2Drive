using System;
using System.Reflection.Emit;

namespace toDrive.Models
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
  }
}