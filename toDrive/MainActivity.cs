using System;
using System.IO;
using System.Threading;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;
using FileProvider = Android.Support.V4.Content.FileProvider;

namespace toDrive
{
  [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
  public class MainActivity : AppCompatActivity
  {
    static readonly string Tag = typeof(MainActivity).FullName;

    private bool _isStarted;
    private Button _stopServiceButton;
    private Button _startServiceButton;
    private Button _sendButton;
    private Intent _startServiceIntent;
    private Intent _stopServiceIntent;
    private TextView _stateTextView;

    protected override void OnStart()
    {
      try
      {
        base.OnStart();
        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted)
        {
          RequestPermissions(new[] { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation }, 0);
        }

        if (PackageManager.CheckPermission(Manifest.Permission.ReadExternalStorage, PackageName) != Permission.Granted
            && PackageManager.CheckPermission(Manifest.Permission.WriteExternalStorage, PackageName) != Permission.Granted)
        {
          var permissions = new[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage };
          RequestPermissions(permissions, 1);
        }
      }
      catch (Exception e)
      {
        ErrorSender.SendError(this, e.ToString());
        throw;
      }
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
      try
      {
        base.OnCreate(savedInstanceState);
        Xamarin.Essentials.Platform.Init(this, savedInstanceState);
        SetContentView(Resource.Layout.main_activity);
        OnNewIntent(Intent);
        Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

        if (savedInstanceState != null)
        {
          _isStarted = savedInstanceState.GetBoolean(Constants.SERVICE_STARTED_KEY);
        }

        _sendButton = FindViewById<Button>(Resource.Id.sendData);
        _sendButton.Click += SendButtonOnClick;

        _stateTextView = FindViewById<TextView>(Resource.Id.state);
        Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;

        _startServiceIntent = new Intent(this, typeof(TrackingService));
        _startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);

        _stopServiceIntent = new Intent(this, typeof(TrackingService));
        _stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);

        _startServiceButton = FindViewById<Button>(Resource.Id.startTrack);
        _stopServiceButton = FindViewById<Button>(Resource.Id.stopTrack);

        if (_isStarted)
        {
          _stopServiceButton.Click += StopServiceButtonOnClick;
          _stopServiceButton.Enabled = true;
          _startServiceButton.Enabled = false;
          _sendButton.Enabled = false;

          try
          {
            Accelerometer.Start(SensorSpeed.UI);
          }
          catch (Exception ex)
          {
            Log.Error(Tag, ex.ToString());
          }
        }
        else
        {
          _startServiceButton.Click += StartServiceButtonOnClick;
          _startServiceButton.Enabled = true;
          _stopServiceButton.Enabled = false;
          _sendButton.Enabled = true;

          try
          {
            Accelerometer.Stop();
            _stateTextView.Text = string.Empty;
          }
          catch (Exception ex)
          {
            Log.Error(Tag, ex.ToString());
          }
        }
      }
      catch (Exception e)
      {
        ErrorSender.SendError(this, e.ToString());
        throw;
      }
    }

    private void SendButtonOnClick(object sender, EventArgs e)
    {
      _sendButton.Enabled = false;
      var package = StateSender.CreateStateSendPackage();
      if (string.IsNullOrEmpty(package))
      {
        _sendButton.Enabled = true;
        return;
      }

      var path = FileProvider.GetUriForFile(this, $"{this.ApplicationContext.PackageName}.provider", new Java.IO.File(package));

      var email = new Intent(Intent.ActionSend);
      email.PutExtra(Intent.ExtraEmail, new string[] { "llc.bissoft@gmail.com" });
      email.PutExtra(Intent.ExtraSubject, "Track states");
      email.PutExtra(Intent.ExtraStream, path);
      email.SetType("message/rfc822");
      StartActivity(email);

      _sendButton.Enabled = true;
    }

    private void StopServiceButtonOnClick(object sender, EventArgs e)
    {
      _stopServiceButton.Click -= StopServiceButtonOnClick;
      _stopServiceButton.Enabled = false;

      Log.Info(Tag, "User stopped tracking.");
      StopService(_stopServiceIntent);
      _isStarted = false;

      try
      {
        Accelerometer.Stop();
        _stateTextView.Text = string.Empty;
      }
      catch (Exception ex)
      {
        Log.Error(Tag, ex.ToString());
      }

      _startServiceButton.Click += StartServiceButtonOnClick;
      _startServiceButton.Enabled = true;
      _sendButton.Enabled = true;
    }

    private void StartServiceButtonOnClick(object sender, EventArgs e)
    {
      _startServiceButton.Enabled = false;
      _startServiceButton.Click -= StartServiceButtonOnClick;

      StartService(_startServiceIntent);
      Log.Info(Tag, "User started tracking.");

      try
      {
        Accelerometer.Start(SensorSpeed.UI);
      }
      catch (Exception ex)
      {
        Log.Error(Tag, ex.ToString());
      }

      _isStarted = true;
      _stopServiceButton.Click += StopServiceButtonOnClick;
      _stopServiceButton.Enabled = true;
      _sendButton.Enabled = false;
    }

    void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
      var data = e.Reading;
      _stateTextView.Text = $"X: {data.Acceleration.X}, Y: {data.Acceleration.Y}, Z: {data.Acceleration.Z}";
    }

    protected override void OnNewIntent(Intent intent)
    {
      if (intent == null)
      {
        return;
      }

      var bundle = intent.Extras;
      if (bundle != null)
      {
        if (bundle.ContainsKey(Constants.SERVICE_STARTED_KEY))
        {
          _isStarted = true;
        }
      }
    }

    protected override void OnSaveInstanceState(Bundle outState)
    {
      outState.PutBoolean(Constants.SERVICE_STARTED_KEY, _isStarted);
      base.OnSaveInstanceState(outState);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
    {
      Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

      base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
  }
}