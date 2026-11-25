# Traindle
 
A Wordle-style daily puzzle for transit fans. Guess a city's rail network, or a single line, using hints about where it is, how big it is, and how old it is.
 
**Play it at [traindle.talinya.ca](https://traindle.talinya.ca)**

## Project structure
 
```
api/
├── Controllers/   # Daily, Endless (random) and game-data endpoints
├── Services/      # Citylines ingestion, game-data processing, daily puzzle generation, tracking
├── Data/          # EF Core models and context, country → continent mapping
├── Migrations/
└── scripts/       # PowerShell shortcuts for run / ingest
www/
└── src/
    ├── api/         # API client, types, game state + localStorage
    ├── components/  # Systems and Lines games, guess input, hint cells, changelog
    └── data/        # Changelog entries
```


## Acknowledgments
 
Transit data comes from [Citylines](https://www.citylines.co) and its contributors. Check their site for the data's license terms.
