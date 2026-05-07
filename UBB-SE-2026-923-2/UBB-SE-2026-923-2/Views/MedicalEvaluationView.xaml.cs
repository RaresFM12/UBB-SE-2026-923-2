namespace UBB_SE_2026_923_2.Views
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using UBB_SE_2026_923_2.ViewModels;

    public sealed partial class MedicalEvaluationView : Page
    {
        public MedicalEvaluationViewModel ViewModel { get; }

        public MedicalEvaluationView()
        {
            this.InitializeComponent();

            this.ViewModel = App.Services.GetRequiredService<MedicalEvaluationViewModel>();
            this.DataContext = this.ViewModel;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Confirm Deletion",
                Content = "This action is permanent. Are you sure you want to delete this diagnosis?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,

                XamlRoot = this.Content.XamlRoot,
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                this.ViewModel.ExecuteDeletion();
            }
        }
    }
}
