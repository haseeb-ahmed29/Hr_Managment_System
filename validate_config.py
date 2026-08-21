import json
from pathlib import Path

path = Path('HrManagmentSystem_API/appsettings.json')
text = path.read_text()
text = text.replace(' // secure to (Users Secrets) in Development', '')
json.loads(text)
print('appsettings.json: valid JSONC configuration')
