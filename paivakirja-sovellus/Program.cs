using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    // ========== MUUTTUJIA ==========
    static List<DiaryEntry> entries = new List<DiaryEntry>(); // Lista kaikista päiväkirjamerkinnöistä

    const int MerkintojaPerSivu = 10;  // Montako merkintää näytetään yhdellä sivulla
    static int nykyinenSivu = 1;       // Minkä sivun merkinnät näytetään tällä hetkellä

    // ========== FUNKTIOT ==========
    /* Sivujen määrän laskeminen */
    // Laskee kuinka monta sivua tarvitaan merkintöjen listaamiseen
    static int laskeSivujenMaara()
    {
        if (entries.Count == 0)
            return 1;

        return (int)Math.Ceiling(entries.Count / (double)MerkintojaPerSivu);
    }

    /* Päiväkirja lista */
    // Tulostaa nykyisen sivun merkinnät numeroituna, ja sivunavigointi jos sivuja on useampi.
    // naytaValintaohjeet lisää sivunavigoinnin yhteyteen myös n/e/x-ohjeet (käytetään muokkaus- ja poistovalikossa)
    static void paivakirjalista(bool naytaValintaohjeet = false)
    {
        Console.WriteLine("Päiväkirjan merkinnät:\n");

        // Jos listassa ei ole yhtään merkintää
        if (entries.Count == 0)
        {
            Console.WriteLine("\t(Ei merkintöjä vielä)\n");
            return; // Poistutaan funktiosta heti
        }

        int sivujenMaara = laskeSivujenMaara();

        // Varmistetaan että nykyinen sivu on yhä kelvollinen (esim. merkinnän poiston jälkeen)
        if (nykyinenSivu < 1) nykyinenSivu = 1;
        if (nykyinenSivu > sivujenMaara) nykyinenSivu = sivujenMaara;

        int alkuIndeksi = (nykyinenSivu - 1) * MerkintojaPerSivu;
        int loppuIndeksi = Math.Min(alkuIndeksi + MerkintojaPerSivu, entries.Count);

        // Käydään nykyisen sivun merkinnät läpi ja tulostetaan ne
        for (int i = alkuIndeksi; i < loppuIndeksi; i++)
        {
            var e = entries[i];
            Console.WriteLine($"\t{i + 1}. [{e.Date:dd.MM.yyyy HH:mm}] {e.Title}");
            Console.WriteLine($"\t   {e.Content}\n");
        }

        // Näytetään sivunavigointi vain jos sivuja on enemmän kuin yksi
        if (sivujenMaara > 1)
        {
            Console.WriteLine($"\tSivu {nykyinenSivu}/{sivujenMaara}");

            if (naytaValintaohjeet)
            {
                // Merkinnän valintanäkymässä sivua vaihdetaan n/e-komennoilla, ei suoraan numerolla
                Console.WriteLine("\tn - Seuraava sivu  e - Edellinen sivu  x - Peruuta\n");
            }
            else
            {
                Console.Write("\tSiirry sivulle (numero): ");

                for (int sivu = 1; sivu <= sivujenMaara; sivu++)
                {
                    Console.Write(sivu == nykyinenSivu ? $"[{sivu}] " : $"{sivu} ");
                }

                Console.WriteLine("\n");
            }
        }
        else if (naytaValintaohjeet)
        {
            // Vain yksi sivu - ei sivunavigointia, mutta peruutusmahdollisuus näytetään silti
            Console.WriteLine("\tx - Peruuta\n");
        }
    }

    /* Tekstin lisääminen */
    // Lisää uuden merkinnän tietokantaan
    static void lisaatekstia()
    {
        Console.WriteLine("Lisää uusi merkintä:\n");

        Console.Write("\tOtsikko: ");
        string title = Console.ReadLine() ?? "";

        Console.Write("\tSisältö: ");
        string content = Console.ReadLine() ?? "";

        // Tarkistetaan onko jompi kumpi kentistä täytetty
        if (!string.IsNullOrWhiteSpace(title) || !string.IsNullOrWhiteSpace(content))
        {
            // Luodaan uusi DiaryEntry-olio
            DiaryEntry uusi = new DiaryEntry
            {
                Date = DateTime.Now,           // Asetetaan nykyinen aika
                Title = title,
                Content = content
            };

            // Tallennetaan tietokantaan
            Database.AddEntry(uusi);

            // Päivitetään paikallinen lista
            entries = Database.LoadEntries();

            Console.Clear();
            Console.Write("\x1b[3J");
            paivakirjalista();
            Console.WriteLine("Merkintä lisätty!\n");
        }
        else // Molemmat kentät olivat tyhjiä
        {
            Console.WriteLine("\nMolemmat kentät olivat tyhjiä. Merkintää ei lisätty.\n");
        }

        Console.Write("Paina jotain nappia jatkaaksesi...");
        Console.ReadKey();
    }

    /* Merkinnän valitseminen sivuttamalla */
    // Näyttää sivutetun merkintälistan ja antaa käyttäjän selata sivuja ("n"/"e") tai
    // valita merkinnän numerolla. Palauttaa valitun merkinnän, tai null jos käyttäjä peruuttaa
    static DiaryEntry? valitseMerkinta(string toiminto)
    {
        while (true)
        {
            Console.Clear();
            Console.Write("\x1b[3J");

            paivakirjalista(true);

            // Jos listassa ei ole merkintöjä, ei voida valita mitään
            if (entries.Count == 0)
            {
                Console.Write("Paina jotain nappia jatkaaksesi...");
                Console.ReadKey();
                return null;
            }

            int sivujenMaara = laskeSivujenMaara();

            Console.WriteLine($"Minkä merkinnän haluat {toiminto}? (numero)\n");

            Console.Write("Valintasi: ");
            string syote = (Console.ReadLine() ?? "").Trim().ToLower();

            if (syote == "x") // Peruutetaan
                return null;

            if (syote == "n") // Seuraava sivu
            {
                if (nykyinenSivu < sivujenMaara) nykyinenSivu++;
                continue;
            }

            if (syote == "e") // Edellinen sivu
            {
                if (nykyinenSivu > 1) nykyinenSivu--;
                continue;
            }

            // Yritetään muuttaa käyttäjän syöte numeroksi
            if (int.TryParse(syote, out int numero))
            {
                int indeksi = numero - 1; // Lista alkaa nollasta, siksi -1

                // Tarkistetaan onko indeksi listan rajojen sisällä
                if (indeksi >= 0 && indeksi < entries.Count)
                {
                    return entries[indeksi];
                }

                Console.WriteLine("\nVirheellinen numero!\n");
            }
            else // Syöte ei ollut numero eikä tunnettu komento
            {
                Console.WriteLine("\nVirheellinen valinta!\n");
            }

            Console.Write("Paina jotain nappia jatkaaksesi...");
            Console.ReadKey();
        }
    }

    /* Tekstin muokkaaminen */
    // Muokkaa olemassa olevaa merkintää
    static void muokkaatekstia()
    {
        DiaryEntry? entry = valitseMerkinta("muokata");

        // Käyttäjä peruutti, tai listassa ei ollut merkintöjä
        if (entry == null)
            return;

        Console.Clear();
        Console.Write("\x1b[3J");

        Console.WriteLine($"Nykyinen merkintä:\n");
        Console.WriteLine($"\tPäivämäärä: {entry.Date:dd.MM.yyyy HH:mm}");
        Console.WriteLine($"\tOtsikko:    {entry.Title}");
        Console.WriteLine($"\tSisältö:    {entry.Content}\n");

        Console.Write("Uusi otsikko (jätä tyhjäksi jos et muuta): ");
        string uusiTitle = Console.ReadLine() ?? "";

        Console.Write("Uusi sisältö (jätä tyhjäksi jos et muuta): ");
        string uusiContent = Console.ReadLine() ?? "";

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

        // Tallennetaan muutos tietokantaan
        Database.UpdateEntry(entry);

        // Päivitetään paikallinen lista
        entries = Database.LoadEntries();

        Console.WriteLine("\nMerkintä muokattu!\n");

        Console.Write("Paina jotain nappia jatkaaksesi...");
        Console.ReadKey();
    }

    /* Tekstin poistaminen */
    // Poistaa merkinnän tietokannasta
    static void poistatekstia()
    {
        DiaryEntry? entry = valitseMerkinta("poistaa");

        // Käyttäjä peruutti, tai listassa ei ollut merkintöjä
        if (entry == null)
            return;

        Console.Clear();
        Console.Write("\x1b[3J");

        Console.WriteLine("Haluatko varmasti poistaa tämän merkinnän?\n");
        Console.WriteLine($"\t[{entry.Date:dd.MM.yyyy HH:mm}] {entry.Title}");
        Console.WriteLine($"\t{entry.Content}\n");
        Console.WriteLine("k - KYLLÄ / e - EI\n");

        // Kysytään vahvistus käyttäjältä
        switch ((Console.ReadLine() ?? "").Trim().ToLower())
        {
            case "k": // Käyttäjä valitsi kyllä
                // Poistetaan tietokannasta Id:n perusteella
                Database.DeleteEntry(entry.Id);

                // Päivitetään paikallinen lista
                entries = Database.LoadEntries();

                Console.WriteLine("\nPoistaminen onnistui!\n");
                break;

            case "e": // Käyttäjä valitsi ei
                Console.WriteLine("\nPoistaminen keskeytetty.\n");
                break;

            default: // Jokin muu syöte
                Console.WriteLine("\nVirheellinen valinta. Poistaminen keskeytetty.\n");
                break;
        }

        Console.Write("Paina jotain nappia jatkaaksesi...");
        Console.ReadKey();
    }

    /* Yhteysasetusten kysyminen */
    // Kysyy käyttäjältä tietokannan yhteysasetukset. Jos nykyinen-parametri annetaan,
    // sen arvoja käytetään oletuksina (tyhjäksi jätetty kenttä säilyttää vanhan arvon)
    static DbConfig kysyYhteysasetukset(DbConfig? nykyinen = null)
    {
        DbConfig config = new DbConfig();

        Console.WriteLine("(Jätä kenttä tyhjäksi käyttääksesi suluissa näkyvää oletusarvoa)\n");

        Console.Write($"\tPalvelin [{nykyinen?.Server ?? "localhost"}]: ");
        string server = Console.ReadLine() ?? "";
        config.Server = string.IsNullOrWhiteSpace(server) ? (nykyinen?.Server ?? "localhost") : server;

        Console.Write($"\tPortti [{nykyinen?.Port ?? 3306}]: ");
        string portSyote = Console.ReadLine() ?? "";
        config.Port = int.TryParse(portSyote, out int portti) ? portti : (nykyinen?.Port ?? 3306);

        Console.Write($"\tTietokanta [{nykyinen?.Database ?? "PaivakirjaDB"}]: ");
        string tietokanta = Console.ReadLine() ?? "";
        config.Database = string.IsNullOrWhiteSpace(tietokanta) ? (nykyinen?.Database ?? "PaivakirjaDB") : tietokanta;

        Console.Write($"\tKäyttäjätunnus [{nykyinen?.User ?? "root"}]: ");
        string kayttaja = Console.ReadLine() ?? "";
        config.User = string.IsNullOrWhiteSpace(kayttaja) ? (nykyinen?.User ?? "root") : kayttaja;

        Console.Write($"\tSalasana{(nykyinen != null ? " (jätä tyhjäksi säilyttääksesi nykyisen)" : "")}: ");
        string salasana = Console.ReadLine() ?? "";
        config.Password = (string.IsNullOrEmpty(salasana) && nykyinen != null) ? nykyinen.Password : salasana;

        return config;
    }

    /* Asetusten muokkaaminen valikosta */
    // Antaa käyttäjälle mahdollisuuden vaihtaa yhteysasetuksia ohjelman ollessa käynnissä
    static void muokkaaAsetuksia(ref DbConfig config)
    {
        Console.WriteLine("Tietokannan yhteysasetukset:\n");

        DbConfig uusi = kysyYhteysasetukset(config);

        // Kokeillaan uusia asetuksia ennen tallentamista
        Database.Configure(uusi);

        if (!Database.TestConnection())
        {
            // Palautetaan vanhat, toimivat asetukset käyttöön
            Database.Configure(config);
            Console.WriteLine("\nYhteys epäonnistui uusilla asetuksilla. Vanhat asetukset säilytetty.\n");
        }
        else
        {
            // Yhteys toimii - tarkistetaan vielä että tietokannan rakenne on oikea
            bool kelvollinen;

            try
            {
                kelvollinen = Database.HasValidSchema(uusi);
            }
            catch
            {
                kelvollinen = false;
            }

            if (!kelvollinen)
            {
                // Palautetaan vanhat, toimivat asetukset käyttöön
                Database.Configure(config);
                Console.WriteLine($"\nTietokanta '{uusi.Database}' ei vastaa Päiväkirjasovelluksen odottamaa rakennetta. Vanhat asetukset säilytetty.\n");
            }
            else
            {
                uusi.Save();
                config = uusi;
                Console.WriteLine("\nYhteysasetukset tallennettu ja yhteys toimii!\n");
            }
        }

        Console.Write("Paina jotain nappia jatkaaksesi...");
        Console.ReadKey();
    }

    /* Asetusvalikko */
    // Näyttää asetusten alivalikon: yhteysasetusten muokkaus tai niiden poistaminen kokonaan
    static void nayttaAsetusValikko(ref DbConfig config)
    {
        Console.WriteLine("Asetukset:\n");
        Console.WriteLine("\tm - Muuta yhteysasetuksia");
        Console.WriteLine("\td - Poista tallennetut asetukset\n");
        Console.WriteLine("\ttakaisin - Palaa päävalikkoon\n");

        Console.Write("Valintasi: ");
        string valinta = (Console.ReadLine() ?? "").Trim().ToLower();

        switch (valinta)
        {
            case "m":
                Console.Clear();
                Console.Write("\x1b[3J");
                muokkaaAsetuksia(ref config);
                break;

            case "d":
                Console.Clear();
                Console.Write("\x1b[3J");
                poistaAsetukset();
                break;
        }
        // Muilla valinnoilla palataan suoraan päävalikkoon
    }

    /* Asetusten poistaminen */
    // Poistaa koko asetuskansion (AppData\Roaming\PaivakirjaSovellus) ja sulkee ohjelman
    static void poistaAsetukset()
    {
        Console.WriteLine("Haluatko varmasti poistaa tallennetut yhteysasetukset?");
        Console.WriteLine("Tämä poistaa koko asetuskansion ja ohjelma suljetaan.\n");
        Console.WriteLine("k - KYLLÄ / e - EI\n");

        string valinta = (Console.ReadLine() ?? "").Trim().ToLower();

        if (valinta == "k")
        {
            DbConfig.DeleteConfigFolder();

            Console.WriteLine("\nAsetukset poistettu. Ohjelma suljetaan...\n");
            Console.Write("Paina jotain nappia lopettaaksesi...");
            Console.ReadKey();

            Environment.Exit(0);
        }
        else
        {
            Console.WriteLine("\nPoistaminen keskeytetty.\n");
            Console.Write("Paina jotain nappia jatkaaksesi...");
            Console.ReadKey();
        }
    }

    /* Liitytäänkö olemassa olevaan tietokantaan vai luodaanko uusi */
    // Kysyy ensimmäisellä käynnistyksellä haluaako käyttäjä käyttää valmiiksi olemassa olevaa
    // tietokantaa, vai luoda kokonaan uuden
    static bool kysyLuodaankoUusiTietokanta()
    {
        Console.WriteLine("Haluatko liittyä olemassa olevaan tietokantaan, vai luoda uuden?\n");
        Console.WriteLine("\tl - Liity olemassa olevaan tietokantaan");
        Console.WriteLine("\tu - Luo uusi tietokanta\n");

        while (true)
        {
            Console.Write("Valintasi: ");
            string valinta = (Console.ReadLine() ?? "").Trim().ToLower();

            if (valinta == "l") return false;
            if (valinta == "u") return true;

            Console.WriteLine("\nVirheellinen valinta. Syötä 'l' tai 'u'.\n");
        }
    }

    /* Uuden tietokannan luonti ja nimiristiriitojen käsittely */
    // Tarkistaa onko annetun nimen tietokanta jo palvelimella olemassa. Jos ei, luo sen
    // PaivakirjaDB.sql-skriptin pohjalta. Jos nimi on jo varattu, kysyy käyttäjältä
    // haluaako hän käyttää olemassa olevaa tietokantaa, vai nimetä uuden toisin
    static DbConfig varmistaTietokannanLuonti(DbConfig config)
    {
        while (true)
        {
            bool onOlemassa;

            try
            {
                onOlemassa = Database.DatabaseExists(config);
            }
            catch
            {
                Console.WriteLine("\nVirhe: Palvelimeen ei saatu yhteyttä annetuilla tunnuksilla.");
                Console.WriteLine("Anna yhteysasetukset uudelleen:\n");
                config = kysyYhteysasetukset(config);
                continue;
            }

            if (!onOlemassa)
            {
                // Nimi on vapaa - luodaan uusi tietokanta skriptin pohjalta
                string sqlPolku = Path.Combine(AppContext.BaseDirectory, "PaivakirjaDB.sql");
                Database.CreateDatabaseFromScript(config, sqlPolku);
                Console.WriteLine($"\nTietokanta '{config.Database}' luotu!\n");
                return config;
            }

            // Samannimine tietokanta löytyi palvelimelta - kysytään käyttäjältä mitä tehdään
            Console.WriteLine($"\nTietokanta '{config.Database}' on jo olemassa palvelimella.\n");
            Console.WriteLine("\tk - Käytä olemassa olevaa tietokantaa");
            Console.WriteLine("\tn - Nimeä uusi tietokanta toisin\n");

            Console.Write("Valintasi: ");
            string valinta = (Console.ReadLine() ?? "").Trim().ToLower();

            if (valinta == "k")
            {
                // Käytetään olemassa olevaa sellaisenaan, ei luoda/muokata mitään
                return config;
            }

            Console.Write("\nUusi tietokannan nimi: ");
            string uusiNimi = Console.ReadLine() ?? "";

            if (!string.IsNullOrWhiteSpace(uusiNimi))
                config.Database = uusiNimi;

            Console.WriteLine();
            // Silmukka jatkuu ja uusi nimi tarkistetaan
        }
    }

    // ========== MAIN ==========
    static void Main(string[] args)
    {
        // Ladataan tallennetut yhteysasetukset, tai kysytään ne käyttäjältä ensimmäisellä käynnistyksellä
        DbConfig? config = DbConfig.Load();

        if (config == null)
        {
            Console.WriteLine("\tPäiväkirjasovellus - Ensimmäinen käynnistys");
            Console.WriteLine("\t-----------------------------------------\n");

            bool luoUusi = kysyLuodaankoUusiTietokanta();

            Console.WriteLine();
            Console.WriteLine("Anna tietokannan yhteystiedot:\n");

            config = kysyYhteysasetukset();

            if (luoUusi)
            {
                config = varmistaTietokannanLuonti(config);
            }
            else
            {
                // Liitytään olemassa olevaan tietokantaan - varmistetaan ensin että sen
                // rakenne vastaa Päiväkirjasovelluksen odottamaa (PaivakirjaDB.sql)
                bool kelvollinen;

                try
                {
                    kelvollinen = Database.HasValidSchema(config);
                }
                catch
                {
                    Console.WriteLine($"\nVirhe: Tietokantaan '{config.Database}' ei saatu yhteyttä annetuilla tiedoilla.");
                    Console.WriteLine("Ohjelma suljetaan, asetuksia ei tallennettu.\n");
                    Console.Write("Paina jotain nappia lopettaaksesi...");
                    Console.ReadKey();
                    return;
                }

                if (!kelvollinen)
                {
                    Console.WriteLine($"\nVirhe: Tietokanta '{config.Database}' ei vastaa Päiväkirjasovelluksen odottamaa rakennetta.");
                    Console.WriteLine("Varmista että liityt oikeaan tietokantaan, tai luo uusi tietokanta sen sijaan.");
                    Console.WriteLine("\nOhjelma suljetaan, asetuksia ei tallennettu.\n");
                    Console.Write("Paina jotain nappia lopettaaksesi...");
                    Console.ReadKey();
                    return;
                }
            }

            config.Save();
            Console.WriteLine();
        }

        Database.Configure(config);

        // Testataan tietokantayhteys, ja kysytään asetukset uudelleen kunnes yhteys onnistuu
        while (!Database.TestConnection())
        {
            Console.WriteLine("Virhe: Tietokantaan ei saatu yhteyttä annetuilla asetuksilla!\n");
            Console.WriteLine("Anna yhteysasetukset uudelleen:\n");

            config = kysyYhteysasetukset(config);
            config.Save();
            Database.Configure(config);
            Console.WriteLine();
        }

        // Pääsilmukka - ohjelma pyörii kunnes käyttäjä valitsee exit
        while (true)
        {
            // Ladataan merkinnät tietokannasta
            entries = Database.LoadEntries();

            Console.Clear();
            Console.Write("\x1b[3J");

            // Ohjelman otsikko
            Console.WriteLine("\tPäiväkirjasovellus");
            Console.WriteLine("\t------------------\n");

            paivakirjalista();  // Näytetään nykyiset merkinnät

            // Käyttäjän valikko
            Console.WriteLine("Mitä haluat tehdä?\n");
            Console.WriteLine("\tl - Lisää merkintä");
            Console.WriteLine("\tm - Muokkaa merkintää");
            Console.WriteLine("\tp - Poista merkintä\n");
            Console.WriteLine("\tsettings - Yhteysasetukset");
            Console.WriteLine("\texit - Sulje ohjelma\n");

            // Luetaan käyttäjän valinta
            string valinta = (Console.ReadLine() ?? "").Trim().ToLower();

            // Jos syöte on numero ja osoittaa kelvolliseen sivuun, vaihdetaan sivua ja piirretään näkymä uudelleen
            if (int.TryParse(valinta, out int valittuSivu) && valittuSivu >= 1 && valittuSivu <= laskeSivujenMaara())
            {
                nykyinenSivu = valittuSivu;
                continue;
            }

            switch (valinta)
            {
                case "l": // Lisää merkintä
                    Console.Clear();
                    Console.Write("\x1b[3J");
                    lisaatekstia();
                    break;

                case "m": // Muokkaa merkintää
                    Console.Clear();
                    Console.Write("\x1b[3J");
                    muokkaatekstia();
                    break;

                case "p": // Poista merkintä
                    Console.Clear();
                    Console.Write("\x1b[3J");
                    poistatekstia();
                    break;

                case "settings": // Yhteysasetukset
                    Console.Clear();
                    Console.Write("\x1b[3J");
                    nayttaAsetusValikko(ref config);
                    break;

                case "exit": // Sulje ohjelma
                    return; // Poistutaan Main-funktiosta → ohjelma loppuu
            }
        }
    }
}