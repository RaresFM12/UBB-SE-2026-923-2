using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Repositories
{
    public class SQLItemsRepository : IItemsRepository
    {
        private const float MinDiscount = 0f;
        private const float MaxDiscount = 1f;
        private const float PercentageDivisor = 100f;
        private const string TestPrescriptionId = "testPrescription";
        private const string DefaultPrescriptionItemName = "Nurofen Express";
        private const int DefaultPrescriptionPills = 40;
        private const int SingleBoxQuantity = 1;
        private const int NoCandidateItemId = -1;
        private const int NoCandidateQuantity = -1;
        private const int EmptyQuantity = 0;
        private const string ImagePathDefault = "..\\..\\Assets\\placeholder.png";
        public SQLItemsRepository()
        {
        }

        public void AddItem(string name, string producer, string category,
            float price, int numberOfPills,
            string label = "", string description = "", string imagePath = ImagePathDefault,
            float discount = 0f)
        {
            string connectionString = SQLUtility.GetConnectionString();
            System.Diagnostics.Debug.WriteLine($"Connection string in SQLItemsRepository.AddItem: {connectionString}");
            string insertNewItemString =
                "INSERT INTO Items (name, price, category, numberOfPills, producer, imagePath, quantity, label, description, discountPercentage) " +
                $"VALUES ('{name}', {price}, '{category}', {numberOfPills}, '{producer}', '{imagePath}', 0, '{label}', '{description}', {discount})";

            using SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlCommand insertNewItemCommand = new SqlCommand(insertNewItemString, sqlConnection);

            sqlConnection.Open();
            insertNewItemCommand.ExecuteNonQuery();
        }

        public void AddItemWithQuantity(string name, string producer, string category,
            float price, int numberOfPills,
            int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches,
            string label = "", string description = "", string imagePath = ImagePathDefault,
            float discount = 0f)
        {
            string connectionString = SQLUtility.GetConnectionString();
            System.Diagnostics.Debug.WriteLine($"Connection string in SQLItemsRepository.AddItemWithQuantity: {connectionString}");
            string insertNewItemString =
                "INSERT INTO Items (name, price, category, numberOfPills, producer, imagePath, quantity, label, description, discountPercentage) " +
                $"VALUES ('{name}', {price}, '{category}', {numberOfPills}, '{producer}', '{imagePath}', {quantity}, '{label}', '{description}', {discount})";

            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand insertNewItemCommand = new SqlCommand(insertNewItemString, sqlConnection);
            sqlConnection.Open();
            insertNewItemCommand.ExecuteNonQuery();

            string insertActiveSubstancesString = $"INSERT INTO ItemSubstances (itemId, name, concentration) VALUES ";
            for (int i = 0; i < activeSubstances.Count; i++)
            {
                if (i == activeSubstances.Count - 1)
                {
                    insertActiveSubstancesString +=
                        $"((SELECT MAX(itemId) FROM Items),'{activeSubstances.ElementAt(i).Key}', {activeSubstances.ElementAt(i).Value});";
                }
                else
                {
                    insertActiveSubstancesString +=
                        $"((SELECT MAX(itemId) FROM Items),'{activeSubstances.ElementAt(i).Key}', {activeSubstances.ElementAt(i).Value}), ";
                }
            }

            SqlCommand insertActiveSubstancesCommand = new SqlCommand(insertActiveSubstancesString, sqlConnection);
            insertActiveSubstancesCommand.ExecuteNonQuery();

            string insertBatchesString = $"INSERT INTO ItemExpirationDates (itemId, expirationDate, numberOfPacks) VALUES ";
            for (int i = 0; i < batches.Count; i++)
            {
                if (i == batches.Count - 1)
                {
                    insertBatchesString +=
                        $"((SELECT MAX(itemId) FROM Items), '{batches.ElementAt(i).Key}', {batches.ElementAt(i).Value});";
                }
                else
                {
                    insertBatchesString +=
                        $"((SELECT MAX(itemId) FROM Items), '{batches.ElementAt(i).Key}', {batches.ElementAt(i).Value}), ";
                }
            }

            SqlCommand insertBatchesCommand = new SqlCommand(insertBatchesString, sqlConnection);
            insertBatchesCommand.ExecuteNonQuery();
        }

        public void RemoveItemById(int idToBeRemoved)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string deleteItemString = $"DELETE FROM Items WHERE itemId={idToBeRemoved}";
            string deleteActiveSubstancesCommandString = $"DELETE FROM ItemSubstances WHERE itemId = {idToBeRemoved}";
            string deleteBatchesCommandString = $"DELETE FROM ItemExpirationDates WHERE itemId = {idToBeRemoved}";
            string deleteItemsFromOrdersCommandString = $"DELETE FROM OrderItems WHERE itemId = {idToBeRemoved}";
            string deleteUserNotificationsCommandString = $"DELETE FROM UserNotifications WHERE itemId = {idToBeRemoved}";
            string deleteUserDiscountsCommandString = $"DELETE FROM UserDiscounts WHERE itemId = {idToBeRemoved}";

            using SqlConnection sqlConnection = new SqlConnection(connectionString);

            sqlConnection.Open();

            SqlCommand deleteActiveSubstancesCommand = new SqlCommand(deleteActiveSubstancesCommandString, sqlConnection);
            deleteActiveSubstancesCommand.ExecuteNonQuery();

            SqlCommand deleteBatchesCommand = new SqlCommand(deleteBatchesCommandString, sqlConnection);
            deleteBatchesCommand.ExecuteNonQuery();

            SqlCommand deleteItemsFromOrdersCommand = new SqlCommand(deleteItemsFromOrdersCommandString, sqlConnection);
            deleteItemsFromOrdersCommand.ExecuteNonQuery();

            SqlCommand deleteUserNotificationsCommand = new SqlCommand(deleteUserNotificationsCommandString, sqlConnection);
            deleteUserNotificationsCommand.ExecuteNonQuery();

            SqlCommand deleteUserDiscountsCommand = new SqlCommand(deleteUserDiscountsCommandString, sqlConnection);
            deleteUserDiscountsCommand.ExecuteNonQuery();

            SqlCommand deleteItemCommand = new SqlCommand(deleteItemString, sqlConnection);
            deleteItemCommand.ExecuteNonQuery();
        }

        public Item GetItemById(int id)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string selectItemString = $"SELECT * FROM Items WHERE itemId={id}";
            string selectActiveSubstances = $"SELECT name, concentration FROM ItemSubstances WHERE itemId={id}";
            string selectBatches = $"SELECT expirationDate, numberOfPacks FROM ItemExpirationDates WHERE itemId={id}";

            using SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlDataAdapter itemAdapter = new SqlDataAdapter(selectItemString, sqlConnection);
            SqlDataAdapter activeSubstancesAdapter = new SqlDataAdapter(selectActiveSubstances, sqlConnection);
            SqlDataAdapter batchesAdapter = new SqlDataAdapter(selectBatches, sqlConnection);
            DataSet itemDataFromDb = new DataSet();

            sqlConnection.Open();
            itemAdapter.Fill(itemDataFromDb, "Items");
            activeSubstancesAdapter.Fill(itemDataFromDb, "ActiveSubstances");
            batchesAdapter.Fill(itemDataFromDb, "Batches");

            DataRow resultRow = itemDataFromDb.Tables["Items"].Rows[0];

            Item resultItem = new Item(
                (int)resultRow["itemId"],
                (string)resultRow["name"],
                (string)resultRow["producer"],
                (string)resultRow["category"],
                (float)(decimal)resultRow["price"],
                (int)resultRow["numberOfPills"],
                (string)resultRow["label"],
                (string)resultRow["description"],
                (string)resultRow["imagePath"],
                (float)(decimal)resultRow["discountPercentage"]);

            foreach (DataRow substanceRow in itemDataFromDb.Tables["ActiveSubstances"].Rows)
            {
                resultItem.AddActiveSubstanceToItem(
                    (string)substanceRow["name"],
                    (float)(decimal)substanceRow["concentration"]);
            }

            foreach (DataRow batchRow in itemDataFromDb.Tables["Batches"].Rows)
            {
                DateOnly extractedExpirationDate = DateOnly.FromDateTime((DateTime)batchRow["expirationDate"]);
                resultItem.AddNewBatchToItem(extractedExpirationDate, (int)batchRow["numberOfPacks"]);
            }

            return resultItem;
        }

        public List<Item> GetAllItems()
        {
            string connectionString = SQLUtility.GetConnectionString();
            List<Item> resultItems = new List<Item>();

            string selectItemString = $"SELECT * FROM Items";

            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlDataAdapter itemAdapter = new SqlDataAdapter(selectItemString, sqlConnection);
            DataSet itemDataFromDb = new DataSet();

            sqlConnection.Open();
            itemAdapter.Fill(itemDataFromDb, "Items");

            foreach (DataRow itemRow in itemDataFromDb.Tables["Items"].Rows)
            {
                Item individualItem = new Item(
                    (int)itemRow["itemId"],
                    (string)itemRow["name"],
                    (string)itemRow["producer"],
                    (string)itemRow["category"],
                    (float)(decimal)itemRow["price"],
                    (int)itemRow["numberOfPills"],
                    (string)itemRow["label"],
                    (string)itemRow["description"],
                    (string)itemRow["imagePath"],
                    (float)(decimal)itemRow["discountPercentage"],
                    (int)itemRow["quantity"]);

                string selectActiveSubstances =
                    $"SELECT name, concentration FROM ItemSubstances WHERE itemId={individualItem.Id}";
                string selectBatches =
                    $"SELECT expirationDate, numberOfPacks FROM ItemExpirationDates WHERE itemId={individualItem.Id}";
                SqlDataAdapter activeSubstancesAdapter = new SqlDataAdapter(selectActiveSubstances, sqlConnection);
                SqlDataAdapter batchesAdapter = new SqlDataAdapter(selectBatches, sqlConnection);

                DataSet individualItemDataFromDb = new DataSet();
                activeSubstancesAdapter.Fill(individualItemDataFromDb, "ActiveSubstances");
                batchesAdapter.Fill(individualItemDataFromDb, "Batches");

                foreach (DataRow substanceRow in individualItemDataFromDb.Tables["ActiveSubstances"].Rows)
                {
                    individualItem.AddActiveSubstanceToItem(
                        (string)substanceRow["name"],
                        (float)(decimal)substanceRow["concentration"]);
                }

                foreach (DataRow batchRow in individualItemDataFromDb.Tables["Batches"].Rows)
                {
                    DateOnly extractedExpirationDate = DateOnly.FromDateTime((DateTime)batchRow["expirationDate"]);
                    individualItem.AddNewBatchToItem(extractedExpirationDate, (int)batchRow["numberOfPacks"]);
                }

                resultItems.Add(individualItem);
            }

            return resultItems;
        }

        public List<Item> GetItemsByName(string name)
        {
            string connectionString = SQLUtility.GetConnectionString();
            List<Item> resultItems = new List<Item>();

            string selectItemString = $"SELECT * FROM Items WHERE name='{name}'";

            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlDataAdapter itemAdapter = new SqlDataAdapter(selectItemString, sqlConnection);
            DataSet itemDataFromDb = new DataSet();

            sqlConnection.Open();
            itemAdapter.Fill(itemDataFromDb, "Items");

            foreach (DataRow itemRow in itemDataFromDb.Tables["Items"].Rows)
            {
                Item individualItem = new Item(
                    (int)itemRow["itemId"],
                    (string)itemRow["name"],
                    (string)itemRow["producer"],
                    (string)itemRow["category"],
                    (float)(decimal)itemRow["price"],
                    (int)itemRow["numberOfPills"],
                    (string)itemRow["label"],
                    (string)itemRow["description"],
                    (string)itemRow["imagePath"],
                    (float)(decimal)itemRow["discountPercentage"]);

                string selectActiveSubstances =
                    $"SELECT name, concentration FROM ItemSubstances WHERE itemId={individualItem.Id}";
                string selectBatches =
                    $"SELECT expirationDate, numberOfPacks FROM ItemExpirationDates WHERE itemId={individualItem.Id}";
                SqlDataAdapter activeSubstancesAdapter = new SqlDataAdapter(selectActiveSubstances, sqlConnection);
                SqlDataAdapter batchesAdapter = new SqlDataAdapter(selectBatches, sqlConnection);

                DataSet individualItemDataFromDb = new DataSet();
                activeSubstancesAdapter.Fill(individualItemDataFromDb, "ActiveSubstances");
                batchesAdapter.Fill(individualItemDataFromDb, "Batches");

                foreach (DataRow substanceRow in individualItemDataFromDb.Tables["ActiveSubstances"].Rows)
                {
                    individualItem.AddActiveSubstanceToItem(
                        (string)substanceRow["name"],
                        (float)(decimal)substanceRow["concentration"]);
                }

                foreach (DataRow batchRow in individualItemDataFromDb.Tables["Batches"].Rows)
                {
                    DateOnly extractedExpirationDate = DateOnly.FromDateTime((DateTime)batchRow["expirationDate"]);
                    individualItem.AddNewBatchToItem(extractedExpirationDate, (int)batchRow["numberOfPacks"]);
                }

                resultItems.Add(individualItem);
            }

            return resultItems;
        }

        public void UpdateItemById(Item newItem)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string updateItemString = $"UPDATE Items " +
                                      $"SET name = '{newItem.Name}', " +
                                      $"price = {newItem.Price}, " +
                                      $"category = '{newItem.Category}', " +
                                      $"numberOfPills = {newItem.NumberOfPills}, " +
                                      $"producer = '{newItem.Producer}', " +
                                      $"imagePath = '{newItem.ImagePath}', " +
                                      $"quantity = {newItem.Quantity}, " +
                                      $"label = '{newItem.Label}', " +
                                      $"description = '{newItem.Description}', " +
                                      $"discountPercentage = {newItem.DiscountPercentage} " +
                                      $"WHERE itemId = {newItem.Id}";

            using SqlConnection sqlConnection = new SqlConnection(connectionString);

            sqlConnection.Open();
            SqlCommand updateItemCommand = new SqlCommand(updateItemString, sqlConnection);
            updateItemCommand.ExecuteNonQuery();

            string deleteActiveSubstancesCommandString = $"DELETE FROM ItemSubstances WHERE itemId = {newItem.Id}";
            SqlCommand deleteActiveSubstancesCommand = new SqlCommand(deleteActiveSubstancesCommandString, sqlConnection);
            deleteActiveSubstancesCommand.ExecuteNonQuery();

            foreach (KeyValuePair<string, float> activeSubstance in newItem.ActiveSubstances)
            {
                string insertActiveSubstanceCommandString =
                    $"INSERT INTO ItemSubstances (itemId, name, concentration) " +
                    $"VALUES ({newItem.Id}, '{activeSubstance.Key}', {activeSubstance.Value})";
                SqlCommand insertActiveSubstanceCommand = new SqlCommand(insertActiveSubstanceCommandString, sqlConnection);
                insertActiveSubstanceCommand.ExecuteNonQuery();
            }

            string deleteBatchesCommandString = $"DELETE FROM ItemExpirationDates WHERE itemId = {newItem.Id}";
            SqlCommand deleteBatchesCommand = new SqlCommand(deleteBatchesCommandString, sqlConnection);
            deleteBatchesCommand.ExecuteNonQuery();

            foreach (KeyValuePair<DateOnly, int> batch in newItem.Batches)
            {
                string insertBatchExpirationDate = $"{batch.Key.Year}-{batch.Key.Month}-{batch.Key.Day}";
                string insertBatchCommandString =
                    $"INSERT INTO ItemExpirationDates (itemId, expirationDate, numberOfPacks) " +
                    $"VALUES ({newItem.Id}, '{insertBatchExpirationDate}', {batch.Value})";
                SqlCommand insertBatchCommand = new SqlCommand(insertBatchCommandString, sqlConnection);
                insertBatchCommand.ExecuteNonQuery();
            }
        }

        public bool ItemExists(int id)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string selectQueryString = $"SELECT * FROM Items WHERE itemId={id}";

            using SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlDataAdapter itemsAdapter = new SqlDataAdapter(selectQueryString, sqlConnection);
            DataSet items = new DataSet();

            sqlConnection.Open();
            itemsAdapter.Fill(items, "Items");

            if (items.Tables["Items"].Rows.Count > 0)
            {
                return true;
            }

            return false;
        }

        public List<Tuple<int, string, int>> GetTop30Items()
        {
            string connectionString = SQLUtility.GetConnectionString();
            List<Tuple<int, string, int>> resultItems = new List<Tuple<int, string, int>>();
            string selectItemString =
                $"SELECT TOP 30 i.itemId, i.name, COUNT(o.orderId) as nbOrders FROM Items i INNER JOIN OrderItems oi ON i.itemId=oi.itemId INNER JOIN Orders o ON oi.orderId=o.orderId WHERE o.pickUpDate >= DATEADD(MONTH, -1, GETDATE()) GROUP BY i.itemId, i.name ORDER BY COUNT(o.orderId) DESC";

            using SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlDataAdapter itemAdapter = new SqlDataAdapter(selectItemString, sqlConnection);
            DataSet itemDataFromDb = new DataSet();

            sqlConnection.Open();
            itemAdapter.Fill(itemDataFromDb, "Items");

            foreach (DataRow itemRow in itemDataFromDb.Tables["Items"].Rows)
            {
                int itemId = (int)itemRow["itemId"];
                string name = (string)itemRow["name"];
                int nbOrders = (int)itemRow["nbOrders"];

                resultItems.Add(new Tuple<int, string, int>(itemId, name, nbOrders));
            }

            return resultItems;
        }
    }
}
