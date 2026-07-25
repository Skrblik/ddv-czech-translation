import zipfile
import json
import sys

zip_backup = r"G:\Games\Disney Dreamlight Valley\ddv_Data\StreamingAssets\Localization\záloha\LocDB_en-US.zip"
zip_game = r"G:\Games\Disney Dreamlight Valley\ddv_Data\StreamingAssets\Localization\LocDB_en-US.zip"
zip_orig_cz = r"G:\Games\Disney Dreamlight Valley\ddv_Data\StreamingAssets\Localization\LocDB_en-US.cz_original.bak"

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
        if pos >= len(data):
            break
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

def main():
    if len(sys.argv) < 4:
        print("Usage: python extract_batch.py <category> <limit> <output_file>")
        sys.exit(1)
        
    category = sys.argv[1]
    limit = int(sys.argv[2])
    output_file = sys.argv[3]

    extracted = {}

    with zipfile.ZipFile(zip_game, 'r') as z_game, \
         zipfile.ZipFile(zip_backup, 'r') as z_back, \
         zipfile.ZipFile(zip_orig_cz, 'r') as z_orig_cz:
         
        game_namelist = set(z_game.namelist())
        back_namelist = set(z_back.namelist())
        orig_cz_namelist = set(z_orig_cz.namelist())
        
        files_to_process = sorted(list(game_namelist | back_namelist))
        
        for f in files_to_process:
            if len(extracted) >= limit:
                break
            if not f.endswith('.locbin'):
                continue
                
            parts = f.split('/')
            cat = parts[0] if len(parts) > 1 else "Root"
            if cat != category:
                continue
                
            if f in game_namelist:
                try:
                    game_entries_dict = dict(parse_locbin(z_game.read(f)))
                except:
                    game_entries_dict = {}
            else:
                game_entries_dict = {}

            if f in orig_cz_namelist:
                try:
                    orig_cz_keys = set(e[0] for e in parse_locbin(z_orig_cz.read(f)))
                except:
                    orig_cz_keys = set()
            else:
                orig_cz_keys = set()
                
            try:
                back_entries = parse_locbin(z_back.read(f))
            except:
                continue
                
            for audio_id, text in back_entries:
                if len(extracted) >= limit:
                    break
                if not text:
                    continue
                    
                if f in orig_cz_namelist and audio_id in orig_cz_keys:
                    continue
                    
                in_game = audio_id in game_entries_dict
                current_text = game_entries_dict.get(audio_id)
                
                if not in_game or current_text == text:
                    extracted[f"{f}::{audio_id}"] = text

    with open(output_file, 'w', encoding='utf-8') as fh:
        json.dump(extracted, fh, ensure_ascii=False, indent=2)

    print(f"Extracted {len(extracted)} keys to {output_file}")

if __name__ == '__main__':
    main()
