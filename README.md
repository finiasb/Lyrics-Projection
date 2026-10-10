# Lyrics Projector

![Proiectie screenshot](Proiectie/Photos/proiectie.png)

A WPF (.NET 8) desktop application developed for my local church to project song lyrics during live services.

## Features

- Search songs by title or lyrics.
- Manage favorites and projection history.
- Add, edit, and delete songs from the local database.
- Temporarily edit a song for the live projection or save changes globally.
- Show or hide a live clock on the projection screen.
- Customize projection font, font sizes, background image, target monitors, and verse tracker color.
- Build a song database from Resurse Crestine links using the included Python scraping script.

## Database & Setup
- The app expects `Cantece.db` to be located beside the executable.
- All songs are scraped from [Resurse Creștine](https://www.resursecrestine.ro/). 
- Scraping steps: Before generating the database, run scrapingLinks.py first to generate a fresh .txt file containing all the song links. Once the link file is created, run build_song_database.py.
- If no monitor is selected in settings, the app projects to non-primary monitors by default.
