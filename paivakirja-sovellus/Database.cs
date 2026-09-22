using System;
using System.Collections.Generic;
using MySqlConnector;

class Database
{
    // Yhteysmerkkijono XAMPP:n oletusasetuksilla
    // Server=localhost, Port=3306, Database=PaivakirjaDB, User=root
    private static string connectionString =
        "Server=localhost;Port=3306;Database=PaivakirjaDB;User=root;Password=;";

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