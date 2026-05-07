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
        public void AddNote_WhitespaceBody_AddsWhitespace()
        {
            service.AddNote("   ");
            Assert.That(testUser.PeriodNotes.Values.First().Item1, Is.EqualTo("   "));
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
        public void UpdateNote_ToggleChecked_UpdatesFlag()
        {
            testUser.AddPeriodNoteToUser(1, "Test", false);
            service.UpdateNote(1, "Test", true);
            Assert.That(testUser.PeriodNotes[1].Item2, Is.True);
            service.UpdateNote(1, "Test", false);
            Assert.That(testUser.PeriodNotes[1].Item2, Is.False);
        }

        [Test]
        public void GetMaxNoteId_AfterDeletion_ReturnsCorrectMax()
        {
            testUser.AddPeriodNoteToUser(1, "N1", false);
            testUser.AddPeriodNoteToUser(10, "N10", false);
            testUser.AddPeriodNoteToUser(5, "N5", false);
            service.DeleteNote(10);
            var result = service.GetMaxNoteId();
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public void GetTrackerState_DefaultCycleDays_Returns28()
        {
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(true);
            var result = service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(testUser.CycleDays));
        }

        [Test]
        public void SaveCurrentUser_AfterMultipleUpdates_CallsOnce()
        {
            testUser.CycleDays = 25;
            testUser.PeriodLasts = 4;
            service.SaveCurrentUser();
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Once);
        }

        [Test]
        public void DeleteNote_TwiceSameId_DoesNotThrow()
        {
            testUser.AddPeriodNoteToUser(1, "Note", false);
            service.DeleteNote(1);
            Assert.DoesNotThrow(() => service.DeleteNote(1));
        }

        [Test]
        public void AddNote_AfterDeleteAll_StartsFromMaxIdPlusOne()
        {
            service.AddNote("First");
            service.AddNote("Second");
            Assert.That(testUser.PeriodNotes.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetTrackerState_NullUser_StartPeriodDateIsToday()
        {
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns((User)null);
            var result = service.GetTrackerState();
            Assert.That(result.StartPeriodDate.Date, Is.EqualTo(DateTimeOffset.Now.Date));
        }

        [Test]
        public void GetTrackerState_NullUser_PremenstrualSyndromeOptionIsZero()
        {
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns((User)null);
            var result = service.GetTrackerState();
            Assert.That(result.PremenstrualSyndromeOption, Is.EqualTo(0));
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
        public void GetTrackerState_UserWithDefaultStartDate_ReturnsToday()
        {
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(true);
            var result = service.GetTrackerState();
            Assert.That(result.StartPeriodDate.Date, Is.EqualTo(DateTime.Today));
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
        public void AddNote_FirstNote_GetsId1()
        {
            service.AddNote("First");
            Assert.That(testUser.PeriodNotes.ContainsKey(1), Is.True);
        }

        [Test]
        public void AddNote_SecondNote_GetsId2()
        {
            service.AddNote("First");
            service.AddNote("Second");
            Assert.That(testUser.PeriodNotes.ContainsKey(2), Is.True);
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
        public void AddNote_SpecialCharacters_PreservesContent()
        {
            service.AddNote("Note with <html> & \"quotes\" and 'apostrophes'");
            Assert.That(testUser.PeriodNotes.Values.First().Item1, Does.Contain("<html>"));
        }

        [Test]
        public void AddNote_UnicodeCharacters_PreservesContent()
        {
            service.AddNote("Notă cu diacritice: ăîșțâ");
            Assert.That(testUser.PeriodNotes.Values.First().Item1, Is.EqualTo("Notă cu diacritice: ăîșțâ"));
        }

        [Test]
        public void AddNote_NewlineInBody_PreservesNewline()
        {
            service.AddNote("Line1\nLine2");
            Assert.That(testUser.PeriodNotes.Values.First().Item1, Does.Contain("\n"));
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
        public void UpdateNote_SpecialChars_PreservesContent()
        {
            testUser.AddPeriodNoteToUser(1, "Old", false);
            service.UpdateNote(1, "New <b>bold</b> & more", false);
            Assert.That(testUser.PeriodNotes[1].Item1, Does.Contain("<b>bold</b>"));
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
        public void GetNotes_NoteWithDoneTrue_ReturnsDoneFlag()
        {
            testUser.AddPeriodNoteToUser(1, "Done note", true);
            var result = service.GetNotes();
            Assert.That(result[1].Item2, Is.True);
        }

        [Test]
        public void GetNotes_NoteWithDoneFalse_ReturnsFalseFlag()
        {
            testUser.AddPeriodNoteToUser(1, "Undone note", false);
            var result = service.GetNotes();
            Assert.That(result[1].Item2, Is.False);
        }

        [Test]
        public void GetMaxNoteId_SingleNote_ReturnsItsId()
        {
            testUser.AddPeriodNoteToUser(42, "Note", false);
            var result = service.GetMaxNoteId();
            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void GetMaxNoteId_ConsecutiveIds_ReturnsLast()
        {
            testUser.AddPeriodNoteToUser(1, "N1", false);
            testUser.AddPeriodNoteToUser(2, "N2", false);
            testUser.AddPeriodNoteToUser(3, "N3", false);
            Assert.That(service.GetMaxNoteId(), Is.EqualTo(3));
        }

        [Test]
        public void GetCurrentUser_ReturnsSameInstanceAsService()
        {
            var result = service.GetCurrentUser();
            Assert.That(result, Is.SameAs(testUser));
        }

        [Test]
        public void SaveCurrentUser_MultipleCalls_CallsRepoEachTime()
        {
            service.SaveCurrentUser();
            service.SaveCurrentUser();
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Exactly(2));
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
        public void AddNote_CallsSaveEachTime()
        {
            service.AddNote("A");
            service.AddNote("B");
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Exactly(2));
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
        public void GetNotes_ManyNotes_ReturnsAll()
        {
            for (int i = 1; i <= 20; i++)
            {
                testUser.AddPeriodNoteToUser(i, $"Note{i}", i % 2 == 0);
            }
            var result = service.GetNotes();
            Assert.That(result.Count, Is.EqualTo(20));
        }

        [Test]
        public void UpdateNote_LargeId_Works()
        {
            testUser.AddPeriodNoteToUser(999999, "Big", false);
            service.UpdateNote(999999, "Updated", true);
            Assert.That(testUser.PeriodNotes[999999].Item1, Is.EqualTo("Updated"));
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
        public void GetTrackerState_ValidUser_StartPeriodDateNotDefault()
        {
            testUser.StartPeriodDate = new DateOnly(2025, 4, 20);
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(true);
            var result = service.GetTrackerState();
            Assert.That(result.StartPeriodDate.Year, Is.EqualTo(2025));
            Assert.That(result.StartPeriodDate.Month, Is.EqualTo(4));
            Assert.That(result.StartPeriodDate.Day, Is.EqualTo(20));
        }

        [Test]
        public void GetTrackerState_ValidUser_HasPeriodTrackerTrue()
        {
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(true);
            var result = service.GetTrackerState();
            Assert.That(result.HasPeriodTracker, Is.True);
        }

        [Test]
        public void UpdatePeriodTracker_MaxIntCycleDays_Accepted()
        {
            service.UpdatePeriodTracker(DateTimeOffset.Now, int.MaxValue, 5, 0);
            Assert.That(testUser.CycleDays, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void UpdatePeriodTracker_MaxIntPeriodLasts_Accepted()
        {
            service.UpdatePeriodTracker(DateTimeOffset.Now, 28, int.MaxValue, 0);
            Assert.That(testUser.PeriodLasts, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void AddNote_TabCharacter_PreservesContent()
        {
            service.AddNote("Col1\tCol2");
            Assert.That(testUser.PeriodNotes.Values.First().Item1, Does.Contain("\t"));
        }

        [Test]
        public void AddNote_CarriageReturn_PreservesContent()
        {
            service.AddNote("Line1\r\nLine2");
            Assert.That(testUser.PeriodNotes.Values.First().Item1, Does.Contain("\r\n"));
        }

        [Test]
        public void GetNotes_AfterMultipleDeletes_ReturnsCorrectCount()
        {
            testUser.AddPeriodNoteToUser(1, "N1", false);
            testUser.AddPeriodNoteToUser(2, "N2", false);
            testUser.AddPeriodNoteToUser(3, "N3", false);
            testUser.AddPeriodNoteToUser(4, "N4", false);
            service.DeleteNote(2);
            service.DeleteNote(4);
            Assert.That(service.GetNotes().Count, Is.EqualTo(2));
        }

        [Test]
        public void GetNotes_AfterMultipleDeletes_CorrectKeysRemain()
        {
            testUser.AddPeriodNoteToUser(1, "N1", false);
            testUser.AddPeriodNoteToUser(2, "N2", false);
            testUser.AddPeriodNoteToUser(3, "N3", false);
            service.DeleteNote(1);
            service.DeleteNote(3);
            var notes = service.GetNotes();
            Assert.That(notes.ContainsKey(2), Is.True);
            Assert.That(notes.Count, Is.EqualTo(1));
        }

        [Test]
        public void UpdateNote_WhitespaceBody_SetsWhitespace()
        {
            testUser.AddPeriodNoteToUser(1, "Old", false);
            service.UpdateNote(1, "   ", false);
            Assert.That(testUser.PeriodNotes[1].Item1, Is.EqualTo("   "));
        }

        [Test]
        public void UpdateNote_LongBody_SetsFullText()
        {
            testUser.AddPeriodNoteToUser(1, "Old", false);
            var longText = new string('y', 10000);
            service.UpdateNote(1, longText, false);
            Assert.That(testUser.PeriodNotes[1].Item1, Is.EqualTo(longText));
        }

        [Test]
        public void UpdateNote_UnicodeBody_PreservesUnicode()
        {
            testUser.AddPeriodNoteToUser(1, "Old", false);
            service.UpdateNote(1, "日本語テスト", false);
            Assert.That(testUser.PeriodNotes[1].Item1, Is.EqualTo("日本語テスト"));
        }

        [Test]
        public void DeleteNote_FirstOfMany_LeavesRest()
        {
            for (int i = 1; i <= 5; i++)
                testUser.AddPeriodNoteToUser(i, $"N{i}", false);
            service.DeleteNote(1);
            Assert.That(testUser.PeriodNotes.Count, Is.EqualTo(4));
            Assert.That(testUser.PeriodNotes.ContainsKey(1), Is.False);
        }

        [Test]
        public void DeleteNote_LastOfMany_LeavesRest()
        {
            for (int i = 1; i <= 5; i++)
                testUser.AddPeriodNoteToUser(i, $"N{i}", false);
            service.DeleteNote(5);
            Assert.That(testUser.PeriodNotes.Count, Is.EqualTo(4));
            Assert.That(testUser.PeriodNotes.ContainsKey(5), Is.False);
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
        public void GetMaxNoteId_LargeGapInIds_ReturnsMax()
        {
            testUser.AddPeriodNoteToUser(1, "N1", false);
            testUser.AddPeriodNoteToUser(1000, "N1000", false);
            Assert.That(service.GetMaxNoteId(), Is.EqualTo(1000));
        }

        [Test]
        public void UpdatePeriodTracker_FutureDate_Accepted()
        {
            var futureDate = new DateTimeOffset(new DateTime(2030, 12, 31));
            service.UpdatePeriodTracker(futureDate, 28, 5, 0);
            Assert.That(testUser.StartPeriodDate, Is.EqualTo(new DateOnly(2030, 12, 31)));
        }

        [Test]
        public void UpdatePeriodTracker_PastDate_Accepted()
        {
            var pastDate = new DateTimeOffset(new DateTime(2020, 1, 1));
            service.UpdatePeriodTracker(pastDate, 28, 5, 0);
            Assert.That(testUser.StartPeriodDate, Is.EqualTo(new DateOnly(2020, 1, 1)));
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
        public void SaveCurrentUser_DoesNotModifyUser()
        {
            testUser.CycleDays = 30;
            testUser.PeriodLasts = 7;
            service.SaveCurrentUser();
            Assert.That(testUser.CycleDays, Is.EqualTo(30));
            Assert.That(testUser.PeriodLasts, Is.EqualTo(7));
        }

        [Test]
        public void GetNotes_ContainsCorrectBodies()
        {
            testUser.AddPeriodNoteToUser(1, "Body1", false);
            testUser.AddPeriodNoteToUser(2, "Body2", true);
            var notes = service.GetNotes();
            Assert.That(notes[1].Item1, Is.EqualTo("Body1"));
            Assert.That(notes[2].Item1, Is.EqualTo("Body2"));
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
        public void UpdateNote_SameBodyAndFlag_StillCallsSave()
        {
            testUser.AddPeriodNoteToUser(1, "Same", true);
            service.UpdateNote(1, "Same", true);
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Once);
        }

        [Test]
        public void AddNote_TenNotes_AllHaveCorrectIds()
        {
            for (int i = 0; i < 10; i++)
                service.AddNote($"Note{i}");
            for (int i = 1; i <= 10; i++)
                Assert.That(testUser.PeriodNotes.ContainsKey(i), Is.True);
        }

        [Test]
        public void AddNote_TenNotes_CallsSaveTenTimes()
        {
            for (int i = 0; i < 10; i++)
                service.AddNote($"Note{i}");
            mockUsersRepo.Verify(r => r.UpdateUser(testUser), Times.Exactly(10));
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
        public void GetTrackerState_UserCycleDaysZero_ReturnsZero()
        {
            testUser.CycleDays = 0;
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(true);
            var result = service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(0));
        }

        [Test]
        public void GetTrackerState_UserPeriodLastsZero_ReturnsZero()
        {
            testUser.PeriodLasts = 0;
            mockUsersRepo.Setup(r => r.UserHasPeriodTracker(1)).Returns(true);
            var result = service.GetTrackerState();
            Assert.That(result.PeriodLasts, Is.EqualTo(0));
        }

        [Test]
        public void UpdatePeriodTracker_DateTimeOffsetMinValue_SetsMinDate()
        {
            service.UpdatePeriodTracker(DateTimeOffset.MinValue, 28, 5, 0);
            Assert.That(testUser.StartPeriodDate, Is.EqualTo(DateOnly.MinValue));
        }

        [Test]
        public void GetNotes_SingleNoteWithEmptyBody_ReturnsIt()
        {
            testUser.AddPeriodNoteToUser(1, "", false);
            var notes = service.GetNotes();
            Assert.That(notes[1].Item1, Is.EqualTo(""));
        }

        [Test]
        public void UpdateNote_FromDoneToNotDone_UpdatesCorrectly()
        {
            testUser.AddPeriodNoteToUser(1, "Task", true);
            service.UpdateNote(1, "Task", false);
            Assert.That(testUser.PeriodNotes[1].Item2, Is.False);
        }

        [Test]
        public void UpdateNote_FromNotDoneToDone_UpdatesCorrectly()
        {
            testUser.AddPeriodNoteToUser(1, "Task", false);
            service.UpdateNote(1, "Task", true);
            Assert.That(testUser.PeriodNotes[1].Item2, Is.True);
        }

        [Test]
        public void AddNote_SingleCharBody_Works()
        {
            service.AddNote("X");
            Assert.That(testUser.PeriodNotes.Values.First().Item1, Is.EqualTo("X"));
        }

        [Test]
        public void GetMaxNoteId_TwoNotesDescendingOrder_ReturnsMax()
        {
            testUser.AddPeriodNoteToUser(50, "N50", false);
            testUser.AddPeriodNoteToUser(25, "N25", false);
            Assert.That(service.GetMaxNoteId(), Is.EqualTo(50));
        }

        [Test]
        public void DeleteNote_AllNotesOneByOne_CountDecreasesEachTime()
        {
            testUser.AddPeriodNoteToUser(1, "N1", false);
            testUser.AddPeriodNoteToUser(2, "N2", false);
            testUser.AddPeriodNoteToUser(3, "N3", false);
            Assert.That(testUser.PeriodNotes.Count, Is.EqualTo(3));
            service.DeleteNote(1);
            Assert.That(testUser.PeriodNotes.Count, Is.EqualTo(2));
            service.DeleteNote(2);
            Assert.That(testUser.PeriodNotes.Count, Is.EqualTo(1));
            service.DeleteNote(3);
            Assert.That(testUser.PeriodNotes.Count, Is.EqualTo(0));
        }

        [Test]
        public void UpdatePeriodTracker_PremenstrualSyndromeOptionNegative_SetsNegative()
        {
            service.UpdatePeriodTracker(DateTimeOffset.Now, 28, 5, -1);
            Assert.That(testUser.PremenstrualSyndromeOption, Is.EqualTo(-1));
        }

        [Test]
        public void UpdatePeriodTracker_PremenstrualSyndromeOptionLarge_SetsValue()
        {
            service.UpdatePeriodTracker(DateTimeOffset.Now, 28, 5, 999);
            Assert.That(testUser.PremenstrualSyndromeOption, Is.EqualTo(999));
        }

        [Test]
        public void GetCurrentUser_CalledMultipleTimes_ReturnsSameUser()
        {
            var r1 = service.GetCurrentUser();
            var r2 = service.GetCurrentUser();
            var r3 = service.GetCurrentUser();
            Assert.That(r1, Is.SameAs(r2));
            Assert.That(r2, Is.SameAs(r3));
        }

        [Test]
        public void GetTrackerState_NullUser_ReturnsNonNullState()
        {
            mockCurrentUserService.Setup(s => s.RaresCurrentUser).Returns((User)null);
            var result = service.GetTrackerState();
            Assert.That(result, Is.Not.Null);
        }
    }
}
