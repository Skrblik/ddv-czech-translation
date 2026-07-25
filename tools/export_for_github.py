"""
Export all translations from Disney Dreamlight Valley localization files.
Categorizes each key into:
  - "pre_existing": was already translated in the original Czech file (before our work)
  - "our_translation": translated by us (not in original CZ, and game text differs from English backup)
  - "untranslated": still in English (game text == English backup text)
  - "do_not_translate": system tags like {DoNotTranslate}, {ItemName} etc.

Outputs:
  - translations/ folder with per-category JSON files
  - translation_stats.json with summary statistics
"""

import zipfile
import json
import os
import sys
import re

zip_backup = r"G:\Games\Disney Dreamlight Valley\ddv_Data\StreamingAssets\Localization\záloha\LocDB_en-US.zip"
zip_game = r"G:\Games\Disney Dreamlight Valley\ddv_Data\StreamingAssets\Localization\LocDB_en-US.zip"
zip_orig_cz = r"G:\Games\Disney Dreamlight Valley\ddv_Data\StreamingAssets\Localization\LocDB_en-US.cz_original.bak"

OUTPUT_DIR = sys.argv[1] if len(sys.argv) > 1 else r"C:\Users\Martin\AppData\LocalLow\Gameloft\Disney Dreamlight Valley\windows_default_r\ddv-czech-translation"

DO_NOT_TRANSLATE_PATTERN = re.compile(r'^\{(DoNotTranslate|ItemName|CharacterName|LocationName|BuildingName|FurnitureName|SetName|ActivityName|UpgradeName|SkinName|PointOfInterestName)[^}]*\}\.?$')

def parse_varint(data, pos):
    result = 0
    shift = 0
    while True:
        if pos >= len(data):
            break
        b = data[pos]
        result |= (b & 0x7f) << shift
        pos += 1
        if not (b & 0x80):
            break
        shift += 7
    return result, pos

def parse_protobuf_list(data):
    pos = 0
    entries = []
    while pos < len(data):
        key = data[pos]
        pos += 1
        wire_type = key & 0x7
        field_num = key >> 3
        if wire_type == 2:
            length, pos = parse_varint(data, pos)
            val = data[pos:pos+length]
            pos += length
            entries.append((field_num, val))
        else:
            break
    return entries

def parse_locbin(data):
    entries = []
    outer_fields = parse_protobuf_list(data)
    for field_num, val in outer_fields:
        if field_num == 1:
            inner_fields = parse_protobuf_list(val)
            inner_dict = {}
            for inf, inv in inner_fields:
                inner_dict[inf] = inv
            audio_id = inner_dict.get(1, b"").decode('utf-8', errors='replace')
            text = inner_dict.get(2, b"").decode('utf-8', errors='replace')
            entries.append((audio_id, text))
    return entries

def get_category(filepath):
    parts = filepath.split('/')
    return parts[0] if len(parts) > 1 else "Root"

def is_do_not_translate(text):
    if not text:
        return True
    text = text.strip()
    if DO_NOT_TRANSLATE_PATTERN.match(text):
        return True
    if text in ('{DoNotTranslate}', '{DoNotTranslate}.'):
        return True
    return False

