namespace UBB_SE_2026_923_2.ViewModels.PeriodTracker
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using Syncfusion.UI.Xaml.Core;
    using Windows.UI.Text;

    public class NoteViewModel : INotifyPropertyChanged
    {
        private readonly Action<NoteViewModel> deleteNoteAction;
        private readonly Action<NoteViewModel> updateNoteAction;

        private readonly bool shouldSuppressPersistenceNotifications;

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public ICommand DeleteNoteCommand { get; }

        public int NoteId { get; }

        private string noteBody;

        public string NoteBody
        {
            get => this.noteBody;
            set
            {
                if (this.noteBody == value)
                {
                    return;
                }

                this.noteBody = value;
                this.OnPropertyChanged();
            }
        }

        private bool noteIsDone;

        public bool NoteIsDone
        {
            get => this.noteIsDone;
            set
            {
                if (this.noteIsDone == value)
                {
                    return;
                }

                this.noteIsDone = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.NoteBodyFontStyle));
            }
        }

        public FontStyle NoteBodyFontStyle => this.NoteIsDone ? FontStyle.Italic : FontStyle.Normal;

        public NoteViewModel(
            int noteId,
            string noteBody,
            bool noteIsDone,
            Action<NoteViewModel> deleteNoteAction,
            Action<NoteViewModel> updateNoteAction)
        {
            this.deleteNoteAction = deleteNoteAction;
            this.updateNoteAction = updateNoteAction;

            this.DeleteNoteCommand = new DelegateCommand(
                ignoredParameter => this.deleteNoteAction?.Invoke(this));

            this.NoteId = noteId;

            this.shouldSuppressPersistenceNotifications = true;
            this.NoteBody = noteBody;
            this.NoteIsDone = noteIsDone;
            this.shouldSuppressPersistenceNotifications = false;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            bool isPersistedProperty =
                propertyName == nameof(this.NoteBody) ||
                propertyName == nameof(this.NoteIsDone);

            if (!this.shouldSuppressPersistenceNotifications && isPersistedProperty)
            {
                this.updateNoteAction?.Invoke(this);
            }
        }
    }
}