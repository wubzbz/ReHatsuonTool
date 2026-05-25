import sys
import json
import os

sys.path.insert(0, os.path.join(os.path.dirname(__file__), "Lib", "site-packages"))

try:
    import yukkurimandarin as ym
    HAS_YM = True
except ImportError:
    HAS_YM = False

def main():
    try:
        input_data = json.load(sys.stdin)
        results = []
        for item in input_data:
            serif = item.get("serif", "")
            idx = item.get("index", 0)
            if HAS_YM:
                hatsuon = ym.text_convert(serif)
            else:
                hatsuon = serif  # fallback: return original
            results.append({"hatsuon": hatsuon, "index": idx})
        json.dump(results, sys.stdout, ensure_ascii=False)
    except Exception as e:
        json.dump({"error": str(e)}, sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()
