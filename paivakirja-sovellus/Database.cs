using System;
using System.Collections.Generic;
using System.IO;
using MySqlConnector;

class Database
{
    // Yhteysmerkkijono asetetaan DbConfig:n perusteella Configure()-kutsulla
    private static string connectionString = "";

    // ========== ASETA YHTEYSASETUKSET ==========
    // Päivittää käytettävän yhteysmerkkijonon annetun konfiguraation perusteella
    public static void Configure(DbConfig config)
    {
        connectionString = config.ToConnectionString();
    }

    // ========== HAE KAIKKI MERKINNÄT ==========
    // Hakee kaikki päiväkirjamerkinnät tietokannasta ja palauttaa ne listana
    public static List<DiaryEntry> LoadEntries()
    {
        List<DiaryEntry> entries = new List<DiaryEntry>();

        // Avataan yhteys tietokantaan
        using var yhteys = new MySqlConnection(connectionString);
        yhteys.Open();

        // SQL-kysely: haetaan kaikki sarakkeet ja järjestetään uusin ensin
        string sql = "SELECT Id, EntryDate, Title, Content FROM DiaryEntries ORDER BY EntryDate DESC";

        using var komento = new MySqlCommand(sql, yhteys);
        using var lukija = komento.ExecuteReader();

        // Käydään tulokset rivi riviltä läpi
        while (lukija.Read())
        {
            DiaryEntry entry = new DiaryEntry
            {
                Id = lukija.GetInt32("Id"),
                Date = lukija.GetDateTime("EntryDate"),
                Title = lukija.GetString("Title"),
                // Content voi olla NULL tietokannassa, siksi tarkistus
                Content = lukija.IsDBNull(lukija.GetOrdinal("Content"))
                            ? ""
                            : lukija.GetString("Content")
            };

            entries.Add(entry);
        }

        return entries;
    }

    // ========== LISÄÄ UUSI MERKINTÄ ==========
    // Lisää uuden merkinnän tietokantaan ja asettaa sille Id:n
    public static void AddEntry(DiaryEntry entry)
    {
        using var yhteys = new MySqlConnection(connectionString);
        yhteys.Open();

        // INSERT + haetaan heti uusi auto-increment Id
        string sql = @"INSERT INTO DiaryEntries (EntryDate, Title, Content) 
                       VALUES (@date, @title, @content);
                       SELECT LAST_INSERT_ID();";

        using var komento = new MySqlCommand(sql, yhteys);

        // Parametrit estävät SQL-injektion ja tekevät koodista turvallisempaa
        komento.Parameters.AddWithValue("@date", entry.Date);
        komento.Parameters.AddWithValue("@title", entry.Title);
        komento.Parameters.AddWithValue("@content", entry.Content);

        // ExecuteScalar palauttaa FIRST_INSERT_ID():n arvon
        entry.Id = Convert.ToInt32(komento.ExecuteScalar());
    }

    // ========== PÄIVITÄ MERKINTÄ ==========
    // Päivittää olemassa olevan merkinnän Id:n perusteella
    public static void UpdateEntry(DiaryEntry entry)
    {
        using var yhteys = new MySqlConnection(connectionString);
        yhteys.Open();

        string sql = @"UPDATE DiaryEntries 
                       SET EntryDate = @date, Title = @title, Content = @content 
                       WHERE Id = @id";

        using var komento = new MySqlCommand(sql, yhteys);

        komento.Parameters.AddWithValue("@id", entry.Id);
        komento.Parameters.AddWithValue("@date", entry.Date);
        komento.Parameters.AddWithValue("@title", entry.Title);
        komento.Parameters.AddWithValue("@content", entry.Content);

        // Suoritetaan päivitys
        komento.ExecuteNonQuery();
    }

    // ========== POISTA MERKINTÄ ==========
    // Poistaa merkinnän Id:n perusteella
    public static void DeleteEntry(int id)
    {
        using var yhteys = new MySqlConnection(connectionString);
        yhteys.Open();

        string sql = "DELETE FROM DiaryEntries WHERE Id = @id";

        using var komento = new MySqlCommand(sql, yhteys);
        komento.Parameters.AddWithValue("@id", id);

        // Suoritetaan poisto
        komento.ExecuteNonQuery();
    }

