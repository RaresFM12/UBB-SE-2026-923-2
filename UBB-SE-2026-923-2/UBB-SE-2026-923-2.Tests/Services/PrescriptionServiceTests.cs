namespace UBB_SE_2026_923_2.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class PrescriptionServiceLogicTests
    {
        private Mock<IItemsRepository> mockItemsRepository;
        private Mock<IEvaluationsRepository> mockEvaluationsRepository;
        private PrescriptionService prescriptionService;

        [SetUp]
        public void Setup()
        {
            this.mockItemsRepository = new Mock<IItemsRepository>();
            this.mockEvaluationsRepository = new Mock<IEvaluationsRepository>();

            this.prescriptionService = new PrescriptionService(
                this.mockItemsRepository.Object,
                this.mockEvaluationsRepository.Object);
        }

        [Test]
        public void GetItemsFromPrescription_WhenPrescriptionIdentifierIsInvalid_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => this.prescriptionService.GetItemsFromPrescription("invalid", new Dictionary<int, float>()));
        }

        [Test]
        public void GetItemsFromPrescription_WhenEvaluationDoesNotExist_ThrowsArgumentException()
        {
            this.mockEvaluationsRepository
                .Setup(evaluationsRepository => evaluationsRepository.GetAllEvaluations())
                .Returns(new List<MedicalEvaluation>());

            Assert.Throws<ArgumentException>(
                () => this.prescriptionService.GetItemsFromPrescription("7", new Dictionary<int, float>()));
        }

        [Test]
        public void GetItemsFromPrescription_WhenExactMedicineIsAvailable_ReturnsExactMedicineIdentifier()
        {
            var medicineItem = CreateItem(10, "Paracetamol", 20, 5, 10, 0);

            this.mockEvaluationsRepository
                .Setup(evaluationsRepository => evaluationsRepository.GetAllEvaluations())
                .Returns(new List<MedicalEvaluation>
                {
                    new MedicalEvaluation
                    {
                        EvaluationID = 7,
                        MedicationsList = "Paracetamol",
                    },
                });

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item> { medicineItem });

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetItemsByName("Paracetamol"))
                .Returns(new List<Item> { medicineItem });

            var prescriptionItems = this.prescriptionService.GetItemsFromPrescription("7", new Dictionary<int, float>());

            Assert.That(prescriptionItems.ContainsKey(10), Is.True);
        }

        [Test]
        public void GetItemsFromPrescription_WhenExactMedicineIsOutOfStock_ReturnsSubstituteMedicineIdentifier()
        {
            var outOfStockPreferredItem = CreateItem(10, "Paracetamol", 20, 0, 10, 0);
            var availableSubstituteItem = CreateItem(11, "Substitute", 20, 5, 8, 0);

            this.mockEvaluationsRepository
                .Setup(evaluationsRepository => evaluationsRepository.GetAllEvaluations())
                .Returns(new List<MedicalEvaluation>
                {
                    new MedicalEvaluation
                    {
                        EvaluationID = 7,
                        MedicationsList = "Paracetamol",
                    },
                });

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item> { outOfStockPreferredItem, availableSubstituteItem });

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetItemsByName("Paracetamol"))
                .Returns(new List<Item> { outOfStockPreferredItem });

            var prescriptionItems = this.prescriptionService.GetItemsFromPrescription("7", new Dictionary<int, float>());

            Assert.That(prescriptionItems.ContainsKey(11), Is.True);
        }

        [Test]
        public void GetItemsFromPrescription_WhenOnlySmallerSubstituteExists_ReturnsRequiredBoxMultiplier()
        {
            var preferredItem = CreateItem(10, "Paracetamol", 20, 0, 10, 0);
            var smallerSubstituteItem = CreateItem(11, "Small substitute", 10, 5, 4, 0);

            this.mockEvaluationsRepository
                .Setup(evaluationsRepository => evaluationsRepository.GetAllEvaluations())
                .Returns(new List<MedicalEvaluation>
                {
                    new MedicalEvaluation
                    {
                        EvaluationID = 7,
                        MedicationsList = "Paracetamol",
                    },
                });

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item> { preferredItem, smallerSubstituteItem });

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetItemsByName("Paracetamol"))
                .Returns(new List<Item> { preferredItem });

            var prescriptionItems = this.prescriptionService.GetItemsFromPrescription("7", new Dictionary<int, float>());

            Assert.That(prescriptionItems[11], Is.EqualTo(2));
        }

        [Test]
        public void GetCheapestPrescriptionItems_WhenExactMatchIsAvailable_ReturnsExactMatchIdentifier()
        {
            var exactMedicineItem = CreateItem(10, "Paracetamol", 20, 5, 10, 0);

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item> { exactMedicineItem });

            var prescriptionItems = this.prescriptionService.GetCheapestPrescriptionItems("Paracetamol", 20);

            Assert.That(prescriptionItems.ContainsKey(10), Is.True);
        }

        [Test]
        public void GetCheapestPrescriptionItems_WhenPreferredItemDoesNotExist_ReturnsEmptyDictionary()
        {
            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetAllItems())
                .Returns(new List<Item>());

            this.mockItemsRepository
                .Setup(itemsRepository => itemsRepository.GetItemsByName("Unknown medicine"))
                .Returns(new List<Item>());

            var prescriptionItems = this.prescriptionService.GetCheapestPrescriptionItems("Unknown medicine", 20);

            Assert.That(prescriptionItems.Count, Is.EqualTo(0));
        }

        private static Item CreateItem(
            int itemIdentifier,
            string itemName,
            int numberOfPills,
            int quantity,
            float price,
            float discountPercentage)
        {
            return new Item
            {
                Id = itemIdentifier,
                Name = itemName,
                Producer = "Producer",
                Category = "Category",
                NumberOfPills = numberOfPills,
                Quantity = quantity,
                Price = price,
                DiscountPercentage = discountPercentage,
                ActiveSubstances = new Dictionary<string, float>
                {
                    { "Substance A", 1 },
                },
                Batches = new Dictionary<DateOnly, int>
                {
                    { DateOnly.FromDateTime(DateTime.Today.AddDays(10)), quantity },
                },
            };
        }
    }
}