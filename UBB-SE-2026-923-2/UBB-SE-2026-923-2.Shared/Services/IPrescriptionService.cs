namespace UBB_SE_2026_923_2.Services
{
    using System;
    using System.Collections.Generic;
    using UBB_SE_2026_923_2.Models;

    public interface IPrescriptionService
    {
        Dictionary<int, int> GetItemsFromPrescription(string prescriptionId, Dictionary<int, float> userDiscounts);

        Dictionary<int, int> GetCheapestPrescriptionItems(string prescriptionName, int requiredPills);
    }
}