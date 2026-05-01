using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Core;
using Windows.UI.Text;

namespace UBB_SE_2026_923_2.ViewModels.PeriodTracker
{
    public class NoteViewModel : INotifyPropertyChanged
    {
        private readonly Action<NoteViewModel> deleteNoteAction;
        private readonly Action<NoteViewModel> updateNoteAction;

        private bool shouldSuppressPersistenceNotifications;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public ICommand DeleteNoteCommand { get; }

        public int NoteId { get; }

        private string noteBody;
        public string NoteBody
        {
            get => noteBody;
            set
            {
                if (noteBody == value)
                {
                    return;
                }

                noteBody = value;
                OnPropertyChanged();
            }
        }

        private bool noteIsDone;
        public bool NoteIsDone
        {
            get => noteIsDone;
            set
            {
                if (noteIsDone == value)
                {
                    return;
                }

                noteIsDone = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NoteBodyFontStyle));
            }
        }

        public FontStyle NoteBodyFontStyle => NoteIsDone ? FontStyle.Italic : FontStyle.Normal;

        public NoteViewModel(
            int noteId,
            string noteBody,
            bool noteIsDone,
            Action<NoteViewModel> deleteNoteAction,
            Action<NoteViewModel> updateNoteAction)
        {
            this.deleteNoteAction = deleteNoteAction;
            this.updateNoteAction = updateNoteAction;

            DeleteNoteCommand = new DelegateCommand(
                ignoredParameter => this.deleteNoteAction?.Invoke(this));

            NoteId = noteId;

            shouldSuppressPersistenceNotifications = true;
            NoteBody = noteBody;
            NoteIsDone = noteIsDone;
            shouldSuppressPersistenceNotifications = false;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            bool isPersistedProperty =
                propertyName == nameof(NoteBody) ||
                propertyName == nameof(NoteIsDone);

            if (!shouldSuppressPersistenceNotifications && isPersistedProperty)
            {
                updateNoteAction?.Invoke(this);
            }
        }
    }
}