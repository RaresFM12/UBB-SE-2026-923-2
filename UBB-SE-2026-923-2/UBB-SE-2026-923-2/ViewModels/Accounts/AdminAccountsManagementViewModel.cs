namespace UBB_SE_2026_923_2.ViewModels.Accounts
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    public class AdminAccountsManagementViewModel : INotifyPropertyChanged
    {
        private readonly IUserAccountService userService;

        private string searchQuery;
        private string errorMessage;

        public ObservableCollection<UserItemViewModel> Users { get; set; }

        public AdminAccountsManagementViewModel(IUserAccountService userService)
        {
            this.userService = userService;
            this.Users = new ObservableCollection<UserItemViewModel>();
            this.searchQuery = string.Empty;

            this.LoadUsers();
        }

        public string SearchQuery
        {
            get => this.searchQuery;
            set
            {
                this.searchQuery = value;
                this.OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => this.errorMessage;
            set
            {
                this.errorMessage = value;
                this.OnPropertyChanged();
            }
        }

        public void LoadUsers()
        {
            try
            {
                this.ErrorMessage = null;
                List<User> users = this.userService.SearchUsers(string.Empty);
                this.UpdateUsers(users);
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        public void Search()
        {
            try
            {
                this.ErrorMessage = null;
                List<User> result = this.userService.SearchUsers(this.SearchQuery ?? string.Empty);
                this.UpdateUsers(result);
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        public void Promote(UserItemViewModel userItem)
        {
            try
            {
                this.ErrorMessage = null;
                this.userService.PromoteToAdmin(userItem.User);
                this.Refresh();
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        public void Disable(UserItemViewModel userItem)
        {
            try
            {
                this.ErrorMessage = null;
                this.userService.DisableAccount(userItem.User);
                this.Refresh();
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        private void Refresh()
        {
            this.Search();
        }

        private void UpdateUsers(List<User> users)
        {
            this.Users.Clear();

            foreach (User user in users)
            {
                this.Users.Add(new UserItemViewModel(user));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}