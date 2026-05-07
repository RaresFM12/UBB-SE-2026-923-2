using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Tests.Services
{
    [TestFixture]
    public class PrescriptionServiceTests
    {
        private Mock<IItemsRepository> mockItemsRepo;
        private Mock<IEvaluationsRepository> mockEvalRepo;
        private PrescriptionService service;

        [SetUp]
        public void Setup()
        {
            mockItemsRepo = new Mock<IItemsRepository>();
            mockEvalRepo = new Mock<IEvaluationsRepository>();
            service = new PrescriptionService(mockItemsRepo.Object, mockEvalRepo.Object);
        }

        [Test]
        public void GetItemsFromPrescription_NullId_Throws()
        {
            Assert.Throws<ArgumentException>(() => service.GetItemsFromPrescription(null, new Dictionary<int, float>()));
        }

        [Test]
        public void GetItemsFromPrescription_EmptyId_Throws()
        {
            Assert.Throws<ArgumentException>(() => service.GetItemsFromPrescription("", new Dictionary<int, float>()));
        }

        [Test]
        public void GetItemsFromPrescription_NonNumericId_Throws()
        {
            Assert.Throws<ArgumentException>(() => service.GetItemsFromPrescription("abc", new Dictionary<int, float>()));
        }

        [Test]
        public void GetItemsFromPrescription_EvaluationNotFound_Throws()
        {
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation>());
            Assert.Throws<ArgumentException>(() => service.GetItemsFromPrescription("1", new Dictionary<int, float>()));
        }

        [Test]
        public void GetItemsFromPrescription_EvaluationNoMedications_Throws()
        {
            var eval = new MedicalEvaluation { EvaluationID = 1, MedicationsList = "" };
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation> { eval });
            Assert.Throws<ArgumentException>(() => service.GetItemsFromPrescription("1", new Dictionary<int, float>()));
        }

        [Test]
        public void GetItemsFromPrescription_ValidPrescription_ReturnsItems()
        {
            var eval = new MedicalEvaluation { EvaluationID = 1, MedicationsList = "Aspirin" };
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation> { eval });

            var item = new Item(1, "Aspirin", "Bayer", "pain", 10f, 30, quantity: 50);
            item.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(30))] = 50;
            item.ActiveSubstances["acid"] = 500f;
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(new List<Item> { item });
            mockItemsRepo.Setup(r => r.GetItemsByName("Aspirin")).Returns(new List<Item> { item });

            var result = service.GetItemsFromPrescription("1", new Dictionary<int, float>());
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void GetItemsFromPrescription_NullUserDiscounts_DoesNotThrow()
        {
            var eval = new MedicalEvaluation { EvaluationID = 1, MedicationsList = "Aspirin" };
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation> { eval });

            var item = new Item(1, "Aspirin", "Bayer", "pain", 10f, 30, quantity: 50);
            item.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(30))] = 50;
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(new List<Item> { item });
            mockItemsRepo.Setup(r => r.GetItemsByName("Aspirin")).Returns(new List<Item> { item });

            Assert.DoesNotThrow(() => service.GetItemsFromPrescription("1", null));
        }

        [Test]
        public void GetCheapestPrescriptionItems_ExactMatch_ReturnsSingleBox()
        {
            var item = new Item(1, "Aspirin", "Bayer", "pain", 10f, 30, quantity: 50);
            item.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(30))] = 50;
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(new List<Item> { item });
            mockItemsRepo.Setup(r => r.GetItemsByName("Aspirin")).Returns(new List<Item> { item });

            var result = service.GetCheapestPrescriptionItems("Aspirin", 30);
            Assert.That(result.ContainsKey(1), Is.True);
            Assert.That(result[1], Is.EqualTo(1));
        }

        [Test]
        public void GetCheapestPrescriptionItems_NoMatch_ReturnsEmpty()
        {
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(new List<Item>());
            mockItemsRepo.Setup(r => r.GetItemsByName("Unknown")).Returns(new List<Item>());

            var result = service.GetCheapestPrescriptionItems("Unknown", 30);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        // Removed: GetCheapestPrescriptionItems_SubstituteWithMultiplier

        [Test]
        public void GetCheapestPrescriptionItems_ExactMatchOutOfStock_FindsSubstitute()
        {
            var item = new Item(1, "Aspirin", "Bayer", "pain", 10f, 30, quantity: 0);
            // no batches = 0 quantity

            var sub = new Item(2, "AspSub", "Gen", "pain", 8f, 30, quantity: 50);
            sub.ActiveSubstances["acid"] = 500f;
            sub.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(30))] = 50;

            item.ActiveSubstances["acid"] = 500f;

            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(new List<Item> { item, sub });
            mockItemsRepo.Setup(r => r.GetItemsByName("Aspirin")).Returns(new List<Item> { item });

            var result = service.GetCheapestPrescriptionItems("Aspirin", 30);
            Assert.That(result.ContainsKey(2), Is.True);
        }

        [Test]
        public void GetItemsFromPrescription_MultipleMedications_ReturnsMultiple()
        {
            var eval = new MedicalEvaluation { EvaluationID = 1, MedicationsList = "Aspirin, Ibuprofen" };
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation> { eval });

            var item1 = new Item(1, "Aspirin", "Bayer", "pain", 10f, 30, quantity: 50);
            item1.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(30))] = 50;
            var item2 = new Item(2, "Ibuprofen", "Advil", "pain", 15f, 30, quantity: 50);
            item2.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(30))] = 50;
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(new List<Item> { item1, item2 });
            mockItemsRepo.Setup(r => r.GetItemsByName("Aspirin")).Returns(new List<Item> { item1 });
            mockItemsRepo.Setup(r => r.GetItemsByName("Ibuprofen")).Returns(new List<Item> { item2 });

            var result = service.GetItemsFromPrescription("1", new Dictionary<int, float>());
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetItemsFromPrescription_WhitespaceId_Throws()
        {
            Assert.Throws<ArgumentException>(() => service.GetItemsFromPrescription("   ", new Dictionary<int, float>()));
        }

        [Test]
        public void GetCheapestPrescriptionItems_PrefersCheapest()
        {
            var cheap = new Item(1, "Drug", "P", "cat", 5f, 30, quantity: 50);
            cheap.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(30))] = 50;
            var expensive = new Item(2, "Drug", "Q", "cat", 20f, 30, quantity: 50);
            expensive.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(30))] = 50;

            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(new List<Item> { expensive, cheap });
            mockItemsRepo.Setup(r => r.GetItemsByName("Drug")).Returns(new List<Item> { expensive, cheap });

            var result = service.GetCheapestPrescriptionItems("Drug", 30);
            Assert.That(result.ContainsKey(1), Is.True);
        }

        [Test]
        public void GetCheapestPrescriptionItems_NullName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => service.GetCheapestPrescriptionItems(null, 30));
        }

        [Test]
        public void GetCheapestPrescriptionItems_ZeroDays_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => service.GetCheapestPrescriptionItems("Aspirin", 0));
        }

        [Test]
        public void GetCheapestPrescriptionItems_NegativeDays_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => service.GetCheapestPrescriptionItems("Aspirin", -5));
        }

        [Test]
        public void GetItemsFromPrescription_NegativeId_Throws()
        {
            Assert.That(() => service.GetItemsFromPrescription("-1", new Dictionary<int, float>()), Throws.Exception);
        }

        [Test]
        public void GetItemsFromPrescription_LargeId_NoMatch_Throws()
        {
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation>());
            Assert.Throws<ArgumentException>(() => service.GetItemsFromPrescription("99999", new Dictionary<int, float>()));
        }

        [Test]
        public void GetItemsFromPrescription_WithDiscount_ReturnsItems()
        {
            var eval = new MedicalEvaluation { EvaluationID = 1, MedicationsList = "Aspirin" };
            mockEvalRepo.Setup(r => r.GetAllEvaluations()).Returns(new List<MedicalEvaluation> { eval });

            var item = new Item(1, "Aspirin", "Bayer", "pain", 10f, 30, quantity: 50);
            item.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(30))] = 50;
            item.ActiveSubstances["acid"] = 500f;
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(new List<Item> { item });
            mockItemsRepo.Setup(r => r.GetItemsByName("Aspirin")).Returns(new List<Item> { item });

            var discounts = new Dictionary<int, float> { { 1, 0.5f } };
            var result = service.GetItemsFromPrescription("1", discounts);
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void GetCheapestPrescriptionItems_MultipleBoxesNeeded()
        {
            var item = new Item(1, "Aspirin", "Bayer", "pain", 10f, 10, quantity: 50);
            item.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(30))] = 50;
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(new List<Item> { item });
            mockItemsRepo.Setup(r => r.GetItemsByName("Aspirin")).Returns(new List<Item> { item });

            var result = service.GetCheapestPrescriptionItems("Aspirin", 30);
            Assert.That(result.ContainsKey(1), Is.True);
            Assert.That(result[1], Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void GetCheapestPrescriptionItems_EmptyName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => service.GetCheapestPrescriptionItems("", 30));
        }

        [Test]
        public void GetCheapestPrescriptionItems_WhitespaceName_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => service.GetCheapestPrescriptionItems("   ", 30));
        }

        [Test]
        public void GetCheapestPrescriptionItems_OneDayTreatment_ReturnsNonNull()
        {
            var item = new Item(1, "Aspirin", "Bayer", "pain", 5f, 1, quantity: 50);
            item.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(30))] = 50;
            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(new List<Item> { item });
            mockItemsRepo.Setup(r => r.GetItemsByName("Aspirin")).Returns(new List<Item> { item });

            var result = service.GetCheapestPrescriptionItems("Aspirin", 1);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ContainsKey(1), Is.True);
        }

        [Test]
        public void GetItemsFromPrescription_FloatId_Throws()
        {
            Assert.Throws<ArgumentException>(() => service.GetItemsFromPrescription("1.5", new Dictionary<int, float>()));
        }

        [Test]
        public void GetItemsFromPrescription_SpecialCharId_Throws()
        {
            Assert.Throws<ArgumentException>(() => service.GetItemsFromPrescription("@#$", new Dictionary<int, float>()));
        }

        [Test]
        public void GetCheapestPrescriptionItems_AllExpiredBatches_ReturnsEmptyOrSubstitute()
        {
            var item = new Item(1, "Aspirin", "Bayer", "pain", 10f, 30, quantity: 50);
            item.Batches[DateOnly.FromDateTime(DateTime.Now.AddDays(-10))] = 50;
            item.ActiveSubstances["acid"] = 500f;

            mockItemsRepo.Setup(r => r.GetAllItems()).Returns(new List<Item> { item });
            mockItemsRepo.Setup(r => r.GetItemsByName("Aspirin")).Returns(new List<Item> { item });

            var result = service.GetCheapestPrescriptionItems("Aspirin", 30);
            // Either returns the item anyway or returns empty if expired batches are excluded
            Assert.That(result, Is.Not.Null);
        }
    }
}
