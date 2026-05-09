namespace UBB_SE_2026_923_2.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;
    using UBB_SE_2026_923_2.Services;

    [TestFixture]
    public class PeriodTrackerServiceTests
    {
        private Mock<IUsersRepository> mockUsersRepository;
        private Mock<RaresICurrentUserService> mockCurrentUserService;
        private PeriodTrackerService service;
        private User testUser;

        [SetUp]
        public void Setup()
        {
            this.mockUsersRepository = new Mock<IUsersRepository>();
            this.mockCurrentUserService = new Mock<RaresICurrentUserService>();
            this.service = new PeriodTrackerService(this.mockUsersRepository.Object, this.mockCurrentUserService.Object);

            this.testUser = new User(1, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            this.mockCurrentUserService.Setup(service => service.RaresCurrentUser).Returns(this.testUser);
        }

        // --- GetCurrentUser ---
        [Test]
        public void GetCurrentUser_ReturnsUser()
        {
            var result = this.service.GetCurrentUser();
            Assert.That(result, Is.EqualTo(this.testUser));
        }

        [Test]
        public void GetCurrentUser_NullUser_ReturnsNull()
        {
            this.mockCurrentUserService.Setup(service => service.RaresCurrentUser).Returns((User)null);
            var result = this.service.GetCurrentUser();
            Assert.That(result, Is.Null);
        }

        // --- GetTrackerState ---
        [Test]
        public void GetTrackerState_NullUser_ReturnsDefault()
        {
            this.mockCurrentUserService.Setup(service => service.RaresCurrentUser).Returns((User)null);
            var result = this.service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(28));
        }

        [Test]
        public void GetTrackerState_ValidUser_ReturnsCycleDays()
        {
            this.testUser.CycleDays = 30;
            this.testUser.PeriodLasts = 6;
            this.mockUsersRepository.Setup(repository => repository.UserHasPeriodTracker(1)).Returns(true);
            var result = this.service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(30));
        }

        // --- GetNotes ---
        [Test]
        public void GetNotes_NullUser_ReturnsEmpty()
        {
            this.mockCurrentUserService.Setup(service => service.RaresCurrentUser).Returns((User)null);
            var result = this.service.GetNotes();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetNotes_UserWithNotes_ReturnsNotes()
        {
            this.testUser.AddPeriodNoteToUser(1, "Note1", false);
            this.testUser.AddPeriodNoteToUser(2, "Note2", true);
            var result = this.service.GetNotes();
            Assert.That(result.Count, Is.EqualTo(2));
        }

        // --- GetMaxNoteId ---
        [Test]
        public void GetMaxNoteId_NoNotes_ReturnsZero()
        {
            var result = this.service.GetMaxNoteId();
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetMaxNoteId_WithNotes_ReturnsMax()
        {
            this.testUser.AddPeriodNoteToUser(1, "N1", false);
            this.testUser.AddPeriodNoteToUser(5, "N5", false);
            this.testUser.AddPeriodNoteToUser(3, "N3", false);
            var result = this.service.GetMaxNoteId();
            Assert.That(result, Is.EqualTo(5));
        }

        // --- UpdatePeriodTracker ---
        [Test]
        public void UpdatePeriodTracker_NullUser_DoesNotThrow()
        {
            this.mockCurrentUserService.Setup(service => service.RaresCurrentUser).Returns((User)null);
            Assert.DoesNotThrow(() => this.service.UpdatePeriodTracker(DateTimeOffset.Now, 28, 5, 0));
        }

        [Test]
        public void UpdatePeriodTracker_ValidUser_UpdatesAndSaves()
        {
            this.service.UpdatePeriodTracker(new DateTimeOffset(new DateTime(2025, 3, 1)), 30, 6, 1);
            this.mockUsersRepository.Verify(repository => repository.UpdateUser(this.testUser), Times.Once);
            Assert.That(this.testUser.CycleDays, Is.EqualTo(30));
        }

        // --- AddNote ---
        [Test]
        public void AddNote_NullUser_DoesNotThrow()
        {
            this.mockCurrentUserService.Setup(service => service.RaresCurrentUser).Returns((User)null);
            Assert.DoesNotThrow(() => this.service.AddNote("test"));
        }

        [Test]
        public void AddNote_ValidUser_AddsNote()
        {
            this.service.AddNote("My note");
            this.mockUsersRepository.Verify(repository => repository.UpdateUser(this.testUser), Times.Once);
            Assert.That(this.testUser.PeriodNotes.Count, Is.EqualTo(1));
        }

        // --- UpdateNote ---
        [Test]
        public void UpdateNote_NullUser_DoesNotThrow()
        {
            this.mockCurrentUserService.Setup(service => service.RaresCurrentUser).Returns((User)null);
            Assert.DoesNotThrow(() => this.service.UpdateNote(1, "body", false));
        }

        [Test]
        public void UpdateNote_ValidUser_Updates()
        {
            this.testUser.AddPeriodNoteToUser(1, "Old", false);
            this.service.UpdateNote(1, "New", true);
            Assert.That(this.testUser.PeriodNotes[1].Item1, Is.EqualTo("New"));
            Assert.That(this.testUser.PeriodNotes[1].Item2, Is.True);
            this.mockUsersRepository.Verify(repository => repository.UpdateUser(this.testUser), Times.Once);
        }

        // --- DeleteNote ---
        [Test]
        public void DeleteNote_NullUser_DoesNotThrow()
        {
            this.mockCurrentUserService.Setup(service => service.RaresCurrentUser).Returns((User)null);
            Assert.DoesNotThrow(() => this.service.DeleteNote(1));
        }

        [Test]
        public void DeleteNote_NoteExists_RemovesNote()
        {
            this.testUser.AddPeriodNoteToUser(1, "Note", false);
            this.service.DeleteNote(1);
            Assert.That(this.testUser.PeriodNotes.ContainsKey(1), Is.False);
            this.mockUsersRepository.Verify(repository => repository.UpdateUser(this.testUser), Times.Once);
        }

        [Test]
        public void DeleteNote_NoteDoesNotExist_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => this.service.DeleteNote(99));
        }

        // --- SaveCurrentUser ---
        [Test]
        public void SaveCurrentUser_NullUser_DoesNotThrow()
        {
            this.mockCurrentUserService.Setup(service => service.RaresCurrentUser).Returns((User)null);
            Assert.DoesNotThrow(() => this.service.SaveCurrentUser());
        }

        [Test]
        public void SaveCurrentUser_ValidUser_CallsUpdate()
        {
            this.service.SaveCurrentUser();
            this.mockUsersRepository.Verify(repository => repository.UpdateUser(this.testUser), Times.Once);
        }
    }
}