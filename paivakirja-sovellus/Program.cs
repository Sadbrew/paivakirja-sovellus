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
        teksti = Console.ReadLine();
        if (teksti != "") //jos käyttäjän syöte EI ole tyhjä, teksti lisätään.
        {
            teksteja.Add(teksti);
        }
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
                    Console.Write("Press any key to continue...");
                    Console.ReadKey();
                    break;

                case "l": //lisää tekstiä
                    Console.Clear(); //tyhjää konsolen
                    lisaatekstia(); //lisätään tekstiä

                    Console.Clear(); //tyhjää konsolen
                    paivakirjalista(); //Päiväkirja lista

                    // odottaa käyttäjän vastausta jatkaakseen
                    Console.Write("Press any key to continue...");
                    Console.ReadKey();
                    break;

                case "m": //muokkaa tekstiä
                    break;

                case "p": //poista tekstiä
                    break;

                case "exit": //poistuu ohjelmasta
                    return;
            }
        }
    }
}