    // ========== TARKISTA ONKO TIETOKANTA OLEMASSA ==========
    // Tarkistaa löytyykö palvelimelta jo tietokanta annetulla nimellä.
    // Yhteys avataan ilman Database-parametria, jotta tarkistus onnistuu vaikka kantaa ei ole vielä valittu
    public static bool DatabaseExists(DbConfig config)
    {
        string palvelinYhteys = $"Server={config.Server};Port={config.Port};User={config.User};Password={config.Password};";

        using var yhteys = new MySqlConnection(palvelinYhteys);
        yhteys.Open();

        string sql = "SHOW DATABASES LIKE @nimi";

        using var komento = new MySqlCommand(sql, yhteys);
        komento.Parameters.AddWithValue("@nimi", config.Database);

        using var lukija = komento.ExecuteReader();
        return lukija.HasRows;
    }

    // ========== LUO TIETOKANTA SKRIPTISTÄ ==========
    // Luo uuden tietokannan ja sen taulut annetun SQL-skriptitiedoston pohjalta.
    // Skriptissä kovakoodattu tietokannan nimi korvataan config.Database-arvolla
    public static void CreateDatabaseFromScript(DbConfig config, string sqlScriptPath)
    {
        string skripti = File.ReadAllText(sqlScriptPath);
        skripti = skripti.Replace("PaivakirjaDB", config.Database);

        string palvelinYhteys = $"Server={config.Server};Port={config.Port};User={config.User};Password={config.Password};";

        using var yhteys = new MySqlConnection(palvelinYhteys);
        yhteys.Open();

        // Skripti jaetaan yksittäisiin lauseisiin puolipisteen kohdalta ja suoritetaan vuorotellen
        foreach (string lause in skripti.Split(';'))
        {
            string siistitty = lause.Trim();
            if (string.IsNullOrWhiteSpace(siistitty))
                continue;

            using var komento = new MySqlCommand(siistitty, yhteys);
            komento.ExecuteNonQuery();
        }
    }

    // ========== TARKISTA TIETOKANNAN RAKENNE ==========
    // Tarkistaa vastaako olemassa olevan tietokannan DiaryEntries-taulu PaivakirjaDB.sql:n rakennetta.
    // Palauttaa false jos taulua ei löydy, tai jos vaaditut sarakkeet/tietotyypit puuttuvat
    public static bool HasValidSchema(DbConfig config)
    {
        using var yhteys = new MySqlConnection(config.ToConnectionString());
        yhteys.Open();

        string sql = @"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
                       WHERE TABLE_SCHEMA = @skeema AND TABLE_NAME = 'DiaryEntries'";

        using var komento = new MySqlCommand(sql, yhteys);
        komento.Parameters.AddWithValue("@skeema", config.Database);

        var sarakkeet = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        using var lukija = komento.ExecuteReader();
        while (lukija.Read())
        {
            sarakkeet[lukija.GetString("COLUMN_NAME")] = lukija.GetString("DATA_TYPE");
        }

        // Taulua DiaryEntries ei löytynyt ollenkaan
        if (sarakkeet.Count == 0)
            return false;

        // PaivakirjaDB.sql:n mukaiset vaaditut sarakkeet ja niiden tietotyypit
        var vaaditut = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Id", "int" },
            { "EntryDate", "datetime" },
            { "Title", "varchar" },
            { "Content", "text" }
        };

        foreach (var vaadittu in vaaditut)
        {
            if (!sarakkeet.TryGetValue(vaadittu.Key, out string? tyyppi))
                return false;

            if (!tyyppi.Equals(vaadittu.Value, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    // ========== TESTIYHTEYS ==========
    // Testaa onko yhteys tietokantaan onnistunut (palauttaa true/false)
    public static bool TestConnection()
    {
        try
        {
            using var yhteys = new MySqlConnection(connectionString);
            yhteys.Open();
            return true;   // Yhteys onnistui
        }
        catch
        {
            return false;  // Yhteys epäonnistui
        }
    }
}