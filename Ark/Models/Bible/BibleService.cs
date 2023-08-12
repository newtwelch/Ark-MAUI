using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

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
            if(await _dbConnection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Bibles WHERE Abbreviation = '{_bible.Abbreviation}'") > 0)
            {
                return;
            }

            await _dbConnection.ExecuteAsync($"CREATE TABLE IF NOT EXISTS {_bible.Abbreviation} (ID INTEGER NOT NULL, Name TEXT NOT NULL, Text TEXT NOT NULL)");
            await _dbConnection.InsertAsync(_bible);

            await _dbConnection.RunInTransactionAsync(t =>
            {
                foreach (var book in _bible.Books)
                {
                    foreach (var chapter in book.Chapters)
                    {
                        foreach (var verse in chapter.Verses)
                        {
                            string id = book.nr.ToString().PadLeft(2, '0') + verse.chapter.ToString().PadLeft(3, '0') + verse.verse.ToString().PadLeft(3, '0');
                            t.Execute($"INSERT INTO {_bible.Abbreviation} (ID, Name, Text) VALUES (?, ?, ?)", id, verse.Name, verse.Text);
                        }
                    }
                }
                t.Commit();
            });
        }

        public async Task<List<Bible>> GetAllBiblesAsync() => await _dbConnection.GetAllWithChildrenAsync<Bible>();

        public async Task RemoveBibleAsync(string _bible)
        {
            await _dbConnection.ExecuteAsync($"DROP TABLE IF EXISTS {_bible}");
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