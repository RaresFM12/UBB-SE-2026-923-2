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

        [Test]
        public void UpdatePeriodTracker_UpdatesCycleDaysToMinimum()
        {
            service.UpdatePeriodTracker(DateTimeOffset.Now, 1, 1, 0);
            Assert.That(testUser.CycleDays, Is.EqualTo(1));
            Assert.That(testUser.PeriodLasts, Is.EqualTo(1));
        }

        [Test]
        public void UpdatePeriodTracker_LargeCycleDays_Accepted()
        {
            service.UpdatePeriodTracker(DateTimeOffset.Now, 100, 10, 0);
            Assert.That(testUser.CycleDays, Is.EqualTo(100));
        }

        [Test]
        public void GetNotes_AfterAddAndDelete_ReturnsCorrectCount()
        {
            testUser.AddPeriodNoteToUser(1, "Note1", false);
            testUser.AddPeriodNoteToUser(2, "Note2", false);
            service.DeleteNote(1);
            var result = service.GetNotes();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddNote_EmptyBody_AddsEmptyString()
        {
            service.AddNote("");
            Assert.That(testUser.PeriodNotes.Values.First().Item1, Is.EqualTo(""));
        }

        [Test]
        public void AddNote_LongBody_AddsFullText()
        {
            var longText = new string('x', 5000);
            service.AddNote(longText);
            Assert.That(testUser.PeriodNotes.Values.First().Item1, Is.EqualTo(longText));
        }

        [Test]
        public void UpdateNote_NonExistentId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => service.UpdateNote(999, "body", false));
        }

        [Test]
        public void GetTrackerState_DefaultCycleDays_Returns28()
        {
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(true);
            var result = service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(testUser.CycleDays));
        }

        [Test]
        public void DeleteNote_TwiceSameId_DoesNotThrow()
        {
            testUser.AddPeriodNoteToUser(1, "Note", false);
            service.DeleteNote(1);
            Assert.DoesNotThrow(() => service.DeleteNote(1));
        }

        [Test]
        public void GetTrackerState_ValidUser_ReturnsPremenstrualSyndromeOption()
        {
            testUser.PremenstrualSyndromeOption = 2;
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(true);
            var result = service.GetTrackerState();
            Assert.That(result.PremenstrualSyndromeOption, Is.EqualTo(2));
        }

        [Test]
        public void GetTrackerState_UserWithConfiguredStartDate_ReturnsConfiguredDate()
        {
            testUser.StartPeriodDate = new DateOnly(2025, 3, 15);
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(true);
            var result = service.GetTrackerState();
            Assert.That(result.StartPeriodDate.DateTime.Date, Is.EqualTo(new DateTime(2025, 3, 15)));
        }

        [Test]
        public void UpdatePeriodTracker_SetsStartPeriodDate()
        {
            var date = new DateTimeOffset(new DateTime(2025, 6, 1));
            service.UpdatePeriodTracker(date, 28, 5, 0);
            Assert.That(testUser.StartPeriodDate, Is.EqualTo(new DateOnly(2025, 6, 1)));
        }

        [Test]
        public void UpdatePeriodTracker_SetsPremenstrualSyndromeOption()
        {
            service.UpdatePeriodTracker(DateTimeOffset.Now, 28, 5, 2);
            Assert.That(testUser.PremenstrualSyndromeOption, Is.EqualTo(2));
        }

        [Test]
        public void UpdatePeriodTracker_ZeroCycleDays_SetsZero()
        {
            service.UpdatePeriodTracker(DateTimeOffset.Now, 0, 0, 0);
            Assert.That(testUser.CycleDays, Is.EqualTo(0));
            Assert.That(testUser.PeriodLasts, Is.EqualTo(0));
        }

        [Test]
        public void UpdatePeriodTracker_CallsSaveCurrentUser()
        {
            service.UpdatePeriodTracker(DateTimeOffset.Now, 28, 5, 0);
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Once);
        }

        [Test]
        public void UpdatePeriodTracker_NegativeValues_SetsNegative()
        {
            service.UpdatePeriodTracker(DateTimeOffset.Now, -1, -1, -1);
            Assert.That(testUser.CycleDays, Is.EqualTo(-1));
            Assert.That(testUser.PeriodLasts, Is.EqualTo(-1));
        }

        [Test]
        public void AddNote_AfterDelete_UsesMaxPlusOne()
        {
            testUser.AddPeriodNoteToUser(1, "N1", false);
            testUser.AddPeriodNoteToUser(5, "N5", false);
            service.DeleteNote(5);
            service.AddNote("New");
            Assert.That(testUser.PeriodNotes.ContainsKey(2), Is.True);
        }

        [Test]
        public void UpdateNote_ChangesBodyOnly_KeepsDone()
        {
            testUser.AddPeriodNoteToUser(1, "Old", true);
            service.UpdateNote(1, "New", true);
            Assert.That(testUser.PeriodNotes[1].Item1, Is.EqualTo("New"));
            Assert.That(testUser.PeriodNotes[1].Item2, Is.True);
        }

        [Test]
        public void UpdateNote_EmptyBody_SetsEmpty()
        {
            testUser.AddPeriodNoteToUser(1, "Old", false);
            service.UpdateNote(1, "", false);
            Assert.That(testUser.PeriodNotes[1].Item1, Is.EqualTo(""));
        }

        [Test]
        public void UpdateNote_CallsSave()
        {
            testUser.AddPeriodNoteToUser(1, "Old", false);
            service.UpdateNote(1, "New", false);
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Once);
        }

        [Test]
        public void DeleteNote_CallsSave()
        {
            testUser.AddPeriodNoteToUser(1, "Note", false);
            service.DeleteNote(1);
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Once);
        }

        [Test]
        public void DeleteNote_MiddleNote_LeavesOthers()
        {
            testUser.AddPeriodNoteToUser(1, "N1", false);
            testUser.AddPeriodNoteToUser(2, "N2", false);
            testUser.AddPeriodNoteToUser(3, "N3", false);
            service.DeleteNote(2);
            Assert.That(testUser.PeriodNotes.ContainsKey(1), Is.True);
            Assert.That(testUser.PeriodNotes.ContainsKey(3), Is.True);
            Assert.That(testUser.PeriodNotes.ContainsKey(2), Is.False);
        }

        [Test]
        public void DeleteNote_NullNotes_DoesNotThrow()
        {
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns((User)null);
            Assert.DoesNotThrow(() => service.DeleteNote(1));
        }

        [Test]
        public void GetNotes_ReturnsReferenceToActualDictionary()
        {
            testUser.AddPeriodNoteToUser(1, "Note1", false);
            var notes = service.GetNotes();
            Assert.That(notes, Is.SameAs(testUser.PeriodNotes));
        }

        [Test]
        public void GetNotes_EmptyNotes_ReturnsEmptyDictionary()
        {
            var result = service.GetNotes();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetMaxNoteId_SingleNote_ReturnsItsId()
        {
            testUser.AddPeriodNoteToUser(42, "Note", false);
            var result = service.GetMaxNoteId();
            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void UpdatePeriodTracker_DoubleCycleDays_ConvertsToInt()
        {
            service.UpdatePeriodTracker(DateTimeOffset.Now, 28.7, 5.9, 0);
            Assert.That(testUser.CycleDays, Is.EqualTo(29));
            Assert.That(testUser.PeriodLasts, Is.EqualTo(6));
        }

        [Test]
        public void AddNote_ThreeNotes_AllPresent()
        {
            service.AddNote("A");
            service.AddNote("B");
            service.AddNote("C");
            Assert.That(testUser.PeriodNotes.Count, Is.EqualTo(3));
            Assert.That(testUser.PeriodNotes[1].Item1, Is.EqualTo("A"));
            Assert.That(testUser.PeriodNotes[2].Item1, Is.EqualTo("B"));
            Assert.That(testUser.PeriodNotes[3].Item1, Is.EqualTo("C"));
        }

        [Test]
        public void UpdateNote_MultipleUpdates_LastWins()
        {
            testUser.AddPeriodNoteToUser(1, "V1", false);
            service.UpdateNote(1, "V2", false);
            service.UpdateNote(1, "V3", true);
            Assert.That(testUser.PeriodNotes[1].Item1, Is.EqualTo("V3"));
            Assert.That(testUser.PeriodNotes[1].Item2, Is.True);
        }

        [Test]
        public void GetTrackerState_AfterUpdate_ReflectsNewValues()
        {
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(true);
            service.UpdatePeriodTracker(new DateTimeOffset(new DateTime(2025, 1, 1)), 35, 7, 1);
            var result = service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(35));
            Assert.That(result.PeriodLasts, Is.EqualTo(7));
            Assert.That(result.PremenstrualSyndromeOption, Is.EqualTo(1));
        }

        [Test]
        public void DeleteNote_AllNotes_LeavesEmpty()
        {
            testUser.AddPeriodNoteToUser(1, "N1", false);
            testUser.AddPeriodNoteToUser(2, "N2", false);
            service.DeleteNote(1);
            service.DeleteNote(2);
            Assert.That(testUser.PeriodNotes.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetMaxNoteId_AfterDeleteAll_ReturnsZero()
        {
            testUser.AddPeriodNoteToUser(1, "N1", false);
            service.DeleteNote(1);
            Assert.That(service.GetMaxNoteId(), Is.EqualTo(0));
        }

        [Test]
        public void AddNote_AfterDeleteAll_StartsFromOne()
        {
            testUser.AddPeriodNoteToUser(1, "N1", false);
            service.DeleteNote(1);
            service.AddNote("New");
            Assert.That(testUser.PeriodNotes.ContainsKey(1), Is.True);
        }

        [Test]
        public void GetTrackerState_StartPeriodDateMidnight_HasZeroTime()
        {
            testUser.StartPeriodDate = new DateOnly(2025, 5, 10);
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(true);
            var result = service.GetTrackerState();
            Assert.That(result.StartPeriodDate.Hour, Is.EqualTo(0));
            Assert.That(result.StartPeriodDate.Minute, Is.EqualTo(0));
        }

        [Test]
        public void DeleteNote_NegativeId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => service.DeleteNote(-1));
        }

        [Test]
        public void UpdateNote_NegativeId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => service.UpdateNote(-1, "body", false));
        }

        [Test]
        public void UpdatePeriodTracker_MaxIntCycleDays_Accepted()
        {
            service.UpdatePeriodTracker(DateTimeOffset.Now, int.MaxValue, 5, 0);
            Assert.That(testUser.CycleDays, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void DeleteNote_DoesNotCallSave_WhenNoteNotFound()
        {
            service.DeleteNote(999);
            mockUsersRepo.Verify(r => r.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void AddNote_AfterGapInIds_UsesMaxPlusOne()
        {
            testUser.AddPeriodNoteToUser(1, "N1", false);
            testUser.AddPeriodNoteToUser(100, "N100", false);
            service.AddNote("New");
            Assert.That(testUser.PeriodNotes.ContainsKey(101), Is.True);
        }

        [Test]
        public void GetTrackerState_AfterMultipleUpdates_ReturnsLatest()
        {
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(true);
            service.UpdatePeriodTracker(DateTimeOffset.Now, 25, 4, 0);
            service.UpdatePeriodTracker(DateTimeOffset.Now, 32, 8, 2);
            var result = service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(32));
            Assert.That(result.PeriodLasts, Is.EqualTo(8));
            Assert.That(result.PremenstrualSyndromeOption, Is.EqualTo(2));
        }

        [Test]
        public void GetNotes_ContainsCorrectFlags()
        {
            testUser.AddPeriodNoteToUser(1, "A", false);
            testUser.AddPeriodNoteToUser(2, "B", true);
            testUser.AddPeriodNoteToUser(3, "C", false);
            var notes = service.GetNotes();
            Assert.That(notes[1].Item2, Is.False);
            Assert.That(notes[2].Item2, Is.True);
            Assert.That(notes[3].Item2, Is.False);
        }

        [Test]
        public void DeleteNote_ZeroId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => service.DeleteNote(0));
        }

        [Test]
        public void UpdateNote_ZeroId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => service.UpdateNote(0, "body", false));
        }

        [Test]
        public void UpdatePeriodTracker_DateTimeOffsetMinValue_SetsMinDate()
        {
            service.UpdatePeriodTracker(DateTimeOffset.MinValue, 28, 5, 0);
            Assert.That(testUser.StartPeriodDate, Is.EqualTo(DateOnly.MinValue));
        }

        [Test]
        public void UpdatePeriodTracker_PremenstrualSyndromeOptionNegative_SetsNegative()
        {
            service.UpdatePeriodTracker(DateTimeOffset.Now, 28, 5, -1);
            Assert.That(testUser.PremenstrualSyndromeOption, Is.EqualTo(-1));
        }
    }
}
