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
using Ark.Models.Songs;

namespace Ark.Pages
{
    public partial class SongLibrary
    {
        private List<Song> songs = new List<Song>();
        private List<Song> songQueue = new List<Song>();

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

            //Events
            appSharedState.BackButtonPressed += onBack;
            this.HotKeysContext = this.HotKeys.CreateContext().Add(Code.Escape, Escape);
            searchText = await sessionStorage.GetItemAsync<string>("searchText");

            //Session
            Song sessionSong = await sessionStorage.GetItemAsync<Song>("selectedSong");
            isEditMode = await sessionStorage.GetItemAsync<bool>("editMode");

            if (sessionSong is not null) selectedSong = sessionSong;
            if (!String.IsNullOrEmpty(searchText)) await searchSongs();
            if (selectedSong.ID != 0) songLyricsHidden = await sessionStorage.GetItemAsync<bool>("lyricHidden");
            
        }

        protected override void OnAfterRender(bool firstRender) { if (firstRender) opacity = "opacity-100"; }
        
        private async Task onSearch(KeyboardEventArgs e) => await searchSongs();

        private async Task searchSongs()
        {
            if (searchText is null) return;

            if (searchText == "")
                songs = await songService.GetAllSongs();
            else if (searchText.StartsWith("."))
                songs = await songService.GetSongsFromLyrics(searchText.Substring(1));
            else if (searchText.StartsWith("*"))
                songs = await songService.GetSongsFromAuthors(searchText.Substring(1));
            else if (searchText.StartsWith("#"))
                songs = await songService.GetSongsFromTags(searchText.Substring(1));
            else
                songs = await songService.GetSongsFromTitle(searchText);
        }

        // SELECT
        private void onSongSelect(Song _selectedSong)
        {
            selectedSong = _selectedSong;
            selectedSong.Lyrics = songService.ParseLyrics(selectedSong.RawLyrics, selectedSong.Sequence);
            selectedSong.Title = selectedSong.Title.Replace("<span class=\"text-orange group-hover:text-white_light\">", "");
            selectedSong.Title = selectedSong.Title.Replace("</span>", "");
            selectedSong.Tags = selectedSong.Tags.Replace("<span class=\"text-orange group-hover:text-white_light\">", "");
            selectedSong.Tags = selectedSong.Tags.Replace("</span>", "");
            selectedSong.Author = selectedSong.Author.Replace("<span class=\"text-orange group-hover:text-white_light\">", "");
            selectedSong.Author = selectedSong.Author.Replace("</span>", "");
            selectedSong.RawLyrics = selectedSong.RawLyrics.Replace("<span class=\"text-orange group-hover:text-white_light\">", "");
            selectedSong.RawLyrics = selectedSong.RawLyrics.Replace("</span>", "");
            songBackUp = songService.newSong(_selectedSong);
            songLyricsHidden = false;
            StateHasChanged();
        }

        private void onLyricSelect(Lyric _selectedLyric)
        {
            _selectedLyric.Text = _selectedLyric.Text.Replace("<span class=\"text-orange group-hover:text-white_light\">", "");
            _selectedLyric.Text = _selectedLyric.Text.Replace("</span>", "");

            deviceOrientation.SetDeviceOrientation(DisplayOrientation.Landscape);
            
#if ANDROID
            showPresentor = true;
#endif
#if WINDOWS
            if (selectedLyric != _selectedLyric && !Application.Current.Windows.Contains(songService.secondWindow))
            {
                songService.secondWindow.Page = new DisplayPage(settingsService);
                Application.Current.OpenWindow(songService.secondWindow);
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
            if (Application.Current.Windows.Contains(songService.secondWindow))
                Application.Current.CloseWindow(songService.secondWindow);

#endif
            //TODO: QUIT App if already true
            if (showPresentor)
            {
                deviceOrientation.SetDeviceOrientation(DisplayOrientation.Portrait);
                showPresentor = false;
                selectedLyric = emptyLyric;
            }
            else if (isNotEditMode)
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
        private async Task onSongAdd(bool isALanguage)
        {
            songs = await songService.GetAllSongs();

            Song newSong = new Song();
            newSong.Title = "Title";
            newSong.Author = "Author";
            newSong.Sequence = "o";
            newSong.RawLyrics = "Sample verse";
            newSong.Language = "DEFAULT";
            newSong.Number = isALanguage ? selectedSong.Number : (await songService.GetAllSongs()).Max(s => s.Number) + 1;

            if (isALanguage)
                toastService.ShowToast($"Added language for {selectedSong.Title}", "Language", ToastLevel.Success);
            else
                toastService.ShowToast($"Added a new song to the library", "New Song", ToastLevel.Success);

            if (settingsService.DeveloperMode())
                await songService.AddUpdateSong(newSong);
            
            await songService.AddSongAsync(newSong);
            

            songs = await songService.GetAllSongs();

            selectedSong = newSong;
            await ToggleEdit(false);
        }

        // UPDATE
        private async Task ToggleEdit(bool discardChanges)
        {
            isEditMode = !isEditMode;

            if (selectedSong.ID == 0 || selectedSong is null)
            {
                toastService.ShowToast($"No song selected", "[!] Error [!]", ToastLevel.Error);
                isEditMode = false;
                return;
            }

            if (discardChanges)
                selectedSong = songService.newSong(songBackUp);
            else
            {
                if (settingsService.DeveloperMode())
                    await songService.AddUpdateSong(selectedSong);
                
                await songService.UpdateSongAsync(selectedSong);
            }
            StateHasChanged();

            if(String.IsNullOrWhiteSpace(searchText) && !discardChanges)
                songs = await songService.GetAllSongs();
            else
                await searchSongs();
        }

        private void openDeleteModal(Song _songToDelete)
        {
            showDeleteModal = true;
            _songToDelete.Title = _songToDelete.Title.Replace("<span class=\"text-orange group-hover:text-white_light\">", "");
            _songToDelete.Title = _songToDelete.Title.Replace("</span>", "");
            songToDelete = _songToDelete;
        }

        // DELETE
        private async Task onSongDelete()
        {
            toastService.ShowToast($"{songToDelete.Title} was removed from the library", "Delete", ToastLevel.Info);

            if (settingsService.DeveloperMode())
                await songService.DeleteSong(songToDelete);
            
            await songService.RemoveSongAsync(songToDelete);

            if (String.IsNullOrWhiteSpace(searchText))
                songs = await songService.GetAllSongs();
            else
                await searchSongs();

            selectedSong = new Song();
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