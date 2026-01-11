using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel; // MediaPicker
using Microsoft.Maui.Storage;          // FileResult, FileSystem, SecureStorage
using Microsoft.Maui.Controls;
using MultiHubApp.Models;
using MultiHubApp.Services;

namespace MultiHubApp.Views
{
    public partial class RegisterPage : ContentPage
    {
        private readonly AuthServiceRest _authService = new AuthServiceRest();
        private readonly LocalProfileService _profileService = new LocalProfileService();
        private FileResult _pickedPhoto;

        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void OnPickPhotoClicked(object sender, EventArgs e)
        {
            try
            {
                _pickedPhoto = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Pick a profile photo"
                });

                if (_pickedPhoto != null)
                {
                    var stream = await _pickedPhoto.OpenReadAsync();
                    AvatarImage.Source = ImageSource.FromStream(() => stream);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Photo Error", $"Photo selection failed: {ex.Message}", "OK");
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await HandleRegisterAsync();
        }

        private async Task HandleRegisterAsync()
        {
            SetLoading(true);

            try
            {
                var firstName = FirstNameEntry.Text?.Trim();
                var lastName = LastNameEntry.Text?.Trim();
                var studentNumber = StudentNumberEntry.Text?.Trim();
                var email = EmailEntry.Text?.Trim();
                var password = PasswordEntry.Text;
                var confirm = ConfirmPasswordEntry.Text;

                if (string.IsNullOrEmpty(firstName) ||
                    string.IsNullOrEmpty(lastName) ||
                    string.IsNullOrEmpty(studentNumber) ||
                    string.IsNullOrEmpty(email) ||
                    string.IsNullOrEmpty(password) ||
                    string.IsNullOrEmpty(confirm))
                {
                    await DisplayAlert("Validation", "Please fill all fields.", "OK");
                    return;
                }

                if (password != confirm)
                {
                    await DisplayAlert("Validation", "Passwords do not match.", "OK");
                    return;
                }

                var (success, message) = await _authService.RegisterAsync(email, password);
                if (!success)
                {
                    await DisplayAlert("Register Failed", message, "OK");
                    return;
                }

                var profile = new UserProfile
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    StudentNumber = studentNumber,
                    CreatedAt = DateTime.UtcNow
                };

                if (_pickedPhoto != null)
                {
                    using var stream = await _pickedPhoto.OpenReadAsync();
                    var savedPath = await _profileService.SavePhotoAsync(_pickedPhoto.FileName, stream);
                    if (!string.IsNullOrEmpty(savedPath))
                        profile.PhotoPath = savedPath;
                }

                var uid = await _authService.GetUserIdAsync();
                if (!string.IsNullOrEmpty(uid))
                    profile.UserId = uid;

                await _profileService.SaveProfileAsync(profile);

                try
                {
                    await SecureStorage.SetAsync("is_logged_in", "1");
                }
                catch { }

                await DisplayAlert("Success", "Registration complete.", "OK");

                await Shell.Current.GoToAsync("//main/home");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void SetLoading(bool isLoading)
        {
            LoadingIndicator.IsRunning = isLoading;
            LoadingIndicator.IsVisible = isLoading;
            RegisterButton.IsEnabled = !isLoading;
            PickPhotoButton.IsEnabled = !isLoading;
        }
    }
}