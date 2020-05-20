using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;

namespace toDrive
{
  [Service]
  public class TrackingService : Service, ISensorEventListener, ILocationListener
  {
    static readonly string TAG = typeof(TrackingService).FullName;

    private SensorManager _sensorManager;
    private LocationManager _locationManager;
    private Location _currentLocation;
    private List<string> _locationProviders;
    private bool _isStarted;

    public override IBinder OnBind(Intent intent)
    {
      return null;
    }

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
      if (intent.Action.Equals(Constants.ACTION_START_SERVICE))
      {
        if (_isStarted)
        {
          Log.Info(TAG, "OnStartCommand: The service is already running.");
        }
        else
        {
          _sensorManager = (SensorManager) GetSystemService(SensorService);
          InitializeLocationManager();

          _sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Gravity), SensorDelay.Normal);
          _sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.Accelerometer), SensorDelay.Normal);
          foreach (var locationProvider in _locationProviders)
          {
            _locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);
          }

          _isStarted = true;
          // RegisterForegroundService();
          Log.Info(TAG, "OnStartCommand: The service is starting.");
        }
      }
      else if(intent.Action.Equals(Constants.ACTION_STOP_SERVICE))
      {
        UnregisterSensors();
        Log.Info(TAG, "OnStartCommand: The service is stopping.");
        _isStarted = false;
      }

      return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
      _isStarted = false;
      UnregisterSensors();
      Log.Info(TAG, "OnDestroy: The started service is shutting down.");
      // var notificationManager = (NotificationManager) GetSystemService(NotificationService);
      // notificationManager.Cancel(Constants.SERVICE_RUNNING_NOTIFICATION_ID);

      base.OnDestroy();
    }

    void RegisterForegroundService()
    {
      var channelId = CreateNotificationChannel(Constants.NOTIFICATION_BROADCAST_ACTION, "2Drive.TrackingService");

      var notification = new Notification.Builder(this, channelId)
        .SetContentTitle(Resources.GetString(Resource.String.app_name))
        .SetContentText("Route tracking")
        .SetContentIntent(BuildIntentToShowMainActivity())
        .SetOngoing(true)
        .AddAction(BuildStopServiceAction())
        .Build();

      // Enlist this instance of the service as a foreground service
      StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);
    }

    private string CreateNotificationChannel(string channelId, string channelName)
    {
      var channel = new NotificationChannel(channelId, channelName, NotificationImportance.None)
      {
        LightColor = Xamarin.Essentials.Resource.Color.ripple_material_light,
        LockscreenVisibility = NotificationVisibility.Public
      };

      var service = (NotificationManager)GetSystemService(NotificationService);
      service.CreateNotificationChannel(channel);
      return channelId;
    }

    private PendingIntent BuildIntentToShowMainActivity()
    {
      var notificationIntent = new Intent(this, typeof(MainActivity));
      notificationIntent.SetAction(Constants.ACTION_MAIN_ACTIVITY);
      notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
      notificationIntent.PutExtra(Constants.SERVICE_STARTED_KEY, true);

      var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
      return pendingIntent;
    }

    private Notification.Action BuildStopServiceAction()
    {
      var stopServiceIntent = new Intent(this, GetType());
      stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
      var stopServicePendingIntent = PendingIntent.GetService(this, 0, stopServiceIntent, 0);

      var builder = new Notification.Action.Builder(Android.Resource.Drawable.IcMediaPause,
        "STOP",
        stopServicePendingIntent);
      return builder.Build();

    }

    private void UnregisterSensors()
    {
      _sensorManager.UnregisterListener(this);
      _locationManager.RemoveUpdates(this);
    }

    private void InitializeLocationManager()
    {
      _locationManager = (LocationManager)GetSystemService(LocationService);
      var criteriaForLocationService = new Criteria
      {
        Accuracy = Accuracy.Medium
      };

      IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

      if (acceptableLocationProviders.Any())
      {
        _locationProviders = acceptableLocationProviders.ToList();
      }
      else
      {
        _locationProviders = new List<string>();
      }
    }

    public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
    {
    }

    public void OnSensorChanged(SensorEvent e)
    {
      if (e.Sensor.Type == SensorType.Accelerometer)
      {
        var trackState = TrackStateFactory.CreateTrackState(DateTime.Now, e.Values[0], e.Values[1], e.Values[2]);
        TrackStateFactory.AddTrackState(trackState);
      }

      if (e.Sensor.Type == SensorType.Gravity)
      {
        var trackState = TrackStateFactory.CreateTrackState(DateTime.Now, 0,0,0, e.Values[0], e.Values[1], e.Values[2]);
        TrackStateFactory.AddTrackState(trackState);
      }

      // Log.Info(TAG, $"X:{e.Values[0]}, Y:{e.Values[1]}, Z:{e.Values[2]}");
    }

    public void OnLocationChanged(Location location)
    {
      _currentLocation = location;
      if (_currentLocation == null)
        return;

      var trackState = TrackStateFactory.CreateTrackState(DateTime.Now, 0, 0, 0, 0, 0, 0, _currentLocation.Latitude,
        _currentLocation.Longitude, _currentLocation.Speed, _currentLocation.Provider, _currentLocation.Accuracy);

      TrackStateFactory.AddTrackState(trackState);
      
      // Log.Info(TAG, $"Lat:{_currentLocation.Latitude}, Long:{_currentLocation.Longitude}, Spd:{_currentLocation.Speed}");
    }

    public void OnProviderDisabled(string provider)
    {

    }

    public void OnProviderEnabled(string provider)
    {

    }

    public void OnStatusChanged(string provider, Availability status, Bundle extras)
    {

    }
  }
}