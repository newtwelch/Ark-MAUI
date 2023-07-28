using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Ark;
using Ark.Shared;
using Ark.Models;
using Ark.Shared.Components;
using Ark.Models.Toast;
using System.Diagnostics;
using Blazored.SessionStorage;
using Toolbelt.Blazor.HotKeys2;

namespace Ark.Pages
{
    public partial class SongLibrary
    {
        private List<Song> songs = new List<Song>();
        private List<Song> songQueue = new List<Song>();
        private IQueryable<Song> songQuery;

        private Song songToDelete = new Song();
        private Song selectedSong = new Song();
        private Song songBackUp = new Song();
        private Lyric selectedLyric = new Lyric();
        private HotKeysContext HotKeysContext;
        private DeviceOrientationPartialClass deviceOrientation = new DeviceOrientationPartialClass();

        private bool showDeleteModal = false;
        private bool isEditMode;
        private bool isNotEditMode { get => !isEditMode; }
        private bool songLyricsHidden = true;
        private bool showPresentor = false;

        private string opacity = "opacity-0";
        private string searchText = "";
        public string selectedLyricText = "";

        protected override async Task OnInitializedAsync()
        {
            //Initial
            songs = await songService.GetAllSongs();
            songQuery = songs.AsQueryable();

            //Events
            appSharedState.BackButtonPressed += onBack;
            this.HotKeysContext = this.HotKeys.CreateContext().Add(Code.Escape, Escape);
            searchText = await sessionStorage.GetItemAsync<string>("searchText");

            //Session
            Song sessionSong = await sessionStorage.GetItemAsync<Song>("selectedSong");
            isEditMode = await sessionStorage.GetItemAsync<bool>("editMode");

            if (sessionSong is not null) selectedSong = sessionSong;
            if (!String.IsNullOrEmpty(searchText)) searchSongs();
            if (selectedSong.ID != 0) songLyricsHidden = await sessionStorage.GetItemAsync<bool>("lyricHidden");
            
            //TODO: session storage the raw lyrics as well
        }


        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                opacity = "opacity-100";
        }

        private void onSearch(KeyboardEventArgs e) => searchSongs();
        
        private void searchSongs()
        {
            if (searchText is null) return;

            if (searchText.StartsWith("."))
                songs = songQuery.AsParallel().AsOrdered().Where(x => x.RawLyrics.Contains(searchText.Substring(1), StringComparison.OrdinalIgnoreCase)).ToList();
            else if (String.IsNullOrEmpty(searchText))
                songs = songQuery.ToList();
            else
                songs = songQuery.AsParallel().AsOrdered().Where(x => x.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // SELECT
        private void onSongSelect(Song _selectedSong)
        {
            selectedSong = _selectedSong;
            selectedSong.Lyrics = songService.ParseLyrics(selectedSong.RawLyrics, selectedSong.Sequence);
            songBackUp = songService.newSong(_selectedSong);
            songLyricsHidden = false;
        }

        Window secondWindow = new Window() { Page = new DisplayPage()};
        private void onLyricSelect(Lyric _selectedLyric)
        {
            //deviceOrientationService.SetDeviceOrientation(DisplayOrientation.Landscape);
            deviceOrientation.SetDeviceOrientation(DisplayOrientation.Landscape);
#if ANDROID
            showPresentor = true;
#endif
#if WINDOWS
            if (selectedLyric != _selectedLyric && !Application.Current.Windows.Contains(secondWindow))
            {
                Application.Current.OpenWindow(secondWindow);
                //TODO: Don't lose focus
            }
#endif
            displayService.LyricToDisplay = _selectedLyric.Text;
            selectedLyric = _selectedLyric;
            selectedLyricText = selectedLyric.Text;
        }

        private void left()
        {
            Lyric previousLyric = selectedSong.Lyrics.Find(l => l.ID == selectedLyric.ID - 1);
            if (previousLyric != null)
            {
                selectedLyric = previousLyric;
                onLyricSelect(selectedLyric);
            }
        }
        private void right()
        {
            Lyric nextLyric = selectedSong.Lyrics.Find(l => l.ID == selectedLyric.ID + 1);
            if (nextLyric != null)
            {
                selectedLyric = nextLyric;
                onLyricSelect(selectedLyric);
            }
        }

        private void Escape() => onBack();

        private Lyric emptyLyric = new Lyric() { ID = 0 };
        private void onBack()
        {
#if WINDOWS
            if (Application.Current.Windows.Contains(secondWindow))
                Application.Current.CloseWindow(secondWindow);

#endif
            //TODO: QUIT App if already true
            if (showPresentor)
            {
                deviceOrientation.SetDeviceOrientation(DisplayOrientation.Portrait);
                showPresentor = false;
                selectedLyric = emptyLyric;
            }
            else if(isNotEditMode)
            {
                songLyricsHidden = true;
            }
            StateHasChanged();
        }

        public async void Dispose()
        {
            await sessionStorage.SetItemAsync<bool>("editMode", isEditMode);
            await sessionStorage.SetItemAsync<bool>("lyricHidden", songLyricsHidden);
            await sessionStorage.SetItemAsync<string>("searchText", searchText);
            await sessionStorage.SetItemAsync<Song>("selectedSong", selectedSong);
            this.HotKeysContext.Dispose();
            appSharedState.BackButtonPressed -= onBack;
        }


        // SONG SECTION
        //================================

        // ADD
        private async Task onSongAdd()
        {
            songs = await songService.GetAllSongs();
            Song newSong = new Song();
            newSong.Title = "Title";
            newSong.Author = "Author";
            newSong.Sequence = "o";
            newSong.RawLyrics = "Sample verse";
            newSong.Language = "DEFAULT";
            newSong.Number = songs.Max(s => s.Number) + 1;
            toastService.ShowToast($"Added a new song to the list", "SongLibrary", ToastLevel.Info);
            await songService.AddSongAsync(newSong);
            songs = await songService.GetAllSongs();
            songQuery = songs.AsQueryable();
        }

        // UPDATE
        private async Task ToggleEdit(bool discardChanges)
        {
            isEditMode = !isEditMode;

            if (!(isEditMode == false) || selectedSong.ID == 0)
                return;
            if (discardChanges)
                selectedSong = songService.newSong(songBackUp);
            else
                await songService.UpdateSongAsync(selectedSong);
            songs = await songService.GetAllSongs();
        }

        private void openDeleteModal(Song _songToDelete)
        {
            showDeleteModal = true;
            songToDelete = _songToDelete;
        }

        // DELETE
        private async Task onSongDelete()
        {
            toastService.ShowToast($"{songToDelete.Title} was deleted", "SongLibrary", ToastLevel.Info);
            await songService.RemoveSongAsync(songToDelete);
            songs = await songService.GetAllSongs();
            showDeleteModal = false;
        }

#if WINDOWS
        private Microsoft.UI.Windowing.AppWindow GetAppWindow(MauiWinUIWindow window)
        {
            var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
            return appWindow;
        }
#endif
    }
}