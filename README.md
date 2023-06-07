# MVC-Garage-and-Gym14
(1) Övn 13 - Garage 3.0 (2) Övn 14 - Användarhantering för passbokning

Garage 3.0 var det mest komplicerade garaget av samtliga garage under vår kurstid i Lexicon. Det har förutom tidigare funktioner
även många andra som rör kostnadshantering, såsom bonus, pro- och guld. Dessa styrs av ålder och hur mycket man använder garaget.
Även intäktshantering i form av fasta och rörliga intäkter, statistik osv. Andra tips på finesser var olika sökfunktioner,
paginering och datafiler för lagring av fasta priser etc. Allt detta var omöjligt att hinna göra på en vecka. Bara att sätta sig
in i hur databaserna fungerade var tillräckligt svårt. Vi fick vårat ER-diagram godkänt den först dagen men av vad jag har förstått
så var det på förhand dömt att misslyckas. Vi saknade helt enkelt den kunskapen. Men efter mycket fnulande och funderande så löste
det sig till sist, efter att sprinttiden förlängdes. Garage 3.0 ansåg de flesta i gänget vara den svåraste övningen hittills. Det
var svårt men lärorikt! Det tvingade oss att förstå och hantera MVC och databaser och detta var vi förstås tacksamma för!

Övning 14 - Användarhantering för passbokning, tränade oss att bygga en MVC-applikation som jobbar med databaser med många till
många förhållanden och som dessutom innefattar "Authentication Individual Accounts", som särskiljer på medlemmar och administratörer.
Vid det här laget gick det lättare att sätta upp databaserna. Faktum var att när man väl fått kläm på hur identitetsramverket fungerade
så flöt det på ganska fort. Det största problemet var att blanda inmatningsformulär med uppdaterbara tabeller på ett och samma ställe.
Min lösning var att använda en statisk klass med statiska listor. Det verkade som att det enda som MVC tillät var statiska variabler
i form av listor eller uppräkningsbara objekt. Jag såg till att ladda dessa listor på strategiska ställen, därefter kunde jag använda
dem som en behållare för att kunna tömma deras innehåll på den plats där jag ville ha datan. Fast vissa hade synpunkter på detta - att
det skulle kunna leda till att systemet blev överbelastat...
