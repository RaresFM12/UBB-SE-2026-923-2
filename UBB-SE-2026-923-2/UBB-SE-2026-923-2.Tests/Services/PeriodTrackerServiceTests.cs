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

        [Test]
        public void GetTrackerState_NullUser_ReturnsDefault()
        {
            this.mockCurrentUserService.Setup(service => service.RaresCurrentUser).Returns((User)null);
            var result = this.service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(28));
            Assert.That(result.PeriodLasts, Is.EqualTo(5));
            Assert.That(result.HasPeriodTracker, Is.False);
        }

        [Test]
        public void GetTrackerState_ValidUser_ReturnsCycleDays()
        {
            this.testUser.CycleDays = 30;
            this.testUser.PeriodLasts = 6;
            this.mockUsersRepository.Setup(repository => repository.UserHasPeriodTracker(1)).Returns(true);
            var result = this.service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(30));
            Assert.That(result.PeriodLasts, Is.EqualTo(6));
            Assert.That(result.HasPeriodTracker, Is.True);
        }

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

        [Test]
        public void GetMaxNoteId_NoNotes_ReturnsZero()
        {
            var result = this.service.GetMaxNoteId();
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetMaxNoteId_NullUser_ReturnsZero()
        {
            this.mockCurrentUserService.Setup(service => service.RaresCurrentUser).Returns((User)null);
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
            Assert.That(this.testUser.PeriodLasts, Is.EqualTo(6));
        }

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
            Assert.That(this.testUser.PeriodNotes.Values.First().Item1, Is.EqualTo("My note"));
        }

        [Test]
        public void AddNote_NullBody_AddsEmptyString()
        {
            this.service.AddNote(null);
            this.mockUsersRepository.Verify(repository => repository.UpdateUser(this.testUser), Times.Once);
            Assert.That(this.testUser.PeriodNotes.Values.First().Item1, Is.EqualTo(string.Empty));
        }

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

        [Test]
        public void UpdateNote_NullBody_SetsEmpty()
        {
            this.testUser.AddPeriodNoteToUser(1, "Old", false);
            this.service.UpdateNote(1, null, false);
            Assert.That(this.testUser.PeriodNotes[1].Item1, Is.EqualTo(string.Empty));
        }

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

        [Test]
        public void AddNote_MultipleNotes_IncrementsId()
        {
            this.service.AddNote("First");
            this.service.AddNote("Second");
            Assert.That(this.testUser.PeriodNotes.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetTrackerState_UserNoPeriodTracker_HasPeriodTrackerFalse()
        {
            this.mockUsersRepository.Setup(repository => repository.UserHasPeriodTracker(1)).Returns(false);
            var result = this.service.GetTrackerState();
            Assert.That(result.HasPeriodTracker, Is.False);
        }

        [Test]
        public void UpdatePeriodTracker_UpdatesCycleDaysToMinimum()
        {
            this.service.UpdatePeriodTracker(DateTimeOffset.Now, 1, 1, 0);
            Assert.That(this.testUser.CycleDays, Is.EqualTo(1));
            Assert.That(this.testUser.PeriodLasts, Is.EqualTo(1));
        }

        [Test]
        public void UpdatePeriodTracker_LargeCycleDays_Accepted()
        {
            this.service.UpdatePeriodTracker(DateTimeOffset.Now, 100, 10, 0);
            Assert.That(this.testUser.CycleDays, Is.EqualTo(100));
        }

        [Test]
        public void GetNotes_AfterAddAndDelete_ReturnsCorrectCount()
        {
            this.testUser.AddPeriodNoteToUser(1, "Note1", false);
            this.testUser.AddPeriodNoteToUser(2, "Note2", false);
            this.service.DeleteNote(1);
            var result = this.service.GetNotes();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddNote_EmptyBody_AddsEmptyString()
        {
            this.service.AddNote(string.Empty);
            Assert.That(this.testUser.PeriodNotes.Values.First().Item1, Is.EqualTo(string.Empty));
        }

        [Test]
        public void AddNote_LongBody_AddsFullText()
        {
            var longText = new string('x', 5000);
            this.service.AddNote(longText);
            Assert.That(this.testUser.PeriodNotes.Values.First().Item1, Is.EqualTo(longText));
        }

        [Test]
        public void UpdateNote_NonExistentId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => this.service.UpdateNote(999, "body", false));
        }

        [Test]
        public void GetTrackerState_DefaultCycleDays_Returns28()
        {
            this.mockUsersRepository.Setup(repository => repository.UserHasPeriodTracker(1)).Returns(true);
            var result = this.service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(this.testUser.CycleDays));
        }

        [Test]
        public void DeleteNote_TwiceSameId_DoesNotThrow()
        {
            this.testUser.AddPeriodNoteToUser(1, "Note", false);
            this.service.DeleteNote(1);
            Assert.DoesNotThrow(() => this.service.DeleteNote(1));
        }

        [Test]
        public void GetTrackerState_ValidUser_ReturnsPremenstrualSyndromeOption()
        {
            this.testUser.PremenstrualSyndromeOption = 2;
            this.mockUsersRepository.Setup(repository => repository.UserHasPeriodTracker(1)).Returns(true);
            var result = this.service.GetTrackerState();
            Assert.That(result.PremenstrualSyndromeOption, Is.EqualTo(2));
        }

        [Test]
        public void GetTrackerState_UserWithConfiguredStartDate_ReturnsConfiguredDate()
        {
            this.testUser.StartPeriodDate = new DateOnly(2025, 3, 15);
            this.mockUsersRepository.Setup(repository => repository.UserHasPeriodTracker(1)).Returns(true);
            var result = this.service.GetTrackerState();
            Assert.That(result.StartPeriodDate.DateTime.Date, Is.EqualTo(new DateTime(2025, 3, 15)));
        }

        [Test]
        public void UpdatePeriodTracker_SetsStartPeriodDate()
        {
            var date = new DateTimeOffset(new DateTime(2025, 6, 1));
            this.service.UpdatePeriodTracker(date, 28, 5, 0);
            Assert.That(this.testUser.StartPeriodDate, Is.EqualTo(new DateOnly(2025, 6, 1)));
        }

        [Test]
        public void UpdatePeriodTracker_SetsPremenstrualSyndromeOption()
        {
            this.service.UpdatePeriodTracker(DateTimeOffset.Now, 28, 5, 2);
            Assert.That(this.testUser.PremenstrualSyndromeOption, Is.EqualTo(2));
        }

        [Test]
        public void UpdatePeriodTracker_ZeroCycleDays_SetsZero()
        {
            this.service.UpdatePeriodTracker(DateTimeOffset.Now, 0, 0, 0);
            Assert.That(this.testUser.CycleDays, Is.EqualTo(0));
            Assert.That(this.testUser.PeriodLasts, Is.EqualTo(0));
        }

        [Test]
        public void UpdatePeriodTracker_CallsSaveCurrentUser()
        {
            this.service.UpdatePeriodTracker(DateTimeOffset.Now, 28, 5, 0);
            this.mockUsersRepository.Verify(repository => repository.UpdateUser(this.testUser), Times.Once);
        }

        [Test]
        public void UpdatePeriodTracker_NegativeValues_SetsNegative()
        {
            this.service.UpdatePeriodTracker(DateTimeOffset.Now, -1, -1, -1);
            Assert.That(this.testUser.CycleDays, Is.EqualTo(-1));
            Assert.That(this.testUser.PeriodLasts, Is.EqualTo(-1));
        }

        [Test]
        public void AddNote_AfterDelete_UsesMaxPlusOne()
        {
            this.testUser.AddPeriodNoteToUser(1, "N1", false);
            this.testUser.AddPeriodNoteToUser(5, "N5", false);
            this.service.DeleteNote(5);
            this.service.AddNote("New");
            Assert.That(this.testUser.PeriodNotes.ContainsKey(2), Is.True);
        }

        [Test]
        public void UpdateNote_ChangesBodyOnly_KeepsDone()
        {
            this.testUser.AddPeriodNoteToUser(1, "Old", true);
            this.service.UpdateNote(1, "New", true);
            Assert.That(this.testUser.PeriodNotes[1].Item1, Is.EqualTo("New"));
            Assert.That(this.testUser.PeriodNotes[1].Item2, Is.True);
        }

        [Test]
        public void UpdateNote_EmptyBody_SetsEmpty()
        {
            this.testUser.AddPeriodNoteToUser(1, "Old", false);
            this.service.UpdateNote(1, string.Empty, false);
            Assert.That(this.testUser.PeriodNotes[1].Item1, Is.EqualTo(string.Empty));
        }

        [Test]
        public void UpdateNote_CallsSave()
        {
            this.testUser.AddPeriodNoteToUser(1, "Old", false);
            this.service.UpdateNote(1, "New", false);
            this.mockUsersRepository.Verify(repository => repository.UpdateUser(this.testUser), Times.Once);
        }

        [Test]
        public void DeleteNote_CallsSave()
        {
            this.testUser.AddPeriodNoteToUser(1, "Note", false);
            this.service.DeleteNote(1);
            this.mockUsersRepository.Verify(repository => repository.UpdateUser(this.testUser), Times.Once);
        }

        [Test]
        public void DeleteNote_MiddleNote_LeavesOthers()
        {
            this.testUser.AddPeriodNoteToUser(1, "N1", false);
            this.testUser.AddPeriodNoteToUser(2, "N2", false);
            this.testUser.AddPeriodNoteToUser(3, "N3", false);
            this.service.DeleteNote(2);
            Assert.That(this.testUser.PeriodNotes.ContainsKey(1), Is.True);
            Assert.That(this.testUser.PeriodNotes.ContainsKey(3), Is.True);
            Assert.That(this.testUser.PeriodNotes.ContainsKey(2), Is.False);
        }

        [Test]
        public void DeleteNote_NullNotes_DoesNotThrow()
        {
            this.mockCurrentUserService.Setup(service => service.RaresCurrentUser).Returns((User)null);
            Assert.DoesNotThrow(() => this.service.DeleteNote(1));
        }

        [Test]
        public void GetNotes_ReturnsReferenceToActualDictionary()
        {
            this.testUser.AddPeriodNoteToUser(1, "Note1", false);
            var notes = this.service.GetNotes();
            Assert.That(notes, Is.SameAs(this.testUser.PeriodNotes));
        }

        [Test]
        public void GetNotes_EmptyNotes_ReturnsEmptyDictionary()
        {
            var result = this.service.GetNotes();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetMaxNoteId_SingleNote_ReturnsItsId()
        {
            this.testUser.AddPeriodNoteToUser(42, "Note", false);
            var result = this.service.GetMaxNoteId();
            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void UpdatePeriodTracker_DoubleCycleDays_ConvertsToInt()
        {
            this.service.UpdatePeriodTracker(DateTimeOffset.Now, 28.7, 5.9, 0);
            Assert.That(this.testUser.CycleDays, Is.EqualTo(29));
            Assert.That(this.testUser.PeriodLasts, Is.EqualTo(6));
        }

        [Test]
        public void AddNote_ThreeNotes_AllPresent()
        {
            this.service.AddNote("A");
            this.service.AddNote("B");
            this.service.AddNote("C");
            Assert.That(this.testUser.PeriodNotes.Count, Is.EqualTo(3));
            Assert.That(this.testUser.PeriodNotes[1].Item1, Is.EqualTo("A"));
            Assert.That(this.testUser.PeriodNotes[2].Item1, Is.EqualTo("B"));
            Assert.That(this.testUser.PeriodNotes[3].Item1, Is.EqualTo("C"));
        }

        [Test]
        public void UpdateNote_MultipleUpdates_LastWins()
        {
            this.testUser.AddPeriodNoteToUser(1, "V1", false);
            this.service.UpdateNote(1, "V2", false);
            this.service.UpdateNote(1, "V3", true);
            Assert.That(this.testUser.PeriodNotes[1].Item1, Is.EqualTo("V3"));
            Assert.That(this.testUser.PeriodNotes[1].Item2, Is.True);
        }

        [Test]
        public void GetTrackerState_AfterUpdate_ReflectsNewValues()
        {
            this.mockUsersRepository.Setup(repository => repository.UserHasPeriodTracker(1)).Returns(true);
            this.service.UpdatePeriodTracker(new DateTimeOffset(new DateTime(2025, 1, 1)), 35, 7, 1);
            var result = this.service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(35));
            Assert.That(result.PeriodLasts, Is.EqualTo(7));
            Assert.That(result.PremenstrualSyndromeOption, Is.EqualTo(1));
        }

        [Test]
        public void DeleteNote_AllNotes_LeavesEmpty()
        {
            this.testUser.AddPeriodNoteToUser(1, "N1", false);
            this.testUser.AddPeriodNoteToUser(2, "N2", false);
            this.service.DeleteNote(1);
            this.service.DeleteNote(2);
            Assert.That(this.testUser.PeriodNotes.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetMaxNoteId_AfterDeleteAll_ReturnsZero()
        {
            this.testUser.AddPeriodNoteToUser(1, "N1", false);
            this.service.DeleteNote(1);
            Assert.That(this.service.GetMaxNoteId(), Is.EqualTo(0));
        }

        [Test]
        public void AddNote_AfterDeleteAll_StartsFromOne()
        {
            this.testUser.AddPeriodNoteToUser(1, "N1", false);
            this.service.DeleteNote(1);
            this.service.AddNote("New");
            Assert.That(this.testUser.PeriodNotes.ContainsKey(1), Is.True);
        }

        [Test]
        public void GetTrackerState_StartPeriodDateMidnight_HasZeroTime()
        {
            this.testUser.StartPeriodDate = new DateOnly(2025, 5, 10);
            this.mockUsersRepository.Setup(repository => repository.UserHasPeriodTracker(1)).Returns(true);
            var result = this.service.GetTrackerState();
            Assert.That(result.StartPeriodDate.Hour, Is.EqualTo(0));
            Assert.That(result.StartPeriodDate.Minute, Is.EqualTo(0));
        }

        [Test]
        public void DeleteNote_NegativeId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => this.service.DeleteNote(-1));
        }

        [Test]
        public void UpdateNote_NegativeId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => this.service.UpdateNote(-1, "body", false));
        }

        [Test]
        public void UpdatePeriodTracker_MaxIntCycleDays_Accepted()
        {
            this.service.UpdatePeriodTracker(DateTimeOffset.Now, int.MaxValue, 5, 0);
            Assert.That(this.testUser.CycleDays, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void DeleteNote_DoesNotCallSave_WhenNoteNotFound()
        {
            this.service.DeleteNote(999);
            this.mockUsersRepository.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void AddNote_AfterGapInIds_UsesMaxPlusOne()
        {
            this.testUser.AddPeriodNoteToUser(1, "N1", false);
            this.testUser.AddPeriodNoteToUser(100, "N100", false);
            this.service.AddNote("New");
            Assert.That(this.testUser.PeriodNotes.ContainsKey(101), Is.True);
        }

        [Test]
        public void GetTrackerState_AfterMultipleUpdates_ReturnsLatest()
        {
            this.mockUsersRepository.Setup(repository => repository.UserHasPeriodTracker(1)).Returns(true);
            this.service.UpdatePeriodTracker(DateTimeOffset.Now, 25, 4, 0);
            this.service.UpdatePeriodTracker(DateTimeOffset.Now, 32, 8, 2);
            var result = this.service.GetTrackerState();
            Assert.That(result.CycleDays, Is.EqualTo(32));
            Assert.That(result.PeriodLasts, Is.EqualTo(8));
            Assert.That(result.PremenstrualSyndromeOption, Is.EqualTo(2));
        }

        [Test]
        public void GetNotes_ContainsCorrectFlags()
        {
            this.testUser.AddPeriodNoteToUser(1, "A", false);
            this.testUser.AddPeriodNoteToUser(2, "B", true);
            this.testUser.AddPeriodNoteToUser(3, "C", false);
            var notes = this.service.GetNotes();
            Assert.That(notes[1].Item2, Is.False);
            Assert.That(notes[2].Item2, Is.True);
            Assert.That(notes[3].Item2, Is.False);
        }

        [Test]
        public void DeleteNote_ZeroId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => this.service.DeleteNote(0));
        }

        [Test]
        public void UpdateNote_ZeroId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => this.service.UpdateNote(0, "body", false));
        }

        [Test]
        public void UpdatePeriodTracker_DateTimeOffsetMinValue_SetsMinDate()
        {
            this.service.UpdatePeriodTracker(DateTimeOffset.MinValue, 28, 5, 0);
            Assert.That(this.testUser.StartPeriodDate, Is.EqualTo(DateOnly.MinValue));
        }

        [Test]
        public void UpdatePeriodTracker_PremenstrualSyndromeOptionNegative_SetsNegative()
        {
            this.service.UpdatePeriodTracker(DateTimeOffset.Now, 28, 5, -1);
            Assert.That(this.testUser.PremenstrualSyndromeOption, Is.EqualTo(-1));
        }
    }
}
