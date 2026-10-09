using System;
using System.IO;
using System.Text.Json;

// Tietokannan yhteysasetukset, jotka tallennetaan käyttäjäkohtaiseen asetustiedostoon
class DbConfig
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string Database { get; set; } = "PaivakirjaDB";
    public string User { get; set; } = "root";
    public string Password { get; set; } = "";

    // Asetustiedosto tallennetaan käyttäjän AppData-kansioon, jotta se säilyy
    // vaikka projekti käännetään uudelleen (bin/obj-kansiot eivät säily)
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PaivakirjaSovellus");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "dbconfig.json");

    // Lataa asetukset tiedostosta, tai palauttaa null jos tiedostoa ei ole / se on virheellinen
    public static DbConfig? Load()
    {
        if (!File.Exists(ConfigPath))
            return null;

        try
        {
            string json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<DbConfig>(json);
        }
        catch
        {
            return null;
        }
    }

    // Tallentaa asetukset tiedostoon
    public void Save()
    {
        Directory.CreateDirectory(ConfigDir);
        string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
    }

    // Rakentaa MySqlConnector-yhteysmerkkijonon näistä asetuksista
    public string ToConnectionString()
    {
        return $"Server={Server};Port={Port};Database={Database};User={User};Password={Password};";
    }

    // Poistaa koko asetuskansion (AppData\Roaming\PaivakirjaSovellus) tallennettuine tiedostoineen
    public static void DeleteConfigFolder()
    {
        if (Directory.Exists(ConfigDir))
            Directory.Delete(ConfigDir, true);
    }
}
