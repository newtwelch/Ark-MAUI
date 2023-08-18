using Microsoft.Maui.ApplicationModel.DataTransfer;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using static System.Reflection.Metadata.BlobBuilder;

namespace Ark.Models.Bible
{
    public class BibleService
    {
        private static SQLiteAsyncConnection _dbConnection;

        public BibleService(SettingsService _settingService)
        {
            _dbConnection = new SQLiteAsyncConnection(Constants.BibleDbPath, Constants.Flags);
            _dbConnection.CreateTableAsync<Bible>();
        }

        public async Task CreateBibleTableAsync(Bible _bible)
        {
            bool bibleExists = await _dbConnection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Bibles WHERE Abbreviation = '{_bible.Abbreviation}'") > 0;
            if (bibleExists)
                return;


            await _dbConnection.ExecuteAsync($"CREATE TABLE IF NOT EXISTS {_bible.Abbreviation} (ID INTEGER NOT NULL, Book INTEGER NOT NULL, Chapter INTEGER NOT NULL, Verse INTEGER NOT NULL, Text TEXT NOT NULL, PRIMARY KEY(ID AUTOINCREMENT))");
            await _dbConnection.ExecuteAsync($"CREATE TABLE IF NOT EXISTS {_bible.Abbreviation}Books (ID INTEGER NOT NULL, Name TEXT NOT NULL)");
            await _dbConnection.InsertAsync(_bible);

            await _dbConnection.RunInTransactionAsync(t =>
            {
                foreach (var book in _bible.Books)
                {
                    foreach (var chapter in book.Chapters)
                    {
                        foreach (var verse in chapter.Verses)
                        {
                            t.Execute($"INSERT INTO {_bible.Abbreviation} (Book, Chapter, Verse, Text) VALUES (?, ?, ?, ?)", book.nr, verse.chapter, verse.verse, verse.Text);
                        }
                    }
                }
                t.Commit();
            });
            await _dbConnection.RunInTransactionAsync(t =>
            {
                var books = new List<Book>();
                foreach (var book in _bible.Books)
                {
                    books.Add(book);
                    t.Execute($"INSERT INTO {_bible.Abbreviation}Books (ID, Name) VALUES (?, ?)", book.nr, book.Name);
                }
                t.Commit();
            });

        }

        public async Task<List<Book>> GetBooks(string _bible, bool useEnglish)
        {
            var books = new List<Book>();

            if (useEnglish)
                books = await _dbConnection.QueryAsync<Book>("SELECT * FROM akjvBooks");
            else
                books = await _dbConnection.QueryAsync<Book>($"SELECT * FROM {_bible}Books");

            return books;
        }

        public async Task<List<Book>> GetBooksWithQuery(string _bible, bool useEnglish, string searchTerm)
        {
            var books = new List<Book>();

            if (useEnglish)
                books = await _dbConnection.QueryAsync<Book>($"SELECT * FROM akjvBooks WHERE Name LIKE '%{searchTerm}%'");
            else
                books = await _dbConnection.QueryAsync<Book>($"SELECT * FROM {_bible}Books WHERE Name LIKE '%{searchTerm}%'");

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                foreach (var book in books)
                {
                    int startIndex = book.Name.ToLower().IndexOf(searchTerm.ToLower());
                    int endIndex = startIndex + searchTerm.Length;
                    string wrap = book.Name.Substring(startIndex, endIndex - startIndex);
                    wrap = book.Name.Replace(wrap, "<span class=\"text-orange group-hover:text-white_light\">" + wrap + "</span>");
                    book.Name = wrap;
                }
            }

            return books;
        }

        public async Task<List<Chapter>> GetChapters(Bible bible, int book)
        {
            var chapters = new List<Chapter>();

            chapters = await _dbConnection.QueryAsync<Chapter>($"SELECT DISTINCT Chapter FROM {bible.Abbreviation} WHERE Book = {book}");

            return chapters;
        }

        public async Task<List<ArkBible>> GetVerses(Bible bible, int book, int chapter)
        {
            var chapters = new List<ArkBible>();

            chapters = await _dbConnection.QueryAsync<ArkBible>($"SELECT * FROM {bible.Abbreviation} WHERE Book = {book} AND CHAPTER = {chapter}");

            return chapters;
        }

        public async Task<List<Bible>> GetAllBiblesAsync() => await _dbConnection.GetAllWithChildrenAsync<Bible>();

        public async Task RemoveBibleAsync(string _bible)
        {
            await _dbConnection.ExecuteAsync($"DROP TABLE IF EXISTS {_bible}");
            await _dbConnection.ExecuteAsync($"DROP TABLE IF EXISTS {_bible}Books");
            await _dbConnection.ExecuteAsync($"DELETE FROM Bibles WHERE Abbreviation = '{_bible}'");
            await _dbConnection.ExecuteAsync($"VACUUM");
        }

        public async Task<Bible> GetBibleFromAPI(string translation)
        {
            Bible bible = new Bible();
            HttpClient client = new HttpClient();
            string url = $"https://api.getbible.net/v2/{translation}.json";
            client.BaseAddress = new Uri(url);
            HttpResponseMessage responseMessage = await client.GetAsync("");

            if (responseMessage.IsSuccessStatusCode)
                bible = await responseMessage.Content.ReadFromJsonAsync<Bible>();

            return await Task.FromResult(bible);
        }
    }
}