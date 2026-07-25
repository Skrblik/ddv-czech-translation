# 🤖 Workflow pro AI – Jak pokračovat v překladu

Tento dokument je určen pro AI asistenta (Claude, Gemini, ChatGPT apod.), který bude pokračovat v překladu nebo vylepšovat existující překlady.

## 📍 Důležité cesty (na počítači uživatele)

```
Hra:           G:\Games\Disney Dreamlight Valley\
Lokalizace:    G:\Games\Disney Dreamlight Valley\ddv_Data\StreamingAssets\Localization\
  LocDB_en-US.zip              ← Aktivní herní soubor (sem se zapisují překlady)
  záloha\LocDB_en-US.zip       ← Záloha originálního anglického souboru (NEMĚNIT!)
  LocDB_en-US.cz_original.bak  ← Záloha původního českého komunitního překladu od 666joxer (NEMĚNIT!)
```

### Zálohy v tomto repozitáři
```
game_backups/
  LocDB_en-US_original_EN.zip  ← Kopie anglického originálu (pro případ ztráty lokálního souboru)
  LocDB_en-US_original_CZ.zip  ← Kopie původní české lokalizace od 666joxer (komunitni-preklady.org)
```
> ⚠️ Pokud zálohy na disku chybí, obnovte je z `game_backups/` složky tohoto repa.

## 🏗️ Architektura lokalizace

- Lokalizační soubory jsou `.locbin` soubory uvnitř ZIP archivu `LocDB_en-US.zip`
- Formát `.locbin` je **Protocol Buffers** (protobuf) serializace
- Každý záznam má `audio_id` (klíč) a `text` (hodnota)
- Klíče se identifikují jako `cesta/soubor.locbin::audio_id`
- Soubory jsou organizovány do kategorií (složek v ZIPu): `Root`, `WinnieDLC_Winnie`, `Hercules_Hercules` atd.

## 📊 Jak rozumět datům v translations/*.json

Každý záznam má pole `source`:

| source | Význam | Co s tím |
|--------|--------|----------|
| `pre_existing` | Překlad od vývojářů hry | Lze vylepšit (často strojový překlad) |
| `our_translation` | Přeloženo námi (kvalitní) | Hotovo, neměnit pokud není chyba |
| `untranslated` | Stále anglicky | **PŘELOŽIT** |
| `do_not_translate` | Systémový tag | Ignorovat, nepřekládat |

## 🔄 Postup: Pokračování po aktualizaci hry

### 1. Příprava
```
- Ověřit, že záloha originálního EN souboru (záloha/LocDB_en-US.zip) odpovídá nové verzi hry
- Pokud hra dostala update, NEJDŘÍVE zálohovat nový originální LocDB_en-US.zip do záloha/
- Původní CZ zálohu (LocDB_en-US.cz_original.bak) ponechat – slouží k rozlišení pre_existing vs nových klíčů
```

### 2. Export aktuálního stavu
```bash
python tools/export_for_github.py "cesta/k/tomuto/repu"
```
Skript porovná 3 ZIP soubory a vytvoří aktualizované JSON soubory v `translations/`.

### 3. Najít nepřeložené klíče
Hledat v JSON souborech záznamy s `"source": "untranslated"`.

### 4. Přeložit
Vytvořit JSON soubor ve formátu:
```json
{
  "cesta/soubor.locbin::audio_id": "Český překlad textu",
  "cesta/soubor.locbin::audio_id2": "Další překlad"
}
```

### 5. Aplikovat překlad do hry
```bash
python tools/translator_tool.py apply "cesta/k/prekladu.json"
```
Tento skript zapíše překlady do herního `LocDB_en-US.zip`.

### 6. Aktualizovat repo
```bash
python tools/export_for_github.py "cesta/k/tomuto/repu"
git add .
git commit -m "Popis změn"
git push
```

## 🔄 Postup: Vylepšení existujících překladů (pre_existing)

### 1. Vybrat kategorii
Prohlédnout `translations/*.json` a najít záznamy s `"source": "pre_existing"`.

### 2. Porovnat kvalitu
Každý `pre_existing` záznam má:
- `en` – anglický originál
- `cz` – současný český překlad (od vývojářů)
- `original_cz` – původní český překlad

### 3. Přeložit lépe
Vytvořit JSON se stejným formátem jako v bodu 4 výše, ale s vylepšenými překlady.

### 4. Aplikovat a uložit
Stejné kroky jako body 5 a 6 výše.

## 📏 Pravidla překladu

### Konzistence pojmů
| Anglicky | Česky |
|----------|-------|
| Winnie the Pooh | Medvídek Pú |
| Piglet | Prasátko |
| Eeyore | Ijáček |
| Tigger | Tygřík |
| Kanga | Klokanice |
| Roo | Klokánek |
| Rabbit | Králíček |
| Owl | Sovička |
| Christopher Robin | Kryštůfek Robin |
| Hundred Acre Wood | Stokorcový les |
| Pooh Sticks | Púovy klacíky |
| Busy Bees' House | Dům pilných včelek |
| Everoak Tree | Věčný dub |
| Ursula | Uršula |
| Dreamlight Valley | Dreamlight Valley (nepřekládat) |
| Star Path | Hvězdná cesta |
| Critters | Zvířátka |
| Realm | Říše |
| Pillar of ... | Pilíř ... |

### Speciální formátování
- `{0}`, `{1}` atd. jsou **proměnné** – zachovat přesně jak jsou
- `<b>text</b>`, `<i>text</i>` – HTML tagy zachovat
- `\n` – odřádkování, zachovat
- `{DoNotTranslate}` – NEPŘEKLÁDAT, ponechat přesně

### Styl překladu
- Vykání hráči (formální "vy")
- Postavy mezi sebou tykají
- Zachovat tón originálu (humor, vážnost, dětský styl)
- U dětských deníků (childhood diaries) zachovat záměrné pravopisné chyby a dětský styl psaní

## ⚠️ Důležitá upozornění

1. **NIKDY neměnit zálohy** (`záloha/LocDB_en-US.zip` a `LocDB_en-US.cz_original.bak`)
2. **Testovat ve hře** po aplikaci překladů
3. **Commitovat často** – aby se práce neztratila
4. Cesty v `tools/*.py` jsou **hardcoded** – při změně instalace je nutné upravit
