using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.ViewModels.Accounts
{
    public class AdminAccountsManagementViewModel : INotifyPropertyChanged
    {
        private readonly IUserAccountService userService;

        private string searchQuery;
        private string errorMessage;

        public ObservableCollection<UserItemViewModel> Users { get; set; }

        public AdminAccountsManagementViewModel(IUserAccountService userService)
        {
            this.userService = userService;
            Users = new ObservableCollection<UserItemViewModel>();
            searchQuery = string.Empty;

            LoadUsers();
        }

        public string SearchQuery
        {
            get => searchQuery;
            set
            {
                searchQuery = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged();
            }
        }

        public void LoadUsers()
        {
            try
            {
                ErrorMessage = null;
                List<User> users = userService.SearchUsers(string.Empty);
                UpdateUsers(users);
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
            }
        }

        public void Search()
        {
            try
            {
                ErrorMessage = null;
                List<User> result = userService.SearchUsers(SearchQuery ?? string.Empty);
                UpdateUsers(result);
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
            }
        }

        public void Promote(UserItemViewModel userItem)
        {
            try
            {
                ErrorMessage = null;
                userService.PromoteToAdmin(userItem.User);
                Refresh();
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
            }
        }

        public void Disable(UserItemViewModel userItem)
        {
            try
            {
                ErrorMessage = null;
                userService.DisableAccount(userItem.User);
                Refresh();
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
            }
        }

        private void Refresh()
        {
            Search();
        }

        private void UpdateUsers(List<User> users)
        {
            Users.Clear();

            foreach (User user in users)
            {
                Users.Add(new UserItemViewModel(user));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}