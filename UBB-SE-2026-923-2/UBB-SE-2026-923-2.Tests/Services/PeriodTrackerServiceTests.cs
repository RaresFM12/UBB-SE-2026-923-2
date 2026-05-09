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
    public class PeriodTrackerServiceLogicTests
    {
        private Mock<IUsersRepository> mockUsersRepository;
        private Mock<RaresICurrentUserService> mockCurrentUserService;
        private PeriodTrackerService periodTrackerService;

        [SetUp]
        public void Setup()
        {
            this.mockUsersRepository = new Mock<IUsersRepository>();
            this.mockCurrentUserService = new Mock<RaresICurrentUserService>();

            this.periodTrackerService = new PeriodTrackerService(
                this.mockUsersRepository.Object,
                this.mockCurrentUserService.Object);
        }

        [Test]
        public void GetTrackerState_WhenCurrentUserIsNull_ReturnsDefaultCycleDays()
        {
            this.mockCurrentUserService
                .Setup(currentUserService => currentUserService.RaresCurrentUser)
                .Returns((User)null);

            var trackerState = this.periodTrackerService.GetTrackerState();

            Assert.That(trackerState.CycleDays, Is.EqualTo(28));
        }

        [Test]
        public void GetTrackerState_WhenCurrentUserExists_ReturnsUserPeriodLength()
        {
            var currentUser = CreateUser(7);
            currentUser.SetPeriodTracker(new DateOnly(2025, 1, 10), 30, 6, 2);

            this.mockCurrentUserService
                .Setup(currentUserService => currentUserService.RaresCurrentUser)
                .Returns(currentUser);

            var trackerState = this.periodTrackerService.GetTrackerState();

            Assert.That(trackerState.PeriodLasts, Is.EqualTo(6));
        }

        [Test]
        public void GetMaxNoteId_WhenUserHasMultipleNotes_ReturnsLargestNoteIdentifier()
        {
            var currentUser = CreateUser(7);
            currentUser.PeriodNotes = new Dictionary<int, Tuple<string, bool>>
            {
                { 1, new Tuple<string, bool>("First note", false) },
                { 5, new Tuple<string, bool>("Second note", true) },
            };

            this.mockCurrentUserService
                .Setup(currentUserService => currentUserService.RaresCurrentUser)
                .Returns(currentUser);

            var maximumNoteIdentifier = this.periodTrackerService.GetMaxNoteId();

            Assert.That(maximumNoteIdentifier, Is.EqualTo(5));
        }

        [Test]
        public void AddNote_WhenCurrentUserExists_AddsNoteWithNextIdentifier()
        {
            var currentUser = CreateUser(7);
            currentUser.PeriodNotes = new Dictionary<int, Tuple<string, bool>>
            {
                { 3, new Tuple<string, bool>("Existing note", false) },
            };

            this.mockCurrentUserService
                .Setup(currentUserService => currentUserService.RaresCurrentUser)
                .Returns(currentUser);

            this.periodTrackerService.AddNote("New note");

            Assert.That(currentUser.PeriodNotes.ContainsKey(4), Is.True);
        }

        [Test]
        public void AddNote_WhenCurrentUserExists_SavesCurrentUser()
        {
            var currentUser = CreateUser(7);

            this.mockCurrentUserService
                .Setup(currentUserService => currentUserService.RaresCurrentUser)
                .Returns(currentUser);

            this.periodTrackerService.AddNote("New note");

            this.mockUsersRepository.Verify(
                usersRepository => usersRepository.UpdateUser(currentUser),
                Times.Once);
        }

        [Test]
        public void UpdateNote_WhenCurrentUserExists_UpdatesSelectedNote()
        {
            var currentUser = CreateUser(7);
            currentUser.PeriodNotes = new Dictionary<int, Tuple<string, bool>>
            {
                { 2, new Tuple<string, bool>("Old note", false) },
            };

            this.mockCurrentUserService
                .Setup(currentUserService => currentUserService.RaresCurrentUser)
                .Returns(currentUser);

            this.periodTrackerService.UpdateNote(2, "Updated note", true);

            Assert.That(currentUser.PeriodNotes[2].Item1, Is.EqualTo("Updated note"));
        }

        [Test]
        public void DeleteNote_WhenNoteExists_RemovesSelectedNote()
        {
            var currentUser = CreateUser(7);
            currentUser.PeriodNotes = new Dictionary<int, Tuple<string, bool>>
            {
                { 2, new Tuple<string, bool>("Note to delete", false) },
            };

            this.mockCurrentUserService
                .Setup(currentUserService => currentUserService.RaresCurrentUser)
                .Returns(currentUser);

            this.periodTrackerService.DeleteNote(2);

            Assert.That(currentUser.PeriodNotes.ContainsKey(2), Is.False);
        }

        [Test]
        public void UpdatePeriodTracker_WhenCurrentUserExists_SavesCurrentUser()
        {
            var currentUser = CreateUser(7);

            this.mockCurrentUserService
                .Setup(currentUserService => currentUserService.RaresCurrentUser)
                .Returns(currentUser);

            this.periodTrackerService.UpdatePeriodTracker(new DateTimeOffset(new DateTime(2025, 1, 10)), 30, 6, 2);

            this.mockUsersRepository.Verify(
                usersRepository => usersRepository.UpdateUser(currentUser),
                Times.Once);
        }

        private static User CreateUser(int userIdentifier)
        {
            return new User(userIdentifier, "user@test.com", "1234567890", "hashedPassword", false, false, "testUser", false, 0);
        }
    }
}