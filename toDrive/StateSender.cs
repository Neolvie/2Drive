using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Android.Util;

namespace toDrive
{
  public static class StateSender
  {
    static readonly string TAG = typeof(StateSender).FullName;
    public static string CreateStateSendPackage()
    {
      var storage = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, "2Drive");

      if (!Directory.Exists(storage))
        Directory.CreateDirectory(storage);

      var packagePath = Path.Combine(storage, $"TrackStates_{DateTime.Now:yy-MM-dd-hh-mm-ss}.zip");

      var statesPath = Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "SavedTrackStates");

      if (!Directory.Exists(statesPath))
        return string.Empty;

      var files = Directory.EnumerateFiles(statesPath).ToList();
      if (!files.Any())
        return string.Empty;

      try
      {
        if (File.Exists(packagePath))
          File.Delete(packagePath);

        using var zip = ZipFile.Open(packagePath, ZipArchiveMode.Create);

        foreach (var file in files)
          zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);

        Log.Info(TAG, $"Package successfully created: {packagePath}, files count: {files.Count}");

        foreach (var file in files.Where(File.Exists))
          File.Delete(file);
        
        Log.Info(TAG, $"TrackStates files deleted.");

        return packagePath;
      }
      catch (Exception e)
      {
        Log.Error(TAG, $"Error preparing state package: {e}");
        return string.Empty;
      }
    }
  }
}