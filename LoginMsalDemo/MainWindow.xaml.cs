using CommunityToolkit.Authentication;
using LoginMsalDemo.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Shared;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LoginMsalDemo
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly IProvider _provider;

        private const string GraphApiEndpoint = "https://graph.microsoft.com/v1.0/me";

        public Person Person { get; set; }

        public MainWindow()
        {
            this.InitializeComponent();

            _provider = ProviderManager.Instance.GlobalProvider;

            ProviderManager.Instance.ProviderStateChanged += (s, e) => UpdateState();

            UpdateState();
        }

        private async void UpdateState()
        {
            switch (_provider.State)
            {
                case ProviderState.SignedIn:
                    {
                        var accessToken = await _provider.GetTokenAsync();

                        ApiResultTextBox.Text = await GetHttpContentWithToken(GraphApiEndpoint, accessToken);

                        TokenInfoTextBox.Text = $"Token: {accessToken}{Environment.NewLine}Username: {((MsalProvider)_provider).Account.Username}";

                        Person = await UpdatePictureAsync(accessToken);

                        Avatar.ProfilePicture = new BitmapImage(new Uri(Person.PicturePath));

                        Avatar.Visibility = Visibility.Visible;

                        break;
                    }
                case ProviderState.SignedOut:
                    ApiResultTextBox.Text = "User has signed-out";

                    TokenInfoTextBox.Text = "";

                    Avatar.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void Login_Clicked(object sender, RoutedEventArgs e)
        {
            if (_provider != null)
            {
                switch (_provider.State)
                {
                    case ProviderState.Loading:
                        break;
                    case ProviderState.SignedOut:
                        _provider.SignInAsync();
                        break;
                    case ProviderState.SignedIn:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void SignOut_Clicked(object sender, RoutedEventArgs e)
        {
            if (_provider.State == ProviderState.SignedIn)
            {
                _provider.SignOutAsync();
            }
        }

        private static async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new HttpClient();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.SendAsync(request);

                var content = await response.Content.ReadAsStringAsync();

                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private static async Task<Person> UpdatePictureAsync(string accessToken)
        {
            //var graphToken = await GetTokenSilentAsync(new string[] { "User.Read" });
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new Person();
            }

            var httpClient = new HttpClient();

            var person = new Person();
            using var profileMsg = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
            profileMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var profileResponseTask = httpClient.SendAsync(profileMsg);

            using var photoMsg = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/beta/me/photo/$value");
            photoMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var photoResponseTask = httpClient.SendAsync(photoMsg);

            var profileResponse = await profileResponseTask;
            var photoResponse = await photoResponseTask;

            if (profileResponse.IsSuccessStatusCode)
            {
                var content = await profileResponse.Content.ReadAsStringAsync();
                var data = JObject.Parse(content);

                if (data is not null)
                {
                    person.Email = data["userPrincipalName"].ToString();
                    person.Firstname = data["givenName"].ToString();
                }
            }

            if (!photoResponse.IsSuccessStatusCode) 
                return person;
            
            const string pictureFileName = "profile.png";

            await using var stream = await photoResponse.Content.ReadAsStreamAsync();

            person.PicturePath = await FileWriter.WriteBitmapAsync(stream, pictureFileName);

            return person;
        }
    }
}
