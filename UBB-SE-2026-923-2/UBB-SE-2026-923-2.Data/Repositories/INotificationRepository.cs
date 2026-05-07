namespace UBB_SE_2026_923_2.Repositories
{
    public interface INotificationRepository
    {
        void AddNotification(int recipientStaffId, string title, string message);
    }
}
