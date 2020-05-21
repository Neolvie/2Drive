using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using toDrive.Domain.Models;

namespace TracksParser
{
  public static class TrackStatesProcessor
  {
    private static readonly TimeSpan MergingDistance = TimeSpan.FromMilliseconds(200);

    public static List<TrackState> MergeTrackStates(List<TrackState> trackStates)
    {
      var result = new List<TrackState>();

      var accelerationTracks = trackStates.Where(x => x.Type == TrackStateType.Acceleration).ToList();
      var gravityTracks = trackStates.Where(x => x.Type == TrackStateType.Gravity).ToList();
      var locationTracks = trackStates.Where(x => x.Type == TrackStateType.Location).ToList();

      List<TrackState> initialTracks;
      List<TrackState> mergingTracks;

      if (accelerationTracks.Count > gravityTracks.Count)
      {
        initialTracks = gravityTracks;
        mergingTracks = accelerationTracks;
      }
      else
      {
        initialTracks = accelerationTracks;
        mergingTracks = gravityTracks;
      }

      foreach (var initialTrack in initialTracks)
      {
        var periodFrom = initialTrack.DateTime - MergingDistance;
        var periodTo = initialTrack.DateTime + MergingDistance;

        var possibleMergingTracks =
          mergingTracks.Where(x => x.DateTime >= periodFrom && x.DateTime <= periodTo).ToList();

        if (!possibleMergingTracks.Any())
          continue;

        var mergingTrack = possibleMergingTracks.OrderBy(x => Math.Abs(initialTrack.DateTime.Ticks - x.DateTime.Ticks))
          .First();

        var possibleLocationTracks =
          locationTracks.Where(x => x.DateTime >= periodFrom && x.DateTime <= periodTo).ToList();

        var locationTrack = possibleLocationTracks
          .OrderBy(x => Math.Abs(initialTrack.DateTime.Ticks - x.DateTime.Ticks)).FirstOrDefault();

        mergingTracks.Remove(mergingTrack);
        locationTracks.Remove(locationTrack);

        var newTrack = MergeTrackState(initialTrack, mergingTrack, locationTrack);
        result.Add(newTrack);
      }

      return result.OrderBy(x => x.DateTime).ToList();
    }

    public static TrackState MergeTrackState(TrackState initialTrackState, TrackState mergingTrackState,
      TrackState locationTrackState = null)
    {
      return new TrackState()
      {
        DateTime = initialTrackState.DateTime,
        AclX = initialTrackState.Type == TrackStateType.Acceleration ? initialTrackState.AclX : mergingTrackState.AclX,
        AclY = initialTrackState.Type == TrackStateType.Acceleration ? initialTrackState.AclY : mergingTrackState.AclY,
        AclZ = initialTrackState.Type == TrackStateType.Acceleration ? initialTrackState.AclZ : mergingTrackState.AclZ,
        GvtX = initialTrackState.Type == TrackStateType.Gravity ? initialTrackState.GvtX : mergingTrackState.GvtX,
        GvtY = initialTrackState.Type == TrackStateType.Gravity ? initialTrackState.GvtY : mergingTrackState.GvtY,
        GvtZ = initialTrackState.Type == TrackStateType.Gravity ? initialTrackState.GvtY : mergingTrackState.GvtZ,
        Lat = locationTrackState?.Lat ?? default,
        Lon = locationTrackState?.Lon ?? default,
        Spd = locationTrackState?.Spd ?? default,
        Acc = locationTrackState?.Acc ?? default,
        Provider = locationTrackState?.Provider
      };
    }
  }
}