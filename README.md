# AssignmentIndigoLabs

API should expose two endpoints:<br/>
  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;/api/region/cases<br/>
  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Supported query parameters should be:<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Region, possible values (LJ, CE, KR, NM, KK, KP, MB, MS, NG, PO, SG, ZA)<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;From - from date<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;To - to date<br/>
  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Resultset should contain the following values for each day in the time frame selected by the filters above, no grouping by day or region is required:<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Date<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Region<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Number of active cases per day<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Number of vaccinated 1st<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Number of vaccinated 2nd<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Deceased to date<br/>
  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;/api/region/lastweek<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;No filters<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Resultset should contain a list of all regions with the average number of new daily cases in the last 7 days per each region<br/>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;List should be sorted in a descending order of average daily cases<br/>


README.md<br/>
Za avtentikacijo je potrebno v headerju poslat ključ: "AuthenticationPassword" z vrednostjo: "IndigoLabs".<br/><br/>

Oba endpointa sta narejena z GET requestom. Pri "cases" sem datume naredil kar z integerjem, format je YYYYMMDD, dnevi in meseci morajo vsebovat po dve vrednosti npr. za 12.marec 2020 = 20200512
