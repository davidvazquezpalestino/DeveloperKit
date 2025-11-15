import pathlib
import re

root = pathlib.Path(r"d:\Github\DeveloperKit")
doc_pattern = re.compile(r"^\s*///")
method_pattern = re.compile(r"^\s*(public|protected)\s+[^=;]*\(")
skip_keywords = (" class ", " interface ", " struct ", " enum ", " delegate ")

results = []
for path in root.rglob("*.cs"):
    try:
        text = path.read_text(encoding="utf-8")
    except UnicodeDecodeError:
        text = path.read_text(encoding="latin-1")
    lines = text.splitlines()
    for idx, line in enumerate(lines):
        if method_pattern.match(line) and not any(keyword in line for keyword in skip_keywords):
            j = idx - 1
            while j >= 0 and (lines[j].strip() == "" or lines[j].lstrip().startswith("[")):
                j -= 1
            if j < 0 or not doc_pattern.match(lines[j]):
                results.append((str(path), idx + 1, line.strip()))

for file_path, line_number, signature in results[:200]:
    print(f"{file_path}:{line_number}: {signature}")
