using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    public class SQLUsersRepository : IUsersRepository
    {
        public SQLUsersRepository()
        {
        }

        private void LoadUserData(User user, SqlConnection connectionString)
        {
            DataSet userDataFromDB = new DataSet();
            int userID = user.Id;

            SqlDataAdapter selectPeriodTrackersAdapter = new SqlDataAdapter($"SELECT * FROM PeriodTrackers WHERE userId={userID}", connectionString);
            SqlDataAdapter selectUserNotificationsAdapter = new SqlDataAdapter($"SELECT * FROM UserNotifications WHERE userId={userID}", connectionString);
            SqlDataAdapter selectUserDiscountsAdapter = new SqlDataAdapter($"SELECT * FROM UserDiscounts WHERE userId={userID}", connectionString);
            SqlDataAdapter selectPeriodNotesAdapter = new SqlDataAdapter($"SELECT * FROM PeriodNotes WHERE userId={userID}", connectionString);

            selectPeriodTrackersAdapter.Fill(userDataFromDB, "PeriodTrackers");
            selectUserNotificationsAdapter.Fill(userDataFromDB, "UserNotifications");
            selectUserDiscountsAdapter.Fill(userDataFromDB, "UserDiscounts");
            selectPeriodNotesAdapter.Fill(userDataFromDB, "PeriodNotes");

            if (userDataFromDB.Tables["PeriodTrackers"].Rows.Count > 0)
            {
                DataRow trackerRow = userDataFromDB.Tables["PeriodTrackers"].Rows[0];
                user.SetPeriodTracker(
                    DateOnly.FromDateTime((DateTime)trackerRow["startPeriodDate"]),
                    (int)trackerRow["cycleDays"],
                    (int)trackerRow["periodLasts"],
                    (int)trackerRow["PMSOption"]);
            }

            foreach (DataRow row in userDataFromDB.Tables["UserNotifications"].Rows)
            {
                if ((bool)row["favouriteItem"])
                {
                    user.AddItemToFavoriteItems((int)row["itemId"]);
                }
                if ((bool)row["stockAlert"])
                {
                    user.AddStockAlertToUser((int)row["itemId"]);
                }
            }

            foreach (DataRow row in userDataFromDB.Tables["UserDiscounts"].Rows)
            {
                user.AddUserDiscount((int)row["itemId"], (float)(decimal)row["itemDiscountPercentage"]);
            }

            foreach (DataRow row in userDataFromDB.Tables["PeriodNotes"].Rows)
            {
                user.AddPeriodNoteToUser((int)row["noteId"], (string)row["noteBody"], (bool)row["isDone"]);
            }
        }

        private User MapUserFromRow(DataRow userRow)
        {
            return new User(
                (int)userRow["userId"],
                (string)userRow["email"],
                (string)userRow["phoneNumber"],
                (string)userRow["passwordHash"],
                (bool)userRow["isAdmin"],
                (bool)userRow["isDisabled"],
                (string)userRow["username"],
                (bool)userRow["discountNotifications"],
                (int)userRow["loyaltyPoints"]);
        }

        public void AddUser(string email, string phoneNumber, string passwordHash, string username,
            bool discountNotifications, bool isDisabled = false, bool isAdmin = false, int loyaltyPoints = 0)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string insertNewUserString =
                "INSERT INTO Users VALUES " +
                $"('{email}', '{phoneNumber}', '{passwordHash}', '{isDisabled}', '{isAdmin}', '{username}', '{discountNotifications}', {loyaltyPoints})";

            using SqlConnection sqlConnection = new (connectionString);

            SqlCommand insertNewUserCommand = new (insertNewUserString, sqlConnection);

            sqlConnection.Open();
            insertNewUserCommand.ExecuteNonQuery();
        }
        public List<User> GetAllUsers()
        {
            string connectionString = SQLUtility.GetConnectionString();
            string selectUsersString = $"SELECT * FROM Users";
            using SqlConnection sqlConnection = new (connectionString);

            SqlDataAdapter selectUsersAdapter = new (selectUsersString, sqlConnection);
            DataSet usersDataFromDB = new ();
            sqlConnection.Open();
            selectUsersAdapter.Fill(usersDataFromDB, "Users");

            if (usersDataFromDB.Tables["Users"].Rows.Count == 0)
            {
                return new List<User>();
            }

            List<User> users = new ();
            foreach (DataRow userRow in usersDataFromDB.Tables["Users"].Rows)
            {
                User resultUser = MapUserFromRow(userRow);
                LoadUserData(resultUser, sqlConnection);
                users.Add(resultUser);
            }

            return users;
        }

        public User GetUserByEmail(string email)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string selectUserString = $"SELECT * FROM Users WHERE email='{email}'";

            using SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlDataAdapter selectUserAdapter = new SqlDataAdapter(selectUserString, sqlConnection);

            DataSet userDataFromDB = new DataSet();

            sqlConnection.Open();
            selectUserAdapter.Fill(userDataFromDB, "Users");

            if (userDataFromDB.Tables["Users"].Rows.Count == 0)
            {
                return null;
            }

            DataRow userRow = userDataFromDB.Tables["Users"].Rows[0];

            return GetUserById((int)userRow["userId"]);
        }

        public User GetUserById(int id)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string selectUserString = $"SELECT * FROM Users WHERE userId={id}";

            using SqlConnection sqlConnection = new (connectionString);

            SqlDataAdapter selectUserAdapter = new (selectUserString, sqlConnection);

            DataSet userDataFromDB = new ();

            sqlConnection.Open();

            selectUserAdapter.Fill(userDataFromDB, "Users");

            if (userDataFromDB.Tables["Users"].Rows.Count == 0)
            {
                return null;
            }

            DataRow userRow = userDataFromDB.Tables["Users"].Rows[0];
            User resultUser = MapUserFromRow(userRow);

            LoadUserData(resultUser, sqlConnection);

            return resultUser;
        }

        private void UpdateUserBasicInfo(User newUser, SqlConnection connectionString)
        {
            string updateUserString = $"UPDATE Users " +
                                      $"SET email = '{newUser.Email}', " +
                                      $"phoneNumber = '{newUser.PhoneNumber}', " +
                                      $"passwordHash = '{newUser.PasswordHash}', " +
                                      $"isDisabled = '{newUser.IsDisabled}', " +
                                      $"isAdmin = '{newUser.IsAdmin}', " +
                                      $"username = '{newUser.Username}', " +
                                      $"discountNotifications = '{newUser.DiscountNotifications}', " +
                                      $"loyaltyPoints = {newUser.LoyaltyPoints} " +
                                      $"WHERE userId={newUser.Id}";

            SqlCommand updateUserCommand = new (updateUserString, connectionString);
            updateUserCommand.ExecuteNonQuery();
        }

        private void UpdateUserPeriodTracker(User newUser, SqlConnection connectionString)
        {
            string deletePeriodTrackerString = $"DELETE FROM PeriodTrackers WHERE userId = {newUser.Id}";
            SqlCommand deletePeriodTrackerCommand = new (deletePeriodTrackerString, connectionString);
            deletePeriodTrackerCommand.ExecuteNonQuery();

            if (newUser.StartPeriodDate != default && newUser.StartPeriodDate != DateOnly.MinValue && newUser.StartPeriodDate != DateOnly.MaxValue)
            {
                string periodDate = $"{newUser.StartPeriodDate.Year}-{newUser.StartPeriodDate.Month}-{newUser.StartPeriodDate.Day}";
                string insertPeriodTrackerString =
                    $"INSERT INTO PeriodTrackers VALUES ({newUser.Id}, '{periodDate}', {newUser.CycleDays}, {newUser.PeriodLasts}, {newUser.PremenstrualSyndromeOption})";
                SqlCommand insertPeriodTrackerCommand = new (insertPeriodTrackerString, connectionString);
                insertPeriodTrackerCommand.ExecuteNonQuery();
            }
        }

        private void UpdateUserNotifications(User newUser, SqlConnection connectionString)
        {
            string deleteUserNotificationsString = $"DELETE FROM UserNotifications WHERE userId = {newUser.Id}";
            SqlCommand deleteUserNotificationsCommand = new (deleteUserNotificationsString, connectionString);
            deleteUserNotificationsCommand.ExecuteNonQuery();

            HashSet<int> allNotificationItems = new HashSet<int>(newUser.FavoriteItems);
            allNotificationItems.UnionWith(newUser.StockAlerts);

            foreach (int itemId in allNotificationItems)
            {
                bool isFavorite = newUser.FavoriteItems.Contains(itemId);
                bool hasStockAlert = newUser.StockAlerts.Contains(itemId);
                string insertUserNotificationsString =
                    $"INSERT INTO UserNotifications VALUES ({newUser.Id}, {itemId}, '{isFavorite}', '{hasStockAlert}')";

                SqlCommand insertUserNotificationsCommand = new (insertUserNotificationsString, connectionString);
                insertUserNotificationsCommand.ExecuteNonQuery();
            }
        }

        private void UpdateUserDiscounts(User newUser, SqlConnection connectionString)
        {
            string deleteUserDiscountsString = $"DELETE FROM UserDiscounts WHERE userId = {newUser.Id}";
            SqlCommand deleteUserDiscountsCommand = new (deleteUserDiscountsString, connectionString);
            deleteUserDiscountsCommand.ExecuteNonQuery();

            foreach (KeyValuePair<int, float> userDiscount in newUser.UserDiscounts)
            {
                string insertUserDiscountString =
                    $"INSERT INTO UserDiscounts VALUES ({newUser.Id}, {userDiscount.Key}, {userDiscount.Value})";
                SqlCommand insertUserDiscountsCommand = new (insertUserDiscountString, connectionString);
                insertUserDiscountsCommand.ExecuteNonQuery();
            }
        }

        private void UpdateUserPeriodNotes(User newUser, SqlConnection connectionString)
        {
            string deletePeriodNotesString = $"DELETE FROM PeriodNotes WHERE userId = {newUser.Id}";
            SqlCommand deletePeriodNotesCommand = new (deletePeriodNotesString, connectionString);
            deletePeriodNotesCommand.ExecuteNonQuery();

            foreach (KeyValuePair<int, Tuple<string, bool>> periodNote in newUser.PeriodNotes)
            {
                string insertPeriodNoteString =
                    $"INSERT INTO PeriodNotes VALUES ({newUser.Id}, {periodNote.Key}, '{periodNote.Value.Item1}', '{periodNote.Value.Item2}')";
                SqlCommand insertPeriodNoteCommand = new (insertPeriodNoteString, connectionString);
                insertPeriodNoteCommand.ExecuteNonQuery();
            }
        }

        public void UpdateUser(User newUser)
        {
            string connectionString = SQLUtility.GetConnectionString();
            using SqlConnection sqlConnection = new (connectionString);
            sqlConnection.Open();

            UpdateUserBasicInfo(newUser, sqlConnection);
            UpdateUserPeriodTracker(newUser, sqlConnection);
            UpdateUserNotifications(newUser, sqlConnection);
            UpdateUserDiscounts(newUser, sqlConnection);
            UpdateUserPeriodNotes(newUser, sqlConnection);
        }

        public bool UserExists(string email)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string selectUserString = $"SELECT * FROM Users WHERE email='{email}'";

            using SqlConnection sqlConnection = new (connectionString);

            SqlDataAdapter selectUserAdapter = new (selectUserString, sqlConnection);

            DataSet userDataFromDB = new ();

            sqlConnection.Open();
            selectUserAdapter.Fill(userDataFromDB, "Users");

            if (userDataFromDB.Tables["Users"].Rows.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool UserExists(int id)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string selectUserString = $"SELECT * FROM Users WHERE userId={id}";

            using SqlConnection sqlConnection = new (connectionString);
            SqlDataAdapter selectUserAdapter = new (selectUserString, sqlConnection);
            DataSet userDataFromDB = new ();

            sqlConnection.Open();
            selectUserAdapter.Fill(userDataFromDB, "Users");

            if (userDataFromDB.Tables["Users"].Rows.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool UserHasPeriodTracker(int id)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string selectUserString = $"SELECT * FROM PeriodTrackers WHERE userId={id}";

            using SqlConnection sqlConnection = new (connectionString);

            SqlDataAdapter selectUserAdapter = new (selectUserString, sqlConnection);

            DataSet userDataFromDB = new ();

            sqlConnection.Open();
            selectUserAdapter.Fill(userDataFromDB, "PeriodTrackers");

            if (userDataFromDB.Tables["PeriodTrackers"].Rows.Count > 0)
            {
                return true;
            }
            return false;
        }
    }
}