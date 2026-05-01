using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UBB_SE_2026_923_2.Models;
using Windows.UI.WebUI;
using Microsoft.Data.SqlClient;

namespace UBB_SE_2026_923_2.Repositories
{
    public class SQLOrdersRepository : IOrdersRepository
    {
        private const int FirstElementIndex = 0;

        public SQLOrdersRepository()
        {
        }

        public int AddOrder(int clientId, DateOnly pickUpDate, bool isCompleted = false, bool isExpired = false)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string pickUpDateString = $"{pickUpDate.Year}-{pickUpDate.Month}-{pickUpDate.Day}";
            string insertCommandString = "INSERT INTO Orders (clientId, isCompleted, isExpired, pickUpDate) " +
                                        $"OUTPUT INSERTED.orderId " +
                                        $"VALUES ({clientId}, '{isCompleted}', '{isExpired}', '{pickUpDateString}')";

            using SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlCommand insertOrderCommand = new SqlCommand(insertCommandString, sqlConnection);
            sqlConnection.Open();
            int insertedId = (int)insertOrderCommand.ExecuteScalar();
            return insertedId;
        }

        public void RemoveOrder(int orderIdToBeRemoved)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string deleteItemsInOrderString = $"DELETE FROM OrderItems WHERE orderId = {orderIdToBeRemoved}";
            string deleteCommandString = $"DELETE FROM Orders WHERE orderId = {orderIdToBeRemoved}";

            using SqlConnection sqlConnection = new (connectionString);

            SqlCommand deleteItemsInOrderCommand = new (deleteItemsInOrderString, sqlConnection);
            SqlCommand deleteOrderCommand = new (deleteCommandString, sqlConnection);

            sqlConnection.Open();
            deleteItemsInOrderCommand.ExecuteNonQuery();
            deleteOrderCommand.ExecuteNonQuery();
        }

        public void UpdateOrder(Order newOrder)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string pickUpDateString = $"{newOrder.PickUpDate.Year}-{newOrder.PickUpDate.Month}-{newOrder.PickUpDate.Day}";
            string updateCommandString = $"UPDATE Orders " +
                                        $"SET clientId = {newOrder.ClientId}, " +
                                        $"isCompleted = '{newOrder.IsCompleted}', " +
                                        $"isExpired = '{newOrder.IsExpired}', " +
                                        $"pickUpDate = '{pickUpDateString}' " +
                                        $"WHERE orderId = {newOrder.Id}";

            using SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlCommand updateOrderCommand = new SqlCommand(updateCommandString, sqlConnection);
            sqlConnection.Open();
            updateOrderCommand.ExecuteNonQuery();

            string deleteItemsInOrderCommandString = $"DELETE FROM OrderItems WHERE orderId = {newOrder.Id}";
            SqlCommand deleteItemsInOrderCommand = new SqlCommand(deleteItemsInOrderCommandString, sqlConnection);
            deleteItemsInOrderCommand.ExecuteNonQuery();

            foreach (KeyValuePair<int, Tuple<int, float>> itemInOrder in newOrder.ItemQuantitiesWithFinalPrice)
            {
                int itemId = itemInOrder.Key;
                int itemQuantity = itemInOrder.Value.Item1;
                float finalPrice = itemInOrder.Value.Item2;

                string insertItemsInOrderCommandString =
                    $"INSERT INTO OrderItems (orderId, itemId, orderQuantity, price) " +
                    $"VALUES ({newOrder.Id}, {itemId}, {itemQuantity}, {finalPrice})";
                SqlCommand insertItemsInOrderCommand = new SqlCommand(insertItemsInOrderCommandString, sqlConnection);
                insertItemsInOrderCommand.ExecuteNonQuery();
            }
        }

        public Order GetOrder(int orderId)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectOrderCommandString = $"SELECT * FROM Orders WHERE orderId = {orderId}";
            string selectItemsInOrderCommandString = $"SELECT itemId, orderQuantity, price FROM OrderItems WHERE orderId = {orderId}";

            using SqlConnection sqlConnection = new SqlConnection(connString);

            SqlDataAdapter orderAdapter = new SqlDataAdapter(selectOrderCommandString, sqlConnection);
            SqlDataAdapter itemsInOrderAdapter = new SqlDataAdapter(selectItemsInOrderCommandString, sqlConnection);
            DataSet orderDataFromDb = new DataSet();

            sqlConnection.Open();
            orderAdapter.Fill(orderDataFromDb, "Orders");
            itemsInOrderAdapter.Fill(orderDataFromDb, "OrderItems");

            DataRow resultingRow = orderDataFromDb.Tables["Orders"].Rows[0];

            int resultingOrderId = (int)resultingRow["orderId"];
            int resultingClientId = (int)resultingRow["clientId"];
            bool resultingCompletedStatus = (bool)resultingRow["isCompleted"];
            bool resultingExpiredStatus = (bool)resultingRow["isExpired"];
            DateOnly resultingPickUpDate = DateOnly.FromDateTime((DateTime)resultingRow["pickUpDate"]);

            Order resultingOrder = new Order(resultingOrderId, resultingClientId, resultingPickUpDate, resultingCompletedStatus, resultingExpiredStatus);

            foreach (DataRow itemInOrderRow in orderDataFromDb.Tables["OrderItems"].Rows)
            {
                int itemId = (int)itemInOrderRow["itemId"];
                int itemQuantity = (int)itemInOrderRow["orderQuantity"];
                float finalPrice = (float)(decimal)itemInOrderRow["price"];
                resultingOrder.AddItemToOrder(itemId, itemQuantity, finalPrice);
            }

            return resultingOrder;
        }

        private List<Order> GetOrdersFromSelectCommand(string selectOrdersCommandString)
        {
            List<Order> orders = new ();
            List<int> orderIds = new ();

            string connectionString = SQLUtility.GetConnectionString();

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlDataAdapter orderAdapter = new SqlDataAdapter(selectOrdersCommandString, sqlConnection);
                DataSet orderInfoFromDb = new DataSet();

                sqlConnection.Open();
                orderAdapter.Fill(orderInfoFromDb, "Orders");

                foreach (DataRow orderRow in orderInfoFromDb.Tables["Orders"].Rows)
                {
                    orderIds.Add((int)orderRow["orderId"]);
                }
            }

            foreach (int orderId in orderIds)
            {
                orders.Add(GetOrder(orderId));
            }
            return orders;
        }

        public List<Order> GetAllOrders()
        {
            string selectAllOrdersCommandString = $"SELECT * FROM Orders";
            return GetOrdersFromSelectCommand(selectAllOrdersCommandString);
        }

        public List<Order> GetOrdersOfClient(int clientId)
        {
            string selectOrdersCommandString = $"SELECT * FROM Orders WHERE clientId = {clientId}";
            return GetOrdersFromSelectCommand(selectOrdersCommandString);
        }

        public bool OrderExists(int orderId)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string selectCommandString = $"SELECT * FROM Orders WHERE orderId = {orderId}";

            using SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlDataAdapter ordersAdapter = new SqlDataAdapter(selectCommandString, sqlConnection);
            DataSet orders = new DataSet();

            sqlConnection.Open();
            ordersAdapter.Fill(orders, "Orders");
            if (orders.Tables["Orders"].Rows.Count > 0)
            {
                return true;
            }

            return false;
        }
    }
}
