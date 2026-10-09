using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class Program
{
    // ========== MUUTTUJIA ==========
    static List<DiaryEntry> entries = new List<DiaryEntry>();        // Kaikki tietokannasta ladatut merkinnät
    static List<DiaryEntry> nakyvatMerkinnat = new List<DiaryEntry>(); // Näytettävät merkinnät (suodatettu tai kaikki)

    const int MerkintojaPerSivu = 10;  // Montako merkintää näytetään yhdellä sivulla
    static int nykyinenSivu = 1;       // Minkä sivun merkinnät näytetään tällä hetkellä

    // Haku/suodatin-asetukset. Null/tyhjä tarkoittaa ettei kyseistä suodatinta käytetä
    static string? hakuSana = null;
    static DateTime? suodatinAlkaen = null;
    static DateTime? suodatinPaattyen = null;

    static bool SuodatinAktiivinen => hakuSana != null || suodatinAlkaen.HasValue || suodatinPaattyen.HasValue;

    // ========== VÄRIAPUFUNKTIOT ==========
    // Kirjoittaa tekstin halutulla värillä ja palauttaa konsolin värin oletukseen heti perään,
    // jotta väri ei vuoda seuraavaan tulostukseen
    static void KirjoitaVarilla(string teksti, ConsoleColor vari, bool uusiRivi = true)
    {
        Console.ForegroundColor = vari;
        if (uusiRivi) Console.WriteLine(teksti); else Console.Write(teksti);
        Console.ResetColor();
    }

    // Otsikot ja näkymien pääotsikot
    static void Otsikko(string teksti, bool uusiRivi = true) => KirjoitaVarilla(teksti, ConsoleColor.Cyan, uusiRivi);

    // Onnistuneet toiminnot (lisäys, muokkaus, poisto, tallennus, yhteys toimii...)
    static void Onnistui(string teksti, bool uusiRivi = true) => KirjoitaVarilla(teksti, ConsoleColor.Green, uusiRivi);

    // Virheet ja epäonnistumiset
    static void Virhe(string teksti, bool uusiRivi = true) => KirjoitaVarilla(teksti, ConsoleColor.Red, uusiRivi);

    // Varoitukset ja huomiot (ei suoranainen virhe, mutta vaatii huomiota - esim. vahvistuskysymykset)
    static void Varoitus(string teksti, bool uusiRivi = true) => KirjoitaVarilla(teksti, ConsoleColor.Yellow, uusiRivi);

    // Ohjeet ja muu toissijainen apuväriteksti (ei itsessään näppäiltäviä komentoja)
    static void Vihje(string teksti, bool uusiRivi = true) => KirjoitaVarilla(teksti, ConsoleColor.DarkGray, uusiRivi);

    // Valikkokomennot ja ohjaimet (esim. "l - Lisää merkintä", "k - KYLLÄ / e - EI", sivunavigointi).
    // Väri+tausta-yhdistelmä erottaa nämä selvästi muusta tekstistä
    static void Komento(string teksti, bool uusiRivi = true) => KirjoitaVarilla(teksti, ConsoleColor.DarkCyan, uusiRivi);

    // ========== FUNKTIOT ==========
    /* Merkintöjen lataus ja suodatus */
    // Lataa merkinnät tietokannasta ja soveltaa nykyisen haku-/suodatintilan
    static void lataaMerkinnat()
    {
        entries = Database.LoadEntries();
        nakyvatMerkinnat = suodataMerkinnat();
    }

    // Suodattaa entries-listan nykyisten hakuSana/suodatinAlkaen/suodatinPaattyen -arvojen perusteella
    static List<DiaryEntry> suodataMerkinnat()
    {
        IEnumerable<DiaryEntry> tulos = entries;

        if (!string.IsNullOrWhiteSpace(hakuSana))
        {
            tulos = tulos.Where(e =>
                e.Title.Contains(hakuSana, StringComparison.OrdinalIgnoreCase) ||
                e.Content.Contains(hakuSana, StringComparison.OrdinalIgnoreCase));
        }

        if (suodatinAlkaen.HasValue)
        {
            tulos = tulos.Where(e => e.Date.Date >= suodatinAlkaen.Value.Date);
        }

        if (suodatinPaattyen.HasValue)
        {
            tulos = tulos.Where(e => e.Date.Date <= suodatinPaattyen.Value.Date);
        }

        return tulos.ToList();
    }

    /* Sivujen määrän laskeminen */
    // Laskee kuinka monta sivua tarvitaan merkintöjen listaamiseen
    static int laskeSivujenMaara()
    {
        if (nakyvatMerkinnat.Count == 0)
            return 1;

        return (int)Math.Ceiling(nakyvatMerkinnat.Count / (double)MerkintojaPerSivu);
    }

    /* Päiväkirja lista */
    // Tulostaa nykyisen sivun merkinnät numeroituna, ja sivunavigointi jos sivuja on useampi.
    // naytaValintaohjeet lisää sivunavigoinnin yhteyteen myös n/e/x-ohjeet (käytetään muokkaus- ja poistovalikossa)
    static void paivakirjalista(bool naytaValintaohjeet = false)
    {
        Otsikko("Päiväkirjan merkinnät:\n");

        if (SuodatinAktiivinen)
        {
            Varoitus($"\t[Suodatin aktiivinen - löytyi {nakyvatMerkinnat.Count} merkintää]\n");
        }

        // Jos (suodatetussa) listassa ei ole yhtään merkintää
        if (nakyvatMerkinnat.Count == 0)
        {
            Vihje("\t(Ei merkintöjä vielä)\n");
            return; // Poistutaan funktiosta heti
        }

        int sivujenMaara = laskeSivujenMaara();

        // Varmistetaan että nykyinen sivu on yhä kelvollinen (esim. merkinnän poiston tai suodatuksen jälkeen)
        if (nykyinenSivu < 1) nykyinenSivu = 1;
        if (nykyinenSivu > sivujenMaara) nykyinenSivu = sivujenMaara;

        int alkuIndeksi = (nykyinenSivu - 1) * MerkintojaPerSivu;
        int loppuIndeksi = Math.Min(alkuIndeksi + MerkintojaPerSivu, nakyvatMerkinnat.Count);

        // Käydään nykyisen sivun merkinnät läpi ja tulostetaan ne
        for (int i = alkuIndeksi; i < loppuIndeksi; i++)
        {
            var e = nakyvatMerkinnat[i];
            Console.WriteLine($"\t{i + 1}. [{e.Date:dd.MM.yyyy HH:mm}] {e.Title}");
            Console.WriteLine($"\t   {e.Content}\n");
        }

        // Näytetään sivunavigointi vain jos sivuja on enemmän kuin yksi
        if (sivujenMaara > 1)
        {
            Vihje($"\tSivu {nykyinenSivu}/{sivujenMaara}");

            if (naytaValintaohjeet)
            {
                // Merkinnän valintanäkymässä sivua vaihdetaan n/e-komennoilla, ei suoraan numerolla
                Komento("\tn - Seuraava sivu  e - Edellinen sivu  x - Peruuta\n");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("\tSiirry sivulle (numero): ");

                for (int sivu = 1; sivu <= sivujenMaara; sivu++)
                {
                    Console.Write(sivu == nykyinenSivu ? $"[{sivu}] " : $"{sivu} ");
                }

                Console.ResetColor();
                Console.WriteLine("\n");
            }
        }
        else if (naytaValintaohjeet)
        {
            // Vain yksi sivu - ei sivunavigointia, mutta peruutusmahdollisuus näytetään silti
            Komento("\tx - Peruuta\n");
        }
    }

    /* Tekstin lisääminen */
    // Lisää uuden merkinnän tietokantaan
    static void lisaatekstia()
    {
        Otsikko("Lisää uusi merkintä:\n");

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
            lataaMerkinnat();

            Console.Clear();
            Console.Write("\x1b[3J");
            paivakirjalista();
            Onnistui("Merkintä lisätty!\n");
        }
        else // Molemmat kentät olivat tyhjiä
        {
            Varoitus("\nMolemmat kentät olivat tyhjiä. Merkintää ei lisätty.\n");
        }

        Vihje("Paina jotain nappia jatkaaksesi...", false);
        Console.ReadKey();
    }

    /* Päivämäärän jäsennys suodatinta varten */
    // Jäsentää käyttäjän syöttämän päivämäärän kiinteillä muodoilla (ei riipu järjestelmän
    // kulttuuriasetuksista, toisin kuin DateTime.TryParse)
    static bool yritaJasentaaPvm(string syote, out DateTime pvm)
    {
        string[] formaatit = { "d.M.yyyy", "d.M.yy" };
        return DateTime.TryParseExact(syote, formaatit, CultureInfo.InvariantCulture, DateTimeStyles.None, out pvm);
    }

    /* Haku ja suodatus */
    // Kysyy käyttäjältä avainsanan ja/tai päivämääräväliin perustuvan suodattimen.
    // Tyhjäksi jätetty kenttä poistaa kyseisen suodattimen käytöstä
    static void asetaSuodatin()
    {
        Otsikko("Hae / suodata merkintöjä:\n");
        Vihje("(Jätä kenttä tyhjäksi jos et halua rajata sillä)\n");

        Console.Write("\tAvainsana (otsikko tai sisältö): ");
        string sana = (Console.ReadLine() ?? "").Trim();

        Console.Write("\tAlkupäivä (esim. 1.1.2026): ");
        string alkuSyote = (Console.ReadLine() ?? "").Trim();

        Console.Write("\tLoppupäivä (esim. 31.12.2026): ");
        string loppuSyote = (Console.ReadLine() ?? "").Trim();

        hakuSana = string.IsNullOrWhiteSpace(sana) ? null : sana;

        // Alkupäivä: tyhjä = ei rajoitusta, virheellinen = ilmoitetaan eikä rajoiteta
        if (string.IsNullOrWhiteSpace(alkuSyote))
        {
            suodatinAlkaen = null;
        }
        else if (yritaJasentaaPvm(alkuSyote, out DateTime alku))
        {
            suodatinAlkaen = alku;
        }
        else
        {
            suodatinAlkaen = null;
            Virhe($"\nVirheellinen alkupäivä '{alkuSyote}' (käytä muotoa pv.kk.vvvv), sitä ei käytetä suodattimena.");
        }

        // Loppupäivä: tyhjä = ei rajoitusta, virheellinen = ilmoitetaan eikä rajoiteta
        if (string.IsNullOrWhiteSpace(loppuSyote))
        {
            suodatinPaattyen = null;
        }
        else if (yritaJasentaaPvm(loppuSyote, out DateTime loppu))
        {
            suodatinPaattyen = loppu;
        }
        else
        {
            suodatinPaattyen = null;
            Virhe($"\nVirheellinen loppupäivä '{loppuSyote}' (käytä muotoa pv.kk.vvvv), sitä ei käytetä suodattimena.");
        }

        // Sovelletaan suodatin heti nykyiseen listaan ja palataan ensimmäiselle sivulle
        nakyvatMerkinnat = suodataMerkinnat();
        nykyinenSivu = 1;

        if (SuodatinAktiivinen)
        {
            Onnistui($"\nSuodatin asetettu. Löytyi {nakyvatMerkinnat.Count} merkintää.\n");
        }
        else
        {
            Vihje("\nSuodatin tyhjennetty, näytetään kaikki merkinnät.\n");
        }

        Vihje("Paina jotain nappia jatkaaksesi...", false);
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
            if (nakyvatMerkinnat.Count == 0)
            {
                Vihje("Paina jotain nappia jatkaaksesi...", false);
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
                if (indeksi >= 0 && indeksi < nakyvatMerkinnat.Count)
                {
                    return nakyvatMerkinnat[indeksi];
                }

                Virhe("\nVirheellinen numero!\n");
            }
            else // Syöte ei ollut numero eikä tunnettu komento
            {
                Virhe("\nVirheellinen valinta!\n");
            }

            Vihje("Paina jotain nappia jatkaaksesi...", false);
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

        Otsikko("Nykyinen merkintä:\n");
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
        lataaMerkinnat();

        Onnistui("\nMerkintä muokattu!\n");

        Vihje("Paina jotain nappia jatkaaksesi...", false);
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

        Varoitus("Haluatko varmasti poistaa tämän merkinnän?\n");
        Console.WriteLine($"\t[{entry.Date:dd.MM.yyyy HH:mm}] {entry.Title}");
        Console.WriteLine($"\t{entry.Content}\n");
        Komento("k - KYLLÄ / e - EI\n");

        // Kysytään vahvistus käyttäjältä
        switch ((Console.ReadLine() ?? "").Trim().ToLower())
        {
            case "k": // Käyttäjä valitsi kyllä
                // Poistetaan tietokannasta Id:n perusteella
                Database.DeleteEntry(entry.Id);

                // Päivitetään paikallinen lista
                lataaMerkinnat();

                Onnistui("\nPoistaminen onnistui!\n");
                break;

            case "e": // Käyttäjä valitsi ei
                Varoitus("\nPoistaminen keskeytetty.\n");
                break;

            default: // Jokin muu syöte
                Virhe("\nVirheellinen valinta. Poistaminen keskeytetty.\n");
                break;
        }

        Vihje("Paina jotain nappia jatkaaksesi...", false);
        Console.ReadKey();
    }

    /* Yhteysasetusten kysyminen */
    // Kysyy käyttäjältä tietokannan yhteysasetukset. Jos nykyinen-parametri annetaan,
    // sen arvoja käytetään oletuksina (tyhjäksi jätetty kenttä säilyttää vanhan arvon)
    static DbConfig kysyYhteysasetukset(DbConfig? nykyinen = null)
    {
        DbConfig config = new DbConfig();

        Vihje("(Jätä kenttä tyhjäksi käyttääksesi suluissa näkyvää oletusarvoa)\n");

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
        Otsikko("Tietokannan yhteysasetukset:\n");

        // Sama valinta kuin ensimmäisellä käynnistyksellä: liitytäänkö olemassa olevaan
        // tietokantaan, vai luodaanko kokonaan uusi
        bool luoUusi = kysyLuodaankoUusiTietokanta();

        Console.WriteLine();

        DbConfig uusi = kysyYhteysasetukset(config);

        if (luoUusi)
        {
            // Sama nimiristiriitojen käsittely kuin ensimmäisellä käynnistyksellä: tarkistaa onko
            // samanniminen tietokanta jo olemassa, ja luo sen tarvittaessa PaivakirjaDB.sql:n pohjalta
            uusi = varmistaTietokannanLuonti(uusi);

            Database.Configure(uusi);

            if (!Database.TestConnection())
            {
                // Palautetaan vanhat, toimivat asetukset käyttöön
                Database.Configure(config);
                Virhe("\nYhteys epäonnistui uusilla asetuksilla. Vanhat asetukset säilytetty.\n");
            }
            else
            {
                uusi.Save();
                config = uusi;
                Onnistui("\nYhteysasetukset tallennettu ja yhteys toimii!\n");
            }
        }
        else
        {
            // Liitytään olemassa olevaan tietokantaan - sama virheenkäsittely kuin ensimmäisellä
            // käynnistyksellä: kokeillaan yhteyttä ja tarkistetaan tietokannan rakenne
            Database.Configure(uusi);

            if (!Database.TestConnection())
            {
                // Palautetaan vanhat, toimivat asetukset käyttöön
                Database.Configure(config);
                Virhe("\nYhteys epäonnistui uusilla asetuksilla. Vanhat asetukset säilytetty.\n");
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
                    Virhe($"\nTietokanta '{uusi.Database}' ei vastaa Päiväkirjasovelluksen odottamaa rakennetta. Vanhat asetukset säilytetty.\n");
                }
                else
                {
                    uusi.Save();
                    config = uusi;
                    Onnistui("\nYhteysasetukset tallennettu ja yhteys toimii!\n");
                }
            }
        }

        Vihje("Paina jotain nappia jatkaaksesi...", false);
        Console.ReadKey();
    }

    /* Asetusvalikko */
    // Näyttää asetusten alivalikon: yhteysasetusten muokkaus tai niiden poistaminen kokonaan
    static void nayttaAsetusValikko(ref DbConfig config)
    {
        Otsikko("Asetukset:\n");
        Komento("\tm - Muuta yhteysasetuksia");
        Komento("\td - Poista tallennetut asetukset\n");
        Komento("\tx - Palaa päävalikkoon\n");

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
        Varoitus("Haluatko varmasti poistaa tallennetut yhteysasetukset?");
        Varoitus("Tämä poistaa koko asetuskansion ja ohjelma suljetaan.\n");
        Komento("k - KYLLÄ / e - EI\n");

        string valinta = (Console.ReadLine() ?? "").Trim().ToLower();

        if (valinta == "k")
        {
            DbConfig.DeleteConfigFolder();

            Onnistui("\nAsetukset poistettu. Ohjelma suljetaan...\n");
            Vihje("Paina jotain nappia lopettaaksesi...", false);
            Console.ReadKey();

            Environment.Exit(0);
        }
        else
        {
            Varoitus("\nPoistaminen keskeytetty.\n");
            Vihje("Paina jotain nappia jatkaaksesi...", false);
            Console.ReadKey();
        }
    }

    /* Liitytäänkö olemassa olevaan tietokantaan vai luodaanko uusi */
    // Kysyy ensimmäisellä käynnistyksellä haluaako käyttäjä käyttää valmiiksi olemassa olevaa
    // tietokantaa, vai luoda kokonaan uuden
    static bool kysyLuodaankoUusiTietokanta()
    {
        Console.WriteLine("Haluatko liittyä olemassa olevaan tietokantaan, vai luoda uuden?\n");
        Komento("\tl - Liity olemassa olevaan tietokantaan");
        Komento("\tu - Luo uusi tietokanta\n");

        while (true)
        {
            Console.Write("Valintasi: ");
            string valinta = (Console.ReadLine() ?? "").Trim().ToLower();

            if (valinta == "l") return false;
            if (valinta == "u") return true;

            Virhe("\nVirheellinen valinta. Syötä 'l' tai 'u'.\n");
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
                Virhe("\nVirhe: Palvelimeen ei saatu yhteyttä annetuilla tunnuksilla.");
                Console.WriteLine("Anna yhteysasetukset uudelleen:\n");
                config = kysyYhteysasetukset(config);
                continue;
            }

            if (!onOlemassa)
            {
                // Nimi on vapaa - luodaan uusi tietokanta skriptin pohjalta
                string sqlPolku = Path.Combine(AppContext.BaseDirectory, "PaivakirjaDB.sql");
                Database.CreateDatabaseFromScript(config, sqlPolku);
                Onnistui($"\nTietokanta '{config.Database}' luotu!\n");
                return config;
            }

            // Samannimine tietokanta löytyi palvelimelta - kysytään käyttäjältä mitä tehdään
            Varoitus($"\nTietokanta '{config.Database}' on jo olemassa palvelimella.\n");
            Komento("\tk - Käytä olemassa olevaa tietokantaa");
            Komento("\tn - Nimeä uusi tietokanta toisin\n");

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
            Otsikko("\tPäiväkirjasovellus - Ensimmäinen käynnistys");
            Otsikko("\t-----------------------------------------\n");

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
                    Virhe($"\nVirhe: Tietokantaan '{config.Database}' ei saatu yhteyttä annetuilla tiedoilla.");
                    Virhe("Ohjelma suljetaan, asetuksia ei tallennettu.\n");
                    Vihje("Paina jotain nappia lopettaaksesi...", false);
                    Console.ReadKey();
                    return;
                }

                if (!kelvollinen)
                {
                    Virhe($"\nVirhe: Tietokanta '{config.Database}' ei vastaa Päiväkirjasovelluksen odottamaa rakennetta.");
                    Virhe("Varmista että liityt oikeaan tietokantaan, tai luo uusi tietokanta sen sijaan.");
                    Virhe("\nOhjelma suljetaan, asetuksia ei tallennettu.\n");
                    Vihje("Paina jotain nappia lopettaaksesi...", false);
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
            Virhe("Virhe: Tietokantaan ei saatu yhteyttä annetuilla asetuksilla!\n");
            Console.WriteLine("Anna yhteysasetukset uudelleen:\n");

            config = kysyYhteysasetukset(config);
            config.Save();
            Database.Configure(config);
            Console.WriteLine();
        }

        // Pääsilmukka - ohjelma pyörii kunnes käyttäjä valitsee exit
        while (true)
        {
            // Ladataan merkinnät tietokannasta ja sovelletaan nykyinen suodatin
            lataaMerkinnat();

            Console.Clear();
            Console.Write("\x1b[3J");

            // Ohjelman otsikko, tietokannan nimi näkyy perässä (vain otsikko alleviivataan)
            const string otsikkoTeksti = "Päiväkirjasovellus";
            Otsikko($"\t{otsikkoTeksti} - {config.Database}");
            Otsikko($"\t{new string('-', otsikkoTeksti.Length)}\n");

            paivakirjalista();  // Näytetään nykyiset merkinnät

            // Käyttäjän valikko
            Console.WriteLine("Mitä haluat tehdä?\n");
            Komento("\tl - Lisää merkintä");
            Komento("\tm - Muokkaa merkintää");
            Komento("\tp - Poista merkintä");
            Komento("\th - Hae/Suodata merkintöjä\n");
            Komento("\tsettings - Yhteysasetukset");
            Komento("\texit - Sulje ohjelma\n");

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

                case "h": // Hae/Suodata merkintöjä
                    Console.Clear();
                    Console.Write("\x1b[3J");
                    asetaSuodatin();
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
