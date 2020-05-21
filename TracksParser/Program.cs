using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

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

      var trackStates = TrackStatesReader.ReadTrackStates(files);

      try
      {
        
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        Environment.Exit(-1);
      }
    }
  }
}
