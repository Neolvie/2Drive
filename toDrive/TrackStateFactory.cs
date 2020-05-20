using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Util;
using Newtonsoft.Json;
using toDrive.Models;

namespace toDrive
{
  public static class TrackStateFactory
  {
    static readonly string TAG = typeof(TrackStateFactory).FullName;

    private static readonly TimeSpan TimePortion = TimeSpan.FromSeconds(30);

    private static DateTime _firstTrackStateTime = DateTime.MinValue;

    private static List<List<TrackState>> SaveWaitingTrackStates { get; set; } = new List<List<TrackState>>();

    private static object _syncState = new object();

    private static List<TrackState> CurrentStates { get; set; } = new List<TrackState>();

    public static TrackState CreateTrackState(DateTime stateDateTime, float aclX = 0, float aclY = 0, float aclZ = 0,
      float gvtX = 0, float gvtY = 0, float gvtZ = 0, double lat = 0, double lon = 0, float spd = 0,
      string provider = "", float accuracy = 0)
    {
      return new TrackState()
      {
        DateTime = stateDateTime,
        AclX = aclX,
        AclY = aclY,
        AclZ = aclZ,
        GvtX = gvtX,
        GvtY = gvtY,
        GvtZ = gvtZ,
        Lat = lat,
        Lon = lon,
        Spd = spd,
        Provider = provider,
        Acc = accuracy
      };
    }

    public static void AddTrackState(TrackState trackState)
    {
      if (trackState.DateTime - _firstTrackStateTime > TimePortion)
      {
        SaveWaitingTrackStates.Add(CurrentStates);
        CurrentStates = new List<TrackState>();
        _firstTrackStateTime = trackState.DateTime;
      }

      CurrentStates.Add(trackState);

      Task.Run(SaveTrackStates);
    }

    private static void SaveTrackStates()
    {
      lock (_syncState)
      {
        if (!SaveWaitingTrackStates.Any())
          return;

        var trackStates = SaveWaitingTrackStates.First();
        SaveWaitingTrackStates.Remove(trackStates);

        if (!trackStates.Any())
          return;

        var records = JsonConvert.SerializeObject(trackStates);

        var recordsFolder = System.IO.Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "SavedTrackStates");
        if (!Directory.Exists(recordsFolder))
          Directory.CreateDirectory(recordsFolder);

        var fileName =
          System.IO.Path.Combine(recordsFolder, trackStates.First().DateTime.ToString("s").Replace(':', '-'));

        File.WriteAllText(fileName, records);
        Log.Info(TAG, $"TrackStates saved: {fileName}. Count: {trackStates.Count}");
      }
    }
  }
}