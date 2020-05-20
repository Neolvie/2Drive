using Android.App;
using Android.Content;

namespace toDrive
{
  public static class ErrorSender
  {
    public static void SendError(Activity activity, string message)
    {
      var email = new Intent(Intent.ActionSend);
      email.PutExtra(Intent.ExtraEmail, new string[] { "llc.bissoft@gmail.com" });
      email.PutExtra(Intent.ExtraSubject, "Error in application: 2Drive");
      email.PutExtra(Intent.ExtraText, message);
      email.SetType("message/rfc822");
      activity.StartActivity(email);
    }
  }
}