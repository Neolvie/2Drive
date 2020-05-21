using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using toDrive.Domain.Models;
using Console = System.Console;

namespace TracksParser
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length < 1)
      {
        Console.WriteLine("To few parameters. Please specify input folder");
        Environment.Exit(-1);
      }

      var path = args[0];
      if (!Directory.Exists(path))
      {
        Console.WriteLine("Wrong input folder");
        Environment.Exit(-1);
      }

      var files = Directory.EnumerateFiles(path).ToList();

      if (!files.Any())
      {
        Console.WriteLine("Directory empty");
        Environment.Exit(-1);
      }

      try
      {
        var sw = Stopwatch.StartNew();
        var trackStates = TrackStatesReader.ReadTrackStates(files);
        var mergedTrackStates = TrackStatesProcessor.MergeTrackStates(trackStates);

        var outputPath = Path.Combine(path, "mergedTracks.json");
        var mergedTracksText = JsonConvert.SerializeObject(mergedTrackStates);
        File.WriteAllText(outputPath, mergedTracksText);

        sw.Stop();

        Console.WriteLine($"Total raw records: {trackStates.Count}");
        Console.WriteLine($"Total merged records: {mergedTrackStates.Count}");
        Console.WriteLine($"Location records: {mergedTrackStates.Count(x => x.Type.HasFlag(TrackStateType.Location))}");
        Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        Environment.Exit(-1);
      }
    }
  }
}
