using System;

namespace toDrive.Domain.Models
{
  [Flags]
  public enum TrackStateType : byte
  {
    Undefined = 0,
    Acceleration = 1,
    Gravity = 2,
    Location = 4
  }
}