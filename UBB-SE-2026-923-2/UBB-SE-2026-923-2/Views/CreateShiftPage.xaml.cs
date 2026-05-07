namespace UBB_SE_2026_923_2.Views
{
    using System;
    using System.Collections.Generic;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public sealed partial class CreateShiftPage : UserControl
    {
        public CreateShiftPage()
        {
            this.InitializeComponent();
            this.LoadStaffData();
        }

        private void LoadStaffData()
        {
            var staff = new List<string> { "Dr. Andrei Ionescu", "Dr. Elena Radu", "Farm. Mihai Pop" };
            EmployeeComboBox.ItemsSource = staff;
        }

        private void SaveShift_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem == null || ShiftDatePicker.Date == null ||
                StartTimePicker.SelectedTime == null || EndTimePicker.SelectedTime == null)
            {
                this.ShowMessage("Eroare: Te rugam sa completezi toate c�mpurile.", InfoBarSeverity.Error);
                return;
            }

            var start = StartTimePicker.SelectedTime.Value;
            var end = EndTimePicker.SelectedTime.Value;

            if (end <= start)
            {
                this.ShowMessage("Aten?ie: Ora de final trebuie sa fie dupa ora de �nceput.", InfoBarSeverity.Warning);
                return;
            }

            this.ShowMessage("Tura a fost salvata cu succes!", InfoBarSeverity.Success);

            EmployeeComboBox.SelectedIndex = -1;
        }

        private void ShowMessage(string message, InfoBarSeverity severity)
        {
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = severity;
            StatusInfoBar.IsOpen = true;
        }
    }
}