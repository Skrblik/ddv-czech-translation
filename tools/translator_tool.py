import zipfile
import json
import os

zip_backup = r"G:\Games\Disney Dreamlight Valley\ddv_Data\StreamingAssets\Localization\záloha\LocDB_en-US.zip"
zip_game = r"G:\Games\Disney Dreamlight Valley\ddv_Data\StreamingAssets\Localization\LocDB_en-US.zip"
zip_temp = r"c:\Users\Martin\AppData\LocalLow\Gameloft\Disney Dreamlight Valley\windows_default_r\LocDB_en-US_temp.zip"

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

def encode_varint(value):
    out = bytearray()
    while True:
        towrite = value & 0x7f
        value >>= 7
        if value:
            out.append(towrite | 0x80)
        else:
            out.append(towrite)
            break
    return bytes(out)

def parse_protobuf_list(data):
    pos = 0
    entries = []
    while pos < len(data):
        if pos >= len(data):
            break
        key = data[pos]
        pos += 1
        wire_type = key & 0x7
        field_num = key >> 3
        if wire_type == 2:  # Length-delimited
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

def serialize_locbin(entries):
    out = bytearray()
    for audio_id, text in entries:
        inner = bytearray()
        audio_bytes = audio_id.encode('utf-8')
        inner.extend([0x0a])
        inner.extend(encode_varint(len(audio_bytes)))
        inner.extend(audio_bytes)
        
        if text:
            text_bytes = text.encode('utf-8')
            inner.extend([0x12])
            inner.extend(encode_varint(len(text_bytes)))
            inner.extend(text_bytes)
        
        out.extend([0x0a])
        out.extend(encode_varint(len(inner)))
        out.extend(inner)
    return bytes(out)

def extract_texts(category_filter=None, max_keys=100, output_json="extract.json"):
    """
    Extracts English texts that are currently in the game's LocDB_en-US.zip
    but have English text (or belong to new folders) and need translation.
    Category filter can be a folder prefix (e.g. 'WinnieDLC_Mini01' or 'Root')
    """
    print(f"Extracting up to {max_keys} keys from {category_filter or 'any category'}...")
    extracted = {}
    
    with zipfile.ZipFile(zip_game, 'r') as z_game, zipfile.ZipFile(zip_backup, 'r') as z_back:
        game_namelist = set(z_game.namelist())
        back_namelist = set(z_back.namelist())
        
        # We want to find keys where:
        # 1. The file is only in backup (Winnie DLC files, etc.) - these are 100% English.
        # 2. The file is in both, but contains keys that were added from the backup (English).
        # To determine which keys were added/English in shared files, we compare game file with backup.
        # Wait, how do we know if a key is "English" in a shared file?
        # A key is "English" in a shared file if it was added from backup and was not present in the original Czech file.
        # Let's read the backup's LocDB_en-US.cz_original.bak to see the original Czech keys!
        bak_cz_path = r"G:\Games\Disney Dreamlight Valley\ddv_Data\StreamingAssets\Localization\LocDB_en-US.cz_original.bak"
        with zipfile.ZipFile(bak_cz_path, 'r') as z_orig_cz:
            orig_cz_namelist = set(z_orig_cz.namelist())
            
            # Sort files to process them deterministically
            files_to_process = sorted(list(game_namelist | back_namelist))
            
            keys_found = 0
            for f in files_to_process:
                if not f.endswith('.locbin'):
                    continue
                
                # Apply category filter
                if category_filter:
                    if category_filter == 'Root':
                        if '/' in f:
                            continue
                    elif not f.startswith(category_filter + '/'):
                        continue
                
                # Get what is currently in game zip
                if f in game_namelist:
                    try:
                        game_entries_dict = dict(parse_locbin(z_game.read(f)))
                    except Exception as e:
                        print(f"Error parsing game file {f}: {e}")
                        game_entries_dict = {}
                else:
                    game_entries_dict = {}

                # Get original Czech keys if this file existed
                if f in orig_cz_namelist:
                    try:
                        orig_cz_keys = set(e[0] for e in parse_locbin(z_orig_cz.read(f)))
                    except:
                        orig_cz_keys = set()
                else:
                    orig_cz_keys = set()
                
                # Read from backup (original English)
                try:
                    back_entries = parse_locbin(z_back.read(f))
                except Exception as e:
                    print(f"Error reading backup file {f}: {e}")
                    continue
                
                for audio_id, text in back_entries:
                    if not text:
                        continue
                    
                    # If this file was in original Czech, only extract keys that were NOT in the original Czech keys
                    if f in orig_cz_namelist and audio_id in orig_cz_keys:
                        continue
                        
                    # Check if it's already translated in game zip
                    in_game = audio_id in game_entries_dict
                    current_text = game_entries_dict.get(audio_id)
                    
                    # Extract if not in game, or if current text is same as English original (meaning untranslated)
                    if not in_game or current_text == text:
                        extracted[f"{f}::{audio_id}"] = text
                        keys_found += 1
                        if keys_found >= max_keys:
                            break
                            
                if keys_found >= max_keys:
                    break
                    
    with open(output_json, 'w', encoding='utf-8') as fh:
        json.dump(extracted, fh, ensure_ascii=False, indent=2)
    print(f"Successfully extracted {len(extracted)} keys to {output_json}")

