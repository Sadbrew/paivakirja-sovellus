using System;
using System.Collections.Generic;
using System.IO;

// Yksinkertainen päiväkirjamerkintä-luokka
class DiaryEntry
{
    public DateTime Date { get; set; }      // Merkinnän päivämäärä ja aika
    public string Title { get; set; }       // Merkinnän otsikko
    public string Content { get; set; }     // Merkinnän sisältö
}

class Program
{
    /* Globaaleja muuttujia */
    static List<DiaryEntry> entries = new List<DiaryEntry>(); // Lista kaikista päiväkirjamerkinnöistä

    // Tiedoston tallennuspolku (samaan kansioon kuin .exe)
    static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "teksteja.csv");

    /* ========== SAVE ========== */
    // Tallentaa kaikki merkinnät tiedostoon
    public static void SaveList()
    {
        List<string> lines = new List<string>();

        // Käydään kaikki merkinnät läpi yksi kerrallaan
        foreach (var entry in entries)
        {
            // Tallennusmuoto: Päivämäärä|Otsikko|Sisältö
            string line = $"{entry.Date:yyyy-MM-dd HH:mm}|{entry.Title}|{entry.Content}";
            lines.Add(line);
        }

        // Kirjoitetaan kaikki rivit tiedostoon
        File.WriteAllLines(filePath, lines);
    }

    /* ========== LOAD ========== */
    // Lataa merkinnät tiedostosta
    public static void LoadList()
    {
        entries.Clear(); // Tyhjennetään lista ennen uutta latausta

        // Tarkistetaan onko tiedosto olemassa
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            // Käydään jokainen rivi läpi
            foreach (string line in lines)
            {
                // Ohitetaan tyhjät rivit
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Jaetaan rivi osiin pystyviivalla
                string[] parts = line.Split('|');

                // Tarkistetaan että rivissä on tarpeeksi osia
                if (parts.Length >= 3)
                {
                    DiaryEntry entry = new DiaryEntry();

                    // Yritetään lukea päivämäärä
                    DateTime.TryParse(parts[0], out DateTime date);
                    entry.Date = date;
                    entry.Title = parts[1];
                    entry.Content = parts[2];

                    entries.Add(entry);
                }
            }
        }
        // Jos tiedostoa ei ole olemassa, lista jää tyhjäksi (ei tehdä mitään)
    }

    /* Päiväkirja lista */
    // Tulostaa kaikki merkinnät numeroituna
    static void paivakirjalista()
    {
        Console.WriteLine("Päiväkirjan merkinnät:\n");

        // Jos listassa ei ole yhtään merkintää
        if (entries.Count == 0)
        {
            Console.WriteLine("\t(Ei merkintöjä vielä)\n");
            return; // Poistutaan funktiosta heti
        }

        // Käydään kaikki merkinnät läpi ja tulostetaan ne
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            Console.WriteLine($"\t{i + 1}. [{e.Date:dd.MM.yyyy HH:mm}] {e.Title}");
            Console.WriteLine($"\t   {e.Content}\n");
        }
    }

    /* Tekstin lisääminen */
    // Lisää uuden merkinnän listaan
    static void lisaatekstia()
    {
        Console.WriteLine("Lisää uusi merkintä:\n");

        Console.Write("\tOtsikko: ");
        string title = Console.ReadLine();

        Console.Write("\tSisältö: ");
        string content = Console.ReadLine();

        // Tarkistetaan onko jompi kumpi kentistä täytetty
        if (!string.IsNullOrWhiteSpace(title) || !string.IsNullOrWhiteSpace(content))
        {
            // Luodaan uusi DiaryEntry-olio
            DiaryEntry uusi = new DiaryEntry
            {
                Date = DateTime.Now,           // Asetetaan nykyinen aika
                Title = title ?? "",
                Content = content ?? ""
            };

            entries.Add(uusi); // Lisätään uusi merkintä listaan

            Console.Clear();
            paivakirjalista();
            Console.WriteLine("Merkintä lisätty!\n");
        }
        else // Molemmat kentät olivat tyhjiä
        {
            Console.WriteLine("\nMolemmat kentät olivat tyhjiä. Merkintää ei lisätty.\n");
        }

        SaveList(); // Tallennetaan muutokset tiedostoon

        Console.Write("Paina jotain nappia jatkaaksesi...");
        Console.ReadKey();
    }

    /* Tekstin muokkaaminen */
    // Muokkaa olemassa olevaa merkintää
    static void muokkaatekstia()
    {
        paivakirjalista();

        // Jos listassa ei ole merkintöjä, ei voida muokata mitään
        if (entries.Count == 0)
        {
            Console.Write("Paina jotain nappia jatkaaksesi...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Mitä merkintää haluat muokata? (numero)\n");
        string syote = Console.ReadLine();

        // Yritetään muuttaa käyttäjän syöte numeroksi
        if (int.TryParse(syote, out int numero))
        {
            int indeksi = numero - 1; // Lista alkaa nollasta, siksi -1

            // Tarkistetaan onko indeksi listan rajojen sisällä
            if (indeksi >= 0 && indeksi < entries.Count)
            {
                var entry = entries[indeksi];

                Console.Clear();
                Console.WriteLine($"Nykyinen merkintä:\n");
                Console.WriteLine($"\tPäivämäärä: {entry.Date:dd.MM.yyyy HH:mm}");
                Console.WriteLine($"\tOtsikko:    {entry.Title}");
                Console.WriteLine($"\tSisältö:    {entry.Content}\n");

                Console.Write("Uusi otsikko (jätä tyhjäksi jos et muuta): ");
                string uusiTitle = Console.ReadLine();

                Console.Write("Uusi sisältö (jätä tyhjäksi jos et muuta): ");
                string uusiContent = Console.ReadLine();

                // Päivitetään otsikko vain jos käyttäjä kirjoitti jotain
                if (!string.IsNullOrWhiteSpace(uusiTitle))
                {
                    entry.Title = uusiTitle;
                }

                // Päivitetään sisältö vain jos käyttäjä kirjoitti jotain
                if (!string.IsNullOrWhiteSpace(uusiContent))
                {
                    entry.Content = uusiContent;
                }

                // Päivitetään myös muokkausaika
                entry.Date = DateTime.Now;

                Console.WriteLine("\nMerkintä muokattu!\n");
            }
            else // Numero ei ole listassa
            {
                Console.WriteLine("\nVirheellinen numero!\n");
            }
        }
        else // Syöte ei ollut numero
        {
            Console.WriteLine("\nSyötä numero!\n");
        }

        SaveList(); // Tallennetaan muutokset

        Console.Write("Paina jotain nappia jatkaaksesi...");
        Console.ReadKey();
    }

    /* Tekstin poistaminen */
    // Poistaa merkinnän listasta
    static void poistatekstia()
    {
        paivakirjalista();

        // Jos listassa ei ole merkintöjä, ei voida poistaa mitään
        if (entries.Count == 0)
        {
            Console.Write("Paina jotain nappia jatkaaksesi...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Minkä merkinnän haluat poistaa? (numero)\n");
        string syote = Console.ReadLine();

        // Yritetään muuttaa käyttäjän syöte numeroksi
        if (int.TryParse(syote, out int numero))
        {
            int indeksi = numero - 1; // Lista alkaa nollasta

            // Tarkistetaan onko indeksi listan rajojen sisällä
            if (indeksi >= 0 && indeksi < entries.Count)
            {
                var entry = entries[indeksi];

                Console.WriteLine("\nHaluatko varmasti poistaa tämän merkinnän?\n");
                Console.WriteLine($"\t{numero}. [{entry.Date:dd.MM.yyyy}] {entry.Title}");
                Console.WriteLine($"\t   {entry.Content}\n");
                Console.WriteLine("k - KYLLÄ / e - EI\n");

                // Kysytään vahvistus käyttäjältä
                switch (Console.ReadLine()?.ToLower())
                {
                    case "k": // Käyttäjä valitsi kyllä
                        entries.RemoveAt(indeksi);
                        Console.WriteLine("\nPoistaminen onnistui!\n");
                        break;

                    case "e": // Käyttäjä valitsi ei
                        Console.WriteLine("\nPoistaminen keskeytetty.\n");
                        break;

                    default: // Jokin muu syöte
                        Console.WriteLine("\nVirheellinen valinta. Poistaminen keskeytetty.\n");
                        break;
                }
            }
            else // Numero ei ole listassa
            {
                Console.WriteLine("\nVirheellinen numero!\n");
            }
        }
        else // Syöte ei ollut numero
        {
            Console.WriteLine("\nSyötä numero!\n");
        }

        SaveList(); // Tallennetaan muutokset

        Console.Write("Paina jotain nappia jatkaaksesi...");
        Console.ReadKey();
    }

    /* Main */
    static void Main(string[] args)
    {
        // Pääsilmukka - ohjelma pyörii kunnes käyttäjä valitsee exit
        while (true)
        {
            LoadList();         // Ladataan tiedot tiedostosta
            Console.Clear();

            // Ohjelman otsikko
            Console.WriteLine("\tPäiväkirjasovellus");
            Console.WriteLine("\t------------------\n");

            paivakirjalista();  // Näytetään nykyiset merkinnät

            // Käyttäjän valikko
            Console.WriteLine("Mitä haluat tehdä?\n");
            Console.WriteLine("\tl - Lisää merkintä");
            Console.WriteLine("\tm - Muokkaa merkintää");
            Console.WriteLine("\tp - Poista merkintä\n");
            Console.WriteLine("\texit - Sulje ohjelma\n");

            // Luetaan käyttäjän valinta
            switch (Console.ReadLine()?.ToLower())
            {
                case "l": // Lisää merkintä
                    Console.Clear();
                    lisaatekstia();
                    break;

                case "m": // Muokkaa merkintää
                    Console.Clear();
                    muokkaatekstia();
                    break;

                case "p": // Poista merkintä
                    Console.Clear();
                    poistatekstia();
                    break;

                case "exit": // Sulje ohjelma
                    return; // Poistutaan Main-funktiosta → ohjelma loppuu
            }
        }
    }
}