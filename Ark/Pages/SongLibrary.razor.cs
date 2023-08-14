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

        private Song songToDelete = new Song();
        private Song selectedSong = new Song();
        private Song songBackUp = new Song();
        private Lyric selectedLyric = new Lyric();
        private HotKeysContext HotKeysContext;
        private DeviceOrientationPartialClass deviceOrientation = new DeviceOrientationPartialClass();

        private bool showDeleteModal = false;
        private bool isQueueMode;
        private bool isEditMode;
        private bool isNotEditMode { get => !isEditMode; }
        private bool songDataHidden = true;
        private bool showPresentor = false;

        private string opacity = "opacity-0";
        private string songListOpacity = "opacity-100";
        private string searchText = "";
        public string selectedLyricText = "";

        protected override async Task OnInitializedAsync()
        {
            //Initial
            songs = await songService.GetAllSongsAsync();

            //Events
            appSharedState.BackButtonPressed += onBack;
            this.HotKeysContext = this.HotKeys.CreateContext().Add(Code.Escape, Escape);
            searchText = await sessionStorage.GetItemAsync<string>("searchText");

            //Session
            Song sessionSong = await sessionStorage.GetItemAsync<Song>("selectedSong");
            isEditMode = await sessionStorage.GetItemAsync<bool>("editMode");
            isQueueMode = await sessionStorage.GetItemAsync<bool>("queueMode");

            if (sessionSong is not null) selectedSong = sessionSong;
            if (!String.IsNullOrEmpty(searchText)) await searchSongs();
            if (selectedSong.ID != 0) songDataHidden = await sessionStorage.GetItemAsync<bool>("lyricHidden");
            
        }

        protected override void OnAfterRender(bool firstRender) { if (firstRender) opacity = "opacity-100"; }
        
        private async Task onSearch(KeyboardEventArgs e) => await searchSongs();

        private async Task searchSongs()
        {
            if (searchText is null) return;

            if (searchText == "")
                songs = await songService.GetAllSongsAsync();
            else if (searchText.StartsWith("."))
                songs = await songService.GetSongsFromLyricsAsync(searchText.Substring(1));
            else if (searchText.StartsWith("*"))
                songs = await songService.GetSongsFromAuthorsAsync(searchText.Substring(1));
            else if (searchText.StartsWith("#"))
                songs = await songService.GetSongsFromTagsAsync(searchText.Substring(1));
            else
                songs = await songService.GetSongsFromTitleAsync(searchText);
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
            songDataHidden = false;
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
                songDataHidden = true;
            }

            StateHasChanged();
        }

        public async void Dispose()
        {
            await sessionStorage.SetItemAsync<bool>("editMode", isEditMode);
            await sessionStorage.SetItemAsync<bool>("queueMode", isQueueMode);
            await sessionStorage.SetItemAsync<bool>("lyricHidden", songDataHidden);
            await sessionStorage.SetItemAsync<string>("searchText", searchText);
            await sessionStorage.SetItemAsync<Song>("selectedSong", selectedSong);
            this.HotKeysContext.Dispose();
            appSharedState.BackButtonPressed -= onBack;
        }


        private async Task addToQueue(Song song)
        {
            song.Title = song.Title.Replace("<span class=\"text-orange group-hover:text-white_light\">", "");
            song.Title = song.Title.Replace("</span>", "");
            song.Tags = song.Tags.Replace("<span class=\"text-orange group-hover:text-white_light\">", "");
            song.Tags = song.Tags.Replace("</span>", "");
            song.Author = song.Author.Replace("<span class=\"text-orange group-hover:text-white_light\">", "");
            song.Author = song.Author.Replace("</span>", "");
            song.RawLyrics = song.RawLyrics.Replace("<span class=\"text-orange group-hover:text-white_light\">", "");
            song.RawLyrics = song.RawLyrics.Replace("</span>", "");
            song.InQueue = true;
            await songService.UpdateSongAsync(song);
            await searchSongs();
            toastService.ShowToast($"{song.Title} was ADDED to the queue", "Song Queue", ToastLevel.Info);
        }
        private async Task removeFromQueue(Song song)
        {
            song.InQueue = false;
            await songService.UpdateSongAsync(song);
            toastService.ShowToast($"{song.Title} was REMOVED from the queue", "Song Queue", ToastLevel.Warning);
            if(isQueueMode) songs = (await songService.GetAllSongsAsync()).Where(s => s.InQueue == true).ToList();
        }
        private async Task ToggleQueueMode()
        {
            isQueueMode = !isQueueMode;
            searchText = "";
            if (isQueueMode)
            {
                songListOpacity = "opacity-0";
                songs = (await songService.GetAllSongsAsync()).Where(s => s.InQueue == true).ToList();
                songListOpacity = "opacity-100";
            }
            else
            {
                songListOpacity = "opacity-0";
                songs = (await songService.GetAllSongsAsync()).ToList();
                songListOpacity = "opacity-100";
            }

            StateHasChanged();
        }
        private async Task clearQueue()
        {
            foreach(var song in songs)
            {
                song.InQueue = false;
                await songService.UpdateSongAsync(song);
            }

            songs = (await songService.GetAllSongsAsync()).Where(s => s.InQueue == true).ToList();
        }

        // SONG SECTION
        //================================

        // ADD
        private async Task onSongAdd(bool isALanguage)
        {
            Song newSong = new Song();
            newSong.Title = "Title";
            newSong.Author = "Author";
            newSong.Sequence = "o";
            newSong.RawLyrics = "Sample verse";
            newSong.Language = "DEFAULT";
            newSong.Number = isALanguage ? selectedSong.Number : (await songService.GetAllSongsAsync()).Max(s => s.Number) + 1;

            if (isALanguage)
                toastService.ShowToast($"Added language for {selectedSong.Title}", "Language", ToastLevel.Success);
            else
                toastService.ShowToast($"Added a new song to the library", "New Song", ToastLevel.Success);

            if (settingsService.DeveloperMode())
                await songService.AddUpdateApiSongAsync(newSong);
            
            await songService.AddSongAsync(newSong);
            
            
            songs = await songService.GetAllSongsAsync();

            selectedSong = newSong;
            onSongSelect(selectedSong);
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
                    await songService.AddUpdateApiSongAsync(selectedSong);
                
                await songService.UpdateSongAsync(selectedSong);
            }
            StateHasChanged();

            if(String.IsNullOrWhiteSpace(searchText) && !discardChanges)
                songs = await songService.GetAllSongsAsync();
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
            toastService.ShowToast($"{songToDelete.Title} was removed from the library", "Delete", ToastLevel.Warning);

            if (settingsService.DeveloperMode())
                await songService.RemoveApiSongAsync(songToDelete);
            
            await songService.RemoveSongAsync(songToDelete);

            if (String.IsNullOrWhiteSpace(searchText))
                songs = isQueueMode ? (await songService.GetAllSongsAsync()).Where(s => s.InQueue == true).ToList() : await songService.GetAllSongsAsync();
            else
                await searchSongs();
            
            selectedSong = new Song();
            showDeleteModal = false;
        }
    }
}