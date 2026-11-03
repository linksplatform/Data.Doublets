"""Check only whether the known 2019 credential is revoked; never print it."""
import json
from pathlib import Path
import re
import subprocess
import urllib.error
import urllib.request

source = subprocess.check_output(['git', 'show', 'bf135cd605a52a68226de6a2a8260168ad586df6:publish-docs.sh'], text=True)
value = re.search(r'^TOKEN="([^"]+)"', source, re.M)[1]
request = urllib.request.Request('https://api.github.com/user', headers={'Authorization': 'Bearer ' + value, 'Accept': 'application/vnd.github+json'})
try:
    with urllib.request.urlopen(request, timeout=30) as response:
        status = response.status
except urllib.error.HTTPError as error:
    status = error.code
result = {'fingerprint': 'bf135cd605a52a68226de6a2a8260168ad586df6:publish-docs.sh:generic-api-key:16', 'checked_at': '2026-10-29', 'github_user_http_status': status, 'revoked': status == 401}
Path('dev/log/issues/524/pulls/526/security/historical-credential.json').write_text(json.dumps(result, indent=2) + '\n')
print(json.dumps(result))
