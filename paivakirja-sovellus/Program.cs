using System;
using System.Collections.Generic;

class Program
{
    /*Globaaleja muuttujia*/
    static List<string> teksteja = new List<string>(); //teksteja lista


    /*funktiot*/
    //päävalikon tekstit
    static void paavalikko()
    {
        Console.Clear();

        // Ohjelman otsikko
        Console.WriteLine("\tPäiväkirjasovellus\r");
        Console.WriteLine("\t------------------\n");

        // Kysyy mitä käyttäjä haluaa tehdä
        Console.WriteLine("Mitä haluat tehdä?\n");
        Console.WriteLine("\ta - Avaa lista");
        Console.WriteLine("\tl - Lisää tekstiä");
        Console.WriteLine("\tm - Muokkaa tekstiä");
        Console.WriteLine("\tp - Poista tekstiä\n");
        Console.WriteLine("\texit - Close the program\n");
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
        teksti = Console.ReadLine();
        if (teksti != "") //jos käyttäjän syöte EI ole tyhjä, teksti lisätään.
        {
            teksteja.Add(teksti);
        }
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
                Console.WriteLine($"\tNykyinen teksti: {teksteja[indeksi]} \n"); //vanha teksti
                Console.Write("\tAnna uusi teksti: "); //uusi teksti

                teksteja[indeksi] = Console.ReadLine();  // korvataan vanha teksti uudella

                Console.WriteLine("\nTeksti muokattu!\n");
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

        // odottaa käyttäjän vastausta jatkaakseen
        Console.Write("Paina jotain nappia jatkaaksesi...");
        Console.ReadKey();
    }


    /*main*/
    static void Main(string[] args)
    {
        while (true)
        {
            paavalikko(); //päävalikko

            //Käyttäjän valinta päävalikossa
            switch (Console.ReadLine())
            {
                case "a": //avaa lista
                    Console.Clear(); //tyhjää konsolen
                    paivakirjalista(); //päiväkirja lista

                    // odottaa käyttäjän vastausta jatkaakseen
                    Console.Write("Paina jotain nappia jatkaaksesi...");
                    Console.ReadKey();
                    break;

                case "l": //lisää tekstiä
                    Console.Clear(); //tyhjää konsolen
                    lisaatekstia(); //lisätään tekstiä

                    Console.Clear(); //tyhjää konsolen
                    paivakirjalista(); //Päiväkirja lista

                    // odottaa käyttäjän vastausta jatkaakseen
                    Console.Write("Paina jotain nappia jatkaaksesi...");
                    Console.ReadKey();
                    break;

                case "m": //muokkaa tekstiä
                    Console.Clear(); //tyhjää konsolen
                    muokkaatekstia(); //muokkaa tekstiä
                    break;

                case "p": //poista tekstiä
                    break;

                case "exit": //poistuu ohjelmasta
                    return;
            }
        }
    }
}