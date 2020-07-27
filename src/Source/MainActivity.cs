namespace PushApp.Source
{
    using System;
    using System.Threading.Tasks;
    using Android.App;
    using Android.Content;
    using Android.Content.PM;
    using Android.Gms.Common;
    using Android.OS;
    using Android.Preferences;
    using Android.Runtime;
    using Android.Views;
    using Android.Widget;
    using AndroidX.AppCompat.App;
    using Loymax.PublicApi.SDK;
    using Xamarin.Essentials;
    using Resource = PushApp.Resource;

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true,
        WindowSoftInputMode = SoftInput.AdjustResize)]
    public class MainActivity : AppCompatActivity
    {
        private EditText _apiText;

        private TextView _debugLabel;
        private EditText _passwordText;
        private ISharedPreferences _prefs;
        private EditText _userText;

        private string Api => _apiText.Text;
        private string User => _userText.Text;
        private string Password => _passwordText.Text;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _prefs = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);

            _debugLabel = FindViewById<TextView>(Resource.Id.debugLabel);
            _apiText = FindViewById<EditText>(Resource.Id.apiText);
            _userText = FindViewById<EditText>(Resource.Id.userText);
            _passwordText = FindViewById<EditText>(Resource.Id.passwordText);

            FindViewById<Button>(Resource.Id.authButton).Click += async delegate
            {
                try
                {
                    CheckNotificationSupport();
                    await InitializeClient(Api, User, Password);
                }
                catch (Exception ex)
                {
                    _debugLabel.Text = ex.Message;
                }
            };
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main_menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.token_action)
            {
                var token = _prefs.GetString(Constants.PrefsFcmToken, "Token is missing");
                _debugLabel.Text = token;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /// <summary>
        /// Push Notification registration example's main logic.
        /// Doesn't support captcha verification.
        /// </summary>
        /// <param name="url">Publicapi's URL</param>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        /// <returns>Async task</returns>
        private async Task InitializeClient(string url, string user, string password)
        {
            _debugLabel.Text = "In progress...";

            // Saving active URL to preferences
            var prefsEditor = _prefs.Edit();
            prefsEditor.PutString(Constants.PrefsBaseUrl, url);
            prefsEditor.Commit();

            // Resolving cached variables 
            var guid = new Guid(_prefs.GetString(Constants.PrefsDeviceGuid, "Guid is missing"));
            var token = _prefs.GetString(Constants.PrefsFcmToken, "Token is missing");

            // Client initialization
            var settings = new ClientSettings(guid, PlatformType.Android);
            var client = new MobileClient(settings) {BaseUrl = url};
            await client.Authorization(user, password);

            //Push registration
            var model = new NotificationRegisterModel {PlatformType = PlatformType.Android, Token = token};
            await client.PushNotification_RegisterAsync(model);

            // Finalizing
            _debugLabel.Text = "Push notifications successfully registered";
        }

        /// <summary>
        /// Throws Exception if Google Services does not support
        /// </summary>
        private void CheckNotificationSupport()
        {
            if (!IsPlayServicesAvailable())
                throw new Exception("Your device should support Google Play Services to get Push notifications");
        }

        /// <summary>
        /// Check if device supports Google Services
        /// </summary>
        /// <returns> <see langword="true"/> - supports, <see langword="false"/> - not</returns>
        private bool IsPlayServicesAvailable()
        {
            var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            return resultCode == ConnectionResult.Success;
        }
    }
}