import argparse
import re
import sqlite3
import sys
import time
from pathlib import Path
from typing import Iterable

import requests
import urllib3
from bs4 import BeautifulSoup
from bs4.element import NavigableString, Tag

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

HEADERS = {
    "User-Agent": (
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
        "AppleWebKit/537.36 (KHTML, like Gecko) "
        "Chrome/124.0.0.0 Safari/537.36"
    )
}

DEFAULT_LINKS_FILE = Path(r"C:\Users\Fineas\source\repos\ResurseScraping\linkuri_cantece.txt")
DEFAULT_DB_FILE = Path(r"C:\Users\Fineas\source\repos\ResurseScraping\Cantece.db")
SONG_URL_RE = re.compile(r"^https://www\.resursecrestine\.ro/cantece/\d+/[^/\s]+/?$")


def normalize_text(text: str) -> str:
    if not text or text.isspace():
        return ""

    text = text.lower()
    text = text.replace("ă", "a").replace("â", "a")
    text = text.replace("î", "i")
    text = text.replace("ș", "s").replace("ş", "s")
    text = text.replace("ț", "t").replace("ţ", "t")
    text = text.replace("-", " ")
    text = re.sub(r"[^a-z0-9]", " ", text)
    text = re.sub(r"\s+", " ", text)
    return text.strip()


def clean_lyrics(container: Tag | None) -> str:
    if container is None:
        return ""

    parts: list[str] = []

    for element in container.descendants:
        if isinstance(element, NavigableString):
            text = str(element)
            if text.strip():
                parts.append(text.strip())
        elif isinstance(element, Tag) and element.name == "br":
            parts.append("\n")

    lyrics = "".join(parts)
    lines = [line.strip() for line in lyrics.splitlines()]
    lyrics = "\n".join(lines)
    return re.sub(r"\n{3,}", "\n\n", lyrics).strip()


def extract_author(soup: BeautifulSoup) -> str:
    author_link = soup.select_one('a.blueLink[href*="/cantece/index-autori/"]')
    if author_link:
        return author_link.get_text(" ", strip=True)

    author_label = soup.find(string=lambda value: isinstance(value, str) and value.strip().lower() == "autor")
    if author_label:
        parent = author_label.parent if isinstance(author_label.parent, Tag) else None
        if parent:
            linked_author = parent.find_next("a", href=re.compile(r"/cantece/index-autori/"))
            if linked_author:
                return linked_author.get_text(" ", strip=True)

            sibling_text = parent.get_text(" ", strip=True)
            sibling_text = re.sub(r"(?i)^autor\s*:?\s*", "", sibling_text).strip()
            if sibling_text and sibling_text.lower() != "autor":
                return sibling_text

    return ""


def iter_links(path: Path) -> Iterable[str]:
    seen: set[str] = set()

    with path.open("r", encoding="utf-8") as handle:
        for raw_line in handle:
            url = raw_line.strip()
            if not url:
                continue
            if "/index-autori/" in url:
                continue
            if not SONG_URL_RE.match(url):
                continue
            if url in seen:
                continue

            seen.add(url)
            yield url


def create_schema(connection: sqlite3.Connection) -> None:
    cursor = connection.cursor()
    cursor.execute(
        """
        CREATE TABLE IF NOT EXISTS Cantece (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            titlu TEXT NOT NULL,
            versuri TEXT NOT NULL,
            titluNormalizat TEXT NOT NULL,
            versuriNormalizate TEXT NOT NULL,
            autor TEXT,
            url TEXT NOT NULL UNIQUE
        )
        """
    )
    cursor.execute("CREATE INDEX IF NOT EXISTS idx_cantece_titlu_normalizat ON Cantece(titluNormalizat)")
    cursor.execute("CREATE INDEX IF NOT EXISTS idx_cantece_autor ON Cantece(autor)")
    connection.commit()


def fetch_song(session: requests.Session, url: str, timeout: int) -> tuple[str, str, str]:
    response = session.get(url, headers=HEADERS, timeout=timeout, verify=False)
    response.raise_for_status()

    soup = BeautifulSoup(response.text, "html.parser")

    title_node = soup.find("div", class_="titleContent")
    title = title_node.get_text(" ", strip=True) if title_node else ""

    lyrics_container = soup.find("div", class_="resized-text")
    lyrics = clean_lyrics(lyrics_container)
    author = extract_author(soup)

    return title, lyrics, author


def upsert_song(connection: sqlite3.Connection, url: str, title: str, lyrics: str, author: str) -> None:
    cursor = connection.cursor()
    cursor.execute(
        """
        INSERT INTO Cantece (
            titlu,
            versuri,
            titluNormalizat,
            versuriNormalizate,
            autor,
            url
        )
        VALUES (?, ?, ?, ?, ?, ?)
        ON CONFLICT(url) DO UPDATE SET
            titlu = excluded.titlu,
            versuri = excluded.versuri,
            titluNormalizat = excluded.titluNormalizat,
            versuriNormalizate = excluded.versuriNormalizate,
            autor = excluded.autor
        """,
        (
            title,
            lyrics,
            normalize_text(title),
            normalize_text(lyrics),
            author,
            url,
        ),
    )


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Construieste baza de date SQLite pentru cantece folosind linkurile existente."
    )
    parser.add_argument("--links", type=Path, default=DEFAULT_LINKS_FILE, help="Fisierul TXT cu linkuri.")
    parser.add_argument("--db", type=Path, default=DEFAULT_DB_FILE, help="Fisierul SQLite rezultat.")
    parser.add_argument("--delay", type=float, default=0.15, help="Pauza dintre request-uri, in secunde.")
    parser.add_argument("--timeout", type=int, default=20, help="Timeout per request, in secunde.")
    parser.add_argument("--commit-every", type=int, default=100, help="Commit la fiecare N cantece.")
    parser.add_argument("--limit", type=int, default=0, help="Proceseaza doar primele N linkuri valide.")
    return parser.parse_args()


def main() -> int:
    args = parse_args()

    if not args.links.exists():
        print(f"Fisierul de linkuri nu exista: {args.links}")
        return 1

    links = list(iter_links(args.links))
    if args.limit > 0:
        links = links[: args.limit]

    print(f"Linkuri valide pentru procesare: {len(links)}")
    print(f"Baza de date: {args.db}")

    args.db.parent.mkdir(parents=True, exist_ok=True)
    connection = sqlite3.connect(args.db)
    create_schema(connection)

    processed = 0
    failed = 0

    session = requests.Session()

    try:
        for index, url in enumerate(links, start=1):
            try:
                title, lyrics, author = fetch_song(session, url, args.timeout)

                if not title:
                    title = "Titlu necunoscut"
                if not lyrics:
                    lyrics = "Versuri negasite"

                upsert_song(connection, url, title, lyrics, author)
                processed += 1

                if index % args.commit_every == 0:
                    connection.commit()

                if index % 25 == 0 or index == len(links):
                    print(f"[{index}/{len(links)}] procesate={processed} erori={failed} titlu={title[:60]}")

                if args.delay > 0:
                    time.sleep(args.delay)
            except KeyboardInterrupt:
                raise
            except Exception as exc:
                failed += 1
                print(f"Eroare la {url}: {exc}")
    except KeyboardInterrupt:
        print("Oprire manuala detectata. Se salveaza progresul curent.")
    finally:
        connection.commit()
        connection.execute("VACUUM")
        connection.commit()
        connection.close()
        session.close()

    print(f"Finalizat. Procesate: {processed}. Erori: {failed}.")
    print(f"DB salvata in: {args.db}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
