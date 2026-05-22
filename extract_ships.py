import os
import re
from PIL import Image

ships_dir = r"E:\NRO\SourceCode\PROJECT_NRO_240_mod\Assets\Resources\Spine\Ships"
out_dir = r"E:\NRO\SourceCode\PROJECT_NRO_240_mod\ExportedShips"

os.makedirs(out_dir, exist_ok=True)

for root, dirs, files in os.walk(ships_dir):
    for f in files:
        if f.endswith(".png.meta"):
            meta_path = os.path.join(root, f)
            png_path = meta_path[:-5]
            if not os.path.exists(png_path):
                continue
                
            ship_id = f.split('.')[0] # e.g. ship_3
            target_name = f"{ship_id}_0"
            
            with open(meta_path, 'r', encoding='utf-8') as mf:
                content = mf.read()
            
            pattern = r"name:\s*" + re.escape(target_name) + r"\s*rect:\s*serializedVersion:\s*\d+\s*x:\s*(\d+)\s*y:\s*(\d+)\s*width:\s*(\d+)\s*height:\s*(\d+)"
            match = re.search(pattern, content)
            
            if match:
                x = int(match.group(1))
                y = int(match.group(2))
                w = int(match.group(3))
                h = int(match.group(4))
                
                try:
                    img = Image.open(png_path)
                    img_w, img_h = img.size
                    
                    left = x
                    lower = y
                    right = x + w
                    upper = lower + h
                    
                    pil_top = img_h - upper
                    pil_bottom = img_h - lower
                    
                    cropped = img.crop((left, pil_top, right, pil_bottom))
                    out_path = os.path.join(out_dir, f"{target_name}.png")
                    cropped.save(out_path)
                    print(f"Exported {target_name} to {out_path}")
                except Exception as e:
                    print(f"Failed to export {target_name}: {e}")
            else:
                # Sometimes it might just be named slightly differently, or there is no _0
                pass
