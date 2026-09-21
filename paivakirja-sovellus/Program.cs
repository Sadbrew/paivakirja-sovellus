using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    /*Globaaleja muuttujia*/
    static List<string> teksteja = new List<string>(); //teksteja lista

    static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "teksteja.csv"); //tallennus paikka teksteja.csv tiedostolle

    /*funktiot*/
    // ========== SAVE ==========
    public static void SaveList() //Tiedostojen tallentaminen tiedostoon.
    {
        File.WriteAllLines(filePath, teksteja);
    }

    // ========== LOAD ==========
    public static void LoadList() //Tietojen lataaminen tiedostosta
    {
        if (File.Exists(filePath)) //jos tiedosto on olemassa
        {
            teksteja = File.ReadAllLines(filePath).ToList(); //päivittää tiedoston
        }
        // jos tiedosto ei ole olemassa -> lsita pysyy sellaisenaan
    }

    //Päiväkirja lista
    static void paivakirjalista()
    {
        Console.WriteLine("Päiväkirjan tekstejä: \n");
        for (int i = 0; i < teksteja.Count; i++) //tulostaa tekstit yksitellen teksteja listasta.
        {
            Console.WriteLine($"\t {i + 1}. {teksteja[i]} \n");
        }
    }

    //Tekstin lisääminen listaan
    static void lisaatekstia()
    {
        string teksti;
        Console.WriteLine("Lisää tekstiä:\n");
        Console.Write("\t");
        teksti = Console.ReadLine(); //käyttäjän syöte
        if (teksti != "") //jos käyttäjän syöte EI ole tyhjä, teksti lisätään.
        {
            teksteja.Add(teksti);

            Console.Clear(); //tyhjää konsolen
            paivakirjalista(); //Päiväkirja lista

            // odottaa käyttäjän vastausta jatkaakseen
            Console.Write("Paina jotain nappia jatkaaksesi...");
            Console.ReadKey();
        }
        else // jos syöte ON tyhjä
        {
            Console.WriteLine("\nKenttä tyhjä. Tekstiä ei lisätty.\n");

            // odottaa käyttäjän vastausta jatkaakseen
            Console.Write("Paina jotain nappia jatkaaksesi...");
            Console.ReadKey();
        }

        SaveList(); //tallentaa tiedot
    }

    //Tekstin muokkaaminen
    static void muokkaatekstia()
    {
        paivakirjalista(); //päiväkirja lista

        Console.WriteLine("Mitä tekstiä haluat muokata?: (numero)\n");

        string syote = Console.ReadLine(); //käyttäjän syöte

        // Yritetään muuttaa syöte numeroksi
        if (int.TryParse(syote, out int numero)) //jos syöte on numero
        {
            int indeksi = numero - 1;  // koska lista alkaa nollasta

            if (indeksi >= 0 && indeksi < teksteja.Count) //jos indeksin numero on listassa
            {
                Console.Clear();

                Console.WriteLine($"\tNykyinen teksti: {teksteja[indeksi]} \n"); //vanha teksti
                Console.Write("\tAnna uusi teksti: "); //uusi teksti

                string uusiteksti; // uusi teksti
                uusiteksti = Console.ReadLine();

                if (uusiteksti != "") //jos käyttäjän syöte EI ole tyhjä, teksti muokataan.
                {
                    teksteja[indeksi] = uusiteksti;  // korvataan vanha teksti uudella

                    Console.WriteLine("\nTeksti muokattu!\n");
                }
                else // jos syöte ON tyhjä
                {
                    Console.WriteLine("\nKenttä tyhjä. Tekstiä ei muutettu.\n");
                }

            }
            else //jos indeksin numeroa ei ole listassa
            {
                Console.WriteLine("\nVirheellinen numero!\n");
            }
        }
        else //jos syöte ei ole numero
        {
            Console.WriteLine("\nSyötä numero!\n");
        }

        SaveList(); //tallentaa tiedot

        // odottaa käyttäjän vastausta jatkaakseen
        Console.Write("Paina jotain nappia jatkaaksesi...");
        Console.ReadKey();
    }

    //Tekstin poistaminen
    static void poistatekstia()
    {
        paivakirjalista(); //päiväkirja lista

        Console.WriteLine("Minkä tekstin haluat poistaa?: (numero)\n");

        string syote = Console.ReadLine(); //käyttäjän syöte

        // Yritetään muuttaa syöte numeroksi
        if (int.TryParse(syote, out int numero)) //jos syöte on numero
        {
            int indeksi = numero - 1;  // koska lista alkaa nollasta

            if (indeksi >= 0 && indeksi < teksteja.Count) //jos indeksin numero on listassa
            {
                Console.WriteLine("\nHaluatko varmasti poistaa tekstin:\n");
                Console.WriteLine($"\t{numero}. {teksteja[indeksi]}\n");

                Console.WriteLine("k - KYLLÄ / e - EI\n");
                switch (Console.ReadLine()) // vahvistuskysely
                {
                    case "k": //kyllä
                        teksteja.RemoveAt(indeksi); //poistaa indeksin listasta
                        Console.WriteLine("\nPoistaminen onnistui!\n");
                        break;
                    case "e": //ei
                        Console.WriteLine("\nPoistaminen keskeytetty.\n");
                        break;
                }
            }
            else //jos indeksin numeroa ei ole listassa
            {
                Console.WriteLine("\nVirheellinen numero!\n");
            }
        }
        else //jos syöte ei ole numero
        {
            Console.WriteLine("\nSyötä numero!\n");
        }

        SaveList(); //tallentaa tiedot

        // odottaa käyttäjän vastausta jatkaakseen
        Console.Write("Paina jotain nappia jatkaaksesi...");
        Console.ReadKey();
    }

    /*main*/
    static void Main(string[] args)
    {
        while (true)
        {
            LoadList(); // tietojen lataus
            Console.Clear();

            // Ohjelman otsikko
            Console.WriteLine("\tPäiväkirjasovellus\r");
            Console.WriteLine("\t------------------\n");

            paivakirjalista(); //päiväkirja lista

            // Kysyy mitä käyttäjä haluaa tehdä
            Console.WriteLine("Mitä haluat tehdä?\n");
            Console.WriteLine("\tl - Lisää tekstiä");
            Console.WriteLine("\tm - Muokkaa tekstiä");
            Console.WriteLine("\tp - Poista tekstiä\n");
            Console.WriteLine("\texit - Close the program\n");

            //Käyttäjän valinta päävalikossa
            switch (Console.ReadLine())
            {
                case "l": //lisää tekstiä
                    Console.Clear(); //tyhjää konsolen
                    lisaatekstia(); //lisätään tekstiä
                    break;

                case "m": //muokkaa tekstiä
                    Console.Clear(); //tyhjää konsolen
                    muokkaatekstia(); //muokkaa tekstiä
                    break;

                case "p": //poista tekstiä
                    Console.Clear(); // tyhjää konsolen
                    poistatekstia(); // poistaa tekstiä
                    break;

                case "exit": //poistuu ohjelmasta
                    return;
            }
        }
    }
}