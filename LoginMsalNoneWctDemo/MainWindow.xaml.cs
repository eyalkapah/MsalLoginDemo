using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using LoginMsalNoneWctDemo.Helpers;
using Shared;
using Microsoft.Identity.Client;
using static System.Formats.Asn1.AsnWriter;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LoginMsalNoneWctDemo
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        //Set the scope for API call to user.read
        string[] scopes = new string[] { "user.read" };

        //Set the API Endpoint to Graph 'me' endpoint
        string graphAPIEndpoint = "https://graph.microsoft.com/v1.0/me";

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void Login_Clicked(object sender, RoutedEventArgs e)
        {
            AuthenticationResult authResult = null;
            var app = App.PublicClientApp;
            ApiResultTextBox.Text = string.Empty;
            TokenInfoTextBox.Text = string.Empty;

            var accounts = await app.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();
            try
            {
                authResult = await app.AcquireTokenSilent(scopes, firstAccount)
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent.
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");
                try
                {
                    authResult = await app.AcquireTokenInteractive(scopes)
                        .WithAccount(accounts.FirstOrDefault())
                        .WithPrompt(Prompt.SelectAccount)
                        .ExecuteAsync();
                }
                catch (MsalException msalex)
                {
                    ApiResultTextBox.Text = $"Error Acquiring Token:{System.Environment.NewLine}{msalex}";
                }
            }
            catch (Exception ex)
            {
                ApiResultTextBox.Text = $"Error Acquiring Token Silently:{System.Environment.NewLine}{ex}";
                return;
            }

            if (authResult != null)
            {
                ApiResultTextBox.Text = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken);
                DisplayBasicTokenInfo(authResult);
            }
        }

        private async void SignOut_Clicked(object sender, RoutedEventArgs e)
        {
            var accounts = await App.PublicClientApp.GetAccountsAsync();

            if (accounts.Any())
            {
                try
                {
                    await App.PublicClientApp.RemoveAsync(accounts.FirstOrDefault());
                    
                    ApiResultTextBox.Text = "User has signed-out";
                    
                    Avatar.Visibility = Visibility.Collapsed;
                }
                catch (MsalException ex)
                {
                    ApiResultTextBox.Text = $"Error signing-out user: {ex.Message}";
                }
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

        /// <summary>
        /// Display basic information contained in the token
        /// </summary>
        private void DisplayBasicTokenInfo(AuthenticationResult authResult)
        {
            TokenInfoTextBox.Text = "";
            if (authResult != null)
            {
                TokenInfoTextBox.Text += $"Username: {authResult.Account.Username}" + Environment.NewLine;
                TokenInfoTextBox.Text += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
            }
        }
    }
}