def apply_translations(translation_json):
    """
    Applies translations from a JSON file into the game's LocDB_en-US.zip.
    The JSON keys are in format "filepath::audio_id".
    """
    if not os.path.exists(translation_json):
        print(f"Translation file {translation_json} not found!")
        return
        
    with open(translation_json, 'r', encoding='utf-8') as fh:
        translations = json.load(fh)
        
    print(f"Applying {len(translations)} translations from {translation_json}...")
    
    # We group translations by file path
    by_file = {}
    for full_key, translation_text in translations.items():
        if "::" not in full_key:
            continue
        fpath, audio_id = full_key.split("::", 1)
        if fpath not in by_file:
            by_file[fpath] = {}
        by_file[fpath][audio_id] = translation_text
        
    # We open the game's current LocDB_en-US.zip, copy everything to a temp zip,
    # also add any missing files from backup, then apply translations
    with zipfile.ZipFile(zip_game, 'r') as z_in, \
         zipfile.ZipFile(zip_backup, 'r') as z_back, \
         zipfile.ZipFile(zip_temp, 'w', compression=zipfile.ZIP_DEFLATED) as z_out:
        
        game_files = set(z_in.namelist())
        back_files = set(z_back.namelist())
        
        # First: copy missing locbin files from backup into output
        missing = back_files - game_files
        missing_added = 0
        for f in sorted(missing):
            if f.endswith('.locbin'):
                if f in by_file:
                    # Apply translations to the missing file too
                    data = z_back.read(f)
                    entries = parse_locbin(data)
                    new_entries = []
                    file_translations = by_file[f]
                    applied_count = 0
                    for audio_id, text in entries:
                        if audio_id in file_translations:
                            new_entries.append((audio_id, file_translations[audio_id]))
                            applied_count += 1
                        else:
                            new_entries.append((audio_id, text))
                    modified_data = serialize_locbin(new_entries)
                    z_out.writestr(f, modified_data)
                    print(f"  Added+translated {applied_count} in missing file {f}")
                else:
                    z_out.writestr(f, z_back.read(f))
                missing_added += 1
        
        if missing_added:
            print(f"  Added {missing_added} missing files from backup")
        
        # Then: process existing game files
        for f in z_in.namelist():
            if f in by_file:
                # Modify file
                data = z_in.read(f)
                entries = parse_locbin(data)
                
                # Apply translations
                new_entries = []
                file_translations = by_file[f]
                applied_count = 0
                for audio_id, text in entries:
                    if audio_id in file_translations:
                        new_entries.append((audio_id, file_translations[audio_id]))
                        applied_count += 1
                    else:
                        new_entries.append((audio_id, text))
                        
                # Re-serialize and write
                modified_data = serialize_locbin(new_entries)
                z_out.writestr(f, modified_data)
                print(f"  Applied {applied_count} translations to {f}")
            else:
                # Copy as is
                z_out.writestr(f, z_in.read(f))
                
    # Safely replace game ZIP with modified ZIP
    import shutil
    shutil.move(zip_temp, zip_game)
    print("Translations applied successfully and game ZIP updated!")

if __name__ == "__main__":
    import sys
    if len(sys.argv) < 2:
        print("Usage:")
        print("  python translator_tool.py extract <category> <max_keys> <output_json>")
        print("  python translator_tool.py apply <translation_json>")
    else:
        cmd = sys.argv[1]
        if cmd == "extract":
            cat = sys.argv[2] if len(sys.argv) > 2 and sys.argv[2] != "None" else None
            max_k = int(sys.argv[3]) if len(sys.argv) > 3 else 100
            out_j = sys.argv[4] if len(sys.argv) > 4 else "extract.json"
            extract_texts(cat, max_k, out_j)
        elif cmd == "apply":
            trans_j = sys.argv[2] if len(sys.argv) > 2 else "extract.json"
            apply_translations(trans_j)
