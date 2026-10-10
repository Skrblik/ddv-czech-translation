# 🏰 Disney Dreamlight Valley – Český překlad

Kompletní český překlad pro hru **Disney Dreamlight Valley** včetně Adventure Packu **Honeyglow Woods** (Medvídek Pú, Prasátko, Ijáček a Stokorcový les), updatu **Pixel Perfect** (Raubíř Ralph a Vanelopka) a updatu **Season of Scares v1.25** (Oogie Boogie questy, Lucifer bundle, halloweenský Floating Festival).

> ### ⚠️ Důležité – verze hry
> **Kompatibilní verze hry:** `v1.25.0-8687` + **4 DLCs**  
> Překlad je určen pro uvedenou verzi hry. Na novějších verzích nemusí být všechny texty funkční.

## 📊 Stav překladu

| Metrika | Počet |
|---------|-------|
| **Celkem klíčů** | 147 866 |
| **Přeložitelné klíče** | 146 819 |
| **Přeloženo celkem** | 146 216 (99,6 %) |
| ↳ Předexistující překlad ([666joxer](https://komunitni-preklady.org/preklad/disney-dreamlight-valley)) | 135 056 |
| ↳ **Náš překlad (AI-assisted)** | **11 160** |
| Nepřeloženo | 603 |
| Systémové tagy (nepřekládá se) | 1 047 |

> **Oprava schránky (mailbox fix):** vývojáři ve v1.25 z databáze smazali 98 starších dopisů (`CharacterMail!*`), které ale starší savy pořád obsahují – hra pak ukazovala chybový řetězec místo textu. Tyto dopisy jsme vrátili zpět do balíčku v původní češtině od 666joxer, takže schránka zase funguje.

## 🎯 Co jsme přeložili my (nově ve v1.25 – Season of Scares)

Náš překlad nově pokrývá **update Season of Scares (v1.25)** – Oogie Boogie questy, Lucifer bundle, halloweenský Floating Festival a nové předměty:

| Kategorie | Náš překlad | Celkem |
|-----------|-------------|--------|
| Root (UI, menu, předměty – nové v 1.25) | 2 492 | 30 384 |
| OogieBoogie_OogieBoogie (questy Oogie Boogieho) | 1 003 | 1 017 |
| WreckItRalph_Ralph (Raubíř Ralph dialogy) | 920 | 999 |
| StarPathFTUE_Merlin | 149 | 149 |
| WreckItRalph_TheForgotten | 116 | 122 |
| OogieBoogie_OogieBoogieQuizz (kvíz) | 67 | 73 |
| LuciferBundle25_Cinderella (Lucifer + Popelka) | 41 | 41 |
| LuciferBundle25_FairyGodMother | 40 | 40 |
| LuciferBundle25_Merlin | 32 | 32 |
| LuciferBundle25_Remy | 31 | 31 |
| LuciferBundle25_Mickey | 30 | 30 |
| OogieBoogie_Scrooge | 28 | 28 |
| OogieBoogie_Merlin | 23 | 24 |
| WreckItRalph_Vanellope | 49 | 49 |
| WreckItRalph_MotherGothel | 23 | 23 |
| WreckItRalph_Sully | 19 | 19 |
| OogieBoogie_Remy | 19 | 19 |
| OogieBoogie_Mickey | 17 | 17 |
| + další LuciferBundle25 postavy (Ariel, Jack, Jasmína, Stitch…) | 65 | 65 |

### Adventure Pack "Honeyglow Woods" (Medvídek Pú) – kompletní překlad (z minulé verze)
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
    ├── ...                    # a další (846 kategorií)
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
| `pre_existing` | Komunitní překlad od [666joxer](https://komunitni-preklady.org/preklad/disney-dreamlight-valley) (AI-assisted, lze vylepšit) |
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
| Wreck-It Ralph | Raubíř Ralph |
| Vanellope | Vanelopka |
| Sulley | Sully |
| The Forgotten | Zapomenutý |
| Scrooge McDuck | Scrooge McKvák |
| Star Path | Hvězdná stezka |
| Dreamlight | Dreamlight |
| Star Coins | Hvězdné mince |
| Moonstones | Měsíční kameny |

## ⚙️ Technické detaily

- Lokalizační soubory používají **Protocol Buffers** (protobuf) serializaci v `.locbin` souborech
- Soubory jsou zabaleny v **ZIP archivu** (`LocDB_en-US.zip`)
- Klíče jsou identifikovány jako `cesta/soubor.locbin::audio_id`

## 🙏 Poděkování

- **[666joxer](https://komunitni-preklady.org/preklad/disney-dreamlight-valley)** – autor původního českého překladu, na kterém tento projekt staví. Bez jeho práce (135 067 přeložených klíčů) by tento projekt neexistoval.
- Komunita **[komunitni-preklady.org](https://komunitni-preklady.org/)** – platforma pro československé herní překlady.

## 📜 Licence

Tento překlad je vytvořen fanoušky pro fanoušky. Základní překlad pochází od **666joxer** z komunitni-preklady.org. Disney Dreamlight Valley je vlastnictvím Gameloft / Disney.
