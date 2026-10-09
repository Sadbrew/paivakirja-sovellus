# Päiväkirjasovellus

Konsolissa toimiva henkilökohtainen päiväkirjasovellus. Merkinnät (päivämäärä, otsikko, sisältö) tallennetaan MySQL-tietokantaan, ja sovellus tarjoaa valikkopohjaisen käyttöliittymän niiden selaamiseen, lisäämiseen, muokkaamiseen, poistamiseen sekä hakemiseen/suodattamiseen.

## Lataus

Valmiiksi käännetty, itsenäinen (self-contained) Windows-käännös löytyy repositorion **[Releases-sivulta](https://github.com/Sadbrew/paivakirja-sovellus/releases/latest)**. Tätä ei tarvitse kääntää itse, eikä koneelle tarvitse asentaa .NET:iä erikseen.

### Käyttöönotto

1. Lataa uusin `paivakirja-sovellus-vX.Y.Z-win-x64.zip` [Releases-sivulta](https://github.com/Sadbrew/paivakirja-sovellus/releases/latest)
2. Pura zip haluamaasi kansioon. Mukana tulevat:
   - `paivakirja-sovellus.exe` – itse sovellus
   - `PaivakirjaDB.sql` – tietokannan luontiskripti (tarvitaan, jos valitset sovelluksessa "luo uusi tietokanta")
   - `esimerkkiTABLE.sql` – valmiit testimerkinnät (ks. alempana, vapaaehtoinen)
3. Varmista, että MySQL-palvelin on käynnissä (esim. XAMPP:n MySQL-moduuli käynnistettynä)
4. Suorita `paivakirja-sovellus.exe`
5. Ensimmäisellä käynnistyksellä sovellus kysyy tietokannan yhteysasetukset — ks. [Oletusasetukset ja konfigurointi](#oletusasetukset-ja-konfigurointi) alempana

> `.exe` on itsenäinen (self-contained) käännös, eli se sisältää tarvittavan .NET-ajoympäristön eikä vaadi erillistä asennusta. Tiedostokoko on tästä syystä suurempi (~70 Mt).

## Ominaisuudet

- Merkintöjen lisäys, muokkaus ja poisto
- Sivutettu listaus (10 merkintää/sivu), sivuja voi selata numerolla tai n/e-komennoilla
- Haku ja suodatus avainsanalla (otsikko tai sisältö) sekä päivämääräväliltä
- Tietokannan yhteysasetusten hallinta: kysytään ensimmäisellä käynnistyksellä, ja niitä voi muuttaa myöhemmin settings-valikosta
- Mahdollisuus liittyä olemassa olevaan tietokantaan tai luoda kokonaan uusi (nimiristiriitojen käsittely mukaan lukien)
- Tietokannan rakenteen tarkistus ennen kantaan liittymistä, ettei sovellus yhdisty väärään/epäyhteensopivaan kantaan
- Väritetty käyttöliittymä: otsikot, onnistumiset, virheet, varoitukset ja komennot erottuvat omilla väreillään

## Ohjelman rakenne

| Tiedosto | Kuvaus |
|---|---|
| `paivakirja-sovellus/Program.cs` | Käyttöliittymä ja päälogiikka: valikot, sivutus, haku/suodatus, värit |
| `paivakirja-sovellus/Database.cs` | Tietokantakutsut (MySqlConnector): haku, lisäys, muokkaus, poisto, rakenteen tarkistus, tietokannan luonti |
| `paivakirja-sovellus/DbConfig.cs` | Yhteysasetusten malli sekä niiden lataus/tallennus levylle |
| `paivakirja-sovellus/DiaryEntry.cs` | Yksittäisen päiväkirjamerkinnän malli |
| `paivakirja-sovellus/PaivakirjaDB.sql` | Tietokannan ja `DiaryEntries`-taulun luontiskripti |
| `paivakirja-sovellus/esimerkkiTABLE.sql` | Valmis testiaineisto (ks. alempana) |

## Oletusasetukset ja konfigurointi

Ensimmäisellä käynnistyksellä sovellus kysyy tietokannan yhteysasetukset. Oletusarvot (suluissa näkyvinä ehdotuksina) vastaavat suoraan XAMPP:n oletus-MySQL-asennusta:

- Palvelin: `localhost`
- Portti: `3306`
- Tietokanta: `PaivakirjaDB`
- Käyttäjätunnus: `root`
- Salasana: *(tyhjä)*

Jokaisen kentän voi myös korvata omalla arvollaan, jos tietokanta ei ole oletuksena XAMPP:ssa (esim. eri palvelin, portti, käyttäjä tai salasana).

Asetukset tallennetaan käyttäjäkohtaiseen tiedostoon:

```
%APPDATA%\PaivakirjaSovellus\dbconfig.json
```

Asetuksia voi muuttaa myöhemmin ohjelman sisältä valikosta **settings -> m**.

### ⚠️ Huomio salasanasta

Yhteysasetukset, mukaan lukien salasana, tallennetaan `dbconfig.json`-tiedostoon **selkeätekstinä, ei hashattuna tai salattuna**. Tämä on hyväksyttävää paikallisessa kehitysympäristössä (esim. XAMPP ilman oikeaa salasanaa), mutta tiedosto **ei ole turvallinen säilytyspaikka oikealle/tuotantosalasanalle**. Älä käytä sovellusta sellaisenaan ympäristössä, jossa tietokannan salasana on arkaluontoinen.

## Asetusten tai koko sovelluksen poistaminen

Tallennetut yhteysasetukset voi poistaa valikosta **settings -> d**. Tämä poistaa koko `%APPDATA%\PaivakirjaSovellus`-kansion ja sulkee ohjelman. Kansion voi vaihtoehtoisesti poistaa myös käsin esim. Resurssienhallinnasta.

Sovelluksen voi poistaa kokonaan koneelta:

1. Poista `%APPDATA%\PaivakirjaSovellus`-kansio (yhteysasetukset), jos niitä ei haluta säilyttää
2. Poista projektin/ohjelman tiedostot (esim. koko kloonattu repo-kansio)
3. Jos myös tietokanta halutaan poistaa, se pudotetaan erikseen MySQL:stä (esim. phpMyAdminilla tai `DROP DATABASE PaivakirjaDB;`) — sovellus ei tee tätä automaattisesti

## esimerkkiTABLE.sql

`paivakirja-sovellus/esimerkkiTABLE.sql` sisältää 23 valmista, satunnaista testimerkintää `DiaryEntries`-tauluun. Tiedosto on tarkoitettu tietokannan täyttämiseen testausta varten, esim. sivutuksen, haun ja suodatuksen kokeiluun ilman että merkintöjä tarvitsee syöttää käsin.

Tuonti esim. phpMyAdminissa:

1. Valitse haluttu tietokanta (esim. `PaivakirjaDB`)
2. **Tuo**-välilehti -> valitse tiedosto `esimerkkiTABLE.sql` -> **Suorita**

Tai komentoriviltä:

```bash
mysql -u root -p PaivakirjaDB < esimerkkiTABLE.sql
```

(Taulun `DiaryEntries` tulee olla olemassa etukäteen — ks. `PaivakirjaDB.sql`, joka luodaan automaattisesti sovelluksen kautta valitsemalla "luo uusi tietokanta".)

## Rakentaminen lähdekoodista (vaihtoehto valmiille julkaisulle)

Jos haluat ajaa uusinta kehitysversiota tai muokata koodia itse, sovelluksen voi myös kääntää lähdekoodista [Lataus](#lataus)-osion valmiin julkaisun sijaan:

1. Asenna [.NET SDK](https://dotnet.microsoft.com/) (projekti kohdistaa .NET 10:een)
2. Kloonaa tai lataa tämä repositorio
3. Varmista, että MySQL-palvelin on käynnissä (esim. XAMPP:n MySQL-moduuli)
4. Aja projektikansiossa (`paivakirja-sovellus/`):

```bash
dotnet run
```

tai käännä ja suorita erikseen:

```bash
dotnet build
```

jonka jälkeen suoritettava tiedosto löytyy kansiosta `paivakirja-sovellus/bin/Debug/net10.0/`.

## Tekniikka

- C# / .NET 10
- Konsolisovellus
- MySQL (MySqlConnector)
