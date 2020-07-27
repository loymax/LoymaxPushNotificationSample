namespace PushApp.Source
{
    using System;
    using Android.App;
    using Android.Graphics;
    using Android.OS;
    using Android.Preferences;
    using Android.Support.V4.App;
    using Android.Util;
    using Firebase.Messaging;

    [Service]
    [IntentFilter(new[] {"com.google.firebase.MESSAGING_EVENT"})]
    public class PushNotificationFirebaseMessagingService : FirebaseMessagingService
    {
        private const string Tag = "PushNotificationFirebaseMessagingService";

        private const string ChannelId = "TEST_CHANNEL_ID";
        private const string ChannelName = "Notification";

        /// <summary>
        ///     Event on notification token update
        /// </summary>
        /// <param name="refreshedToken">Updated notification token</param>
        public override void OnNewToken(string refreshedToken)
        {
            Log.Debug(Tag, "Refreshed token: " + refreshedToken);
            SaveTokenToStorage(refreshedToken);
        }

        /// <summary>
        ///     Saves notification token into preferences
        /// </summary>
        /// <param name="token">Notification token</param>
        private void SaveTokenToStorage(string token)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);
            var prefsEditor = prefs.Edit();

            prefsEditor.PutString(Constants.PrefsFcmToken, token);

            if (prefs.GetString(Constants.PrefsDeviceGuid, "") == "")
                prefsEditor.PutString(Constants.PrefsDeviceGuid, Guid.NewGuid().ToString());

            prefsEditor.Commit();
        }

        /// <summary>
        ///     Event on notification receive
        /// </summary>
        /// <param name="message">Message model</param>
        public override void OnMessageReceived(RemoteMessage message)
        {
            var data = message.Data;
            if (data != null)
            {
                ShowSmallNotification(null,
                    data["title"],
                    data["message"],
                    null);
            }
            else
            {
                var notificationData = message.GetNotification();
                ShowSmallNotification(null,
                    notificationData.Title,
                    notificationData.Body,
                    null);
            }
        }

        /// <summary>
        ///     Main logic for notification rendering
        /// </summary>
        /// <param name="icon">Lagge notification icon</param>
        /// <param name="title">Title of notification</param>
        /// <param name="message">Body of notification</param>
        /// <param name="resultPendingIntent">Intent action on notification click</param>
        private void ShowSmallNotification(Bitmap icon, string title, string message, PendingIntent resultPendingIntent)
        {
            var notificationManager = (NotificationManager) GetSystemService(NotificationService);
            var inboxStyle = new NotificationCompat.InboxStyle();

            inboxStyle.AddLine(message);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.High);
                channel.EnableVibration(true);
                channel.EnableLights(true);
                channel.SetShowBadge(true);
                notificationManager.CreateNotificationChannel(channel);
            }

            var notificationBuilder = new NotificationCompat.Builder(ApplicationContext, ChannelId)
                .SetVibrate(new long[] {0, 100})
                .SetAutoCancel(true)
                .SetContentTitle(title)
                .SetContentIntent(resultPendingIntent)
                .SetSmallIcon(Resource.Mipmap.ic_launcher)
                .SetStyle(inboxStyle)
                .SetLargeIcon(icon)
                .SetContentText(message);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O) notificationBuilder.SetChannelId(ChannelId);

            notificationManager.Notify(ChannelId, 1, notificationBuilder.Build());
        }
    }
}