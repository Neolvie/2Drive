using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using toDrive.Domain.Models;

namespace TracksParser
{
  public static class TrackStatesReader
  {
    public static List<TrackState> ReadTrackStates(List<string> pathList)
    {
      var trackStates = new List<TrackState>();

      foreach (var file in pathList)
      {
        var recordsText = File.ReadAllText(file);

        try
        {
          trackStates.AddRange(JsonConvert.DeserializeObject<List<TrackState>>(recordsText));
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
        }
        
      }
      
      return trackStates;
    }
  }
}