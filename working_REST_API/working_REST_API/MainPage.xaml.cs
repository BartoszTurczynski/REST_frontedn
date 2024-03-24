using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace working_REST_API
{
    public partial class MainPage : ContentPage
    {
        public class User
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        private HttpClient _httpClient;

        private const string apiUrl = "https://192.168.88.122:44304/users";

        public MainPage()
        {
            InitializeComponent();

            // Create HttpClient with custom handler to bypass SSL certificate validation
            _httpClient = CreateHttpClientWithBypassCertificateValidation();
        }

        private HttpClient CreateHttpClientWithBypassCertificateValidation()
        {
            // Create HttpClientHandler with certificate validation disabled
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            return new HttpClient(handler);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefreshUsers();
        }

        private async void OnAddClicked(object sender, EventArgs e)
        {
            var userName = await DisplayPromptAsync("Add User", "Enter user name:");
            if (!string.IsNullOrWhiteSpace(userName))
            {
                var user = new User { name = userName };
                await PostUserAsync(user);
                await RefreshUsers();
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is SwipeItem button && button.BindingContext is User user)
            {
                var confirmed = await DisplayAlert("Delete User", $"Are you sure you want to delete {user.name}?", "Yes", "No");
                if (confirmed)
                {
                    await DeleteUserAsync(user.id);
                    await RefreshUsers();
                }
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is SwipeItem button && button.BindingContext is User user)
            {
                var newName = await DisplayPromptAsync("Edit User", "Enter new name:", initialValue: user.name);
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    user.name = newName;
                    await PutUserAsync(user);
                    await RefreshUsers();
                }
            }
        }

        private async System.Threading.Tasks.Task RefreshUsers()
        {
            var users = await GetUserAsync<List<User>>();
            usersListView.ItemsSource = users;
            usersListView.HeightRequest = (users.Count * 62 + 10);
        }

        private async System.Threading.Tasks.Task<List<User>> GetUserAsync<T>()
        {
            var response = await _httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
                return JsonConvert.DeserializeObject<List<User>>(content);
            }
            return default;
        }

        private async System.Threading.Tasks.Task PostUserAsync(User user)
        {
            var userJson = JsonConvert.SerializeObject(user);
            var content = new StringContent(userJson, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();
        }

        private async System.Threading.Tasks.Task PutUserAsync(User user)
        {
            var userJson = JsonConvert.SerializeObject(user);
            var content = new StringContent(userJson, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{apiUrl}/{user.id}", content);
            response.EnsureSuccessStatusCode();
        }

        private async System.Threading.Tasks.Task DeleteUserAsync(int userId)
        {
            var response = await _httpClient.DeleteAsync($"{apiUrl}/{userId}");
            response.EnsureSuccessStatusCode();
        }
    }
}
