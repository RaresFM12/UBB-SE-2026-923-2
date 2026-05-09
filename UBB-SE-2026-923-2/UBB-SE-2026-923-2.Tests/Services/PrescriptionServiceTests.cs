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
    public class PrescriptionServiceTests
    {
        private Mock<IItemsRepository> mockItemsRepository;
        private Mock<IEvaluationsRepository> mockEvaluationsRepository;
        private PrescriptionService service;

        [SetUp]
        public void Setup()
        {
            this.mockItemsRepository = new Mock<IItemsRepository>();
            this.mockEvaluationsRepository = new Mock<IEvaluationsRepository>();
            this.service = new PrescriptionService(this.mockItemsRepository.Object, this.mockEvaluationsRepository.Object);
        }

        // --- GetItemsFromPrescription ---
        [Test]
        public void GetItemsFromPrescription_NullId_Throws()
        {
            Assert.Throws<ArgumentException>(() => this.service.GetItemsFromPrescription(null, new Dictionary<int, float>()));
        }

        [Test]
        public void GetItemsFromPrescription_EvaluationNotFound_Throws()
        {
            this.mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation>());
            Assert.Throws<ArgumentException>(() => this.service.GetItemsFromPrescription("1", new Dictionary<int, float>()));
        }

        [Test]
        public void GetItemsFromPrescription_EvaluationNoMedications_Throws()
        {
            var evaluation = new MedicalEvaluation { EvaluationID = 1, MedicationsList = string.Empty };
            this.mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation> { evaluation });
            Assert.Throws<ArgumentException>(() => this.service.GetItemsFromPrescription("1", new Dictionary<int, float>()));
        }

        [Test]
        public void GetItemsFromPrescription_ValidPrescription_ReturnsItems()
        {
            var evaluation = new MedicalEvaluation { EvaluationID = 1, MedicationsList = "Aspirin" };
            this.mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation> { evaluation });

            var item = new Item(1, "Aspirin", "Bayer", "pain", 10f, 30, quantity: 50);
            item.ActiveSubstances["acid"] = 500f;
            this.mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { item });
            this.mockItemsRepository.Setup(repository => repository.GetItemsByName("Aspirin")).Returns(new List<Item> { item });

            var result = this.service.GetItemsFromPrescription("1", new Dictionary<int, float>());
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void GetItemsFromPrescription_MultipleMedications_ReturnsMultiple()
        {
            var evaluation = new MedicalEvaluation { EvaluationID = 1, MedicationsList = "Aspirin, Ibuprofen" };
            this.mockEvaluationsRepository.Setup(repository => repository.GetAllEvaluations()).Returns(new List<MedicalEvaluation> { evaluation });

            var item1 = new Item(1, "Aspirin", "Bayer", "pain", 10f, 30, quantity: 50);
            var item2 = new Item(2, "Ibuprofen", "Advil", "pain", 15f, 30, quantity: 50);

            this.mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { item1, item2 });
            this.mockItemsRepository.Setup(repository => repository.GetItemsByName("Aspirin")).Returns(new List<Item> { item1 });
            this.mockItemsRepository.Setup(repository => repository.GetItemsByName("Ibuprofen")).Returns(new List<Item> { item2 });

            var result = this.service.GetItemsFromPrescription("1", new Dictionary<int, float>());
            Assert.That(result.Count, Is.EqualTo(2));
        }

        // --- GetCheapestPrescriptionItems ---
        [Test]
        public void GetCheapestPrescriptionItems_NoMatch_ReturnsEmpty()
        {
            this.mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item>());
            this.mockItemsRepository.Setup(repository => repository.GetItemsByName("Unknown")).Returns(new List<Item>());

            var result = this.service.GetCheapestPrescriptionItems("Unknown", 30);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetCheapestPrescriptionItems_ExactMatchOutOfStock_FindsSubstitute()
        {
            var item = new Item(1, "Aspirin", "Bayer", "pain", 10f, 30, quantity: 0);
            var substituteItem = new Item(2, "AspSub", "Gen", "pain", 8f, 30, quantity: 50);

            substituteItem.ActiveSubstances["acid"] = 500f;
            item.ActiveSubstances["acid"] = 500f;

            this.mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { item, substituteItem });
            this.mockItemsRepository.Setup(repository => repository.GetItemsByName("Aspirin")).Returns(new List<Item> { item });

            var result = this.service.GetCheapestPrescriptionItems("Aspirin", 30);
            Assert.That(result.ContainsKey(2), Is.True);
        }

        [Test]
        public void GetCheapestPrescriptionItems_MultipleBoxesNeeded_CalculatesMultiplier()
        {
            var item = new Item(1, "Aspirin", "Bayer", "pain", 10f, 10, quantity: 50);
            this.mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { item });
            this.mockItemsRepository.Setup(repository => repository.GetItemsByName("Aspirin")).Returns(new List<Item> { item });

            var result = this.service.GetCheapestPrescriptionItems("Aspirin", 30);
            Assert.That(result.ContainsKey(1), Is.True);
        }
    }
}