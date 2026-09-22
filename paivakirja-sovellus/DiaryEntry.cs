// Yksinkertainen päiväkirjamerkintä-luokka
class DiaryEntry
{
    public int Id { get; set; }                 // Tietokannan pääavain
    public DateTime Date { get; set; }          // Merkinnän päivämäärä ja aika
    public string Title { get; set; } = "";     // Merkinnän otsikko
    public string Content { get; set; } = "";   // Merkinnän sisältö
}