def main():
    translations_dir = os.path.join(OUTPUT_DIR, "translations")
    os.makedirs(translations_dir, exist_ok=True)

    print("Loading ZIP files...")
    with zipfile.ZipFile(zip_game, 'r') as z_game, \
         zipfile.ZipFile(zip_backup, 'r') as z_back, \
         zipfile.ZipFile(zip_orig_cz, 'r') as z_orig_cz:

        game_namelist = set(z_game.namelist())
        back_namelist = set(z_back.namelist())
        orig_cz_namelist = set(z_orig_cz.namelist())

        all_files = sorted(list(game_namelist | back_namelist))
        locbin_files = [f for f in all_files if f.endswith('.locbin')]

        print(f"Processing {len(locbin_files)} locbin files...")

        # Collect data by category
        categories = {}  # cat -> { file -> [ {key, en_text, cz_text, source} ] }
        
        # Global stats
        stats = {
            "total_keys": 0,
            "pre_existing": 0,
            "our_translation": 0,
            "untranslated": 0,
            "do_not_translate": 0,
            "categories": {}
        }

        for i, f in enumerate(locbin_files):
            if i % 1000 == 0:
                print(f"  {i}/{len(locbin_files)}...")
            
            cat = get_category(f)
            if cat not in categories:
                categories[cat] = {}
            
            # Read game (current state with our translations)
            if f in game_namelist:
                try:
                    game_entries = dict(parse_locbin(z_game.read(f)))
                except:
                    game_entries = {}
            else:
                game_entries = {}

            # Read backup (original English)
            if f in back_namelist:
                try:
                    back_entries = parse_locbin(z_back.read(f))
                except:
                    back_entries = []
            else:
                back_entries = []

            # Read original Czech
            if f in orig_cz_namelist:
                try:
                    orig_cz_entries = dict(parse_locbin(z_orig_cz.read(f)))
                except:
                    orig_cz_entries = {}
            else:
                orig_cz_entries = {}

            file_entries = []
            for audio_id, en_text in back_entries:
                if not en_text:
                    continue

                cz_text = game_entries.get(audio_id, "")
                orig_cz_text = orig_cz_entries.get(audio_id)

                # Determine source
                if is_do_not_translate(en_text):
                    source = "do_not_translate"
                elif orig_cz_text is not None:
                    # Key existed in original Czech translation
                    source = "pre_existing"
                elif cz_text and cz_text != en_text:
                    # Not in original CZ, but has different text = we translated it
                    source = "our_translation"
                else:
                    # Not translated
                    source = "untranslated"

                entry = {
                    "key": audio_id,
                    "en": en_text,
                    "cz": cz_text if cz_text != en_text else "",
                    "source": source
                }
                
                # For pre-existing, also store the original CZ text
                if source == "pre_existing" and orig_cz_text:
                    entry["cz"] = cz_text if cz_text else orig_cz_text
                    entry["original_cz"] = orig_cz_text

                file_entries.append(entry)
                stats["total_keys"] += 1
                stats[source] += 1

            if file_entries:
                categories[cat][f] = file_entries

        # Write per-category JSON files
        print(f"\nWriting {len(categories)} category files...")
        for cat in sorted(categories.keys()):
            cat_data = categories[cat]
            cat_stats = {"files": 0, "total": 0, "pre_existing": 0, "our_translation": 0, "untranslated": 0, "do_not_translate": 0}
            
            # Flatten for output
            output = {}
            for filepath in sorted(cat_data.keys()):
                entries = cat_data[filepath]
                cat_stats["files"] += 1
                for e in entries:
                    cat_stats["total"] += 1
                    cat_stats[e["source"]] += 1
                
                output[filepath] = entries

            cat_stats["translated_pct"] = round(
                (cat_stats["pre_existing"] + cat_stats["our_translation"]) / 
                max(1, cat_stats["total"] - cat_stats["do_not_translate"]) * 100, 1
            )
            
            stats["categories"][cat] = cat_stats

            # Write category file
            safe_cat = cat.replace("/", "_")
            cat_file = os.path.join(translations_dir, f"{safe_cat}.json")
            with open(cat_file, 'w', encoding='utf-8') as fh:
                json.dump(output, fh, ensure_ascii=False, indent=2)

        # Write stats
        stats_file = os.path.join(OUTPUT_DIR, "translation_stats.json")
        with open(stats_file, 'w', encoding='utf-8') as fh:
            json.dump(stats, fh, ensure_ascii=False, indent=2)

        # Print summary
        translatable = stats["total_keys"] - stats["do_not_translate"]
        translated = stats["pre_existing"] + stats["our_translation"]
        print(f"\n{'='*60}")
        print(f"EXPORT COMPLETE")
        print(f"{'='*60}")
        print(f"Total keys:          {stats['total_keys']}")
        print(f"  Do not translate:  {stats['do_not_translate']}")
        print(f"  Translatable:      {translatable}")
        print(f"    Pre-existing CZ: {stats['pre_existing']}")
        print(f"    Our translation: {stats['our_translation']}")
        print(f"    Untranslated:    {stats['untranslated']}")
        print(f"  Translation %:     {translated / max(1, translatable) * 100:.1f}%")
        print(f"\nCategories: {len(categories)}")
        print(f"Output: {OUTPUT_DIR}")

if __name__ == '__main__':
    main()
