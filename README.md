Kode for å generere en syntetisk befolkningdatabase (folkeregisteret). Dette realiseres ved å:
1.	Lag en statisk modell av kilden. I denne koden er det vedlagt en liten fil som emulerer en database
2.	Bruke denne modellen til å lage en ny database som statistisk sett er tilnærmet lik utganspunktet

Del 1 trenger bare å gjøres èn gang. Del 2 gjøres hver gang man ønsker en ny database. 

Fordelen med en syntetisk løsning er at oppbygningen av den nye databasen har ingen rot i virkeligheten. Ved å bygge opp dataene basert på statistiske parametere, vil man få testdatakombinasjoner som det ikke finnes tilsvarende av i produksjon som er ønskelig i testing. 

1.	Opprettelse av statistikkmodell
I statistikkmodellen finnes følgende parametere:
Parameter	Beskrivelse
Kjønn	Sannsynligheten for en person er kvinne 
HasDnummer_Kjonn	Gitt at personen har d-nummer, sannsynligheten for en person er kvinne
Age	Aldersfordeling (hvor mange personer finnes per 5-år-kvantil)
IsDead	Gitt 5-år-kvantil, hva er sannsynligheten for at en person er død
HasFather	Gitt 5-år-kvantil, hva er sannsynligheten for at person har en far
HasMother	Gitt 5-år-kvantil, hva er sannsynligheten for at person har en mor
HasSpouse	Gitt 5-år-kvantil, hva er sannsynligheten for at person har en ektefelle
HasDnummer	Gitt 5-år-kvantil, hva er sannsynligheten for at person har d-nummer
HasValidNin	Hva er sannsynligheten for at et personnummer er gyldig ihht mod-11
Citizenship	Statistisk fordeling for landskoder  
Custody	Statistisk fordeling av foreldrerett til en person
HasDufNo	Gitt 5-år-kvantil, hva er sannsynligheten for at person har duf-nummer
MaritalStatus	Statistisk fordeling av sivilstatus
RegStatus	Statistisk fordeling av registreringskode
HasWorkPermit	Gitt 5-år-kvantil, hva er sannsynligheten for at person har arbeidstillatelse
PostalCode	Statistisk fordeling av postnummer
Children	Gitt 5-år-kvantil, hva er statistik fordeling av antall barn

I tillegg til denne informasjonen, lagres det informasjon om korrelasjonen mellom alle parametere som er binære (enten/eller). For eksempel: Gitt at du har registrert en mor, hva er sannsynligheten for at du har registrert en far? Merk her at det ikke lagres korrelasjon mellom verdier, kun om en verdi er satt. For eksempel lagres ikke korrelasjon mellom landskode=Brasil og Sivilstatus=gift, heller korrelasjonen mellom Har-Landskode og Har-Sivilstatus («Har» som i betydningen: har verdi for dette feltet).

I solution er det "Model.csproj" som gjør denne jobben. Output er en json-fil som er noen mb stor. 

2. Oppbygning av korrelasjonsmatrise
For å få en korrelasjonsmatrise som kan brukes for å lage ny data basert på binære sannsynlighetsmodeller, må modellen laget i punkt 1 gjennom en prosess (i "Correlation.csproj"). Her lages en korrelasjonsmatrise basert på paper: http://epub.wu.ac.at/286/1/document.pdf

For at dette skal fungere, må R være installert på din maskin. 
Output fra dette steget er en ny modell tilsvarende i punkt 1, men nå med en korrekt korrelasjonsmatrise. 

3.	Oppbygning om syntetisk data basert på statistikkmodell
Basert på modellen i punkt 1, er det en algoritme som kan lage et valgfritt antall personer til en ny person-database. Målet med denne algoritmen er å lage en tilnærmet lik, statistisk sett, database som utgangspunktet. Herunder følger de viktigste elementene fra denne algoritmen. 
Dersom det finnes få datapunkter innenfor en statistisk gruppe, kan det være en risiko for å utlevere spor av reell informasjon ved å basere seg på denne. Derfor er det satt en grense på at det må finnes minst 1000 datapunkter for å bruke en statistisk parameter. 
Korrelasjonen mellom datapunktene følger en normalisert korrelasjonsmatrise for hele befolkningen når det regnes ut hvordan avhengigheter mellom dataen skal være. 
Algoritmen gir en gitt person et gyldig personnummer «Gyldig» her betyr at personnummeret følger modulus-11 utregning; et personnummer inneholder informasjon om en dato og kjønn, mens de andre siffrene er kontrollsiffer. Sjansen for at algoritmen genererer et personnummer som faktisk er i bruk i befolkningen, er tilnærmet 100% fordi algoritmen ikke vet hvilke personnummer som finnes i produksjon og dermed ikke kan kontrollere dette. Det er heller ikke ønskelig å gjøre en slik kontroll, da det vil utlevere informasjon om hvilke personnummer som finnes i befolkningen (det vil være den disjunkte mengden). 
Algortimen gir en adresse til en person, og disse adressene er hentet fra posten.no (alle mulige postadresser per postnummer er hentet og lagret). Adressene er derfor ekte, i betydningen av de finnes i Norge. 
Algoritmen gir et navn til en person, og disse navnene er hentet fra SSB. SSB gir ut statistisk fordeling av både fornavn og etternavn, gitt at det er flere enn 200 som har samme navn. Denne statistiske fordelingen er gjenspeilet i denne løsningen. 

I solution er det "Build.csproj" som gjør denne jobben. Output er en json-fil som er noen mb stor. Man kan endre koden til å bruke lagre dataen til en database. 
