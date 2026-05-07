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
    public class PeriodTrackerServiceTests
    {
        private Mock<IUsersRepository> mockUsersRepo;
        private Mock<RaresICurrentUserService> mockCurrentUserService;
        private PeriodTrackerService service;
        private User testUser;

        [SetUp]
        public void Setup()
        {
            mockUsersRepo = new Mock<IUsersRepository>();
            mockCurrentUserService = new Mock<RaresICurrentUserService>();
            service = new PeriodTrackerService(mockUsersRepo.Object, mockCurrentUserService.Object);

            testUser = new User(1, "a@b.com", "123", "hash", false, false, "user1", false, 0);
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns(testUser);
        }

        [Test]
        public void GetCurrentUser_ReturnsUser()
        {
            var result = service.GetCurrentUser();
            Assert.That(result, Is.EqualTo(testUser));
        }

        [Test]
        public void GetCurrentUser_NullUser_ReturnsNull()
        {
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns((User)null);
            var result = service.GetCurrentUser();
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetTrackerState_NullUser_ReturnsDefault()
        {
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns((User)null);
            var result = service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(28));
            Assert.That(result.PeriodLasts, Is.EqualTo(5));
            Assert.That(result.HasPeriodTracker, Is.False);
        }

        [Test]
        public void GetTrackerState_ValidUser_ReturnsCycleDays()
        {
            testUser.CycleDays = 30;
            testUser.PeriodLasts = 6;
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(true);
            var result = service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(30));
            Assert.That(result.PeriodLasts, Is.EqualTo(6));
            Assert.That(result.HasPeriodTracker, Is.True);
        }

        [Test]
        public void GetNotes_NullUser_ReturnsEmpty()
        {
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns((User)null);
            var result = service.GetNotes();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetNotes_UserWithNotes_ReturnsNotes()
        {
            testUser.AddPeriodNoteToUser(1, "Note1", false);
            testUser.AddPeriodNoteToUser(2, "Note2", true);
            var result = service.GetNotes();
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetMaxNoteId_NoNotes_ReturnsZero()
        {
            var result = service.GetMaxNoteId();
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetMaxNoteId_NullUser_ReturnsZero()
        {
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns((User)null);
            var result = service.GetMaxNoteId();
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetMaxNoteId_WithNotes_ReturnsMax()
        {
            testUser.AddPeriodNoteToUser(1, "N1", false);
            testUser.AddPeriodNoteToUser(5, "N5", false);
            testUser.AddPeriodNoteToUser(3, "N3", false);
            var result = service.GetMaxNoteId();
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public void UpdatePeriodTracker_NullUser_DoesNotThrow()
        {
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns((User)null);
            Assert.DoesNotThrow(() => service.UpdatePeriodTracker(DateTimeOffset.Now, 28, 5, 0));
        }

        [Test]
        public void UpdatePeriodTracker_ValidUser_UpdatesAndSaves()
        {
            service.UpdatePeriodTracker(new DateTimeOffset(new DateTime(2025, 3, 1)), 30, 6, 1);
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Once);
            Assert.That(testUser.CycleDays, Is.EqualTo(30));
            Assert.That(testUser.PeriodLasts, Is.EqualTo(6));
        }

        [Test]
        public void AddNote_NullUser_DoesNotThrow()
        {
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns((User)null);
            Assert.DoesNotThrow(() => service.AddNote("test"));
        }

        [Test]
        public void AddNote_ValidUser_AddsNote()
        {
            service.AddNote("My note");
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Once);
            Assert.That(testUser.PeriodNotes.Count, Is.EqualTo(1));
            Assert.That(testUser.PeriodNotes.Values.First().Item1, Is.EqualTo("My note"));
        }

        [Test]
        public void AddNote_NullBody_AddsEmptyString()
        {
            service.AddNote(null);
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Once);
            Assert.That(testUser.PeriodNotes.Values.First().Item1, Is.EqualTo(""));
        }

        [Test]
        public void UpdateNote_NullUser_DoesNotThrow()
        {
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns((User)null);
            Assert.DoesNotThrow(() => service.UpdateNote(1, "body", false));
        }

        [Test]
        public void UpdateNote_ValidUser_Updates()
        {
            testUser.AddPeriodNoteToUser(1, "Old", false);
            service.UpdateNote(1, "New", true);
            Assert.That(testUser.PeriodNotes[1].Item1, Is.EqualTo("New"));
            Assert.That(testUser.PeriodNotes[1].Item2, Is.True);
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Once);
        }

        [Test]
        public void UpdateNote_NullBody_SetsEmpty()
        {
            testUser.AddPeriodNoteToUser(1, "Old", false);
            service.UpdateNote(1, null, false);
            Assert.That(testUser.PeriodNotes[1].Item1, Is.EqualTo(""));
        }

        [Test]
        public void DeleteNote_NullUser_DoesNotThrow()
        {
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns((User)null);
            Assert.DoesNotThrow(() => service.DeleteNote(1));
        }

        [Test]
        public void DeleteNote_NoteExists_RemovesNote()
        {
            testUser.AddPeriodNoteToUser(1, "Note", false);
            service.DeleteNote(1);
            Assert.That(testUser.PeriodNotes.ContainsKey(1), Is.False);
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Once);
        }

        [Test]
        public void DeleteNote_NoteDoesNotExist_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => service.DeleteNote(99));
        }

        [Test]
        public void SaveCurrentUser_NullUser_DoesNotThrow()
        {
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns((User)null);
            Assert.DoesNotThrow(() => service.SaveCurrentUser());
        }

        [Test]
        public void SaveCurrentUser_ValidUser_CallsUpdate()
        {
            service.SaveCurrentUser();
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Once);
        }

        [Test]
        public void AddNote_MultipleNotes_IncrementsId()
        {
            service.AddNote("First");
            service.AddNote("Second");
            Assert.That(testUser.PeriodNotes.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetTrackerState_UserNoPeriodTracker_HasPeriodTrackerFalse()
        {
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(false);
            var result = service.GetTrackerState();
            Assert.That(result.HasPeriodTracker, Is.False);
        }
    }
}
