using SQLite;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace Ark.Models.Songs
{
    public class SongService
    {
        private static SQLiteAsyncConnection _dbConnection;

        public SongService()
        {
            _dbConnection = new SQLiteAsyncConnection(Constants.SongDbPath, Constants.Flags);
            _dbConnection.CreateTableAsync<Song>();
            _dbConnection.ExecuteAsync("CREATE VIRTUAL TABLE SongFts USING Fts5 (ID, Number, Title, Author, RawLyrics, Language, Sequence, Tags)");
        }


        public async Task<List<Song>> GetAllSongs() => await _dbConnection.Table<Song>().ToListAsync();
        public async Task AddSongAsync(Song song) 
        {
            await _dbConnection.InsertAsync(song);
        }
        public async Task UpdateSongAsync(Song song)
        {
            await _dbConnection.UpdateAsync(song);
        }
        public async Task RemoveSongAsync(Song song)
        {
            await _dbConnection.DeleteAsync(song);
        }

        public async Task<List<Song>> GetSongsFromTitle(string searchTerm)
        {
            var songsFts = new List<SongFts>();
            var songs = new List<Song>();
            searchTerm = searchTerm.Trim().Replace("'", " ").Replace(" ", "* ");
            songsFts = await _dbConnection.QueryAsync<SongFts>($"SELECT ID, Number, Language, highlight(SongFts, 2, '<span class=\"text-orange group-hover:text-white_light\">', '</span>') AS Title, Author, RawLyrics, Sequence, Tags FROM SongFts WHERE Title MATCH '\"{searchTerm}\"*' ORDER BY rank");

            songs.AddRange(songsFts);
            return songs;
        }

        public async Task<List<Song>> GetSongsFromLyrics(string searchTerm)
        {
            var songsFts = new List<SongFts>();
            var songs = new List<Song>();
            searchTerm = searchTerm.Trim().Replace("'", " ").Replace(" ", "* ");
            songsFts = await _dbConnection.QueryAsync<SongFts>($"SELECT ID, Number, Language, Title, Author, highlight(SongFts, 4, '<span class=\"text-orange group-hover:text-white_light\">', '</span>') AS RawLyrics, Sequence, Tags FROM SongFts WHERE RawLyrics MATCH '\"{searchTerm}\"*' ORDER BY rank");

            songs.AddRange(songsFts);
            return songs;
        }

        public async Task<List<Song>> GetSongsFromAPI()
        {
            List<Song> songs = new List<Song>();
            HttpClient client = new HttpClient();
            string url = Constants.webAPI + "/getSongs";
            client.BaseAddress = new Uri(url);
            HttpResponseMessage responseMessage = await client.GetAsync("");

            if (responseMessage.IsSuccessStatusCode)
                songs = await responseMessage.Content.ReadFromJsonAsync<List<Song>>();

            return await Task.FromResult(songs);
        }


        public List<Lyric> ParseLyrics(string rawlyric, string sequence)
        {
            if (rawlyric is null) return null;
            List<Lyric> Lyrics = new List<Lyric>();
            List<Lyric> SequencedLyrics = new List<Lyric>();
            sequence?.TrimEnd(' ', ',');
            int verseid = 1;
            int verseBeforeChorus = 0;
            int verseBeforeChorusCounter = 0;

            string[] paragraphs = Array.FindAll(Regex.Split(rawlyric, "(\r?\n){2,}", RegexOptions.Multiline), p => !string.IsNullOrWhiteSpace(p));
            foreach (string p in paragraphs)
            {
                if (p.StartsWith("CHORUS", StringComparison.OrdinalIgnoreCase) && Lyrics.Any(x => x.Type == LyricType.Chorus))
                    continue;

                Lyrics.Add(new Lyric()
                {
                    Line = p.StartsWith("CHORUS", StringComparison.OrdinalIgnoreCase) ? "C" :
                           p.StartsWith("BRIDGE", StringComparison.OrdinalIgnoreCase) ? "B" : verseid++.ToString(),

                    Text = p.StartsWith("CHORUS", StringComparison.OrdinalIgnoreCase) ? Regex.Replace(p, "^(.*\n){1}", "") :
                           p.StartsWith("BRIDGE", StringComparison.OrdinalIgnoreCase) ? Regex.Replace(p, "^(.*\n){1}", "") : p,

                    Type = p.StartsWith("CHORUS", StringComparison.OrdinalIgnoreCase) ? LyricType.Chorus :
                           p.StartsWith("BRIDGE", StringComparison.OrdinalIgnoreCase) ? LyricType.Bridge : LyricType.Stanza,
                });

            }

            if (sequence == "o" || string.IsNullOrWhiteSpace(sequence))
            {
                Lyric chorus = Lyrics.Find(x => x.Type == LyricType.Chorus);
                int lyricID = 1;

                foreach (Lyric lyric in Lyrics)
                {
                    bool isStanza = lyric.Type.Equals(LyricType.Stanza);
                    bool isChorus = lyric.Type.Equals(LyricType.Chorus);
                    bool isBridge = lyric.Type.Equals(LyricType.Bridge);

                    if (!SequencedLyrics.Any(x => x.Type.Equals(LyricType.Chorus)))
                    {
                        if (isStanza) verseBeforeChorus++;
                        lyric.ID = lyricID++;
                        SequencedLyrics.Add(newLyric(lyric));
                        continue;
                    }

                    if (verseBeforeChorus.Equals(0))
                        verseBeforeChorus = 1;

                    if (isStanza)
                    {
                        lyric.ID = lyricID++;
                        SequencedLyrics.Add(newLyric(lyric));
                        verseBeforeChorusCounter++;
                    }

                    if (isBridge)
                    {
                        lyric.ID = lyricID++;
                        SequencedLyrics.Add(newLyric(lyric));
                    }

                    if (verseBeforeChorusCounter == verseBeforeChorus)
                    {
                        chorus.ID = lyricID++;
                        SequencedLyrics.Add(newLyric(chorus));
                        verseBeforeChorusCounter = 0;
                    }
                }
            }
            else
            {
                string[] sequencer = sequence.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in sequencer)
                {
                    int lyricID = 0;
                    Lyric lyric = Lyrics.Find(x => x.Line.ToUpper() == line.ToUpper().Replace("S", ""));
                    if (lyric is not null)
                    {
                        lyric.ID = lyricID++;
                        SequencedLyrics.Add(newLyric(lyric));
                    }
                }
            }

            return SequencedLyrics;
        }

        public Lyric newLyric(Lyric lyricToCopy)
        {
            Lyric lyric = new Lyric();

            lyric.ID = lyricToCopy.ID;
            lyric.Text = lyricToCopy.Text;
            lyric.Line = lyricToCopy.Line;
            lyric.Type = lyricToCopy.Type;

            return lyric;
        }

        public Song newSong(Song songToCopy)
        {
            Song song = new Song();
            song.ID = songToCopy.ID;
            song.Title = songToCopy.Title;
            song.Author = songToCopy.Author;
            song.Language = songToCopy.Language;
            song.Tags = songToCopy.Tags;
            song.Number = songToCopy.Number;
            song.RawLyrics = songToCopy.RawLyrics;
            song.Sequence = songToCopy.Sequence;
            song.Lyrics = ParseLyrics(song.RawLyrics, song.Sequence);

            return song;
        }

        public async Task SyncFromWebAPI()
        {
            await _dbConnection.DropTableAsync<Song>();
            await _dbConnection.CreateTableAsync<Song>();
            await _dbConnection.DropTableAsync<SongFts>();
            await _dbConnection.ExecuteAsync("CREATE VIRTUAL TABLE SongFts USING Fts5(ID, Number, Title, Author, RawLyrics, Language, Sequence, Tags)");

            await _dbConnection.InsertAllAsync(await GetSongsFromAPI());
            await _dbConnection.ExecuteAsync("INSERT INTO SongFts(ID, Number, Title, Author, RawLyrics, Language, Sequence, Tags) SELECT ID, Number, Title, Author, RawLyrics, Language, Sequence, Tags FROM Song");

        }
    }
}
