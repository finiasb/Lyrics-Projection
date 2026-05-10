import requests
from bs4 import BeautifulSoup
import time
import urllib3

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

BASE_URL = "https://www.resursecrestine.ro"
ALFABET = "abcdefghijklmnopqrstuvwxyz" 
OUTPUT_FILE = "toate_linkurile_cantece2.txt"

HEADERS = {
    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36'
}

def collect_links():
    print(f"--- Începem colectarea link-urilor (A-Z) ---")
    
    seen_links = set()
    
    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        for litera in ALFABET:
            pagina = 1
            print(f"\n--- Procesăm litera: {litera.upper()} ---")
            
            while True:
                url_index = f"{BASE_URL}/cantece/index-alfabetic/{litera}/pagina/{pagina}"
                
                try:
                    response = requests.get(url_index, headers=HEADERS, timeout=15, verify=False)
                    
                    if response.status_code != 200:
                        print(f"Final de secțiune sau eroare la {litera}/{pagina}.")
                        break
                    
                    if "Nu s-au gasit rezultate" in response.text:
                        print(f"Am terminat litera {litera.upper()}.")
                        break
                    
                    soup = BeautifulSoup(response.text, 'html.parser')
                    links_found_on_page = 0
                    
                    for link in soup.find_all('a', href=True):
                        href = link['href']
                        
                        is_song = "/cantece/" in href
                        is_index = any(x in href for x in ["index-alfabetic", "index-autori", "index-categorii", "index-tematic", "index-referinta", "index-ultimele","/pagina/"])
                        
                        if is_song and not is_index:
                            full_url = BASE_URL + href if href.startswith('/') else href
                            
                            if full_url not in seen_links:
                                seen_links.add(full_url)
                                f.write(full_url + "\n")
                                links_found_on_page += 1
                    
                    if links_found_on_page == 0:
                        break
                        
                    print(f"Litera {litera.upper()} - Pagina {pagina}: {links_found_on_page} link-uri noi.")
                    pagina += 1
                    
                    time.sleep(0.1)
                    
                except Exception as e:
                    print(f"Eroare la {litera.upper()} pagina {pagina}: {e}")
                    break

    print(f"\n--- GATA! ---")
    print(f"S-au salvat {len(seen_links)} link-uri în {OUTPUT_FILE}")

if __name__ == "__main__":
    collect_links()