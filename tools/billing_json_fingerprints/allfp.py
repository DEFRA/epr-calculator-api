# A short hash of each section of a billing JSON file, each results section and each producer field, and counts of names
# with commas, spaces at either end, or nothing. Prints no names or figures.
# Usage: python allfp.py 10billing.json 51billing.json ...
import json, hashlib, sys
h = lambda v: hashlib.sha256(json.dumps(v, sort_keys=True).encode()).hexdigest()[:8]
for f in sys.argv[1:]:
    d = json.load(open(f, encoding='utf-8-sig'))
    print('==', f)
    for k, v in d.items(): print('top', k, h(v))
    for k, v in (d.get('calcResultDetail') or {}).items(): print('detail', k, h(v))
    r = d.get('calculationResults') or {}
    for k, v in r.items(): print('results', k, h(v))
    p = r.get('producerCalculationResults') or []
    for k in (p[0] if p else {}): print('field', k, h([x.get(k) for x in p]))
    for k in ('producerName', 'tradingName'):
        v = [x.get(k) or '' for x in p]
        print('names', k, sum(',' in s for s in v), sum(s != s.strip() for s in v), sum(s == '' for s in v))
