using System;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using MultiHubApp.Models;
using MultiHubApp.Services;

namespace MultiHubApp.Views
{
    public partial class HomePage : ContentPage
    {
        private readonly LocalProfileService _profileService = new LocalProfileService();

        public HomePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                var profile = await _profileService.LoadProfileAsync();
                if (profile != null)
                {
                    NameLabel.Text = $"{profile.FirstName} {profile.LastName}";
                    StudentNumberLabel.Text = profile.StudentNumber ?? "";
                    StudentNumberLabel.IsVisible = !string.IsNullOrWhiteSpace(profile.StudentNumber);
                    DeptLabel.Text = profile.GetType().GetProperty("Department")?.GetValue(profile) as string
                                     ?? "Bartın Üniversitesi Bilgisayar Mühendisliği Bölümü";

                    if (!string.IsNullOrEmpty(profile.PhotoPath) && File.Exists(profile.PhotoPath))
                    {
                        ProfileImage.Source = ImageSource.FromFile(profile.PhotoPath);
                    }
                    else
                    {
                        ProfileImage.Source = "avatar_placeholder.png";
                    }
                }
                else
                {
                    
                    NameLabel.Text = "";
                    StudentNumberLabel.Text = "";
                    StudentNumberLabel.IsVisible = false;
                    DeptLabel.Text = "Bartın Üniversitesi Bilgisayar Mühendisliği Bölümü";
                    ProfileImage.Source = "avatar_placeholder.png";
                }
            }
            catch
            {
                
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            try
            {
                SecureStorage.Remove("is_logged_in");
            }
            catch { /* ignore */ }

           
            await Shell.Current.GoToAsync("//login");
        }
    }
}