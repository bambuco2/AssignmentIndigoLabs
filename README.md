# AssignmentIndigoLabs

API should expose two endpoints:
  /api/region/cases
  Supported query parameters should be:
    Region, possible values (LJ, CE, KR, NM, KK, KP, MB, MS, NG, PO, SG, ZA)
    From - from date
    To - to date
  Resultset should contain the following values for each day in the time frame selected by the filters above, no grouping by day or region is required:
    Date
    Region
    Number of active cases per day
    Number of vaccinated 1st
    Number of vaccinated 2nd
    Deceased to date
  /api/region/lastweek
    No filters
    Resultset should contain a list of all regions with the average number of new daily cases in the last 7 days per each region
    List should be sorted in a descending order of average daily cases


README.md
Za avtentikacijo je potrebno v headerju poslat ključ: "AuthenticationPassword" z vrednostjo: "IndigoLabs".

Oba endpointa sta narejena z GET requestom. Pri "cases" sem datume naredil kar z integerjem, format je YYYYMMDD, dnevi in meseci morajo vsebovat po dve vrednosti npr. za 12.marec 2020 = 20200512
