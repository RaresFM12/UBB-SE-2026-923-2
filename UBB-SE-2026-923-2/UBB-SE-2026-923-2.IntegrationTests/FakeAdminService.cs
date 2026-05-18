using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.IntegrationTests;

public class FakeAdminService : IAdminService
{
    public List<Item> GetAllItems() => new List<Item>();

    public List<Substance> GetAllSubstances() => new List<Substance>();

    public List<Item> SearchItemsByName(string query) => new List<Item>();

    public Item GetItemById(int itemId) => null!;

    public Substance GetSubstanceByName(string name) => null!;

    public bool SubstanceExists(string name) => false;

    public void AddItem(Item newItem) { }

    public void AddItemWithQuantity(Item newItem) { }

    public void RemoveItemById(int itemId) { }

    public void UpdateItemById(int itemId, Item updatedItem) { }

    public void AddSubstance(Substance newSubstance) { }

    public void RemoveSubstanceByName(Substance substance) { }

    public void UpdateSubstanceByName(string name, Substance substance) { }

    public void ValidateItemForAdd(Item item) { }

    public List<Item> GetExpiredItems() => new List<Item>();

    public Notification SendNewStockNotification(Item item) => throw new NotImplementedException();

    public Notification SendAboutToExpireNotification() => throw new NotImplementedException();

    public List<Notification> GetNotificationsForUser(User user) => throw new NotImplementedException();

    public List<Tuple<int, string, int>> GetTop30Items() => new List<Tuple<int, string, int>>();

    public Dictionary<string, int> GetTop30Substances() => new Dictionary<string, int>();
}
