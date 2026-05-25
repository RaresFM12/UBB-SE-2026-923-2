namespace UBB_SE_2026_923_2.ViewModels.Accounts
{
    using UBB_SE_2026_923_2.Models;

    public class UserItemViewModel
    {
        private readonly User user;

        public UserItemViewModel(User user)
        {
            this.user = user;
        }

        public User User => this.user;

        public string Email => this.user.Email;

        public string Username => string.IsNullOrEmpty(this.user.Username) ? "(no username)" : this.user.Username;

        public string PhoneNumber => string.IsNullOrEmpty(this.user.PhoneNumber) ? "(no phone)" : this.user.PhoneNumber;

        public bool IsAdmin => this.user.IsAdmin;

        public bool IsDisabled => this.user.IsDisabled;

        public double Opacity => this.IsDisabled ? 0.7 : 1.0;

        public string Background
        {
            get
            {
                if (this.IsDisabled)
                {
                    return "#E8F5E9";
                }

                if (this.IsAdmin)
                {
                    return "#FFF8E1";
                }

                return "#F4F8F6";
            }
        }

        public bool ShowPromote => !this.IsAdmin && !this.IsDisabled;

        public bool ShowDisable => !this.IsAdmin && !this.IsDisabled;

        public bool ShowDisabledLabel => this.IsDisabled;

        public bool ShowAdminLabel => this.IsAdmin;

        public bool ShowActiveLabel => !this.IsAdmin && !this.IsDisabled;
    }
}
