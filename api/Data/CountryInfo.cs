namespace Traindle.Data;

// Map countries to continents
public static class CountryInfo
{
    private const string Africa = "Africa";
    private const string Asia = "Asia";
    private const string Europe = "Europe";
    private const string NorthAmerica = "North America";
    private const string SouthAmerica = "South America";
    private const string Oceania = "Oceania";

    private static readonly Dictionary<string, (string Country, string Continent)> Countries =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Africa
            ["Algeria"] = ("Algeria", Africa),
            ["Congo"] = ("DR Congo", Africa),
            ["Djibouti"] = ("Djibouti", Africa),
            ["Egypt"] = ("Egypt", Africa),
            ["Ethiopia"] = ("Ethiopia", Africa),
            ["Ivory Coast"] = ("Ivory Coast", Africa),
            ["Kenya"] = ("Kenya", Africa),
            ["Libya"] = ("Libya", Africa),
            ["Madagascar"] = ("Madagascar", Africa),
            ["Namibia"] = ("Namibia", Africa),
            ["Nigeria"] = ("Nigeria", Africa),
            ["Senegal"] = ("Senegal", Africa),
            ["South Africa"] = ("South Africa", Africa),

            // Asia
            ["Afghanistan"] = ("Afghanistan", Asia),
            ["Armenia"] = ("Armenia", Asia),
            ["Azerbaijan"] = ("Azerbaijan", Asia),
            ["Bangladesh"] = ("Bangladesh", Asia),
            ["China"] = ("China", Asia),
            ["India"] = ("India", Asia),
            ["Indonesia"] = ("Indonesia", Asia),
            ["Iran"] = ("Iran", Asia),
            ["Israel"] = ("Israel", Asia),
            ["Japan"] = ("Japan", Asia),
            ["Malaysia"] = ("Malaysia", Asia),
            ["Myanmar"] = ("Myanmar", Asia),
            ["Pakistan"] = ("Pakistan", Asia),
            ["Philippines"] = ("Philippines", Asia),
            ["Qatar"] = ("Qatar", Asia),
            ["Saudi Arabia"] = ("Saudi Arabia", Asia),
            ["Singapore"] = ("Singapore", Asia),
            ["South Korea"] = ("South Korea", Asia),
            ["Taiwan"] = ("Taiwan", Asia),
            ["Thailand"] = ("Thailand", Asia),
            ["United Arab Emirates"] = ("United Arab Emirates", Asia),
            ["Uzbekistan"] = ("Uzbekistan", Asia),
            ["Vietnam"] = ("Vietnam", Asia),

            // Europe
            ["Austria"] = ("Austria", Europe),
            ["Belarus"] = ("Belarus", Europe),
            ["Belgium"] = ("Belgium", Europe),
            ["Bulgaria"] = ("Bulgaria", Europe),
            ["Czech Republic"] = ("Czech Republic", Europe),
            ["Denmark"] = ("Denmark", Europe),
            ["England"] = ("United Kingdom", Europe),
            ["Finland"] = ("Finland", Europe),
            ["France"] = ("France", Europe),
            ["Germany"] = ("Germany", Europe),
            ["Greece"] = ("Greece", Europe),
            ["Hungary"] = ("Hungary", Europe),
            ["Iceland"] = ("Iceland", Europe),
            ["Ireland"] = ("Ireland", Europe),
            ["Italy"] = ("Italy", Europe),
            ["Lithuania"] = ("Lithuania", Europe),
            ["Luxembourg"] = ("Luxembourg", Europe),
            ["Netherlands"] = ("Netherlands", Europe),
            ["Northern Ireland"] = ("United Kingdom", Europe),
            ["Norway"] = ("Norway", Europe),
            ["Poland"] = ("Poland", Europe),
            ["Portugal"] = ("Portugal", Europe),
            ["Romania"] = ("Romania", Europe),
            ["Russia"] = ("Russia", Europe),   // most of its systems are west of the Urals
            ["Scotland"] = ("United Kingdom", Europe),
            ["Serbia"] = ("Serbia", Europe),
            ["Slovakia"] = ("Slovakia", Europe),
            ["Spain"] = ("Spain", Europe),
            ["Sweden"] = ("Sweden", Europe),
            ["Switzerland"] = ("Switzerland", Europe),
            ["Turkey"] = ("Turkey", Europe),   // Istanbul dominates
            ["Ukraine"] = ("Ukraine", Europe),
            ["United Kingdom"] = ("United Kingdom", Europe),
            ["Wales"] = ("United Kingdom", Europe),

            // North America
            ["Canada"] = ("Canada", NorthAmerica),
            ["Cuba"] = ("Cuba", NorthAmerica),
            ["Guatemala"] = ("Guatemala", NorthAmerica),
            ["Jamaica"] = ("Jamaica", NorthAmerica),
            ["Mexico"] = ("Mexico", NorthAmerica),
            ["México"] = ("Mexico", NorthAmerica),
            ["M�xico"] = ("Mexico", NorthAmerica),   // mis-encoded in the source data
            ["Panama"] = ("Panama", NorthAmerica),
            ["United States"] = ("United States", NorthAmerica),

            // South America
            ["Argentina"] = ("Argentina", SouthAmerica),
            ["Bolivia"] = ("Bolivia", SouthAmerica),
            ["Brazil"] = ("Brazil", SouthAmerica),
            ["Chile"] = ("Chile", SouthAmerica),
            ["Colombia"] = ("Colombia", SouthAmerica),
            ["Ecuador"] = ("Ecuador", SouthAmerica),
            ["French Guiana"] = ("French Guiana", SouthAmerica),
            ["Guyana"] = ("Guyana", SouthAmerica),
            ["Paraguay"] = ("Paraguay", SouthAmerica),
            ["Peru"] = ("Peru", SouthAmerica),
            ["Suriname"] = ("Suriname", SouthAmerica),
            ["Uruguay"] = ("Uruguay", SouthAmerica),
            ["Venezuela"] = ("Venezuela", SouthAmerica),

            // Oceania
            ["Australia"] = ("Australia", Oceania),
            ["New Zealand"] = ("New Zealand", Oceania),
            ["Papua New Guinea"] = ("Papua New Guinea", Oceania),
            ["Tasmania"] = ("Australia", Oceania),
        };

    // Unknown countries pass through unchanged with no continent
    public static (string? Country, string? Continent) Resolve(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return (null, null);
        return Countries.TryGetValue(raw.Trim(), out var info) ? info : (raw, null);
    }
}
