# 🏰 Disney Dreamlight Valley – Český překlad

Kompletní český překlad pro hru **Disney Dreamlight Valley** včetně Adventure Packu **Honeyglow Woods** (Medvídek Pú, Prasátko, Ijáček a Stokorcový les).

## 📊 Stav překladu

| Metrika | Počet |
|---------|-------|
| **Celkem klíčů** | 142 401 |
| **Přeložitelné klíče** | 141 354 |
| **Přeloženo celkem** | 141 181 (99,9 %) |
| ↳ Předexistující překlad (hra) | 135 067 |
| ↳ **Náš překlad (AI-assisted)** | **6 114** |
| Nepřeloženo | 173 |
| Systémové tagy (nepřekládá se) | 1 047 |

## 🎯 Co jsme přeložili my

Náš překlad pokrývá **6 114 klíčů**, které v originální české lokalizaci chyběly. Jedná se především o:

### Adventure Pack "Honeyglow Woods" (Medvídek Pú) – kompletní překlad
| Kategorie | Náš překlad | Celkem |
|-----------|-------------|--------|
| WinnieDLC_Winnie | 1 715 | 1 736 |
| WinnieDLC_Eeyore | 1 285 | 1 301 |
| WinnieDLC_Piglet | 1 196 | 1 236 |
| WinnieDLC_Mini01 (minihra Púovy klacíky) | 673 | 737 |
| WinnieDLC_Joy | 43 | 43 |
| WinnieDLC_WallE | 40 | 42 |
| WinnieDLC_Stitch | 33 | 34 |
| WinnieDLC_Beast | 30 | 32 |
| WinnieDLC_Sadness | 19 | 22 |
| WinnieDLC_Moana | 27 | 27 |
| WinnieDLC_Remy | 14 | 14 |
| WinnieDLC_Goofy | 14 | 14 |
| WinnieDLC_Minnie | 13 | 13 |
| WinnieDLC_Vanellope | 13 | 13 |
| WinnieDLC_Daisy | 9 | 9 |
| WinnieDLC_Scrooge | 8 | 8 |
| WinnieDLC_Olaf | 8 | 8 |
| WinnieDLC_Aladdin | 4 | 4 |
| WinnieDLC_Tigger | 2 | 2 |

### Hlavní hra – doplněné chybějící překlady
| Kategorie | Náš překlad | Celkem |
|-----------|-------------|--------|
| Root (UI, menu, popisy předmětů) | 907 | 28 626 |
| ProtoDB (databáze předmětů) | 57 | 771 |
| Hercules_Hercules | 2 | 1 276 |
| Pocahontas_Pocahontas | 1 | 985 |
| DarkMountainsUpdate_TheForgotten | 1 | 209 |

## 📁 Struktura repozitáře

```
ddv-czech-translation/
├── README.md                  # Tento soubor
├── translation_stats.json     # Statistiky překladu (strojově čitelné)
├── tools/                     # Nástroje pro práci s překlady
│   ├── export_for_github.py   # Export překladů ze hry
│   └── translator_tool.py     # Aplikace překladů do hry
└── translations/              # Překlady po kategoriích (JSON)
    ├── Root.json              # Hlavní herní texty
    ├── ProtoDB.json           # Databáze předmětů
    ├── WinnieDLC_Winnie.json  # Medvídek Pú dialogy
    ├── WinnieDLC_Piglet.json  # Prasátko dialogy
    ├── WinnieDLC_Eeyore.json  # Ijáček dialogy
    ├── ...                    # a další (751 kategorií)
    └── [kategorie].json
```

## 📖 Formát dat

Každý soubor v `translations/` obsahuje JSON strukturu:

```json
{
  "cesta/k/souboru.locbin": [
    {
      "key": "identifikátor_klíče",
      "en": "English text",
      "cz": "Český překlad",
      "source": "our_translation"
    }
  ]
}
```

### Hodnoty pole `source`:

| Hodnota | Význam |
|---------|--------|
| `pre_existing` | Překlad byl součástí originální české lokalizace hry (potenciálně nižší kvalita, strojový překlad) |
| `our_translation` | **Přeloženo námi** – kvalitní AI-assisted překlad s lidskou kontrolou |
| `untranslated` | Dosud nepřeloženo (stále anglicky) |
| `do_not_translate` | Systémový tag/placeholder (`{DoNotTranslate}`, `{ItemName}` atd.) |

## 🔄 Jak pokračovat po aktualizaci hry

1. **Zálohujte** nový `LocDB_en-US.zip` do `záloha/`
2. Spusťte `tools/export_for_github.py` pro export nového stavu
3. Porovnejte s tímto repozitářem – nové klíče budou mít `source: "untranslated"`
4. Přeložte nové klíče a aplikujte pomocí `tools/translator_tool.py`
5. Commitněte aktualizace

## 🏷️ Konzistence pojmů

Při překladu jsme dodržovali tyto české názvy:

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

## ⚙️ Technické detaily

- Lokalizační soubory používají **Protocol Buffers** (protobuf) serializaci v `.locbin` souborech
- Soubory jsou zabaleny v **ZIP archivu** (`LocDB_en-US.zip`)
- Klíče jsou identifikovány jako `cesta/soubor.locbin::audio_id`
- Překlad byl vytvořen v červenci 2025 pro verzi hry s DLC "A Rift in Time"

## 📜 Licence

Tento překlad je vytvořen fanoušky pro fanoušky. Disney Dreamlight Valley je vlastnictvím Gameloft / Disney.
